namespace Adapt.Presentation
{
    /// <summary>
    /// Status of a permission
    /// </summary>
    public enum PermissionStatus
    {
        /// <summary>
        /// Denied by user
        /// </summary>
        Denied,
        /// <summary>
        /// Feature is disabled on device
        /// </summary>
        Disabled,
        /// <summary>
        /// Granted by user
        /// </summary>
        Granted,
        /// <summary>
        /// Restricted (only iOS)
        /// </summary>
        Restricted,
        /// <summary>
        /// Permission is in an unknown state
        /// </summary>
        Unknown
    }
}
