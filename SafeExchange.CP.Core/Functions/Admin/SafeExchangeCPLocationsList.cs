
namespace SafeExchange.CP.Core.Functions.Admin
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Filters;
    using SafeExchange.CP.Core.Model;
    using SafeExchange.CP.Core.Model.Dto.Output;
    using System;
    using System.Net;
    using System.Security.Claims;

    public class SafeExchangeCPLocationsList
    {
        private readonly SafeExchangeCPDbContext dbContext;

        private readonly ITokenHelper tokenHelper;

        private readonly GlobalFilters globalFilters;

        public SafeExchangeCPLocationsList(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters)
        {
            this.globalFilters = globalFilters ?? throw new ArgumentNullException(nameof(globalFilters));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<HttpResponseData> RunList(
            HttpRequestData request, ClaimsPrincipal principal, ILogger log)
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

            log.LogInformation($"{nameof(SafeExchangeCPLocationsList)} triggered by {subjectType} {subjectId}, [{request.Method}].");

            switch (request.Method.ToLower())
            {
                case "get":
                    return await this.HandleListLocations(request, subjectType, subjectId, log);

                default:
                    return await ActionResults.CreateResponseAsync(
                        request, HttpStatusCode.BadRequest,
                        new BaseResponseObject<object> { Status = "error", Error = "Request method not recognized." });
            }
        }

        private async Task<HttpResponseData> HandleListLocations(HttpRequestData request, SubjectType subjectType, string subjectId, ILogger log)
            => await TryCatch(request, async () =>
            {
                var existingRegistrations = await this.dbContext.Locations.ToListAsync();

                log.LogInformation($"Locations list read by {subjectType} '{subjectId}'.");
                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.OK,
                    new BaseResponseObject<List<LocationOutput>>
                    {
                        Status = "ok",
                        Result = existingRegistrations.Select(p => p.ToDto()).ToList()
                    });
            }, nameof(HandleListLocations), log);

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
