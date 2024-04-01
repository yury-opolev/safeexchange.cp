/// <summary>
/// SafeCPAdminApplicationsList
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

    public class SafeCPAdminApplicationsList
    {
        private const string Version = "v1";

        private SafeExchangeCPApplicationsList safeExchangeCPApplicationsListHandler;

        private readonly ILogger log;

        public SafeCPAdminApplicationsList(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters, ILogger<SafeCPAdminApplications> log)
        {
            this.safeExchangeCPApplicationsListHandler = new SafeExchangeCPApplicationsList(dbContext, tokenHelper, globalFilters);
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [Function("SafeExchangeCP-ApplicationList")]
        public async Task<HttpResponseData> RunListApplications(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"{Version}/applications-list")]
            HttpRequestData request)
        {
            var principal = request.FunctionContext.GetPrincipal();
            return await this.safeExchangeCPApplicationsListHandler.RunList(request, principal, this.log);
        }
    }
}
