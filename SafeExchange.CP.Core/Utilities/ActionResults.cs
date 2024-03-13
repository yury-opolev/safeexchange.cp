/// <summary>
/// ActionResults
/// </summary>

namespace SafeExchange.CP.Core
{
    using Microsoft.Azure.Functions.Worker.Http;
    using System.Net;

    public static class ActionResults
    {
        public static async Task<HttpResponseData> CreateResponseAsync<T>(HttpRequestData request, HttpStatusCode statusCode, T resultObject)
        {
            var response = request.CreateResponse();
            await response.WriteAsJsonAsync(resultObject);
            response.StatusCode = statusCode;
            return response;
        }
    }
}
