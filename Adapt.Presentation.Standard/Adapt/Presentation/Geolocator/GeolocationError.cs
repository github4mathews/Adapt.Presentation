namespace Adapt.Presentation.Geolocator
{
    /// <summary>
    /// Error for geolocator
    /// </summary>
    public enum GeolocationError
    {
        /// <summary>
        /// The provider was unable to retrieve any position data.
        /// </summary>
        PositionUnavailable,

        /// <summary>
        /// The app is not, or no longer, authorized to receive location data.
        /// </summary>
        Unauthorized
    }
}