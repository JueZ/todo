using System;
using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace juez.functions
{
    public class AddTask
    {
        private readonly ILogger _logger;

        public AddTask(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AddTask>();
        }

        [Function("AddTask")]
        [TableOutput("todos", Connection = "AzureWebJobsStorage")]
        public TodoTask Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/tasks")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AddTask");
            logger.LogInformation("Adding a new task.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            TodoTask newTask = JsonConvert.DeserializeObject<TodoTask>(requestBody);

            // Use username or userID as the PartitionKey
            if (string.IsNullOrEmpty(newTask.PartitionKey))
            {
                logger.LogError("PartitionKey (username or userID) is missing.");
                throw new InvalidOperationException("PartitionKey is required.");
            }

            // Generate RowKey if not set
            if (string.IsNullOrEmpty(newTask.RowKey))
            {
                newTask.RowKey = Guid.NewGuid().ToString();
            }

            newTask.CreatedAt = DateTime.UtcNow.ToString("o"); // ISO 8601 format

            return newTask;
        }
    }
}
