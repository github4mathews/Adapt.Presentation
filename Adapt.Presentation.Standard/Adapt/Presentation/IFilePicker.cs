using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adapt.Presentation
{
    /// <summary>
    /// Common interface for file picking
    /// </summary>
    public interface IFilePicker
    {
        Task<FileData> PickAndOpenFileForReading();
        Task<FileData> PickAndOpenFileForWriting(IDictionary<string, IList<string>> fileTypes, string fileName);
    }
}