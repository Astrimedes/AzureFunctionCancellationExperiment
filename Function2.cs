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
using System.Xml.Linq;

namespace FunctionApp1
{
    public static class Function2
    {
        [FunctionName("Function2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, CancellationToken cancellationToken)
        {
            try
            {
                log.LogInformation("Function2 was triggered...");

                var rand = new Random();
                await Task.Delay(TimeSpan.FromSeconds(rand.NextInt64(1, 4)), cancellationToken);

                return new OkObjectResult("Function2 executed successfully.");
            }
            catch (OperationCanceledException ex)
            {
                log.LogError(ex, "Function2: Cancellation signal received from Host {0}", req.Host.ToString());
				return new BadRequestObjectResult(ex);
			}
            catch (Exception ex)
            {
                log.LogError($"Function2: Error: {ex.Message}");
				return new BadRequestObjectResult(ex);
			}
            
        }
    }
}
