using System.Net;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace juez.functions
{
    public class DeleteTask
    {
        private readonly ILogger _logger;

        public DeleteTask(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeleteTask>();
        }

        [Function("DeleteTask")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/tasks/{taskId}")] HttpRequestData req,
            string taskId, // Task ID from the route
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("DeleteTask");
            logger.LogInformation("Deleting a task.");

            var tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = tableServiceClient.GetTableClient("todos");
            var response = req.CreateResponse(HttpStatusCode.NoContent);

            try
            {
                await tableClient.DeleteEntityAsync("default", taskId); // Assuming 'default' is your PartitionKey
            }
            catch (RequestFailedException e)
            {
                logger.LogInformation($"Could not delete task: {e.Message}");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return response;
        }

    }
}
