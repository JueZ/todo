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
                await tableClient.DeleteEntityAsync("default_partition", taskId); // Assuming 'default' is your PartitionKey
            }
            catch (RequestFailedException e)
            {
                logger.LogInformation($"Could not delete task: {e.Message}");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
            }

                // Manually add CORS headers
    response.Headers.Add("Access-Control-Allow-Origin", "*");
    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Set-Cookie");
    response.Headers.Add("Access-Control-Allow-Credentials", "true");


            return response;
        }

    }
}
