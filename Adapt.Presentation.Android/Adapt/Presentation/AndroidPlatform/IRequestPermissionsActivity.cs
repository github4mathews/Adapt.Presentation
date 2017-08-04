using acp = Android.Content.PM;

namespace Adapt.Presentation.AndroidPlatform
{
    public delegate void PermissionsRequestCompletedHander(int requestCode, string[] permissions, acp.Permission[] grantResults);

    /// <summary>
    /// This interface is designed to work around a design quirk of Android's permission request system. On Android, there is no way to simply request permission and await the result. A call must be made to pop up the request window, and then the result will be returned to the OnRequestPermissionsResult method on the Activity. So, the activity must implement this interface, and raise the PermissionsRequestCompleted event when teh callback is raised.
    /// </summary>
    public interface IRequestPermissionsActivity
    {
        /// <summary>
        /// The OnRequestPermissionsResult method was called on the Activity
        /// </summary>
        event PermissionsRequestCompletedHander PermissionsRequestCompleted;
    }
}