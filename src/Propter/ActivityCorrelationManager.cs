using System;
using System.Threading;

namespace Propter
{
	/// <summary>
	///		The correlation manager for logical activities.
	/// </summary>
    public sealed class ActivityCorrelationManager
		: IDisposable
	{
		/// <summary>
		///		Activity-correlation manager for the current logical call-context.
		/// </summary>
		static readonly AsyncLocal<ActivityCorrelationManager>  _currentCorrelationManager = new AsyncLocal<ActivityCorrelationManager>();

		/// <summary>
		///		An object used to synchronise access to <see cref="_currentCorrelationManager"/>.
		/// </summary>
		static readonly object                                  _staticStateLock = new object();

		/// <summary>
		///		The Id of the activity (if any) currently tracked by the correlation manager.
		/// </summary>
		Guid?													_activityId;

		/// <summary>
		///		Create a new logical-activity correlation manager.
		/// </summary>
		public ActivityCorrelationManager()
		{
		}

		/// <summary>
		///		Dispose of resources being used by the <see cref="ActivityCorrelationManager"/>.
		/// </summary>
		public void Dispose()
		{
			IsDisposed = true;
		}

		/// <summary>
		///		Has the <see cref="ActivityCorrelationManager"/> been disposed?
		/// </summary>
		bool IsDisposed { get; set; }

		/// <summary>
		///		Check if the <see cref="ActivityCorrelationManager"/> has been disposed.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		///		The <see cref="ActivityCorrelationManager"/> has been disposed.
		/// </exception>
		void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(ActivityCorrelationManager));
		}

		/// <summary>
		///		The activity Id (if any) tracked by the correlation manager.
		/// </summary>
		public Guid? ActivityId
		{
			get
			{
				CheckDisposed();

				return _activityId;
			}
			set
			{
				CheckDisposed();

				_activityId = value;
			}
		}

		/// <summary>
		///		Is the <see cref="ActivityCorrelationManager"/> currently tracking a logical activity?
		/// </summary>
		public bool HasActivity => ActivityId.HasValue;

		/// <summary>
		///		The current activity-correlation manager.
		/// </summary>
		public static ActivityCorrelationManager Current
		{
			get
			{
				lock (_staticStateLock)
				{
					return _currentCorrelationManager.Value;
				}
			}
			set
			{
				lock (_staticStateLock)
				{
					_currentCorrelationManager.Value = value;
				}
			}
		}

		/// <summary>
		///		Ensure that there is a current <see cref="ActivityCorrelationManager"/>.
		/// </summary>
		/// <returns>
		///		The <see cref="ActivityCorrelationManager"/>.
		/// </returns>
		public static ActivityCorrelationManager EnsureCurrent()
		{
			lock (_staticStateLock)
			{
				return Current ?? (Current = new ActivityCorrelationManager());
			}
		}

		/// <summary>
		///		Start a logical activity.
		/// </summary>
		/// <param name="activityId">
		///		An optional logical activity Id (if <c>null</c> or not specified, a new activity Id will be generated).
		/// </param>
		/// <returns>
		///		An <see cref="ActivityScope"/> representing the logical activity.
		/// 
		///		When the scope is disposed the previous activity (if any) will be restored.
		/// </returns>
		public ActivityScope BeginActivity(Guid? activityId = null)
		{
			ActivityCorrelationManager correlationManager = EnsureCurrent();

			return new ActivityScope(correlationManager,
				activityId: activityId ?? Guid.NewGuid()
			);
		}

		/// <summary>
		///		Start a logical activity if one is not already in progress.
		/// </summary>
		/// <returns>
		///		An <see cref="ActivityScope"/> representing the logical activity.
		/// 
		///		When the scope is disposed the previous activity (if any) will be restored.
		/// </returns>
		public ActivityScope RequireActivity()
		{
			ActivityCorrelationManager correlationManager = EnsureCurrent();

			return new ActivityScope(correlationManager,
				activityId: correlationManager.ActivityId ?? Guid.NewGuid()
			);
		}

		/// <summary>
		///		Suppress the current logical activity (if any).
		/// </summary>
		/// <returns>
		///		An <see cref="ActivityScope"/> representing the suppression of the current logical activity.
		/// 
		///		When the scope is disposed the previous activity (if any) will be restored.
		/// </returns>
		public ActivityScope SuppressActivity()
		{
			ActivityCorrelationManager correlationManager = EnsureCurrent();

			return new ActivityScope(correlationManager,
				activityId: null
			);
		}

		/// <summary>
		///		Get the Id of the activity (if any) tracked by the current <see cref="ActivityCorrelationManager"/>.
		/// </summary>
		/// <returns>
		///		The activity Id, or <c>null</c> if there is no current activity (or current <see cref="ActivityCorrelationManager"/>).
		/// </returns>
		public static Guid? GetCurrentActivityId()
		{
			lock (_staticStateLock)
			{
				ActivityCorrelationManager current = Current;
				if (current == null)
					return null;

				return current.ActivityId;
			}
		}

		/// <summary>
		///		Get the Id of the activity (if any) tracked by the current <see cref="ActivityCorrelationManager"/>.
		/// </summary>
		/// <returns>
		///		The activity Id, or <c>null</c> if there is no current activity (or current <see cref="ActivityCorrelationManager"/>).
		/// </returns>
		public static void SetCurrentActivityId(Guid? activityId)
		{
			if (activityId == Guid.Empty)
				throw new ArgumentException("GUID cannot be empty: 'activityId'.", nameof(activityId));

			lock (_staticStateLock)
			{
				ActivityCorrelationManager current = Current;
				if (Current == null)
					Current = current = new ActivityCorrelationManager();

				current.ActivityId = activityId;
			}
		}

		/// <summary>
		///		Clear the current activity Id (if any).
		/// </summary>
		public static void ClearCurrentActivityId()
		{
			lock (_staticStateLock)
			{
				ActivityCorrelationManager current = Current;
				if (Current == null)
					return;

				current.ActivityId = null;
			}
		}
	}
}
