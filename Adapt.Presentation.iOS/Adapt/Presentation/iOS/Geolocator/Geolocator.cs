using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CoreLocation;
using Foundation;
using Adapt.Presentation.Geolocator;
#if __IOS__ || __TVOS__
using UIKit;
#elif __MACOS__
using AppKit;
#endif


namespace Adapt.Presentation.iOS.Geolocator
{
    /// <summary>
    /// Implementation for Geolocator
    /// </summary>
    [Preserve(AllMembers = true)]
    public class Geolocator : IGeolocator
    {
        #region Fields
        private bool _DeferringUpdates;
        private readonly CLLocationManager _Manager;
        private Position _Position;
        private ListenerSettings _ListenerSettings;
        #endregion

        public Geolocator()
        {
            DesiredAccuracy = 100;
            _Manager = GetManager();
            _Manager.AuthorizationChanged += OnAuthorizationChanged;
            _Manager.Failed += OnFailed;

#if __IOS__
            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                _Manager.LocationsUpdated += OnLocationsUpdated;
            else
                _Manager.UpdatedLocation += OnUpdatedLocation;

            _Manager.UpdatedHeading += OnUpdatedHeading;
#elif __MACOS__ || __TVOS__
            manager.LocationsUpdated += OnLocationsUpdated;
#endif

#if __IOS__ || __MACOS__
            _Manager.DeferredUpdatesFinished += OnDeferredUpdatedFinished;
#endif

            RequestAuthorization();
        }

        private void OnDeferredUpdatedFinished(object sender, NSErrorEventArgs e) => _DeferringUpdates = false;


#if __IOS__
        private static bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable && UIDevice.CurrentDevice.CheckSystemVersion(6, 0);
#elif __MACOS__
        bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable;
#elif __TVOS__
        bool CanDeferLocationUpdate => false;
#endif

        /// <summary>
        /// Position error event handler
        /// </summary>
        public event EventHandler<PositionErrorEventArgs> PositionError;

        /// <summary>
        /// Position changed event handler
        /// </summary>
        public event EventHandler<PositionEventArgs> PositionChanged;

        /// <summary>
        /// Desired accuracy in meters
        /// </summary>
        public double DesiredAccuracy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets if you are listening for location changes
        /// </summary>
        public bool IsListening { get; private set; }

#if __IOS__ || __MACOS__
        /// <summary>
        /// Gets if device supports heading
        /// </summary>
        public bool SupportsHeading => CLLocationManager.HeadingAvailable;
#elif __TVOS__
        /// <summary>
        /// Gets if device supports heading
        /// </summary>
        public bool SupportsHeading => false;
#endif


        /// <summary>
        /// Gets if geolocation is available on device
        /// </summary>
        public bool IsGeolocationAvailable => true; //all iOS devices support Geolocation

        /// <summary>
        /// Gets if geolocation is enabled on device
        /// </summary>
        public bool IsGeolocationEnabled
        {
            get
            {         
                var status = CLLocationManager.Status;

#if __IOS__ || __TVOS__
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    return CLLocationManager.LocationServicesEnabled && (status == CLAuthorizationStatus.AuthorizedAlways
                    || status == CLAuthorizationStatus.AuthorizedWhenInUse);
                }
#endif

                return CLLocationManager.LocationServicesEnabled && status == CLAuthorizationStatus.Authorized;
            }
        }

        private void RequestAuthorization()
        {
#if __IOS__
            var info = NSBundle.MainBundle.InfoDictionary;

            if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                return;
            }

            if (info.ContainsKey(new NSString("NSLocationAlwaysUsageDescription")))
                _Manager.RequestAlwaysAuthorization();
            else if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                _Manager.RequestWhenInUseAuthorization();
            else
                throw new UnauthorizedAccessException("On iOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription or NSLocationAlwaysUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
#elif __MACOS__
            //nothing to do here.
#elif __TVOS__
            var info = NSBundle.MainBundle.InfoDictionary;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                if (info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                    manager.RequestWhenInUseAuthorization();
                else
                    throw new UnauthorizedAccessException("On tvOS 8.0 and higher you must set either NSLocationWhenInUseUsageDescription in your Info.plist file to enable Authorization Requests for Location updates!");
            }
#endif
        }

        /// <summary>
        /// Gets the last known and most accurate location.
        /// This is usually cached and best to display first before querying for full position.
        /// </summary>
        /// <returns>Best and most recent location or null if none found</returns>
        public Task<Position> GetLastKnownLocationAsync()
        {
            var m = GetManager();
            var newLocation = m?.Location;

            if (newLocation == null)
                return null;

            var position = new Position
            {
                Accuracy = newLocation.HorizontalAccuracy,
                Altitude = newLocation.Altitude,
                AltitudeAccuracy = newLocation.VerticalAccuracy,
                Latitude = newLocation.Coordinate.Latitude,
                Longitude = newLocation.Coordinate.Longitude,
                Speed = newLocation.Speed
            };

#if !__TVOS__
#endif 

            try
            {
                position.Timestamp = new DateTimeOffset(newLocation.Timestamp.ToDateTime());
            }
            catch (Exception ex)
            {
                position.Timestamp = DateTimeOffset.UtcNow;
            }

            return Task.FromResult(position);
        }

        /// <summary>
        /// Gets position async with specified parameters
        /// </summary>
        /// <param name="timeout">Timeout to wait, Default Infinite</param>
        /// <param name="token">Cancelation token</param>
        /// <param name="includeHeading">If you would like to include heading</param>
        /// <returns>Position</returns>
		public Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
        {
            var timeoutMilliseconds = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : Timeout.Infinite;

            if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive or Timeout.Infinite");

            if (!cancelToken.HasValue)
                cancelToken = CancellationToken.None;

            TaskCompletionSource<Position> tcs;
            if (!IsListening)
            {
                var m = GetManager();

#if __IOS__
                // permit background updates if background location mode is enabled
                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    var backgroundModes = NSBundle.MainBundle.InfoDictionary[(NSString)"UIBackgroundModes"] as NSArray;
                    m.AllowsBackgroundLocationUpdates = backgroundModes != null && (backgroundModes.Contains((NSString)"Location") || backgroundModes.Contains((NSString)"location"));
                }

                // always prevent location update pausing since we're only listening for a single update.
                if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                    m.PausesLocationUpdatesAutomatically = false;
#endif

                new TaskCompletionSource<Position>(m);
                var singleListener = new GeolocationSingleUpdateDelegate(m, DesiredAccuracy, includeHeading, timeoutMilliseconds, cancelToken.Value);
                m.Delegate = singleListener;

#if __IOS__ || __MACOS__
                m.StartUpdatingLocation();
#elif __TVOS__
                m.RequestLocation();
#endif


#if __IOS__
                if (includeHeading && SupportsHeading)
                    m.StartUpdatingHeading();
#endif

                return singleListener.Task;
            }


            tcs = new TaskCompletionSource<Position>();
            if (_Position == null)
            {
                if (cancelToken != CancellationToken.None)
                {
                    cancelToken.Value.Register(() => tcs.TrySetCanceled());
                }

                void GotError(object s, PositionErrorEventArgs e)
                {
                    tcs.TrySetException(new GeolocationException(e.Error));
                    PositionError -= GotError;
                }

                PositionError += GotError;

                void GotPosition(object s, PositionEventArgs e)
                {
                    tcs.TrySetResult(e.Position);
                    PositionChanged -= GotPosition;
                }

                PositionChanged += GotPosition;
            }
            else
                tcs.SetResult(_Position);


            return tcs.Task;
        }

        /// <summary>
        /// Retrieve addresses for position.
        /// </summary>
        /// <param name="location">Desired position (latitude and longitude)</param>
        /// <returns>Addresses of the desired position</returns>
        public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position location)
        {
            if (location == null)
                return null;

            var geocoder = new CLGeocoder();
            var addressList = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(location.Latitude, location.Longitude));
            return addressList.ToAddresses();
        }

        /// <summary>
        /// Start listening for changes
        /// </summary>
        public Task<bool> StartListeningAsync(TimeSpan minTime, double minDistance, bool includeHeading = false, ListenerSettings settings = null)
        {
            if (minDistance < 0)
                throw new ArgumentOutOfRangeException(nameof(minDistance));
            if (IsListening)
                throw new InvalidOperationException("Already listening");

            // if no settings were passed in, instantiate the default settings. need to check this and create default settings since
            // previous calls to StartListeningAsync might have already configured the location manager in a non-default way that the
            // caller of this method might not be expecting. the caller should expect the defaults if they pass no settings.
            if (settings == null)
                settings = new ListenerSettings();

            // keep reference to settings so that we can stop the listener appropriately later
            _ListenerSettings = settings;

            var desiredAccuracy = DesiredAccuracy;

// set background flag
#if __IOS__
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                _Manager.AllowsBackgroundLocationUpdates = settings.AllowBackgroundUpdates;

            // configure location update pausing
            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
            {
                _Manager.PausesLocationUpdatesAutomatically = settings.PauseLocationUpdatesAutomatically;

                switch(settings.ActivityType)
                {
                    case ActivityType.AutomotiveNavigation:
                        _Manager.ActivityType = CLActivityType.AutomotiveNavigation;
                        break;
                    case ActivityType.Fitness:
                        _Manager.ActivityType = CLActivityType.Fitness;
                        break;
                    case ActivityType.OtherNavigation:
                        _Manager.ActivityType = CLActivityType.OtherNavigation;
                        break;
                    default:
                        _Manager.ActivityType = CLActivityType.Other;
                        break;
                }
            }
#endif

            // to use deferral, CLLocationManager.DistanceFilter must be set to CLLocationDistance.None, and CLLocationManager.DesiredAccuracy must be 
            // either CLLocation.AccuracyBest or CLLocation.AccuracyBestForNavigation. deferral only available on iOS 6.0 and above.
            if (CanDeferLocationUpdate && settings.DeferLocationUpdates)
            {
                minDistance = CLLocationDistance.FilterNone;
                desiredAccuracy = CLLocation.AccuracyBest;
            }

            IsListening = true;
            _Manager.DesiredAccuracy = desiredAccuracy;
            _Manager.DistanceFilter = minDistance;

#if __IOS__ || __MACOS__
            if (settings.ListenForSignificantChanges)
                _Manager.StartMonitoringSignificantLocationChanges();
            else
                _Manager.StartUpdatingLocation();
#elif __TVOS__
            //not supported
#endif

#if __IOS__
            if (includeHeading && CLLocationManager.HeadingAvailable)
                _Manager.StartUpdatingHeading();
#endif

            return Task.FromResult(true);
        }

        /// <summary>
        /// Stop listening
        /// </summary>
        public Task<bool> StopListeningAsync()
        {
            if (!IsListening)
                return Task.FromResult(true);

            IsListening = false;
#if __IOS__
            if (CLLocationManager.HeadingAvailable)
                _Manager.StopUpdatingHeading();

            // it looks like deferred location updates can apply to the standard service or significant change service. disallow deferral in either case.
            if ((_ListenerSettings?.DeferLocationUpdates ?? false) && CanDeferLocationUpdate)
                _Manager.DisallowDeferredLocationUpdates();
#endif


#if __IOS__ || __MACOS__
            if (_ListenerSettings?.ListenForSignificantChanges ?? false)
                _Manager.StopMonitoringSignificantLocationChanges();
            else
                _Manager.StopUpdatingLocation();
#endif

            _ListenerSettings = null;
            _Position = null;

            return Task.FromResult(true);
        }

        private CLLocationManager GetManager()
        {
            CLLocationManager m = null;
            new NSObject().InvokeOnMainThread(() => m = new CLLocationManager());
            return m;
        }

#if __IOS__

        private void OnUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            if (e.NewHeading.TrueHeading == -1)
                return;

            var p = _Position == null ? new Position() : new Position(_Position);

            p.Heading = e.NewHeading.TrueHeading;

            _Position = p;

            OnPositionChanged(new PositionEventArgs(p));
        }
#endif

        private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            foreach (var location in e.Locations)
                UpdatePosition(location);

            // defer future location updates if requested
            if ((_ListenerSettings?.DeferLocationUpdates ?? false) && !_DeferringUpdates && CanDeferLocationUpdate)
            {
#if __IOS__
                _Manager.AllowDeferredLocationUpdatesUntil(_ListenerSettings.DeferralDistanceMeters == null ? CLLocationDistance.MaxDistance : _ListenerSettings.DeferralDistanceMeters.GetValueOrDefault(),
                    _ListenerSettings.DeferralTime == null ? CLLocationManager.MaxTimeInterval : _ListenerSettings.DeferralTime.GetValueOrDefault().TotalSeconds);
#endif

                _DeferringUpdates = true;
            }
        }

#if __IOS__ || __MACOS__
        private void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e) => UpdatePosition(e.NewLocation);
#endif


        private void UpdatePosition(CLLocation location)
        {
            var p = _Position == null ? new Position() : new Position(_Position);

            if (location.HorizontalAccuracy > -1)
            {
                p.Accuracy = location.HorizontalAccuracy;
                p.Latitude = location.Coordinate.Latitude;
                p.Longitude = location.Coordinate.Longitude;
            }

            if (location.VerticalAccuracy > -1)
            {
                p.Altitude = location.Altitude;
                p.AltitudeAccuracy = location.VerticalAccuracy;
            }

#if __IOS__ || __MACOS__
            if (location.Speed > -1)
                p.Speed = location.Speed;
#endif

            try
            {
                var date = location.Timestamp.ToDateTime();
                p.Timestamp = new DateTimeOffset(date);
            }
            catch (Exception ex)
            {
                p.Timestamp = DateTimeOffset.UtcNow;
            }
            

            _Position = p;

            OnPositionChanged(new PositionEventArgs(p));

            location.Dispose();
        }


        private void OnPositionChanged(PositionEventArgs e) => PositionChanged?.Invoke(this, e);


        private async void OnPositionError(PositionErrorEventArgs e)
        {
            await StopListeningAsync();
            PositionError?.Invoke(this, e);
        }

        private void OnFailed(object sender, NSErrorEventArgs e)
        {
            if ((CLError)(int)e.Error.Code == CLError.Network)
                OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
        }

        private void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
        {
            if (e.Status == CLAuthorizationStatus.Denied || e.Status == CLAuthorizationStatus.Restricted)
                OnPositionError(new PositionErrorEventArgs(GeolocationError.Unauthorized));
        }

     }
}