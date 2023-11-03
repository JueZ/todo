using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

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

            // Read the request body
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            TodoTask newTask = JsonConvert.DeserializeObject<TodoTask>(requestBody);

            // Validate and assign a RowKey if it's not set
            if (string.IsNullOrEmpty(newTask.RowKey) || !IsValidKey(newTask.RowKey))
            {
                newTask.RowKey = Guid.NewGuid().ToString();
            }

            // Validate and assign a PartitionKey if it's not set
            if (string.IsNullOrEmpty(newTask.PartitionKey) || !IsValidKey(newTask.PartitionKey))
            {
                newTask.PartitionKey = "default_partition";
            }

            newTask.CreatedAt = DateTime.UtcNow.ToString("o"); // ISO 8601 format

            // Log the response that would be sent back
            string responseLog = JsonConvert.SerializeObject(newTask);
            logger.LogInformation($"Response: {responseLog}");

            // Return the newly created task to be stored in the table
            return newTask;
        }

        private static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (key.Length >= 1024)
                return false;

            if (key.Contains("/") || key.Contains("\\") || key.Contains("#") || key.Contains("?"))
                return false;

            return true;
        }
    }
} 
