using System.Threading.Tasks;

namespace Adapt.Presentation
{
    /// <summary>
    /// Common interface for file picking
    /// </summary>
    public interface IFilePicker
    {
        Task<FileData> PickAndOpenFileForReading();
        Task<FileData> PickAndOpenFileForWriting(IFileSelectionDictionary fileTypes, string fileName);
    }
}