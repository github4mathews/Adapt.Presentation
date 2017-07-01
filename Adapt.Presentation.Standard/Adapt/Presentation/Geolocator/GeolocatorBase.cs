namespace Adapt.Presentation
{
    public class GeolocatorBase
    {
        protected IPermissions CurrentPermissions { get; }

        protected GeolocatorBase(IPermissions currentPermissions)
        {
            CurrentPermissions = currentPermissions;
        }
    }
}
