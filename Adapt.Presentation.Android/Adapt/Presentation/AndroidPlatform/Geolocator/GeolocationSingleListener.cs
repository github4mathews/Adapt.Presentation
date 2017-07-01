using System;
using System.Threading.Tasks;
using Android.Locations;
using Android.OS;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Adapt.Presentation.Geolocator;

namespace Adapt.Presentation.AndroidPlatform.Geolocator
{
    [Preserve(AllMembers = true)]
    internal class GeolocationSingleListener
       : Java.Lang.Object, ILocationListener
    {
        #region Fields
        private readonly object _LocationSync = new object();
        private Location _BestLocation;
        private readonly Action _FinishedCallback;
        private readonly float _DesiredAccuracy;
        private readonly TaskCompletionSource<Position> _CompletionSource = new TaskCompletionSource<Position>();
        private readonly HashSet<string> _ActiveProviders;
        #endregion

        #region Public Properties
        public Task<Position> Task => _CompletionSource.Task;
        #endregion

        public GeolocationSingleListener(LocationManager manager, float desiredAccuracy, int timeout, IEnumerable<string> activeProviders, Action finishedCallback)
        {
            _DesiredAccuracy = desiredAccuracy;
            _FinishedCallback = finishedCallback;

            var activeProviderStrings = activeProviders as string[] ?? activeProviders.ToArray();
            _ActiveProviders = new HashSet<string>(activeProviderStrings);

            foreach(var provider in activeProviderStrings)
            {
                var location = manager.GetLastKnownLocation(provider);
                if (location != null && GeolocationUtils.IsBetterLocation(location, _BestLocation))
                {
                    _BestLocation = location;
                }
            }
            

            if (timeout != Timeout.Infinite)
            {
                new Timer(TimesUp, null, timeout, 0);
            }
        }

        public void OnLocationChanged(Location location)
        {
            if (location.Accuracy <= _DesiredAccuracy)
            {
                Finish(location);
                return;
            }

            lock (_LocationSync)
            {
                if (GeolocationUtils.IsBetterLocation(location, _BestLocation))
                {
                    _BestLocation = location;
                }
            }
        }

        

        public void OnProviderDisabled(string provider)
        {
            lock (_ActiveProviders)
            {
                if (_ActiveProviders.Remove(provider) && _ActiveProviders.Count == 0)
                {
                    _CompletionSource.TrySetException(new GeolocationException(GeolocationError.PositionUnavailable));
                }
            }
        }

        public void OnProviderEnabled(string provider)
        {
            lock (_ActiveProviders)
            {
                _ActiveProviders.Add(provider);
            }
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            switch (status)
            {
                case Availability.Available:
                    OnProviderEnabled(provider);
                    break;

                case Availability.OutOfService:
                    OnProviderDisabled(provider);
                    break;
            }
        }

        public void Cancel()
        {
            _CompletionSource.TrySetCanceled();
        }

        private void TimesUp(object state)
        {
            lock (_LocationSync)
            {
                if (_BestLocation == null)
                {
                    if (_CompletionSource.TrySetCanceled())
                    {
                        _FinishedCallback?.Invoke();
                    }
                }
                else
                {
                    Finish(_BestLocation);
                }
            }
        }

        private void Finish(Location location)
        {
            _FinishedCallback?.Invoke();
            _CompletionSource.TrySetResult(location.ToPosition());
        }
    }
}