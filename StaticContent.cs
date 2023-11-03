using System.Net;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace juez.functions
{
    public class StaticContent
    {
        private readonly ILogger _logger;

        public StaticContent(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StaticContent>();
        }

        [Function("StaticContent")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/{filename}")] HttpRequestData req,
            string filename,
            FunctionContext context)
        {
            var logger = context.GetLogger("StaticContent");
            logger.LogInformation($"Serving static content for filename: {filename}");

            var response = req.CreateResponse(HttpStatusCode.OK);

            // Determine the content type
            var contentType = "text/plain";
            if (filename.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "application/x-yaml";
            }
            else if (filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "application/json";
            }

            // Set the content type header
            response.Headers.Add("Content-Type", contentType);

            // Retrieve the embedded resource stream
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"todo.StaticFiles.{filename}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    // Copy the stream content to the response body
                    await stream.CopyToAsync(response.Body);
                }
                else
                {
                    // Resource not found
                    response = req.CreateResponse(HttpStatusCode.NotFound);
                }
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
