using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Net.Http;

namespace Http_Trigger
{
    public static class Function1
    {
        [FunctionName("Http_Trigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [DurableClient] IDurableClient starter,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Myself data = JsonConvert.DeserializeObject<Myself>(requestBody);

            string instanceId = await starter.StartNewAsync("My_Orchestrator", data);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("My_Orchestrator")]
        public static async Task<string> MyOchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var obInput = context.GetInput<Myself>();
            return await context.CallActivityAsync<string>("My_Activity", obInput.Name);
        }

        public static string newName = "";

        [FunctionName("My_Activity")]
        public static string MyActivity([ActivityTrigger] string myName, ILogger log)
        {
            newName += myName;
            return newName;
        }
    }

    class Myself
    {
        public string Name { get; set; }
    }
}
