using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, CancellationToken cancellationToken)
        {
            try
            {
                log.LogInformation("Function1 is processing a request.");

                // call Function2 using same cancellationToken
                using var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:7138/api/Function2", cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new OkObjectResult($"Function1 received '{responseContent}' from Function2");
            }
            catch (OperationCanceledException ex)
            {
                log.LogError(ex, "Function1: Cancellation signal sent from User-Agent {0}", req.Headers["User-Agent"].ToString());
                throw;
            }
            catch (Exception ex)
            {
                log.LogError($"Function1: Error: {ex}");
                return null;
            }
        }
    }
}
