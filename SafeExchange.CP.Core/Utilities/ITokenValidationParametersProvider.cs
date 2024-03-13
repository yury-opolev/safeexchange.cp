/// <summary>
/// ITokenValidationParametersProvider
/// </summary>

namespace SafeExchange.CP.Core
{
    using Microsoft.IdentityModel.Tokens;

    public interface ITokenValidationParametersProvider
    {
        public Task<TokenValidationParameters> GetTokenValidationParametersAsync();
    }
}