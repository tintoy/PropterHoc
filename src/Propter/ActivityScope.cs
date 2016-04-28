using System;

namespace Propter
{
	/// <summary>
	///		Represents a scope for an activity.
	/// </summary>
	/// <remarks>
	///		When the scope is disposed, the previous activity Id (if any) will be restored.
	/// </remarks>
	/// <seealso cref="ActivityCorrelationManager"/>
	public struct ActivityScope
		: IDisposable
	{
		/// <summary>
		///		Create a new activity scope.
		/// </summary>
		/// <param name="correlationManager">
		///		The <see cref="ActivityCorrelationManager"/> that created the scope.
		/// </param>
		/// <param name="activityId">
		///		The current activity Id (if any).
		/// </param>
		internal ActivityScope(ActivityCorrelationManager correlationManager, Guid? activityId)
		{
			if (correlationManager == null)
				throw new ArgumentNullException(nameof(correlationManager));

			CorrelationManager = correlationManager;
			PreviousActivityId = correlationManager.ActivityId;

			ActivityId = activityId;
			CorrelationManager.ActivityId = activityId;
		}

		/// <summary>
		///		Dispose of resources being used by the object.
		/// </summary>
		public void Dispose()
		{
			// If the correlation manager does not have the expected activity Id, it's safer to not clean up.
			if (CorrelationManager.ActivityId != ActivityId)
				return;

			// Restore previous activity Id (if any).
			CorrelationManager.ActivityId = PreviousActivityId;
		}

		/// <summary>
		///		The <see cref="ActivityCorrelationManager"/> that created the scope.
		/// </summary>
		ActivityCorrelationManager CorrelationManager { get; }

		/// <summary>
		///		The current activity Id (if any).
		/// </summary>
		public Guid? ActivityId { get; }

		/// <summary>
		///		The previous activity Id (if any).
		/// </summary>
		public Guid? PreviousActivityId { get; }
	}
}
