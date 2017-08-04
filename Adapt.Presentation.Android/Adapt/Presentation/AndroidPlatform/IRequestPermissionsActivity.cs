using acp = Android.Content.PM;

namespace Adapt.Presentation.AndroidPlatform
{
    public delegate void PermissionsRequestCompletedHander(int requestCode, string[] permissions, acp.Permission[] grantResults);

    public interface IRequestPermissionsActivity
    {
        event PermissionsRequestCompletedHander PermissionsRequestCompleted;
    }
}