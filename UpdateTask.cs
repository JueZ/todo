using System.Net;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace juez.functions
{
    public class UpdateTask
    {
        private readonly ILogger _logger;

        public UpdateTask(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateTask>();
        }

        [Function("UpdateTask")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "api/tasks/{taskId}")] HttpRequestData req,
            string taskId, // Task ID from the route
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateTask");
            logger.LogInformation("Updating a task.");

            var tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = tableServiceClient.GetTableClient("todos");
            var response = req.CreateResponse(HttpStatusCode.OK);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                TodoTask updatedTask = JsonConvert.DeserializeObject<TodoTask>(requestBody);
                updatedTask.RowKey = taskId; // Ensure the RowKey is set to the taskId from the route
                updatedTask.PartitionKey = "default"; // Assuming 'default' is your PartitionKey
                await tableClient.UpdateEntityAsync(updatedTask, updatedTask.ETag, TableUpdateMode.Replace);
            }
            catch (RequestFailedException e)
            {
                logger.LogInformation($"Could not update task: {e.Message}");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return response;
        }

    }
}
