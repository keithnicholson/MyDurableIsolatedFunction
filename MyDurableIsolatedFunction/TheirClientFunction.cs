using System.Net;
using System.Reflection.Metadata.Ecma335;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
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
            
            var parallelTasks = new List<Task<string>>();

            //Get a list of N work items to process in parallel.
            Task<string> task1 = context.CallActivityAsync<string>(nameof(HelloMetroCities), payload[0]);
            Task<string> task2 = context.CallActivityAsync<string>(nameof(HelloMyCollegeCities), payload[1]);
            Task<string> task3 = context.CallActivityAsync<string>(nameof(HelloOtherCollegeCities), payload[2]);

            parallelTasks.Add(task1);
            parallelTasks.Add(task2);
            parallelTasks.Add(task3);

            var answer = await Task.WhenAll(parallelTasks);
            var places = string.Join(", ", answer);


            _logger.LogWarning("Might be done. Let's see!");
            _logger.LogWarning($"Yep, you got {places}");

            //string response = await context.CallActivityAsync<string>(nameof(HelloMetroCities), payload[0]);
            //response += await context.CallActivityAsync<string>(nameof(HelloMyCollegeCities), payload[1]);
            //response += await context.CallActivityAsync<string>(nameof(HelloOtherCollegeCities), payload[2]);

            //foreach (string name in payload)
            //{
            //    _logger.LogInformation("Starting activity for name = {name}", name);
            //    string response = await context.CallActivityAsync<string>(nameof(HelloCities), name);
            //}
        }

        [Function(nameof(HelloMetroCities))]
        public async Task<string> HelloMetroCities([ActivityTrigger] string cityName, FunctionContext executionContext)
        {
            _logger.LogError("The road construction capital of the world!!!");
            await LongTimer(25);

            _logger.LogCritical("Saying hello to big metro: {name}", cityName);
            return $"Hello, {cityName}!";
        }

        [Function(nameof(HelloMyCollegeCities))]
        public async Task<string> HelloMyCollegeCities([ActivityTrigger] string cityName, FunctionContext executionContext)
        {
            _logger.LogError("A small efficient wait!");
            await LongTimer(3);

            _logger.LogCritical("Saying hello to OSU City: {name}", cityName);
            return $"Hello, {cityName}!";
        }


        [Function(nameof(HelloOtherCollegeCities))]
        public async Task<string> HelloOtherCollegeCities([ActivityTrigger] string cityName, FunctionContext executionContext)
        {
            _logger.LogError("Could be waiting a while for this school, Big 12 or SEC!");
            await LongTimer(10);
            _logger.LogCritical("Saying hello to UT City: {name}", cityName);
            return $"Hello, {cityName}!";
        }

        private async Task LongTimer(int seconds)
        {
            await Task.Delay(seconds * 1000);
            //return "Well that took some time.";
        }
    }
}
