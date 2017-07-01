using System;
using System.Threading;
using Android.Locations;
using Android.OS;
using System.Collections.Generic;
using Android.Runtime;
using Adapt.Presentation.Geolocator;
using lang = Java.Lang;

namespace Adapt.Presentation.AndroidPlatform.Geolocator
{
    [Preserve(AllMembers = true)]
    internal class GeolocationContinuousListener
      : lang.Object, ILocationListener
    {
        #region Fields
        private readonly HashSet<string> _ActiveProviders = new HashSet<string>();
        private readonly LocationManager _Manager;
        private string _ActiveProvider;
        private Location _LastLocation;
        private TimeSpan _TimePeriod;
        #endregion

        #region Events
        public event EventHandler<PositionErrorEventArgs> PositionError;
        public event EventHandler<PositionEventArgs> PositionChanged;
        #endregion

        public GeolocationContinuousListener(LocationManager manager, TimeSpan timePeriod, IEnumerable<string> providers)
        {
            _Manager = manager;
            _TimePeriod = timePeriod;

            foreach (var p in providers)
            {
                if (manager.IsProviderEnabled(p))
                {
                    _ActiveProviders.Add(p);
                }
            }
        }

        public void OnLocationChanged(Location location)
        {
            if (location.Provider != _ActiveProvider)
            {
                if (_ActiveProvider != null && _Manager.IsProviderEnabled(_ActiveProvider))
                {
                    var pr = _Manager.GetProvider(location.Provider);
                    var lapsed = GetTimeSpan(location.Time) - GetTimeSpan(_LastLocation.Time);

                    if (pr.Accuracy > _Manager.GetProvider(_ActiveProvider).Accuracy
                      && lapsed < _TimePeriod.Add(_TimePeriod))
                    {
                        location.Dispose();
                        return;
                    }
                }

                _ActiveProvider = location.Provider;
            }

            var previous = Interlocked.Exchange(ref _LastLocation, location);
            previous?.Dispose();

            PositionChanged?.Invoke(this, new PositionEventArgs(location.ToPosition()));
        }

        public void OnProviderDisabled(string provider)
        {
            if (provider == LocationManager.PassiveProvider)
            {
                return;
            }

            lock (_ActiveProviders)
            {
                if (_ActiveProviders.Remove(provider) && _ActiveProviders.Count == 0)
                {
                    OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
                }
            }
        }

        public void OnProviderEnabled(string provider)
        {
            if (provider == LocationManager.PassiveProvider)
            {
                return;
            }

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

        private static TimeSpan GetTimeSpan(long time)
        {
            return new TimeSpan(TimeSpan.TicksPerMillisecond * time);
        }

        private void OnPositionError(PositionErrorEventArgs e)
        {
            PositionError?.Invoke(this, e);
        }
    }
}