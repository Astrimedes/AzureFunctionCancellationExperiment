using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace FunctionApp1;

public static class RetryPolicyFactory
{
	public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(int retryLimit, TimeSpan retryDelay, int retryAttemptTimeoutSeconds, Action<DelegateResult<HttpResponseMessage>, TimeSpan> onRetry = default)
	{
		var delaySpans = Backoff.DecorrelatedJitterBackoffV2(retryDelay, retryLimit);
		var retryPolicy = Policy
			.Handle<SocketException>()
			.Or<TimeoutRejectedException>()
			.OrTransientHttpError()
			.WaitAndRetryAsync(retryLimit, x => delaySpans.ElementAt(x - 1),
				(result, timespan) =>
				{
					onRetry(result, timespan);
				}
			);

		return retryPolicy.WrapAsync(Policy.TimeoutAsync<HttpResponseMessage>(retryAttemptTimeoutSeconds));
	}
}