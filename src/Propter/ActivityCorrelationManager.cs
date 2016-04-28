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
		///		The Id of the activity (if any) tracked by the current <see cref="ActivityCorrelationManager"/>.
		/// </summary>
		public static Guid? CurrentActivityId
		{
			get
			{
				lock (_staticStateLock)
				{
					ActivityCorrelationManager current = Current;
					if (current == null)
						return null;

					return current.ActivityId;
				}
			}
			set
			{
				lock (_staticStateLock)
				{
					ActivityCorrelationManager current = Current;
					if (current == null)
					{
						current = new ActivityCorrelationManager();
						Current = current;
					}

					current.ActivityId = value;
				}
			}
		}
	}
}
