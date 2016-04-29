using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Propter.MessageHandlers
{
	/// <summary>
	///		HTTP message handler that adds the "X-ActivityId" header to outgoing request messages using the ambient <see cref="ActivityCorrelationManager.ActivityId"/>.
	/// </summary>
	public class CorrelationMessageHandler
		: DelegatingHandler
    {
		/// <summary>
		///		Create a new <see cref="CorrelationMessageHandler"/>.
		/// </summary>
		public CorrelationMessageHandler()
		{
		}

		/// <summary>
		///		Asynchronously process an HTTP request message and its response.
		/// </summary>
		/// <param name="request">
		///		The outgoing <see cref="HttpRequestMessage"/>.
		/// </param>
		/// <param name="cancellationToken">
		///		A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
		/// </param>
		/// <returns>
		///		The incoming HTTP response message.
		/// </returns>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			Guid? activityId = ActivityCorrelationManager.GetCurrentActivityId();
			if (activityId.HasValue)
			{
				request.Headers.Remove("X-ActivityId");
				request.Headers.Add("X-ActivityId",
					activityId.Value.ToString()
				);
			}

			return base.SendAsync(request, cancellationToken);
		}
    }
}
