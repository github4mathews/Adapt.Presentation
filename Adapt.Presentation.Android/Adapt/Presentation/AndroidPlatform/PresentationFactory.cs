using Android.Content;

namespace Adapt.Presentation.AndroidPlatform
{
    public class PresentationFactory : IPresentationFactory
    {
        #region Public Properties

        private Context Context { get; }
        private Permissions _Permissions;
        #endregion

        #region Constructor
        public PresentationFactory(Context context, Permissions permissions)
        {
            Context = context;
            _Permissions = permissions;
        }
        #endregion

        #region Implementation
        public IFilePicker CreateFilePicker()
        {
            return new FilePicker(Context, _Permissions);
        }

        public IMedia CreateMedia(IPermissions currentPermissions)
        {
            return new Media(currentPermissions);
        }
        #endregion
    }
}