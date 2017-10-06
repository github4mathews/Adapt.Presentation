using Adapt.Presentation;

namespace Adapt.Presentation.AndroidPlatform
{
    public class FileSource : IFileSource
    {
        public string GetFileString(string name, IFileSourceType type)
        {
            //TODO: Implement type
            return name.ToLower();
        }
    }
}