/// <summary>
/// SafeAdminOperations
/// </summary>

namespace SafeExchange.ControlPlane
{
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;
    using SafeExchange.CP.Core;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Filters;
    using SafeExchange.CP.Core.Functions.Admin;
    using SafeExchange.CP.Core.Middleware;
    using System.Threading.Tasks;

    public class SafeAdminOperations
    {
        private const string Version = "v1";

        private SafeExchangeCPAdminOperations adminOperationsHandler;

        private readonly ILogger log;

        public SafeAdminOperations(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, GlobalFilters globalFilters, ILogger<SafeAdminOperations> log)
        {
            this.adminOperationsHandler = new SafeExchangeCPAdminOperations(dbContext, tokenHelper, globalFilters);
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [Function("SafeExchangeCP-AdminOperations")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = $"{Version}/admops/{{operationName}}")]
            HttpRequestData request,
            string operationName)
        {
            var principal = request.FunctionContext.GetPrincipal();
            return await this.adminOperationsHandler.Run(request, operationName, principal, this.log);
        }
    }
}
