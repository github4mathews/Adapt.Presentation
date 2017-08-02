using System;

namespace Adapt.Presentation.Geolocator
{
    /// <summary>
    /// Error ARgs
    /// </summary>
    public class PositionErrorEventArgs
        : EventArgs
    {
        /// <summary>
        /// Constructor for event error args
        /// </summary>
        /// <param name="error"></param>
        public PositionErrorEventArgs(GeolocationError error)
        {
            Error = error;
        }

        /// <summary>
        /// The Error
        /// </summary>
        public GeolocationError Error
        {
            get;
            private set;
        }
    }
}