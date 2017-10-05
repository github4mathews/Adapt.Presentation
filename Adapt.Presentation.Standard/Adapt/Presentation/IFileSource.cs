namespace Adapt.Presentation
{
    public interface IFileSource
    {
        string GetFileString(string name, IFileSourceType type);
    }


    /// <summary>
    /// Used in implementations to switch between folders or catalogs when getting a file string
    /// </summary>
    public interface IFileSourceType
    {
    }
}
