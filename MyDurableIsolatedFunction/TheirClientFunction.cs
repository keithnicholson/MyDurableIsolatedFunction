using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
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

        [Function("IsolatedHttpTriggerDurableStarter")]
        public async Task<HttpResponseData> HttpTriggerDurableStarter(
            [HttpTrigger(AuthorizationLevel.Anonymous,"get","post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation("Keith trigger processed");

            string[] payload = { "Oklahoma City", "Stillwater", "Austin" };
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(Orchestration), payload);

            _logger.LogInformation("Marvin orch");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Nicholson Welcome");

            return response;
        }

        [Function(nameof(Orchestration))]
        public async Task Orchestration(
            [OrchestrationTrigger] TaskOrchestrationContext context,
            string[] payload)
        {

            _logger.LogInformation("Starting orchestration with instance ID = {instanceId}", context.InstanceId);
            foreach (string name in payload)
            {
                _logger.LogInformation("Starting activity for name = {name}", name);
                string response = await context.CallActivityAsync<string>(nameof(HelloCities), name);
            }
        }

        [Function(nameof(HelloCities))]
        public string HelloCities([ActivityTrigger] string cityName, FunctionContext executionContext)
        {
            _logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }
    }
}
