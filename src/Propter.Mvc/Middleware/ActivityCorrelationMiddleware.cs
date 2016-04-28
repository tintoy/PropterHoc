using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
			
			Guid requestActivityId = GetOrCreateActivityId(context);
			using (CreateActivityLogScope(context, requestActivityId))
			{
				// Override current activity Id.
				context.TraceIdentifier = requestActivityId.ToString();

				// Set up the correlation manager for the current request.
				ActivityCorrelationManager previousCorrelationManager = ActivityCorrelationManager.Current;
				ActivityCorrelationManager requestCorrelationManager = context.RequestServices.GetRequiredService<ActivityCorrelationManager>();
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
		}

		/// <summary>
		///		Get or create an activity Id for the current request.
		/// </summary>
		/// <param name="context">
		///		The <see cref="HttpContext"/> for the current request.
		/// </param>
		/// <returns>
		///		The activity Id.
		/// </returns>
		Guid GetOrCreateActivityId(HttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			return
				GetActivityIdFromHeader(context)			// First try for activity Id from header.
				??
				GetActivityIdFromTraceIdentifier(context)	// Fall back to ASP.NET's activity Id.
				??
				Guid.NewGuid();                             // Otherwise, create a new activity.
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

		/// <summary>
		///		Create a logger scope for the specified logical activity.
		/// </summary>
		/// <param name="context">
		///		The <see cref="HttpContext"/> for the current request.
		/// </param>
		/// <param name="activityId">
		///		The current logical activity Id.
		/// </param>
		/// <returns>
		///		An <see cref="IDisposable"/> representing the logger scope, or <c>null</c> if logging support is not enabled.
		/// </returns>
		IDisposable CreateActivityLogScope(HttpContext context, Guid activityId)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (activityId == Guid.Empty)
				throw new ArgumentException("GUID cannot be empty: 'activityId'.", nameof(activityId));

			// Log scopes are (by convention) shared across all loggers of a given type, so this will be propagated to all registered loggers that are scope-aware.
			ILogger<ActivityCorrelationMiddleware> requestLogger = context.RequestServices.GetService<ILogger<ActivityCorrelationMiddleware>>();

			return requestLogger?.BeginScope("{ActivityId}", activityId);
		}
	}
}
