namespace Adapt.Presentation.iOS
{
    public class FileSource : IFileSource
    {
        public string GetFileString(string name, IFileSourceType type)
        {
            //TODO: Implement type
            //I don't actually know if this is right, it will probably be wrong though, iOS does like, catalogs and stuff
            return name.ToLower();
        }
    }
}