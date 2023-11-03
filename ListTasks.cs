using System.Net;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/tasks")] HttpRequestData req,
            [TableInput("todos", Connection = "AzureWebJobsStorage")] IEnumerable<TodoTask> tableInputs,
            FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            // Serialize the IEnumerable of TodoTask to JSON and write it to the response body.
            // Exclude the Description property from serialization
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new IgnorePropertyContractResolver(nameof(TodoTask.Description))
            };
            response.WriteString(JsonConvert.SerializeObject(tableInputs, settings));
            
            return response;
        }

        // Custom contract resolver to ignore specific properties during serialization
        public class IgnorePropertyContractResolver : DefaultContractResolver
        {
            private readonly HashSet<string> _ignoreProps;
            public IgnorePropertyContractResolver(params string[] propNames)
            {
                _ignoreProps = new HashSet<string>(propNames);
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (_ignoreProps.Contains(property.PropertyName))
                {
                    property.ShouldSerialize = _ => false;
                }
                return property;
            }
        }

    }
}