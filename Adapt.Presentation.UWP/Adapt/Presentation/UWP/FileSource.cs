using System.IO;

namespace Adapt.Presentation.UWP.Adapt.Presentation.UWP
{
    public class FileSource : IFileSource
    {
        public string GetFileString(string name, FileSourceType type)
        {
            //TODO: Implement type
            return Path.Combine("Assets", name);
        }
    }
}