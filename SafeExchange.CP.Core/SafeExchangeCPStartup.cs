/// <summary>
/// SafeExchangeCPStartup
/// </summary>

namespace SafeExchange.CP.Core
{
    using Azure.Extensions.AspNetCore.Configuration.Secrets;
    using Azure.Identity;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using SafeExchange.CP.Core.AzureAd;
    using SafeExchange.CP.Core.Configuration;
    using SafeExchange.CP.Core.DatabaseContext;
    using SafeExchange.CP.Core.Filters;
    using SafeExchange.CP.Core.Graph;
    using SafeExchange.CP.Core.Middleware;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class SafeExchangeCPStartup
    {
        public static bool IsHttpTrigger(FunctionContext context)
            => context.FunctionDefinition.InputBindings.Values
                .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";

        public static void ConfigureWorkerDefaults(HostBuilderContext context, IFunctionsWorkerApplicationBuilder builder)
        {
            builder.UseWhen<DefaultAuthenticationMiddleware>(IsHttpTrigger);
            builder.UseWhen<TokenFilterMiddleware>(IsHttpTrigger);
        }

        public static void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
        {
            var interimConfiguration = configurationBuilder.Build();
            var keyVaultUri = new Uri(interimConfiguration["KEYVAULT_BASEURI"]);

            configurationBuilder.AddAzureKeyVault(
                keyVaultUri, new DefaultAzureCredential(), new AzureKeyVaultConfigurationOptions()
                {
                    Manager = new KeyVaultSecretManager(),
                    ReloadInterval = TimeSpan.FromMinutes(5)
                });
        }

        public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddScoped<ITokenMiddlewareCore, TokenMiddlewareCore>();
            services.AddSingleton<ITokenValidationParametersProvider, TokenValidationParametersProvider>();

            var cosmosDbConfig = new CosmosDbConfiguration();
            configuration.GetSection("CosmosDb").Bind(cosmosDbConfig);

            services.AddDbContext<SafeExchangeCPDbContext>(
                options => options.UseCosmos(
                    cosmosDbConfig.CosmosDbEndpoint, new DefaultAzureCredential(), cosmosDbConfig.DatabaseName));

            services.AddSingleton<ITokenHelper, TokenHelper>();
            services.AddSingleton<IConfidentialClientProvider, ConfidentialClientProvider>();
            services.AddSingleton<IGraphDataProvider, GraphDataProvider>();
            services.AddSingleton<GlobalFilters>();

            services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        }
    }
}
