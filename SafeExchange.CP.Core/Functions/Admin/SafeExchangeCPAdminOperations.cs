/// <summary>
/// SafeExchangeAdminOperations
/// </summary>

namespace SafeExchange.CP.Core.Functions.Admin
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using SafeExchange.CP.Core;
    using SafeExchange.CP.Core.Configuration;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Filters;
    using SafeExchange.CP.Core.Model;
    using System;
    using System.Net;
    using System.Security.Claims;

    public class SafeExchangeCPAdminOperations
    {
        private readonly ITokenHelper tokenHelper;

        private readonly GlobalFilters globalFilters;

        private readonly CosmosDbConfiguration cosmosDbConfiguration;

        private readonly CosmosDbKeys cosmosDbKeys;

        private readonly SafeExchangeCPDbContext dbContext;

        public SafeExchangeCPAdminOperations(IOptions<CosmosDbConfiguration> cosmosDbConfiguration, IOptions<CosmosDbKeys> cosmosDbKeys, SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters)
        {
            this.globalFilters = globalFilters ?? throw new ArgumentNullException(nameof(globalFilters));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.cosmosDbConfiguration = cosmosDbConfiguration.Value ?? throw new ArgumentNullException(nameof(cosmosDbConfiguration));
            this.cosmosDbKeys = cosmosDbKeys.Value ?? throw new ArgumentNullException(nameof(cosmosDbKeys));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<HttpResponseData> Run(
            HttpRequestData request,
            string operationName, ClaimsPrincipal principal, ILogger log)
        {
            var (shouldReturn, filterResult) = await this.globalFilters.GetFilterResultAsync(request, principal, this.dbContext);
            if (shouldReturn)
            {
                return filterResult ?? request.CreateResponse(HttpStatusCode.NoContent);
            }

            (SubjectType subjectType, string subjectId) = await SubjectHelper.GetSubjectInfoAsync(this.tokenHelper, principal, this.dbContext);
            if (SubjectType.Application.Equals(subjectType))
            {
                await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.Forbidden,
                    new BaseResponseObject<object> { Status = "forbidden", Error = "Applications cannot use this API." });
            }

            log.LogInformation($"{nameof(SafeExchangeCPAdminOperations)} triggered for operation '{operationName}' by {subjectType} {subjectId}, [{request.Method}].");

            switch (request.Method.ToLower())
            {
                case "post":
                    return await this.PerformOperationAsync(operationName, request, log);

                default:
                    var response = await ActionResults.CreateResponseAsync(
                        request, HttpStatusCode.InternalServerError,
                        new BaseResponseObject<object> { Status = "error", Error = "Request method not recognized" });
                    return response;
            }
        }

        private async Task<HttpResponseData> PerformOperationAsync(string operationName, HttpRequestData request, ILogger log)
            => await TryCatch(request, async () =>
        {
            switch (operationName)
            {
                case "ensure_dbcreated":
                    await EnsureDbCreatedAsync();
                    break;

                default:
                    // no-op
                    break;
            }

            return await ActionResults.CreateResponseAsync(
                request, HttpStatusCode.OK,
                new BaseResponseObject<object> { Status = "ok", Result = "ok" });
        }, nameof(PerformOperationAsync), log);

        private async Task EnsureDbCreatedAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SafeExchangeCPDbContext>();
            optionsBuilder.UseCosmos(
                cosmosDbConfiguration.CosmosDbEndpoint,
                cosmosDbKeys.PrimaryKey,
                cosmosDbConfiguration.DatabaseName);

            using SafeExchangeCPDbContext adminDbContext = new SafeExchangeCPDbContext(optionsBuilder.Options);
            await adminDbContext.Database.EnsureCreatedAsync();
        }

        private static async Task<HttpResponseData> TryCatch(HttpRequestData request, Func<Task<HttpResponseData>> action, string actionName, ILogger log)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, $"Exception in {actionName}: {ex.GetType()}: {ex.Message}.");

                return await ActionResults.CreateResponseAsync(
                    request, HttpStatusCode.InternalServerError,
                    new BaseResponseObject<object> { Status = "error", SubStatus = "error", Error = $"{ex.GetType()}: {ex.Message ?? "Unknown exception."}" });
            }
        }
    }
}
