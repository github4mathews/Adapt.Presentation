namespace Adapt.Presentation
{
    public interface IPresentationFactory
    {
        IMedia CreateMedia(IPermissions currentPermissions);
        IFilePicker CreateFilePicker();
    }
}
