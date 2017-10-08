namespace Adapt.Presentation
{
    public interface IFileSource
    {
        string GetFileString(string name, FileSourceType type);
    }
}
