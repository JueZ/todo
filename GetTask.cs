using System.Net;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace juez.functions
{
    public class GetTask
    {
        private readonly ILogger _logger;

        public GetTask(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetTask>();
        }

        [Function("GetTask")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/tasks/{partitionKey}/{taskId}")] HttpRequestData req,
            string partitionKey, string taskId,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetTask");
            logger.LogInformation("Getting a specific task.");

            var tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = tableServiceClient.GetTableClient("todos");
            var response = req.CreateResponse(HttpStatusCode.OK);

            try
            {
                var taskEntity = await tableClient.GetEntityAsync<TodoTask>(partitionKey, taskId);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString(JsonConvert.SerializeObject(taskEntity.Value));
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                logger.LogInformation("Task not found.");
                response = req.CreateResponse(HttpStatusCode.NotFound);
            }

            return response;
        }
    }
}
