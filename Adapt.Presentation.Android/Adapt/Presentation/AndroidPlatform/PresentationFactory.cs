using Android.Content;

namespace Adapt.Presentation.AndroidPlatform
{
    public class PresentationFactory : IPresentationFactory
    {
        #region Public Properties

        private Context Context { get; }
        #endregion

        #region Constructor
        public PresentationFactory(Context context)
        {
            Context = context;
        }
        #endregion

        #region Implementation
        public IFilePicker CreateFilePicker()
        {
            return new FilePicker(Context);
        }

        public IMedia CreateMedia(IPermissions currentPermissions)
        {
            return new Media(currentPermissions);
        }
        #endregion
    }
}