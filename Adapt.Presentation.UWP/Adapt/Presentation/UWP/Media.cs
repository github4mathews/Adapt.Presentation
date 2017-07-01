using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Adapt.Presentation.UWP
{
    /// <summary>
    /// Implementation for Media
    /// </summary>
    public class Media : IMedia
    {
        #region Static Fields
        private static readonly IEnumerable<string> SupportedVideoFileTypes = new List<string> { ".mp4", ".wmv", ".avi" };
        private static readonly IEnumerable<string> SupportedImageFileTypes = new List<string> { ".jpeg", ".jpg", ".png", ".gif", ".bmp" };
        #endregion

        #region Private Fields
        private readonly Task _InitializeTask;
        private readonly HashSet<string> _Devices = new HashSet<string>();
        #endregion

        #region Public Properties

        /// <inheritdoc/>
        public bool IsTakePhotoSupported => true;

        /// <inheritdoc/>
        public bool IsPickPhotoSupported => true;

        /// <inheritdoc/>
        public bool IsTakeVideoSupported => true;

        /// <inheritdoc/>
        public bool IsPickVideoSupported => true;
        #endregion

        #region Constructor
        /// <summary>
        /// Implementation
        /// </summary>
        public Media()
        {
            _InitializeTask = InitializeAsync();

            var watcher = DeviceInformation.CreateWatcher(DeviceClass.VideoCapture);
            watcher.Added += OnDeviceAdded;
            watcher.Updated += OnDeviceUpdated;
            watcher.Removed += OnDeviceRemoved;
            watcher.Start();
        }
        #endregion

        #region Public Methods
        public async Task<bool> GetIsCameraAvailable()
        {
            await _InitializeTask;
            return _Devices.Count > 0;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var info = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                lock (_Devices)
                {
                    foreach (var device in info)
                    {
                        if (device.IsEnabled)
                            _Devices.Add(device.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to detect cameras: " + ex);
            }
        }

        /// <summary>
        /// Take a photo async with specified options
        /// </summary>
        public async Task<MediaFile> TakePhotoAsync(StoreCameraMediaOptions options)
        {
            await _InitializeTask;

            if (_Devices.Count == 0)
                throw new NotSupportedException();

            options.VerifyOptions();

            var capture = new CameraCaptureUI();
            capture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            capture.PhotoSettings.MaxResolution = GetMaxResolution(options?.PhotoSize ?? PhotoSize.Full, options?.CustomPhotoSize ?? 100);
            //we can only disable cropping if resolution is set to max
            if (capture.PhotoSettings.MaxResolution == CameraCaptureUIMaxPhotoResolution.HighestAvailable)
                capture.PhotoSettings.AllowCropping = options?.AllowCropping ?? true;


            var result = await capture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (result == null)
                return null;

            var folder = ApplicationData.Current.LocalFolder;

            var path = options.GetFilePath(folder.Path);
            var directoryFull = Path.GetDirectoryName(path);
            var newFolder = directoryFull.Replace(folder.Path, string.Empty);
            if (!string.IsNullOrWhiteSpace(newFolder))
                await folder.CreateFolderAsync(newFolder, CreationCollisionOption.OpenIfExists);

            folder = await StorageFolder.GetFolderFromPathAsync(directoryFull);

            var filename = Path.GetFileName(path);

            string aPath = null;
            if (options?.SaveToAlbum ?? false)
            {
                try
                {
                    var fileNameNoEx = Path.GetFileNameWithoutExtension(path);
                    var copy = await result.CopyAsync(KnownFolders.PicturesLibrary, fileNameNoEx + result.FileType, NameCollisionOption.GenerateUniqueName);
                    aPath = copy.Path;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("unable to save to album:" + ex);
                }
            }

            var file = await result.CopyAsync(folder, filename, NameCollisionOption.GenerateUniqueName).AsTask();
            return new MediaFile(file.Path, () => file.OpenStreamForReadAsync().Result, aPath);
        }

        public static CameraCaptureUIMaxPhotoResolution GetMaxResolution(PhotoSize photoSize, int customPhotoSize)
        {
            if (photoSize == PhotoSize.Custom)
            {
                if (customPhotoSize <= 25)
                    photoSize = PhotoSize.Small;
                else if (customPhotoSize <= 50)
                    photoSize = PhotoSize.Medium;
                else if (customPhotoSize <= 75)
                    photoSize = PhotoSize.Large;
                else
                    photoSize = PhotoSize.Large;
            }
            switch (photoSize)
            {
                case PhotoSize.Full:
                    return CameraCaptureUIMaxPhotoResolution.HighestAvailable;
                case PhotoSize.Large:
                    return CameraCaptureUIMaxPhotoResolution.Large3M;
                case PhotoSize.Medium:
                    return CameraCaptureUIMaxPhotoResolution.MediumXga;
                case PhotoSize.Small:
                    return CameraCaptureUIMaxPhotoResolution.SmallVga;

            }

            return CameraCaptureUIMaxPhotoResolution.HighestAvailable;
        }

        /// <summary>
        /// Picks a photo from the default gallery
        /// </summary>
        /// <returns>Media file or null if canceled</returns>
        public async Task<MediaFile> PickPhotoAsync(PickMediaOptions options = null)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };

            foreach (var filter in SupportedImageFileTypes)
                picker.FileTypeFilter.Add(filter);

            var result = await picker.PickSingleFileAsync();
            if (result == null)
                return null;

            var aPath = result.Path;
            var path = result.Path;
            StorageFile copy = null;
            //copy local
            try
            {
                var fileNameNoEx = Path.GetFileNameWithoutExtension(aPath);
                copy = await result.CopyAsync(ApplicationData.Current.LocalFolder,
                    fileNameNoEx + result.FileType, NameCollisionOption.GenerateUniqueName);

                path = copy.Path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("unable to save to app directory:" + ex);
            }

            return new MediaFile(path, () => copy != null ? copy.OpenStreamForReadAsync().Result : result.OpenStreamForReadAsync().Result, aPath);
        }

        /// <summary>
        /// Take a video with specified options
        /// </summary>
        public async Task<MediaFile> TakeVideoAsync(StoreVideoOptions options)
        {
            await _InitializeTask;

            if (_Devices.Count == 0)
                throw new NotSupportedException();

            options.VerifyOptions();

            var capture = new CameraCaptureUI();
            capture.VideoSettings.MaxResolution = GetResolutionFromQuality(options.Quality);
            capture.VideoSettings.AllowTrimming = options?.AllowCropping ?? true;

            if (capture.VideoSettings.AllowTrimming)
                capture.VideoSettings.MaxDurationInSeconds = (float)options.DesiredLength.TotalSeconds;

            capture.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

            var result = await capture.CaptureFileAsync(CameraCaptureUIMode.Video);
            if (result == null)
                return null;

            if (!(options?.SaveToAlbum ?? false))
            {
                return new MediaFile(result.Path, () => result.OpenStreamForReadAsync().Result);
            }

            string aPath = null;
            try
            {
                var fileNameNoEx = Path.GetFileNameWithoutExtension(result.Path);
                var copy = await result.CopyAsync(KnownFolders.VideosLibrary, fileNameNoEx + result.FileType, NameCollisionOption.GenerateUniqueName);
                aPath = copy.Path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("unable to save to album:" + ex);
            }

            return new MediaFile(result.Path, () => result.OpenStreamForReadAsync().Result, aPath);
        }

        /// <summary>
        /// Picks a video from the default gallery
        /// </summary>
        /// <returns>Media file of video or null if canceled</returns>
        public async Task<MediaFile> PickVideoAsync()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };

            foreach (var filter in SupportedVideoFileTypes)
                picker.FileTypeFilter.Add(filter);

            var result = await picker.PickSingleFileAsync();
            if (result == null)
                return null;

            var aPath = result.Path;
            var path = result.Path;
            StorageFile copy = null;
            //copy local
            try
            {
                var fileNameNoEx = Path.GetFileNameWithoutExtension(aPath);
                copy = await result.CopyAsync(ApplicationData.Current.LocalFolder,
                    fileNameNoEx + result.FileType, NameCollisionOption.GenerateUniqueName);

                path = copy.Path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("unable to save to app directory:" + ex);
            }

            return new MediaFile(path, () => copy != null ? copy.OpenStreamForReadAsync().Result : result.OpenStreamForReadAsync().Result, aPath);
        }
        #endregion

        #region Private Methods
        private static CameraCaptureUIMaxVideoResolution GetResolutionFromQuality(VideoQuality quality)
        {
            switch (quality)
            {
                case VideoQuality.High:
                    return CameraCaptureUIMaxVideoResolution.HighestAvailable;
                case VideoQuality.Medium:
                    return CameraCaptureUIMaxVideoResolution.StandardDefinition;
                case VideoQuality.Low:
                    return CameraCaptureUIMaxVideoResolution.LowDefinition;
                default:
                    return CameraCaptureUIMaxVideoResolution.HighestAvailable;
            }
        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate update)
        {
            object value;
            if (!update.Properties.TryGetValue("System.Devices.InterfaceEnabled", out value))
                return;

            lock (_Devices)
            {
                if ((bool)value)
                    _Devices.Add(update.Id);
                else
                    _Devices.Remove(update.Id);
            }
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
        {
            lock (_Devices)
            {
                _Devices.Remove(update.Id);
            }
        }

        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation device)
        {
            if (!device.IsEnabled)
                return;

            lock (_Devices)
            {
                _Devices.Add(device.Id);
            }
        }
        #endregion
    }
}