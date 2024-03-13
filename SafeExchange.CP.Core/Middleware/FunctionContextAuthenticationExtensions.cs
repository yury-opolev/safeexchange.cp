/// <summary>
/// FunctionContextAuthenticationExtensions
/// </summary>

namespace SafeExchange.CP.Core.Middleware
{
    using Microsoft.Azure.Functions.Worker;
    using System.Security.Claims;

    public static class FunctionContextAuthenticationExtensions
    {
        public static ClaimsPrincipal GetPrincipal(this FunctionContext context)
        {
            return context.Items[DefaultAuthenticationMiddleware.ClaimsPrincipalKey] as ClaimsPrincipal
                ?? throw new ArgumentNullException(DefaultAuthenticationMiddleware.ClaimsPrincipalKey);
        }
    }
}
