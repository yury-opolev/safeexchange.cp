
using Microsoft.Extensions.Hosting;
using SafeExchange.CP.Core;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(SafeExchangeCPStartup.ConfigureWorkerDefaults)
    .ConfigureAppConfiguration(SafeExchangeCPStartup.ConfigureAppConfiguration)
    .ConfigureServices((hostBuilderContext, serviceCollection) =>
    {
        SafeExchangeCPStartup.ConfigureServices(hostBuilderContext.Configuration, serviceCollection);
    })
    .Build();

host.Run();
