using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace MyDurableIsolatedFunction
{
    public class TheirClientFunction
    {
        private readonly ILogger _logger;

        public TheirClientFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TheirClientFunction>();
        }

        [Function("TheirClientFunction")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Their Azure Functions!");

            return response;
        }
    }
}
