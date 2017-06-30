namespace Adapt.Presentation
{
    public class MediaBase
    {
        protected IPermissions CurrentPermissions { get; }

        protected MediaBase(IPermissions currentPermissions)
        {
            CurrentPermissions = currentPermissions;
        }
    }
}
