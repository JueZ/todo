using System;
using System.IO;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/tasks/{partitionKey}/{taskId}")] HttpRequestData req,
            string partitionKey, string taskId,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateTask");
            logger.LogInformation("Updating a task.");

            var tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = tableServiceClient.GetTableClient("todos");
            var response = req.CreateResponse(HttpStatusCode.OK);

            try
            {
                // Fetch the existing task
                var existingTaskResult = await tableClient.GetEntityAsync<TodoTask>(partitionKey, taskId);
                var existingTask = existingTaskResult.Value;

                // Read and update the task
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                TodoTask updatedTask = JsonConvert.DeserializeObject<TodoTask>(requestBody);
                updatedTask.ETag = existingTask.ETag; // Set the ETag from the existing entity
                updatedTask.PartitionKey = partitionKey;
                updatedTask.RowKey = taskId;

                await tableClient.UpdateEntityAsync(updatedTask, updatedTask.ETag, TableUpdateMode.Replace);
            }
            catch (RequestFailedException e)
            {
                logger.LogError($"Could not update task: {e.Message}");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return response;
        }
    }
}
