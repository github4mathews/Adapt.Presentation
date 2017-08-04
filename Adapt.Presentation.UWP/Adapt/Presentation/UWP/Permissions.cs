using System;
using System.Linq;
using System.Threading.Tasks;
using wingeo = Windows.Devices.Geolocation;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Enumeration;

namespace Adapt.Presentation.UWP
{
    /// <summary>
    /// Implementation for Permissions
    /// </summary>
    public class Permissions : IPermissions
    {
        #region Fields
        private readonly Guid _ActivitySensorClassId = new Guid("9D9E0118-1807-4F2E-96E4-2CE57142E196");
        #endregion

        #region Public Methods

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Request to see if you should show a rationale for requesting permission
        /// Only on Android
        /// </summary>
        public async Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return true;
        }

        /// <summary>
        /// Determines whether this instance has permission the specified permission.
        /// </summary>
        public async Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission)
        {
            switch (permission)
            {
                case Permission.Calendar:
                    break;
                case Permission.Camera:
                    break;
                case Permission.Contacts:
                    return await CheckContactsAsync();
                case Permission.Location:
                    return await CheckLocationAsync();
                case Permission.Microphone:
                    break;
                case Permission.Phone:
                    break;
                case Permission.Photos:
                    break;
                case Permission.Reminders:
                    break;
                case Permission.Sensors:
                    {
                        // Determine if the user has allowed access to activity sensors
                        var deviceAccessInfo = DeviceAccessInformation.CreateFromDeviceClassId(_ActivitySensorClassId);
                        switch (deviceAccessInfo.CurrentStatus)
                        {
                            case DeviceAccessStatus.Allowed:
                                return PermissionStatus.Granted;
                            case DeviceAccessStatus.DeniedBySystem:
                                return PermissionStatus.Denied;
                            case DeviceAccessStatus.DeniedByUser:
                                return PermissionStatus.Denied;
                            case DeviceAccessStatus.Unspecified:
                                return PermissionStatus.Unknown;
                            default:
                                return PermissionStatus.Unknown;
                        }
                    }
                case Permission.Sms:
                    break;
                case Permission.Storage:
                    break;
                case Permission.Speech:
                    break;
                case Permission.Unknown:
                    break;
                default:
                    return PermissionStatus.Granted;
            }
            return PermissionStatus.Granted;
        }
        #endregion

        private static async Task<PermissionStatus> CheckContactsAsync()
        {
            var accessStatus = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
            return accessStatus == null ? PermissionStatus.Denied : PermissionStatus.Granted;
        }

        private static async Task<PermissionStatus> CheckLocationAsync()
        {

            var accessStatus = await wingeo.Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case wingeo.GeolocationAccessStatus.Allowed:
                    return PermissionStatus.Granted;
                case wingeo.GeolocationAccessStatus.Unspecified:
                    return PermissionStatus.Unknown;
                case wingeo.GeolocationAccessStatus.Denied:
                    return PermissionStatus.Denied;
                default:
                    return PermissionStatus.Denied;
            }
        }

        /// <summary>
        /// Requests the permissions from the users
        /// </summary>
        /// <returns>The permissions and their status.</returns>
        /// <param name="permissions">Permissions to request.</param>
        public Task<PermissionStatusDictionary> RequestPermissionsAsync(params Permission[] permissions)
        {
            var results = permissions.ToDictionary(permission => permission, permission => PermissionStatus.Granted);

            var retVal = new PermissionStatusDictionary();
            foreach (var key in results.Keys)
            {
                retVal.Add(key, results[key]);
            }

            return Task.FromResult(retVal);
        }

        public bool OpenAppSettings()
        {
            return false;
        }
    }
}