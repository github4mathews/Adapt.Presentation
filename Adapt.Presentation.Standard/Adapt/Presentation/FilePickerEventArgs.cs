using System;

namespace Adapt.Presentation
{
    public class FilePickerEventArgs : EventArgs
    {
        public string FileName { get; }

        public string FilePath { get; }

        private FilePickerEventArgs (byte [] fileByte)
        {
        }

        public FilePickerEventArgs (byte [] fileByte, string fileName)
            : this (fileByte)
        {
            FileName = fileName;
        }

        public FilePickerEventArgs (byte [] fileByte, string fileName, string filePath)
            : this (fileByte, fileName)
        {
            FilePath = filePath;
        }
    }
}