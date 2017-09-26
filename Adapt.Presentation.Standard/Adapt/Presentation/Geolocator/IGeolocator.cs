using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adapt.Presentation.Geolocator
{
    /// <summary>
    /// Interface for Geolocator
    /// </summary>
    public interface IGeolocator
    {
        /// <summary>
        /// Position error event handler
        /// </summary>
        event EventHandler<PositionErrorEventArgs> PositionError;

        /// <summary>
        /// Position changed event handler
        /// </summary>
        event EventHandler<PositionEventArgs> PositionChanged;

        /// <summary>
        /// Desired accuracy in meters
        /// </summary>
        double DesiredAccuracy { get; set; }

        /// <summary>
        /// Gets if you are listening for location changes
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets if device supports heading
        /// </summary>
        bool SupportsHeading { get; }

        /// <summary>
        /// Gets if geolocation is available on device
        /// </summary>
        bool IsGeolocationAvailable { get; }

        /// <summary>
        /// Gets if geolocation is enabled on device
        /// </summary>
        bool IsGeolocationEnabled { get; }

        /// <summary>
        /// Gets the last known and most accurate location.
        /// This is usually cached and best to display first before querying for full position.
        /// </summary>
        Task<Position> GetLastKnownLocationAsync();

        /// <summary>
        /// Gets position async with specified parameters
        /// </summary>
        Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? token, bool includeHeading);

        /// <summary>
        /// Retrieve addresses for position.
        /// </summary>
        Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position);

        /// <summary>
        /// Start listening for changes
        /// </summary>
        Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null);


        /// <summary>
        /// Stop listening
        /// </summary>
        Task<bool> StopListeningAsync();
    }
}
