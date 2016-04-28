using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Propter.Mvc.Middleware
{
	/// <summary>
	///		Middleware for activity-correlation tracking.
	/// </summary>
	public sealed class ActivityCorrelationMiddleware
	{
		/// <summary>
		///		A delegate representing the next middleware component in the pipeline.
		/// </summary>
		readonly RequestDelegate    _nextMiddleware;

		/// <summary>
		///		The name of the header to that will hold the logical activity Id.
		/// </summary>
		readonly string				_headerName;

		/// <summary>
		///		Populate the <see cref="ActivityCorrelationManager.Current">current</see> correlation manager for the lifetime of the request?
		/// </summary>
		readonly bool				_setCurrentCorrelationManager;

		/// <summary>
		///		Create new activity-correlation middleware.
		/// </summary>
		/// <param name="nextMiddleware">
		///		A delegate representing the next middleware component in the pipeline.
		/// </param>
		/// <param name="options">
		///		<see cref="ActivityCorrelationOptions"/> used to configure activity correlation.
		/// </param>
		public ActivityCorrelationMiddleware(RequestDelegate nextMiddleware, ActivityCorrelationOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			if (String.IsNullOrWhiteSpace(options.ActivityIdHeaderName))
				throw new ArgumentException($"Invalid activity-correlation options ({nameof(options.ActivityIdHeaderName)} cannot be null, empty, or entirely composed of whitespace).", nameof(options));

			if (nextMiddleware == null)
				throw new ArgumentNullException(nameof(nextMiddleware));

			_nextMiddleware = nextMiddleware;
			_headerName = options.ActivityIdHeaderName;
			_setCurrentCorrelationManager = options.SetCurrentCorrelationManager;
		}

		/// <summary>
		///		Asynchronously invoke the middleware to process the specified request.
		/// </summary>
		/// <param name="context">
		///		The <see cref="HttpContext"/> representing the request.
		/// </param>
		/// <returns>
		///		A <see cref="Task"/> representing the asynchronous operation.
		/// </returns>
		public async Task Invoke(HttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			// Get or create the request activity Id.
			Guid requestActivityId =
					GetActivityIdFromHeader(context)			// First try for activity Id from header.
					??
					GetActivityIdFromTraceIdentifier(context)	// Fall back to ASP.NET's activity Id.
					??
					Guid.NewGuid();                             // Otherwise, create a new activity.

			context.TraceIdentifier = requestActivityId.ToString();

			// Set up the correlation manager for the current request.
			ActivityCorrelationManager requestCorrelationManager = context.RequestServices.GetService<ActivityCorrelationManager>();

			ActivityCorrelationManager previousCorrelationManager = ActivityCorrelationManager.Current;
			if (_setCurrentCorrelationManager)
				ActivityCorrelationManager.Current = requestCorrelationManager;

			try
			{
				using (requestCorrelationManager.BeginActivity(requestActivityId))
				{
					await _nextMiddleware(context);
				}
			}
			finally
			{
				if (_setCurrentCorrelationManager)
					ActivityCorrelationManager.Current = previousCorrelationManager;

				context.Response.Headers[_headerName] = requestActivityId.ToString();
			}
		}

		/// <summary>
		///		Try to get a logical activity Id from the request headers.
		/// </summary>
		/// <param name="context">
		///		The <see cref="HttpContext"/> representing the current request.
		/// </param>
		/// <returns>
		///		The activity Id, or <c>null</c> if no valid activity Id was found in the request headers.
		/// </returns>
		Guid? GetActivityIdFromHeader(HttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			string activityIdHeader = context.Request.Headers[_headerName].FirstOrDefault();
			if (activityIdHeader != null)
			{
				Guid activityId;
				if (Guid.TryParse(activityIdHeader, out activityId))
					return activityId;
			}

			return null;
		}

		/// <summary>
		///		Try to get a logical activity Id from ASP.NET's <see cref="HttpContext.TraceIdentifier"/>.
		/// </summary>
		/// <param name="context">
		///		The <see cref="HttpContext"/> representing the current request.
		/// </param>
		/// <returns>
		///		The activity Id, or <c>null</c> if no valid activity Id was found in <see cref="HttpContext.TraceIdentifier"/> for the current request.
		/// </returns>
		Guid? GetActivityIdFromTraceIdentifier(HttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			Guid activityId;
			if (Guid.TryParse(context.TraceIdentifier, out activityId))
				return activityId;
			
			return null;
		}
	}
}
