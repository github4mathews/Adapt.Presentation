namespace Adapt.Presentation
{
    /// <summary>
    /// Store Video options
    /// </summary>
    public class StoreVideoOptions
        : StoreCameraMediaOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StoreVideoOptions()
        {
            Quality = VideoQuality.High;
            DesiredLength = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Desired Length
        /// </summary>
        public TimeSpan DesiredLength
        {
            get;
        }

        /// <summary>
        /// Desired Quality
        /// </summary>
        public VideoQuality Quality
        {
            get;
        }
    }
}