using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Adapt.Presentation.UWP
{
    /// <summary>
    /// UWP File Picker
    /// </summary>
    public class FilePicker : IFilePicker
    {
        #region Public Methods (Implementation)
        public async Task<FileData> PickAndOpenFileForWriting(IDictionary<string, IList<string>> fileTypes, string fileName)
        {
            if (fileTypes == null)
            {
                throw new ArgumentNullException(nameof(fileTypes));
            }

            if (fileTypes.Count == 0)
            {
                throw new Exception("At least one file type must be specified");
            }

            var picker = new FileSavePicker();

            if (fileName != null)
            {
                picker.SuggestedFileName = fileName;
            }

            foreach (var key in fileTypes.Keys)
            {
                picker.FileTypeChoices.Add(key, fileTypes[key]);
            }

            var file = await picker.PickSaveFileAsync();

            if (file == null)
            {
                return null;
            }

            var fileStream = await file.OpenStreamForWriteAsync();

            var retVal = new FileData
            {
                FileName = file.Name,
                FileStream = fileStream
            };


            return retVal;
        }

        public async Task<FileData> PickAndOpenFileForReading()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();

            var retVal = new FileData();

            var randomAccessStream = await file.OpenReadAsync();

            retVal.FileStream = randomAccessStream.AsStreamForRead();

            return retVal;
        }

        #endregion

        #region Helpers

        #endregion
    }
}