
namespace Adapt.Presentation.iOS
{
    public class PresentationFactory : IPresentationFactory
    {
        #region Implementation
        public IFilePicker CreateFilePicker()
        {
            return new FilePicker();
        }

        public IMedia CreateMedia(IPermissions currentPermissions)
        {
            return new Media();
        }
        #endregion
    }
}