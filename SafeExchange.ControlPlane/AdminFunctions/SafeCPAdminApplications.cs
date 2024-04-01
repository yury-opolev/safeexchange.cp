/// <summary>
/// SafeCPAdminApplications
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

    public class SafeCPAdminApplications
    {
        private const string Version = "v1";

        private SafeExchangeCPApplications safeExchangeCPApplicationsHandler;

        private readonly ILogger log;

        public SafeCPAdminApplications(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters, ILogger<SafeCPAdminApplications> log)
        {
            this.safeExchangeCPApplicationsHandler = new SafeExchangeCPApplications(dbContext, tokenHelper, globalFilters);
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [Function("SafeExchangeCP-Application")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get", "patch", "delete", Route = $"{Version}/applications/{{applicationId}}")]
            HttpRequestData request,
            string applicationId)
        {
            var principal = request.FunctionContext.GetPrincipal();
            return await this.safeExchangeCPApplicationsHandler.Run(request, applicationId, principal, this.log);
        }
    }
}
