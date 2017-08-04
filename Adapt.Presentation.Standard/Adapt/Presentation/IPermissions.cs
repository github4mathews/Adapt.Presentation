using System.Threading.Tasks;

namespace Adapt.Presentation
{
    /// <summary>
    /// Interface for Permissions
    /// </summary>
    public interface IPermissions
    {
        /// <summary>
        /// Request to see if you should show a rationale for requesting permission
        /// Only on Android
        /// </summary>
        Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission);

        /// <summary>
        /// Determines whether this instance has permission the specified permission.
        /// </summary>
        Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission);

        /// <summary>
        /// Requests the permissions from the users
        /// </summary>
        Task<PermissionStatusDictionary> RequestPermissionsAsync(params Permission[] permissions);


        /// <summary>
        /// Attempts to open the app settings to adjust the permissions.
        /// </summary>
        bool OpenAppSettings();
    }
}
