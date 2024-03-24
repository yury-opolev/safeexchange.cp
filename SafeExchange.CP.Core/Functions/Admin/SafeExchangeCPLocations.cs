
namespace SafeExchange.CP.Core.Functions.Admin
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Filters;
    using SafeExchange.CP.Core.Model;
    using SafeExchange.CP.Core.Model.Dto.Input;
    using SafeExchange.CP.Core.Model.Dto.Output;
    using System;
    using System.Net;
    using System.Security.Claims;
    using System.Text.RegularExpressions;

    public class SafeExchangeCPLocations
    {
        private readonly SafeExchangeCPDbContext dbContext;

        private readonly ITokenHelper tokenHelper;

        private readonly GlobalFilters globalFilters;

        public SafeExchangeCPLocations(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters)
        {
            this.globalFilters = globalFilters ?? throw new ArgumentNullException(nameof(globalFilters));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<HttpResponseData> Run(
            HttpRequestData request,
            string locationName,
            ClaimsPrincipal principal, ILogger log)
        {
            var (shouldReturn, filterResponse) = await this.globalFilters.GetFilterResultAsync(request, principal, this.dbContext);
            if (shouldReturn)
            {
                return filterResponse ?? request.CreateResponse(HttpStatusCode.NoContent);
            }

            (SubjectType subjectType, string subjectId) = await SubjectHelper.GetSubjectInfoAsync(this.tokenHelper, principal, this.dbContext);
            if (SubjectType.Application.Equals(subjectType))
            {
                await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.Forbidden,
                    new BaseResponseObject<object> { Status = "forbidden", Error = "Applications cannot use this API." });
            }

            log.LogInformation($"{nameof(SafeExchangeCPLocations)} triggered for '{locationName}' by {subjectType} {subjectId} [{request.Method}].");

            switch (request.Method.ToLower())
            {
                case "post":
                    return await this.HandleLocationRegistration(request, locationName, subjectType, subjectId, log);

                case "get":
                    return await this.HandleLocationRead(request, locationName, subjectType, subjectId, log);

                case "patch":
                    return await this.HandleLocationUpdate(request, locationName, subjectType, subjectId, log);

                case "delete":
                    return await this.HandleLocationDeletion(request, locationName, subjectType, subjectId, log);

                default:
                    return await ActionResults.CreateResponseAsync(
                        request, HttpStatusCode.BadRequest,
                        new BaseResponseObject<object> { Status = "error", Error = "Request method not recognized." });
            }
        }

        private async Task<HttpResponseData> HandleLocationRegistration(HttpRequestData request, string locationName, SubjectType subjectType, string subjectId, ILogger log)
            => await TryCatch(request, async () =>
        {
            var existingRegistration = await this.dbContext.Locations.FirstOrDefaultAsync(o => o.Name.Equals(locationName));
            if (existingRegistration != null)
            {
                log.LogInformation($"Cannot register location '{locationName}', because it already exists.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.Conflict,
                    new BaseResponseObject<object> { Status = "conflict", Error = $"Location '{locationName}' is already registered." });
            }

            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            LocationInput? registrationInput;
            try
            {
                registrationInput = DefaultJsonSerializer.Deserialize<LocationInput>(requestBody);
            }
            catch
            {
                log.LogInformation($"Could not parse input data for '{locationName}'.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = "Location details are not provided." });
            }

            if (registrationInput is null)
            {
                log.LogInformation($"Input data for '{locationName}' is not provided.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = "Location details are not provided." });
            }

            if (string.IsNullOrEmpty(registrationInput.DisplayName))
            {
                log.LogInformation($"Input data for '{locationName}' {nameof(registrationInput.DisplayName)} is not provided.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = $"Location details ({nameof(registrationInput.DisplayName)}) are not provided." });
            }

            if (string.IsNullOrEmpty(registrationInput.RegionalDisplayName))
            {
                log.LogInformation($"Input data for '{locationName}' {nameof(registrationInput.RegionalDisplayName)} is not provided.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = $"Location details ({nameof(registrationInput.RegionalDisplayName)}) are not provided." });
            }

            existingRegistration = await this.dbContext.Locations
                .FirstOrDefaultAsync(r => r.DisplayName.Equals(registrationInput.DisplayName));
            if (existingRegistration != null)
            {
                log.LogInformation($"Cannot register location '{locationName}', as it already exists with provided display name.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.Conflict,
                    new BaseResponseObject<object> { Status = "conflict", Error = $"Location with display name '{registrationInput.DisplayName}' already exists." });
            }

            existingRegistration = await this.dbContext.Locations
                .FirstOrDefaultAsync(r => r.RegionalDisplayName.Equals(registrationInput.RegionalDisplayName));
            if (existingRegistration != null)
            {
                log.LogInformation($"Cannot register location '{locationName}', as it already exists with provided regional display name.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.Conflict,
                    new BaseResponseObject<object> { Status = "conflict", Error = $"Location with regional display name '{registrationInput.RegionalDisplayName}' already exists." });
            }

            var registeredLocation = await this.RegisterLocationAsync(locationName, registrationInput);
            log.LogInformation($"Location '{locationName}' ('{registrationInput.DisplayName}', '{registrationInput.RegionalDisplayName}') registered by {subjectType} '{subjectId}'.");

            return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.OK,
                    new BaseResponseObject<LocationOutput> { Status = "ok", Result = registeredLocation.ToDto() });

        }, nameof(HandleLocationRegistration), log);

        private async Task<HttpResponseData> HandleLocationRead(HttpRequestData request, string locationName, SubjectType subjectType, string subjectId, ILogger log)
            => await TryCatch(request, async () =>
        {
            var existingRegistration = await this.dbContext.Locations.FirstOrDefaultAsync(o => o.Name.Equals(locationName));
            if (existingRegistration == null)
            {
                log.LogInformation($"Cannot get location registration '{locationName}', as does not exist.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.NotFound,
                    new BaseResponseObject<object> { Status = "not_found", Error = $"Location registration '{locationName}' does not exist." });
            }

            log.LogInformation($"Location '{locationName}' ('{existingRegistration.DisplayName}', '{existingRegistration.RegionalDisplayName}') read by {subjectType} '{subjectId}'.");
            return await ActionResults.CreateResponseAsync(
                request, HttpStatusCode.OK,
                new BaseResponseObject<LocationOutput> { Status = "ok", Result = existingRegistration.ToDto() });
        }, nameof(HandleLocationRead), log);

        private async Task<HttpResponseData> HandleLocationUpdate(HttpRequestData request, string locationName, SubjectType subjectType, string subjectId, ILogger log)
            => await TryCatch(request, async () =>
        {
            var existingRegistration = await this.dbContext.Locations.FirstOrDefaultAsync(o => o.Name.Equals(locationName));
            if (existingRegistration == null)
            {
                log.LogInformation($"Cannot update location registration '{locationName}', because it does not exist.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.NotFound,
                    new BaseResponseObject<object> { Status = "not_found", Error = $"Location registration '{locationName}' does not exist." });
            }

            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            LocationInput? updateInput;
            try
            {
                updateInput = DefaultJsonSerializer.Deserialize<LocationInput>(requestBody);
            }
            catch
            {
                log.LogInformation($"Could not parse input data for '{locationName}' update.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = "Update data is not provided." });
            }

            if (updateInput is null)
            {
                log.LogInformation($"Update input for '{locationName}' is not provided.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = "Update data is not provided." });
            }

            if (string.IsNullOrEmpty(updateInput.DisplayName) && string.IsNullOrEmpty(updateInput.RegionalDisplayName))
            {
                log.LogInformation($"Update input for '{locationName}', both {nameof(updateInput.DisplayName)} and {nameof(updateInput.RegionalDisplayName)} are not provided.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.BadRequest,
                    new BaseResponseObject<object> { Status = "error", Error = "Update data is not provided." });
            }

            var updatedRegistration = await this.UpdateLocationRegistrationAsync(existingRegistration, updateInput, log);
            log.LogInformation($"{subjectType} '{subjectId}' updated location registration '{existingRegistration.Name}': '{updateInput.DisplayName ?? "-"}', '{updateInput.RegionalDisplayName ?? "-"}'.");

            return await ActionResults.CreateResponseAsync(
                request, HttpStatusCode.OK,
                new BaseResponseObject<LocationOutput> { Status = "ok", Result = updatedRegistration.ToDto() });
        }, nameof(HandleLocationUpdate), log);

        private async Task<HttpResponseData> HandleLocationDeletion(HttpRequestData request, string locationName, SubjectType subjectType, string subjectId, ILogger log)
            => await TryCatch(request, async () =>
        {
            var existingRegistration = await this.dbContext.Locations.FirstOrDefaultAsync(o => o.Name.Equals(locationName));
            if (existingRegistration == null)
            {
                log.LogInformation($"Cannot delete location registration '{locationName}', because it does not exist.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.NoContent,
                    new BaseResponseObject<string> { Status = "no_content", Result = $"Location registration '{locationName}' does not exist." });
            }

            this.dbContext.Locations.Remove(existingRegistration);
            await dbContext.SaveChangesAsync();

            log.LogInformation($"{subjectType} '{subjectId}' deleted location registration '{locationName}'.");

            return await ActionResults.CreateResponseAsync(
                request, HttpStatusCode.OK,
                new BaseResponseObject<string> { Status = "ok", Result = "ok" });
        }, nameof(HandleLocationDeletion), log);

        private async Task<Location> RegisterLocationAsync(string locationName, LocationInput registrationInput)
        {
            var locationRegistration = new Location(locationName, registrationInput.DisplayName, registrationInput.RegionalDisplayName);
            var entity = await this.dbContext.Locations.AddAsync(locationRegistration);

            await this.dbContext.SaveChangesAsync();

            return entity.Entity;
        }

        private async Task<Location> UpdateLocationRegistrationAsync(Location existingLocation, LocationInput updateInput, ILogger log)
        {
            if (!string.IsNullOrEmpty(updateInput.DisplayName))
            {
                existingLocation.DisplayName = updateInput.DisplayName;
            }

            if (!string.IsNullOrEmpty(updateInput.RegionalDisplayName))
            {
                existingLocation.RegionalDisplayName = updateInput.RegionalDisplayName;
            }

            var updatedLocationEntity = this.dbContext.Locations.Update(existingLocation);
            await this.dbContext.SaveChangesAsync();

            return updatedLocationEntity.Entity;
        }

        private static async Task<HttpResponseData> TryCatch(HttpRequestData request, Func<Task<HttpResponseData>> action, string actionName, ILogger log)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, $"Exception in {actionName}: {ex.GetType()}: {ex.Message}");

                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.InternalServerError,
                    new BaseResponseObject<object> { Status = "error", SubStatus = "internal_exception", Error = $"{ex.GetType()}: {ex.Message ?? "Unknown exception."}" });
            }
        }
    }
}
