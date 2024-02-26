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
using Polly.Extensions.Http;
using Polly;
using System.Collections.Generic;
using Polly.Contrib.WaitAndRetry;
using System.Net.Sockets;
using System.Net;
using Polly.Timeout;
using System.Linq;
using Polly.Retry;
using Polly.Wrap;
using System.Runtime.CompilerServices;

namespace FunctionApp1
{
	public static class Function1
    {
        private static readonly string URL_FUNCTION2 = "http://localhost:7138/api/Function2";

        private static int RetryCount = 0;

		[FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, CancellationToken cancellationToken)
        {
            try
            {
                RetryCount = 0;
                log.LogInformation("Function1 is processing a request.");

				using var client = new HttpClient();
                var policy = RetryPolicyFactory.CreateRetryPolicy(5, TimeSpan.FromMilliseconds(500), 2,
                    (retry, timeout) =>
                    {
                        RetryCount++;
                        log.LogInformation($"Attempt: {RetryCount}");
                    }
                );

				// allow either policy timeout or user token to signal cancellation
				var response = await policy.ExecuteAsync(
					async (ct) => 
                    {
                        return await client.GetAsync(URL_FUNCTION2, ct);
                    },
					cancellationToken
				);

				var responseContent = await response.Content.ReadAsStringAsync();

                return new OkObjectResult($"Function1 received '{responseContent}' from Function2");
            }
            catch (OperationCanceledException ex)
            {
                log.LogError(ex, "Function1: Cancellation signal sent from User-Agent {0}", req.Headers["User-Agent"].ToString());
                return new BadRequestObjectResult(ex);
            }
            catch (Exception ex)
            {
                log.LogError($"Function1: Error: {ex}");
				return new BadRequestObjectResult(ex);
			}
        }
    }
}
