/// <summary>
/// SafeCPAdminLocations
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

    public class SafeCPAdminLocations
    {
        private const string Version = "v1";

        private SafeExchangeCPLocations safeExchangeCPLocationsHandler;

        private readonly ILogger log;

        public SafeCPAdminLocations(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters, ILogger<SafeCPAdminApplications> log)
        {
            this.safeExchangeCPLocationsHandler = new SafeExchangeCPLocations(dbContext, tokenHelper, globalFilters);
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [Function("SafeExchangeCP-Location")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get", "patch", "delete", Route = $"{Version}/locations/{{locationId}}")]
            HttpRequestData request,
            string locationId)
        {
            var principal = request.FunctionContext.GetPrincipal();
            return await this.safeExchangeCPLocationsHandler.Run(request, locationId, principal, this.log);
        }
    }
}
