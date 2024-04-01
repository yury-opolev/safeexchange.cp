/// <summary>
/// SafeCPAdminLocationsList
/// </summary>

namespace SafeExchange.ControlPlane
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Extensions.Logging;
    using SafeExchange.CP.Core;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Filters;
    using SafeExchange.CP.Core.Functions.Admin;
    using SafeExchange.CP.Core.Middleware;

    public class SafeCPAdminLocationsList
    {
        private const string Version = "v1";

        private SafeExchangeCPLocationsList safeExchangeCPLocationsListHandler;

        private readonly ILogger log;

        public SafeCPAdminLocationsList(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters, ILogger<SafeCPAdminApplications> log)
        {
            this.safeExchangeCPLocationsListHandler = new SafeExchangeCPLocationsList(dbContext, tokenHelper, globalFilters);
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [Function("SafeExchangeCP-LocationList")]
        public async Task<HttpResponseData> RunListApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"{Version}/locations-list")]
            HttpRequestData request)
        {
            var principal = request.FunctionContext.GetPrincipal();
            return await this.safeExchangeCPLocationsListHandler.RunList(request, principal, this.log);
        }
    }
}
