
using System;
using CoreLocation;
using Foundation;
using System.Threading.Tasks;
using System.Threading;
using Adapt.Presentation.Geolocator;

namespace Adapt.Presentation.iOS.Geolocator
{
    [Preserve(AllMembers = true)]
    internal class GeolocationSingleUpdateDelegate : CLLocationManagerDelegate
    {
        private bool _HaveHeading;
        private bool _HaveLocation;
        private readonly Position _Position = new Position();
#if __IOS__
        private CLHeading _BestHeading;
#endif

        private readonly double _DesiredAccuracy;
        private readonly bool _IncludeHeading;
        private readonly TaskCompletionSource<Position> _Tcs;
        private readonly CLLocationManager _Manager;

        public GeolocationSingleUpdateDelegate(CLLocationManager manager, double desiredAccuracy, bool includeHeading, int timeout, CancellationToken cancelToken)
        {
            this._Manager = manager;
            _Tcs = new TaskCompletionSource<Position>(manager);
            this._DesiredAccuracy = desiredAccuracy;
            this._IncludeHeading = includeHeading;

            if (timeout != Timeout.Infinite)
            {
                Timer t = null;
                t = new Timer(s =>
                {
                    if (_HaveLocation)
                        _Tcs.TrySetResult(new Position(_Position));
                    else
                        _Tcs.TrySetCanceled();

                    StopListening();
                    t.Dispose();
                }, null, timeout, 0);
            }

            cancelToken.Register(() =>
            {
                StopListening();
                _Tcs.TrySetCanceled();
            });
        }

        public Task<Position> Task => _Tcs?.Task;


        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            // If user has services disabled, we're just going to throw an exception for consistency.
            if (status != CLAuthorizationStatus.Denied && status != CLAuthorizationStatus.Restricted)
            {
                return;
            }

            StopListening();
            _Tcs.TrySetException(new GeolocationException(GeolocationError.Unauthorized));
        }

        public override void Failed(CLLocationManager manager, NSError error)
        {
            switch ((CLError)(int)error.Code)
            {
                case CLError.Network:
                    StopListening();
                    _Tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
                    break;
                case CLError.LocationUnknown:
                    StopListening();
                    _Tcs.TrySetException(new GeolocationException(GeolocationError.PositionUnavailable));
                    break;
            }
        }


#if __IOS__
        public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => true;
#endif

#if __TVOS__
        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
            var newLocation = locations.FirstOrDefault();
            if (newLocation == null)
                return;

#else
        public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
        {
#endif
            if (newLocation.HorizontalAccuracy < 0)
                return;

            if (_HaveLocation && newLocation.HorizontalAccuracy > _Position.Accuracy)
                return;

            _Position.Accuracy = newLocation.HorizontalAccuracy;
            _Position.Altitude = newLocation.Altitude;
            _Position.AltitudeAccuracy = newLocation.VerticalAccuracy;
            _Position.Latitude = newLocation.Coordinate.Latitude;
            _Position.Longitude = newLocation.Coordinate.Longitude;
#if __IOS__ || __MACOS__
            _Position.Speed = newLocation.Speed;
#endif
            try
            {
                _Position.Timestamp = new DateTimeOffset(newLocation.Timestamp.ToDateTime());
            }
            catch(Exception ex)
            {
                _Position.Timestamp = DateTimeOffset.UtcNow;
            }
            _HaveLocation = true;

            if (_IncludeHeading && !_HaveHeading || !(_Position.Accuracy <= _DesiredAccuracy))
            {
                return;
            }

            _Tcs.TrySetResult(new Position(_Position));
            StopListening();
        }

#if __IOS__
        public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
        {
            if (newHeading.HeadingAccuracy < 0)
                return;
            if (_BestHeading != null && newHeading.HeadingAccuracy >= _BestHeading.HeadingAccuracy)
                return;

            _BestHeading = newHeading;
            _Position.Heading = newHeading.TrueHeading;
            _HaveHeading = true;

            if (!_HaveLocation || !(_Position.Accuracy <= _DesiredAccuracy))
            {
                return;
            }

            _Tcs.TrySetResult(new Position(_Position));
            StopListening();
        }
#endif


        private void StopListening()
        {
#if __IOS__
            if (CLLocationManager.HeadingAvailable)
                _Manager.StopUpdatingHeading();
#endif

            _Manager.StopUpdatingLocation();
        }
    }
}