using Android.App;


namespace Adapt.Presentation.AndroidPlatform
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class CurrentActivityImplementation : ICurrentActivity
    {
        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        /// <value>The activity.</value>
        public Activity Activity
        {
            get;
            set;
        }
    }
}