/// <summary>
/// HealthPing
/// </summary>

namespace SafeExchange.ControlPlane
{
    using System.Net;
    using System.Text.Json;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;
    using Microsoft.Extensions.Logging;

    public class HealthPing
    {
        private readonly ILogger logger;

        public HealthPing(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<HealthPing>();
        }

        [Function(nameof(HealthPing))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request)
        {
            this.logger.LogInformation($"Starting '{nameof(HealthPing)}-{nameof(Run)}' request.");

            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var assemblyVersion = typeof(Program).Assembly.GetName().Version;
            var responseJson = JsonSerializer.Serialize(new { Status = "OK", Version = $"{assemblyVersion}" });
            response.WriteString(responseJson);

            this.logger.LogInformation($"Finishing '{nameof(HealthPing)}-{nameof(Run)}' request.");
            return response;
        }
    }
}
