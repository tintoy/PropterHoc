namespace Propter.Mvc
{
	/// <summary>
	///		Options for activity correlation.
	/// </summary>
    public sealed class ActivityCorrelationOptions
    {
		/// <summary>
		///		The default name for activity Id headers.
		/// </summary>
		public const string DefaultActivityIdHeaderName = "X-ActivityId";

		/// <summary>
		///		Create new <see cref="ActivityCorrelationOptions"/>.
		/// </summary>
		public ActivityCorrelationOptions()
		{
		}

		/// <summary>
		///		The name of the header that holds the logical activity Id.
		/// </summary>
		public string ActivityIdHeaderName { get; set; } = DefaultActivityIdHeaderName;

		/// <summary>
		///		Populate the <see cref="ActivityCorrelationManager.Current">current</see> correlation manager for the lifetime of the request?
		/// </summary>
		public bool SetCurrentCorrelationManager { get; set; } = true;
    }
}
