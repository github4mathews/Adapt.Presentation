using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Locations;
using System.Threading;
using app = Android.App;
using Android.OS;
using Android.Content;
using Android.Runtime;
using Address = Adapt.Presentation.Geolocator.Address;
using Adapt.Presentation.Geolocator;
using System.Linq;

namespace Adapt.Presentation.AndroidPlatform.Geolocator
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    [Preserve(AllMembers = true)]
    public class GeolocatorImplementation : GeolocatorBase, IGeolocator
    {
        string[] allProviders;
        LocationManager locationManager;

        GeolocationContinuousListener listener;

        readonly object positionSync = new object();
        Position lastPosition;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GeolocatorImplementation(IPermissions currentPermissions) : base(currentPermissions)
        {
            DesiredAccuracy = 100;
        }

        string[] Providers => Manager.GetProviders(enabledOnly: false).ToArray();
        string[] IgnoredProviders => new string[] { LocationManager.PassiveProvider, "local_database" };

        private LocationManager Manager => locationManager ?? (locationManager = (LocationManager) app.Application.Context.GetSystemService(Context.LocationService));

        /// <inheritdoc/>
        public event EventHandler<PositionErrorEventArgs> PositionError;
        /// <inheritdoc/>
        public event EventHandler<PositionEventArgs> PositionChanged;
        /// <inheritdoc/>
        public bool IsListening => listener != null;


        /// <inheritdoc/>
        public double DesiredAccuracy
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public bool SupportsHeading => true;


        /// <inheritdoc/>
        public bool IsGeolocationAvailable => Providers.Length > 0;


        /// <inheritdoc/>
        public bool IsGeolocationEnabled => Providers.Any(p => !IgnoredProviders.Contains(p) && Manager.IsProviderEnabled(p));


        public async Task<Position> GetLastKnownLocationAsync()
        {
            var hasPermission = await CheckPermissions();
            if (!hasPermission)
                return null;

            Location bestLocation = null;
            foreach (var provider in Providers)
            {
                var location = Manager.GetLastKnownLocation(provider);
                if (location != null && GeolocationUtils.IsBetterLocation(location, bestLocation))
                    bestLocation = location;
            }

            return bestLocation?.ToPosition();

        }

        private async Task<bool> CheckPermissions()
        {
            var status = await CurrentPermissions.CheckPermissionStatusAsync(Permission.Location);
            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            Console.WriteLine("Currently does not have Location permissions, requesting permissions");

            var request = await CurrentPermissions.RequestPermissionsAsync(Permission.Location);

            if (request[Permission.Location] == PermissionStatus.Granted)
            {
                return true;
            }

            Console.WriteLine("Location permission denied, can not get positions async.");
            return false;
        }


        /// <inheritdoc/>
        public async Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
        {
            var timeoutMilliseconds = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : Timeout.Infinite;

            if (timeoutMilliseconds <= 0 && timeoutMilliseconds != Timeout.Infinite)
                throw new ArgumentOutOfRangeException(nameof(timeout), "timeout must be greater than or equal to 0");

            if (!cancelToken.HasValue)
                cancelToken = CancellationToken.None;

            var hasPermission = await CheckPermissions();
            if (!hasPermission)
                return null;

            var tcs = new TaskCompletionSource<Position>();

            if (!IsListening)
            {
                var providers = Providers;
                GeolocationSingleListener singleListener = null;
                singleListener = new GeolocationSingleListener(Manager, (float)DesiredAccuracy, timeoutMilliseconds, providers.Where(Manager.IsProviderEnabled),
                    finishedCallback: () =>
                {
                    for (var i = 0; i < providers.Length; ++i)
                        Manager.RemoveUpdates(singleListener);
                });

                if (cancelToken != CancellationToken.None)
                {
                    cancelToken.Value.Register(() =>
                    {
                        singleListener.Cancel();

                        for (var i = 0; i < providers.Length; ++i)
                            Manager.RemoveUpdates(singleListener);
                    }, true);
                }

                try
                {
                    var looper = Looper.MyLooper() ?? Looper.MainLooper;

                    var enabled = 0;
                    foreach (var provider in providers)
                    {
                        if (Manager.IsProviderEnabled(provider))
                            enabled++;

                        Manager.RequestLocationUpdates(provider, 0, 0, singleListener, looper);
                    }

                    if (enabled == 0)
                    {
                        for (var i = 0; i < providers.Length; ++i)
                            Manager.RemoveUpdates(singleListener);

                        tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
                        return await tcs.Task;
                    }
                }
                catch (Java.Lang.SecurityException ex)
                {
                    tcs.SetException(new GeolocationException(GeolocationError.Unauthorized, ex));
                    return await tcs.Task;
                }

                return await singleListener.Task;
            }

            // If we're already listening, just use the current listener
            lock (positionSync)
            {
                if (lastPosition == null)
                {
                    if (cancelToken != CancellationToken.None)
                    {
                        cancelToken.Value.Register(() => tcs.TrySetCanceled());
                    }

                    void GotPosition(object s, PositionEventArgs e)
                    {
                        tcs.TrySetResult(e.Position);
                        PositionChanged -= GotPosition;
                    }

                    PositionChanged += GotPosition;
                }
                else
                {
                    tcs.SetResult(lastPosition);
                }
            }

            return await tcs.Task;
        }

        /// <summary>
        /// Retrieve addresses for position.
        /// </summary>
        /// <param name="position">Desired position (latitude and longitude)</param>
        /// <returns>Addresses of the desired position</returns>
        public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position position)
        {
            if (position == null)
                return null;

            var geocoder = new Geocoder(app.Application.Context);
            var addressList = await geocoder.GetFromLocationAsync(position.Latitude, position.Longitude, 10);
            return addressList.ToAddresses();
        }

        /// <inheritdoc/>
        public async Task<bool> StartListeningAsync(TimeSpan minTime, double minDistance, bool includeHeading = false, ListenerSettings settings = null)
        {
            var hasPermission = await CheckPermissions();
            if (!hasPermission)
                return false;


            var minTimeMilliseconds = minTime.TotalMilliseconds;
            if (minTimeMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(minTime));
            if (minDistance < 0)
                throw new ArgumentOutOfRangeException(nameof(minDistance));
            if (IsListening)
                throw new InvalidOperationException("This Geolocator is already listening");

            var providers = Providers;
            listener = new GeolocationContinuousListener(Manager, minTime, providers);
            listener.PositionChanged += OnListenerPositionChanged;
            listener.PositionError += OnListenerPositionError;

            var looper = Looper.MyLooper() ?? Looper.MainLooper;
            foreach (var provider in providers)
            {
                Manager.RequestLocationUpdates(provider, (long) minTimeMilliseconds, (float) minDistance, listener, looper);
            }

            return true;
        }
        /// <inheritdoc/>
        public Task<bool> StopListeningAsync()
        {
            if (listener == null)
                return Task.FromResult(true);

            var providers = Providers;
            listener.PositionChanged -= OnListenerPositionChanged;
            listener.PositionError -= OnListenerPositionError;

            for (var i = 0; i < providers.Length; ++i)
                Manager.RemoveUpdates(listener);

            listener = null;
            return Task.FromResult(true);
        }


        /// <inheritdoc/>
        private void OnListenerPositionChanged(object sender, PositionEventArgs e)
        {
            if (!IsListening) // ignore anything that might come in afterwards
                return;

            lock (positionSync)
            {
                lastPosition = e.Position;

                PositionChanged?.Invoke(this, e);
            }
        }
        /// <inheritdoc/>
        private async void OnListenerPositionError(object sender, PositionErrorEventArgs e)
        {
            await StopListeningAsync();

            PositionError?.Invoke(this, e);
        }
    }
}