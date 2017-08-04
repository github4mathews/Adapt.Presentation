using Android.Content;
using Android.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Adapt.Presentation.AndroidPlatform
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    ///
    [Preserve(AllMembers = true)]
    public class FilePicker : IFilePicker
    {
        #region Fields
        private int _RequestId;
        private TaskCompletionSource<FileData> _CompletionSource;
        private readonly Context _Context;
        private Permissions _Permission;
        #endregion

        #region Constructor
        public FilePicker(Context context, Permissions permission)
        {
            _Context = context;
            _Permission = permission;
        }
        #endregion

        #region Private Methods
        private int GetRequestId()
        {
            var id = _RequestId;

            if (_RequestId == int.MaxValue)
            {
                _RequestId = 0;
            }
            else
            {
                _RequestId++;
            }

            return id;
        }

        private Task<FileData> PickAndOpenFile(bool isSave)
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<FileData>(id);

            if (Interlocked.CompareExchange(ref _CompletionSource, ntcs, null) != null)
            {
                throw new InvalidOperationException("Only one operation can be active at a time");
            }

            try
            {
                var pickerIntent = new Intent(_Context, typeof(FilePickerActivity));
                pickerIntent.SetFlags(ActivityFlags.NewTask);

                _Context.StartActivity(pickerIntent);

                EventHandler<FilePickerEventArgs> handler = null;
                EventHandler<EventArgs> cancelledHandler = null;

                handler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref _CompletionSource, null);

                    FilePickerActivity.FilePicked -= handler;

                    var fileStream = isSave ? File.OpenWrite(e.FilePath) : File.OpenRead(e.FilePath);

                    tcs?.SetResult(new FileData { FileName = e.FileName, FileStream = fileStream, IsPermissionGranted = true });
                };

                cancelledHandler = (s, e) =>
                {
                    var tcs = Interlocked.Exchange(ref _CompletionSource, null);

                    FilePickerActivity.FilePickCancelled -= cancelledHandler;

                    tcs?.SetResult(null);
                };

                FilePickerActivity.FilePickCancelled += cancelledHandler;
                FilePickerActivity.FilePicked += handler;
            }
            catch (Exception exAct)
            {
                Debug.Write(exAct);
            }

            return _CompletionSource.Task;
        }

        #endregion

        #region Public Methods (Implementation)
        public async Task<FileData> PickAndOpenFileForReading()
        {
            var permissionStatus = await _Permission.CheckPermissionStatusAsync(Permission.Storage);

            if (permissionStatus != PermissionStatus.Granted)
            {
                var permissionStatusDictionary = await _Permission.RequestPermissionsAsync(Permission.Storage);
                if (permissionStatusDictionary.ContainsKey(Permission.Storage) && permissionStatusDictionary[Permission.Storage] != PermissionStatus.Granted)
                {
                    return new FileData { IsPermissionGranted = false };
                }
            }

            return await PickAndOpenFile(false);
        }

        public async Task<FileData> PickAndOpenFileForWriting(FileSelectionDictionary fileTypes, string fileName)
        {
            var directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

            var filePath = Path.Combine(directory, fileName);

            var retVal = new FileData { FileName = fileName };

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            retVal.FileStream = File.Create(filePath);

            return retVal;
        }
        #endregion
    }
}