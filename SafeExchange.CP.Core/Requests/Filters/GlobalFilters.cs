/// <summary>
/// SafeExchange
/// </summary>

namespace SafeExchange.CP.Core.Filters
{
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using SafeExchange.CP.Core.Configuration;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Graph;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class GlobalFilters : IRequestFilter
    {
        private IList<IRequestFilter> currentAdminFilters;

        public GlobalFilters(IConfiguration configuration, ITokenHelper tokenHelper, IGraphDataProvider graphDataProvider, ILogger<GlobalFilters> log)
        {
            var adminConfiguration = new AdminConfiguration();
            configuration.GetSection("AdminConfiguration").Bind(adminConfiguration);

            this.currentAdminFilters = new List<IRequestFilter>();
            currentAdminFilters.Add(new AdminGroupFilter(adminConfiguration, tokenHelper, log));
        }

        public async ValueTask<(bool shouldReturn, HttpResponseData? response)> GetFilterResultAsync(HttpRequestData req, ClaimsPrincipal principal, SafeExchangeCPDbContext dbContext)
        {
            foreach (var filter in this.currentAdminFilters)
            {
                var filterResult = await filter.GetFilterResultAsync(req, principal, dbContext);
                if (filterResult.shouldReturn)
                {
                    return filterResult;
                }
            }

            return (shouldReturn: false, response: null);
        }
    }
}
