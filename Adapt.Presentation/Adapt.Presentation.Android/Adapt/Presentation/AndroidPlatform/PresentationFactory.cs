using Android.Content;

namespace Adapt.Presentation.AndroidPlatform
{
    public class PresentationFactory : IPresentationFactory
    {
        #region Public Properties
        public Context Context { get; private set; }
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

        public IMedia CreateMedia()
        {
            return new Media();
        }
        #endregion
    }
}