/// <summary>
/// ConfigurationBuilderExtensions
/// </summary>

namespace SafeExchange.CP.Core.Configuration
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddCosmosDbKeysConfiguration(this IConfigurationBuilder builder)
        {
            var interimConfig = builder.Build();
            return builder.Add(new CosmosDbKeysSource(interimConfig));
        }
    }
}
