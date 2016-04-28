using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Propter.Mvc
{
	using Middleware;

	/// <summary>
	///		Extension methods for <see cref="IApplicationBuilder"/>.
	/// </summary>
    public static class Extensions
    {
		/// <summary>
		///		Add the <see cref="ActivityCorrelationManager"/> as a request-scoped service.
		/// </summary>
		/// <param name="services">
		///		The service collection.
		/// </param>
		/// <returns>
		///		The service collection (enables method chaining).
		/// </returns>
		public static IServiceCollection AddActivityCorrelation(IServiceCollection services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			services.AddScoped<ActivityCorrelationManager>();

			return services;
		}

		/// <summary>
		///		Add activity-correlation support to the application.
		/// </summary>
		/// <param name="app">
		///		The application builder.
		/// </param>
		public static void UseActivityCorrelation(this IApplicationBuilder app)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			app.UseActivityCorrelation(new ActivityCorrelationOptions());
		}

		/// <summary>
		///		Add activity-correlation support to the application.
		/// </summary>
		/// <param name="app">
		///		The application builder.
		/// </param>
		/// <param name="activityIdHeaderName">
		///		The name of the header that holds the logical activity Id.
		/// </param>
		public static void UseActivityCorrelation(this IApplicationBuilder app, string activityIdHeaderName)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			if (String.IsNullOrWhiteSpace(activityIdHeaderName))
				throw new ArgumentException("Argument cannot be null, empty, or composed entirely of whitespace: 'headerName'.", nameof(activityIdHeaderName));

			app.UseActivityCorrelation(new ActivityCorrelationOptions
			{
				ActivityIdHeaderName = activityIdHeaderName
			});
		}

		/// <summary>
		///		Add activity-correlation support to the application.
		/// </summary>
		/// <param name="app">
		///		The application builder.
		/// </param>
		/// <param name="options">
		///		<see cref="ActivityCorrelationOptions"/> used to configure activity correlation.
		/// </param>
		public static void UseActivityCorrelation(this IApplicationBuilder app, ActivityCorrelationOptions options)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			app.UseMiddleware<ActivityCorrelationMiddleware>(options);
		}
    }
}
