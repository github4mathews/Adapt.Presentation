using Android;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using acp = Android.Content.PM;
using app = Android.App;

namespace Adapt.Presentation.AndroidPlatform
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class Permissions : IPermissions, IDisposable
    {
        #region Fields
        private readonly object _Locker = new object();
        private TaskCompletionSource<PermissionStatusDictionary> _Tcs;
        private PermissionStatusDictionary _Results;
        private IList<string> _RequestedPermissions;
        private app.Activity _Activity;
        private IRequestPermissionsActivity _RequestPermissionsActivity;
        private const int PermissionCode = 25;
        #endregion

        #region Constructor
        public Permissions(app.Activity activity)
        {
            _Activity = activity;

            _RequestPermissionsActivity = _Activity as IRequestPermissionsActivity;
            if (_RequestPermissionsActivity == null)
            {
                throw new Exception($"The Activity must implement the {typeof(IRequestPermissionsActivity).FullName} interface, and this Activity must raise the {nameof(IRequestPermissionsActivity.PermissionsRequestCompleted)} event when the OnRequestPermissionsResult callback is made. OnRequestPermissionsResult must be overriden in the Activity for this to work. See XML documentation in the interface for more information.");
            }

            _RequestPermissionsActivity.PermissionsRequestCompleted += RequestPermissionsActivity_PermissionsRequestCompleted;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Request to see if you should show a rationale for requesting permission
        /// Only on Android
        /// </summary>
        public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission)
        {
            if (_Activity == null)
            {
                Debug.WriteLine("Unable to detect current Activity. Please ensure Plugin.CurrentActivity is installed in your Android project and your Application class is registering with Application.IActivityLifecycleCallbacks.");
                return Task.FromResult(false);
            }

            var names = GetManifestNames(permission);

            //if isn't an android specific group then go ahead and return false;
            if (names == null)
            {
                Debug.WriteLine("No android specific permissions needed for: " + permission);
                return Task.FromResult(false);
            }

            if (names.Count != 0)
            {
                return Task.FromResult(names.Any(name => ActivityCompat.ShouldShowRequestPermissionRationale(_Activity, name)));
            }

            Debug.WriteLine("No permissions found in manifest for: " + permission + " no need to show request rationale");
            return Task.FromResult(false);
        }

        /// <summary>
        /// Determines whether this instance has permission the specified permission.
        /// </summary>
        public async Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission)
        {
            var names = GetManifestNames(permission);

            //if isn't an android specific group then go ahead and return true;
            if (names == null)
            {
                Debug.WriteLine("No android specific permissions needed for: " + permission);
                return PermissionStatus.Granted;
            }

            //if no permissions were found then there is an issue and persmission is not set in Android manifest
            if (names.Count == 0)
            {
                Debug.WriteLine("No permissions found in manifest for: " + permission);
                return PermissionStatus.Unknown;
            }

            var context = _Activity ?? app.Application.Context;
            if (context != null)
            {
                return names.Any(name => ContextCompat.CheckSelfPermission(context, name) == acp.Permission.Denied) ? PermissionStatus.Denied : PermissionStatus.Granted;
            }

            Debug.WriteLine("Unable to detect current Activity or App Context. Please ensure Plugin.CurrentActivity is installed in your Android project and your Application class is registering with Application.IActivityLifecycleCallbacks.");
            return PermissionStatus.Unknown;
        }

        /// <summary>
        /// Requests the permissions from the users
        /// </summary>
        public async Task<PermissionStatusDictionary> RequestPermissionsAsync(params Permission[] permissions)
        {
            if (_Tcs != null && !_Tcs.Task.IsCompleted)
            {
                _Tcs.SetCanceled();
                _Tcs = null;
            }
            lock (_Locker)
            {
                _Results = new PermissionStatusDictionary();
            }
            if (_Activity == null)
            {
                Debug.WriteLine("Unable to detect current Activity. Please ensure Plugin.CurrentActivity is installed in your Android project and your Application class is registering with Application.IActivityLifecycleCallbacks.");
                foreach (var permission in permissions)
                {
                    if (_Results.ContainsKey(permission))
                    {
                        continue;
                    }

                    _Results.Add(permission, PermissionStatus.Unknown);
                }

                return _Results;
            }

            var permissionsToRequest = new List<string>();

            foreach (var permission in permissions)
            {
                var result = await CheckPermissionStatusAsync(permission).ConfigureAwait(false);
                if (result != PermissionStatus.Granted)
                {
                    var names = GetManifestNames(permission);
                    //check to see if we can find manifest names
                    //if we can't add as unknown and continue
                    if ((names?.Count ?? 0) == 0)
                    {
                        lock (_Locker)
                        {
                            _Results.Add(permission, PermissionStatus.Unknown);
                        }
                        continue;
                    }

                    permissionsToRequest.AddRange(names);
                }
                else
                {
                    //if we are granted you are good!
                    lock (_Locker)
                    {
                        _Results.Add(permission, PermissionStatus.Granted);
                    }
                }
            }

            if (permissionsToRequest.Count == 0)
            {
                return _Results;
            }

            _Tcs = new TaskCompletionSource<PermissionStatusDictionary>();

            ActivityCompat.RequestPermissions(_Activity, permissionsToRequest.ToArray(), PermissionCode);

            return await _Tcs.Task.ConfigureAwait(true);
        }

        public void Dispose()
        {
            if (_RequestPermissionsActivity != null)
            {
                _RequestPermissionsActivity.PermissionsRequestCompleted -= RequestPermissionsActivity_PermissionsRequestCompleted;
            }

            _Tcs?.Task?.Dispose();
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Callback that must be set when request permissions has finished
        /// </summary>
        private void RequestPermissionsActivity_PermissionsRequestCompleted(int requestCode, string[] permissions, acp.Permission[] grantResults)
        {
            if (requestCode != PermissionCode)
            {
                return;
            }

            if (_Tcs == null)
            {
                return;
            }

            for (var i = 0; i < permissions.Length; i++)
            {
                if (_Tcs.Task.Status == TaskStatus.Canceled)
                {
                    return;
                }

                var permission = GetPermissionForManifestName(permissions[i]);
                if (permission == Permission.Unknown)
                {
                    continue;
                }

                lock (_Locker)
                {
                    if (permission == Permission.Microphone)
                    {
                        if (!_Results.ContainsKey(Permission.Speech))
                        {
                            _Results.Add(Permission.Speech, grantResults[i] == acp.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
                        }
                    }
                    if (!_Results.ContainsKey(permission))
                    {
                        _Results.Add(permission, grantResults[i] == acp.Permission.Granted ? PermissionStatus.Granted : PermissionStatus.Denied);
                    }
                }
            }
            _Tcs.SetResult(_Results);
        }
        #endregion

        #region Private Static Methods
        private static Permission GetPermissionForManifestName(string permission)
        {
            switch (permission)
            {
                case Manifest.Permission.ReadCalendar:
                case Manifest.Permission.WriteCalendar:
                    return Permission.Calendar;
                case Manifest.Permission.Camera:
                    return Permission.Camera;
                case Manifest.Permission.ReadContacts:
                case Manifest.Permission.WriteContacts:
                case Manifest.Permission.GetAccounts:
                    return Permission.Contacts;
                case Manifest.Permission.AccessCoarseLocation:
                case Manifest.Permission.AccessFineLocation:
                    return Permission.Location;
                case Manifest.Permission.RecordAudio:
                    return Permission.Microphone;
                case Manifest.Permission.ReadPhoneState:
                case Manifest.Permission.CallPhone:
                case Manifest.Permission.ReadCallLog:
                case Manifest.Permission.WriteCallLog:
                case Manifest.Permission.AddVoicemail:
                case Manifest.Permission.UseSip:
                case Manifest.Permission.ProcessOutgoingCalls:
                    return Permission.Phone;
                case Manifest.Permission.BodySensors:
                    return Permission.Sensors;
                case Manifest.Permission.SendSms:
                case Manifest.Permission.ReceiveSms:
                case Manifest.Permission.ReadSms:
                case Manifest.Permission.ReceiveWapPush:
                case Manifest.Permission.ReceiveMms:
                    return Permission.Sms;
                case Manifest.Permission.ReadExternalStorage:
                case Manifest.Permission.WriteExternalStorage:
                    return Permission.Storage;
            }

            return Permission.Unknown;
        }

        private List<string> GetManifestNames(Permission permission)
        {
            var permissionNames = new List<string>();
            switch (permission)
            {
                case Permission.Calendar:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.ReadCalendar))
                        {
                            permissionNames.Add(Manifest.Permission.ReadCalendar);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.WriteCalendar))
                        {
                            permissionNames.Add(Manifest.Permission.WriteCalendar);
                        }
                    }
                    break;
                case Permission.Camera:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.Camera))
                        {
                            permissionNames.Add(Manifest.Permission.Camera);
                        }
                    }
                    break;
                case Permission.Contacts:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.ReadContacts))
                        {
                            permissionNames.Add(Manifest.Permission.ReadContacts);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.WriteContacts))
                        {
                            permissionNames.Add(Manifest.Permission.WriteContacts);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.GetAccounts))
                        {
                            permissionNames.Add(Manifest.Permission.GetAccounts);
                        }
                    }
                    break;
                case Permission.Location:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.AccessCoarseLocation))
                        {
                            permissionNames.Add(Manifest.Permission.AccessCoarseLocation);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.AccessFineLocation))
                        {
                            permissionNames.Add(Manifest.Permission.AccessFineLocation);
                        }
                    }
                    break;
                case Permission.Speech:
                case Permission.Microphone:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.RecordAudio))
                        {
                            permissionNames.Add(Manifest.Permission.RecordAudio);
                        }
                    }
                    break;
                case Permission.Phone:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.ReadPhoneState))
                        {
                            permissionNames.Add(Manifest.Permission.ReadPhoneState);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.CallPhone))
                        {
                            permissionNames.Add(Manifest.Permission.CallPhone);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.ReadCallLog))
                        {
                            permissionNames.Add(Manifest.Permission.ReadCallLog);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.WriteCallLog))
                        {
                            permissionNames.Add(Manifest.Permission.WriteCallLog);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.AddVoicemail))
                        {
                            permissionNames.Add(Manifest.Permission.AddVoicemail);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.UseSip))
                        {
                            permissionNames.Add(Manifest.Permission.UseSip);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.ProcessOutgoingCalls))
                        {
                            permissionNames.Add(Manifest.Permission.ProcessOutgoingCalls);
                        }
                    }
                    break;
                case Permission.Sensors:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.BodySensors))
                        {
                            permissionNames.Add(Manifest.Permission.BodySensors);
                        }
                    }
                    break;
                case Permission.Sms:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.SendSms))
                        {
                            permissionNames.Add(Manifest.Permission.SendSms);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.ReceiveSms))
                        {
                            permissionNames.Add(Manifest.Permission.ReceiveSms);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.ReadSms))
                        {
                            permissionNames.Add(Manifest.Permission.ReadSms);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.ReceiveWapPush))
                        {
                            permissionNames.Add(Manifest.Permission.ReceiveWapPush);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.ReceiveMms))
                        {
                            permissionNames.Add(Manifest.Permission.ReceiveMms);
                        }
                    }
                    break;
                case Permission.Storage:
                    {
                        if (HasPermissionInManifest(Manifest.Permission.ReadExternalStorage))
                        {
                            permissionNames.Add(Manifest.Permission.ReadExternalStorage);
                        }

                        if (HasPermissionInManifest(Manifest.Permission.WriteExternalStorage))
                        {
                            permissionNames.Add(Manifest.Permission.WriteExternalStorage);
                        }
                    }
                    break;
                default:
                    return null;
            }

            return permissionNames;
        }

        private bool HasPermissionInManifest(string permission)
        {
            try
            {
                if (_RequestedPermissions != null)
                {
                    return _RequestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));
                }

                //try to use current activity else application context
                var context = _Activity ?? app.Application.Context;

                if (context == null)
                {
                    Debug.WriteLine("Unable to detect current Activity or App Context. Please ensure Plugin.CurrentActivity is installed in your Android project and your Application class is registering with Application.IActivityLifecycleCallbacks.");
                    return false;
                }

                var info = context.PackageManager.GetPackageInfo(context.PackageName, Android.Content.PM.PackageInfoFlags.Permissions);

                if (info == null)
                {
                    Debug.WriteLine("Unable to get Package info, will not be able to determine permissions to request.");
                    return false;
                }

                _RequestedPermissions = info.RequestedPermissions;

                if (_RequestedPermissions != null)
                {
                    return _RequestedPermissions.Any(r => r.Equals(permission, StringComparison.InvariantCultureIgnoreCase));
                }

                Debug.WriteLine("There are no requested permissions, please check to ensure you have marked permissions you want to request.");
                return false;
            }
            catch (Exception ex)
            {
                Console.Write("Unable to check manifest for permission: " + ex);
            }
            return false;
        }

        public bool OpenAppSettings()
        {

            if (_Activity == null)
            {
                return false;
            }

            try
            {
                var settingsIntent = new Intent();
                settingsIntent.SetAction(Android.Provider.Settings.ActionApplicationDetailsSettings);
                settingsIntent.AddCategory(Intent.CategoryDefault);
                settingsIntent.SetData(Android.Net.Uri.Parse("package:" + _Activity.PackageName));
                settingsIntent.AddFlags(ActivityFlags.NewTask);
                settingsIntent.AddFlags(ActivityFlags.NoHistory);
                settingsIntent.AddFlags(ActivityFlags.ExcludeFromRecents);
                _Activity.StartActivity(settingsIntent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}