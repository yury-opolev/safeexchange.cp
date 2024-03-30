/// <summary>
/// ConfigurationBuilderExtensions
/// </summary>

namespace SafeExchange.CP.Core.Configuration
{
    using Azure.Core;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddCosmosDbKeysConfiguration(this IConfigurationBuilder builder, TokenCredential tokenCredential, CosmosDbConfiguration cosmosDbConfiguration)
        {
            return builder.Add(new CosmosDbKeysSource(cosmosDbConfiguration, tokenCredential));
        }
    }
}
