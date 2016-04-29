using HTTPlease;
using System;

namespace Propter
{
	using MessageHandlers;

	/// <summary>
	///		Propter Hoc extension methods for the <see cref="ClientBuilder">HTTP client builder</see>.
	/// </summary>
    public static class ClientBuilderExtensions
    {
		/// <summary>
		///		Create a copy of the <see cref="ClientBuilder"/>, enabling activity-correlation support for its clients.
		/// </summary>
		/// <param name="clientBuilder">
		///		The HTTP client builder.
		/// </param>
		/// <returns>
		///		The new HTTP client builder.
		/// </returns>
		public static ClientBuilder WithActivityCorrelation(this ClientBuilder clientBuilder)
		{
			if (clientBuilder == null)
				throw new ArgumentNullException(nameof(clientBuilder));

			return clientBuilder.AddHandler(
				() => new CorrelationMessageHandler()
			);
		}
    }
}
