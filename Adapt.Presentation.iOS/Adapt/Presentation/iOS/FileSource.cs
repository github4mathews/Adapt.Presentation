using System.IO;

namespace Adapt.Presentation.iOS
{
    public class FileSource : IFileSource
    {
        public string GetFileString(string name, FileSourceType type)
        {
            //I don't actually know if this is right, it will probably be wrong though, iOS does like, catalogs and stuff and I don't understand how it works
            if (type == FileSourceType.Image) return Path.GetFileNameWithoutExtension(name);
            return name.ToLower();
        }
    }
}