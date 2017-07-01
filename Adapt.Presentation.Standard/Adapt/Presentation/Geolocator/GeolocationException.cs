using System;

namespace Adapt.Presentation.Geolocator
{
    /// <summary>
    /// Location exception
    /// </summary>
    public class GeolocationException
        : Exception
    {
        /// <summary>
        /// Location exception
        /// </summary>
        public GeolocationException(GeolocationError error)
            : base("A geolocation error occurred: " + error)
        {
            if (!Enum.IsDefined(typeof(GeolocationError), error))
            {
                throw new ArgumentException("error is not a valid GelocationError member", nameof(error));
            }

            Error = error;
        }

        /// <summary>
        /// Geolocation error
        /// </summary>
        public GeolocationException(GeolocationError error, Exception innerException)
            : base("A geolocation error occurred: " + error, innerException)
        {
            if (!Enum.IsDefined(typeof(GeolocationError), error))
            {
                throw new ArgumentException("error is not a valid GelocationError member", nameof(error));
            }

            Error = error;
        }

        //The error
        private GeolocationError Error
        {
            get;
        }
    }
}