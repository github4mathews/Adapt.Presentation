using Android.Locations;
using Adapt.Presentation.Geolocator;
using System;
using System.Collections.Generic;
using Address = Adapt.Presentation.Geolocator.Address;
using System.Linq;

namespace Adapt.Presentation.AndroidPlatform.Geolocator
{
    public static class GeolocationUtils
    {
        private const int TwoMinutes = 120000;

        internal static bool IsBetterLocation(Location location, Location bestLocation)
        {

            if (bestLocation == null)
            {
                return true;
            }

            var timeDelta = location.Time - bestLocation.Time;
            var isSignificantlyNewer = timeDelta > TwoMinutes;
            var isSignificantlyOlder = timeDelta < -TwoMinutes;
            var isNewer = timeDelta > 0;

            if (isSignificantlyNewer)
            {
                return true;
            }

            if (isSignificantlyOlder)
            {
                return false;
            }

            var accuracyDelta = (int)(location.Accuracy - bestLocation.Accuracy);
            var isLessAccurate = accuracyDelta > 0;
            var isMoreAccurate = accuracyDelta < 0;
            var isSignificantlyLessAccurage = accuracyDelta > 200;

            var isFromSameProvider = IsSameProvider(location.Provider, bestLocation.Provider);

            if (isMoreAccurate)
            {
                return true;
            }

            if (isNewer && !isLessAccurate)
            {
                return true;
            }

            return isNewer && !isSignificantlyLessAccurage && isFromSameProvider;
        }

        private static bool IsSameProvider(string provider1, string provider2)
        {
            if (provider1 == null)
            {
                return provider2 == null;
            }

            return provider1.Equals(provider2);
        }

        internal static Position ToPosition(this Location location)
        {
            var p = new Position();
            if (location.HasAccuracy)
            {
                p.Accuracy = location.Accuracy;
            }

            if (location.HasAltitude)
            {
                p.Altitude = location.Altitude;
            }

            if (location.HasBearing)
            {
                p.Heading = location.Bearing;
            }

            if (location.HasSpeed)
            {
                p.Speed = location.Speed;
            }

            p.Longitude = location.Longitude;
            p.Latitude = location.Latitude;
            p.Timestamp = location.GetTimestamp();
            return p;
        }

        internal static IEnumerable<Address> ToAddresses(this IEnumerable<Android.Locations.Address> addresses)
        {
            return addresses.Select(address => new Address
            {
                Longitude = address.Longitude,
                Latitude = address.Latitude,
                FeatureName = address.FeatureName,
                PostalCode = address.PostalCode,
                SubLocality = address.SubLocality,
                CountryCode = address.CountryCode,
                CountryName = address.CountryName,
                Thoroughfare = address.Thoroughfare,
                SubThoroughfare = address.SubThoroughfare,
                Locality = address.Locality
            });
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static DateTimeOffset GetTimestamp(this Location location)
        {
            try
            {
                return new DateTimeOffset(Epoch.AddMilliseconds(location.Time));
            }
            catch (Exception e)
            {
                return new DateTimeOffset(Epoch);
            }
        }
    }
}