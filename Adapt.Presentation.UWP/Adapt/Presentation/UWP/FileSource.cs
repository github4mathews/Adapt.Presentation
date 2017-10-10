using System.IO;

namespace Adapt.Presentation.UWP.Adapt.Presentation.UWP
{
    public class FileSource : IFileSource
    {
        public string GetFileString(string name, FileSourceType type)
        {
            var value = Path.Combine("Assets", name);
            if (type == FileSourceType.PackageImage) value = Path.Combine(@"ms-appx:///", value);
            return value;
        }
    }
}