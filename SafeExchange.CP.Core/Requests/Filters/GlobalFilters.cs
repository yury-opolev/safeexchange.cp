/// <summary>
/// SafeExchange
/// </summary>

namespace SafeExchange.CP.Core.Filters
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using SafeExchange.CP.Core.Configuration;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Graph;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class GlobalFilters : IRequestFilter
    {
        private readonly IOptionsMonitor<AdminConfiguration> adminConfiguration;

        private readonly ITokenHelper tokenHelper;
        private readonly ILogger log;

        public GlobalFilters(IOptionsMonitor<AdminConfiguration> adminConfiguration, ITokenHelper tokenHelper, IGraphDataProvider graphDataProvider, ILogger<GlobalFilters> log)
        {
            this.adminConfiguration = adminConfiguration ?? throw new ArgumentNullException(nameof(adminConfiguration));
            this.tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async ValueTask<(bool shouldReturn, HttpResponseData? response)> GetFilterResultAsync(HttpRequestData req, ClaimsPrincipal principal, SafeExchangeCPDbContext dbContext)
        {
            var adminFilter = new AdminGroupFilter(this.adminConfiguration.CurrentValue, this.tokenHelper, this.log);
            var filterResult = await adminFilter.GetFilterResultAsync(req, principal, dbContext);
            if (filterResult.shouldReturn)
            {
                return filterResult;
            }

            return (shouldReturn: false, response: null);
        }
    }
}
