/// <summary>
/// TokenFilterMiddleware
/// </summary>

namespace SafeExchange.CP.Core.Middleware
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core;
    using System;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class TokenMiddlewareCore : ITokenMiddlewareCore
    {
        private readonly SafeExchangeCPDbContext dbContext;

        private readonly ITokenHelper tokenHelper;

        private readonly ILogger log;

        public TokenMiddlewareCore(SafeExchangeCPDbContext dbContext, ITokenHelper tokenHelper, ILogger<TokenMiddlewareCore> log)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async ValueTask<(bool shouldReturn, HttpResponseData? response)> RunAsync(HttpRequestData request, ClaimsPrincipal principal)
        {
            (bool shouldReturn, HttpResponseData? response) result = (shouldReturn: false, response: null);

            var isUserToken = this.tokenHelper.IsUserToken(principal);
            if (!isUserToken && await this.IsRegisteredApplicationAsync(principal))
            {
                return result;
            }

            var tenantId = this.tokenHelper.GetTenantId(principal);
            var objectId = this.tokenHelper.GetObjectId(principal);
            var clientId = this.tokenHelper.GetApplicationClientId(principal);

            this.log.LogInformation($"Caller [{clientId}] '{tenantId}.{objectId}' is not authenticated with a token from registered application.");
            result.shouldReturn = true;
            result.response = await ActionResults.CreateResponseAsync(
                request, HttpStatusCode.Forbidden,
                new BaseResponseObject<object> { Status = "forbidden", Error = "Forbidden." });

            return result;
        }

        private async Task<bool> IsRegisteredApplicationAsync(ClaimsPrincipal principal)
        {
            var clientId = this.tokenHelper.GetApplicationClientId(principal);
            if (string.IsNullOrEmpty(clientId))
            {
                return false;
            }

            var tenantId = this.tokenHelper.GetTenantId(principal);
            if (string.IsNullOrEmpty(tenantId))
            {
                return false;
            }

            var registeredApplication = await this.dbContext.Applications.FirstOrDefaultAsync(a => a.Enabled && a.AadClientId.Equals(clientId) && a.AadTenantId.Equals(tenantId));
            return registeredApplication != default;
        }
    }
}
