namespace Adapt.Presentation
{
    public class GeolocatorBase
    {
        public IPermissions CurrentPermissions { get; private set; }

        public GeolocatorBase(IPermissions currentPermissions)
        {
            CurrentPermissions = currentPermissions;
        }
    }
}
