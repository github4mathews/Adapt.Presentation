namespace Adapt.Presentation
{
    public class MediaBase
    {
        public IPermissions CurrentPermissions { get; private set; }

        public MediaBase(IPermissions currentPermissions)
        {
            CurrentPermissions = currentPermissions;
        }
    }
}
