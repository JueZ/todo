using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using System.Collections.Generic;

namespace juez.functions
{
    public class ListTasks
    {
        private readonly ILogger _logger;

        public ListTasks(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ListTasks>();
        }

        [Function("ListTasks")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/tasks/{partitionKey}")] HttpRequestData req,
            string partitionKey,
            FunctionContext context)
        {
            var logger = context.GetLogger("ListTasks");
            logger.LogInformation("Listing tasks.");

            var tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = tableServiceClient.GetTableClient("todos");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            await foreach (var task in tableClient.QueryAsync<TodoTask>(entity => entity.PartitionKey == partitionKey))
            {
                response.WriteString(JsonConvert.SerializeObject(task));
            }

            return response;
        }
    }
}
