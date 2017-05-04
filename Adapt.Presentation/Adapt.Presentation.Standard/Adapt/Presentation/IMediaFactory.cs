namespace Adapt.Presentation
{
    public interface IPresentationFactory
    {
        IMedia CreateMedia();
        IFilePicker CreateFilePicker();
    }
}
