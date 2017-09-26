using System;

namespace Adapt.Presentation
{
    public class FilePickerEventArgs : EventArgs
    {
        #region Public Properties
        public string FileName { get; }
        public string FilePath { get; }
        #endregion

        #region Constructors
        public FilePickerEventArgs(string fileName)

        {
            FileName = fileName;
        }

        public FilePickerEventArgs(string fileName, string filePath) : this(fileName)
        {
            FilePath = filePath;
        }
        #endregion  
    }
}