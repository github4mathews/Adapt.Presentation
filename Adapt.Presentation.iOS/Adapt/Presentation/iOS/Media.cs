//
//  Copyright 2011-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using Foundation;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Adapt.Presentation.iOS
{
    /// <summary>
    /// Implementation for Media
    /// </summary>
    public class Media : IMedia
    {
        #region Fields
        private UIPopoverController _Popover;
        private UIImagePickerControllerDelegate _PickerDelegate;
        private readonly bool _IsCameraAvailable;
        #endregion

        #region Public Consts
        /// <summary>
        /// image type
        /// </summary>
        public const string TypeImage = "public.image";
        /// <summary>
        /// movie type
        /// </summary>
        public const string TypeMovie = "public.movie";
        #endregion

        #region Public Properties
        /// <inheritdoc/>
        public bool IsTakePhotoSupported { get; }

        /// <inheritdoc/>
        public bool IsPickPhotoSupported { get; }

        /// <inheritdoc/>
        public bool IsTakeVideoSupported { get; }

        /// <inheritdoc/>
        public bool IsPickVideoSupported { get; }
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Color of the status bar
        /// </summary>
        public static UIStatusBarStyle StatusBarStyle { get; set; }
        #endregion

        #region Public Methods
        ///<inheritdoc/>
        public Task InitializeAsync() => Task.FromResult(true);
        #endregion

        #region Constructor
        /// <summary>
        /// Implementation
        /// </summary>
        public Media()
        {
            StatusBarStyle = UIApplication.SharedApplication.StatusBarStyle;
            _IsCameraAvailable = UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

            var availableCameraMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera) ?? new string[0];
            var avaialbleLibraryMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) ?? new string[0];

            foreach (var type in availableCameraMedia.Concat(avaialbleLibraryMedia))
            {
                switch (type)
                {
                    case TypeMovie:
                        IsTakeVideoSupported = IsPickVideoSupported = true;
                        break;
                    case TypeImage:
                        IsTakePhotoSupported = IsPickPhotoSupported = true;
                        break;
                }
            }
        }
        #endregion

        #region Public Methods

        public async Task<bool> GetIsCameraAvailable()
        {
            return await Task.FromResult(_IsCameraAvailable);
        }

        /// <summary>
        /// Picks a photo from the default gallery
        /// </summary>
        /// <returns>Media file or null if canceled</returns>
        public Task<MediaFile> PickPhotoAsync(PickMediaOptions options = null)
        {
            if (!IsPickPhotoSupported)
                throw new NotSupportedException();

            CheckPhotoUsageDescription();

            var cameraOptions = new StoreCameraMediaOptions
            {
                PhotoSize = options?.PhotoSize ?? PhotoSize.Full,
                CompressionQuality = options?.CompressionQuality ?? 100
            };

            return GetMediaAsync(UIImagePickerControllerSourceType.PhotoLibrary, TypeImage, cameraOptions);
        }


        /// <summary>
        /// Take a photo async with specified options
        /// </summary>
        /// <param name="options">Camera Media Options</param>
        /// <returns>Media file of photo or null if canceled</returns>
        public Task<MediaFile> TakePhotoAsync(StoreCameraMediaOptions options)
        {
            if (!IsTakePhotoSupported)
                throw new NotSupportedException();
            if (!_IsCameraAvailable)
                throw new NotSupportedException();

            CheckCameraUsageDescription();

            VerifyCameraOptions(options);

            return GetMediaAsync(UIImagePickerControllerSourceType.Camera, TypeImage, options);
        }


        /// <summary>
        /// Picks a video from the default gallery
        /// </summary>
        /// <returns>Media file of video or null if canceled</returns>
        public Task<MediaFile> PickVideoAsync()
        {
            if (!IsPickVideoSupported)
                throw new NotSupportedException();


            CheckPhotoUsageDescription();

            return GetMediaAsync(UIImagePickerControllerSourceType.PhotoLibrary, TypeMovie);
        }


        /// <summary>
        /// Take a video with specified options
        /// </summary>
        /// <param name="options">Video Media Options</param>
        /// <returns>Media file of new video or null if canceled</returns>
        public Task<MediaFile> TakeVideoAsync(StoreVideoOptions options)
        {
            if (!IsTakeVideoSupported)
                throw new NotSupportedException();
            if (!_IsCameraAvailable)
                throw new NotSupportedException();

            CheckCameraUsageDescription();

            VerifyCameraOptions(options);

            return GetMediaAsync(UIImagePickerControllerSourceType.Camera, TypeMovie, options);
        }
        #endregion

        #region Private Methods
        private void VerifyOptions(StoreMediaOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.Directory != null && Path.IsPathRooted(options.Directory))
                throw new ArgumentException("options.Directory must be a relative path", nameof(options));
        }

        private void VerifyCameraOptions(StoreCameraMediaOptions options)
        {
            VerifyOptions(options);
            if (!Enum.IsDefined(typeof(CameraDevice), options.DefaultCamera))
                throw new ArgumentException("options.Camera is not a member of CameraDevice");
        }

        private Task<MediaFile> GetMediaAsync(UIImagePickerControllerSourceType sourceType, string mediaType, StoreCameraMediaOptions options = null)
        {
            UIViewController viewController = null;
            var window = UIApplication.SharedApplication.KeyWindow;
            if (window == null)
                throw new InvalidOperationException("There's no current active window");

            if (window.WindowLevel == UIWindowLevel.Normal)
                viewController = window.RootViewController;

            if (viewController == null)
            {
                window = UIApplication.SharedApplication.Windows.OrderByDescending(w => w.WindowLevel).FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);
                if (window == null)
                    throw new InvalidOperationException("Could not find current view controller");
                viewController = window.RootViewController;
            }

            while (viewController.PresentedViewController != null)
                viewController = viewController.PresentedViewController;

            var ndelegate = new MediaPickerDelegate(viewController, sourceType, options);
            var od = Interlocked.CompareExchange(ref _PickerDelegate, ndelegate, null);
            if (od != null)
                throw new InvalidOperationException("Only one operation can be active at at time");

            var picker = SetupController(ndelegate, sourceType, mediaType, options);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && sourceType == UIImagePickerControllerSourceType.PhotoLibrary)
            {
                ndelegate.Popover = new UIPopoverController(picker)
                {
                    Delegate = new MediaPickerPopoverDelegate(ndelegate, picker)
                };

                ndelegate.DisplayPopover();
            }
            else
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    picker.ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
                }
                viewController.PresentViewController(picker, true, null);
            }

            return ndelegate.Task.ContinueWith(t =>
            {
                if (_Popover != null)
                {
                    _Popover.Dispose();
                    _Popover = null;
                }

                Interlocked.Exchange(ref _PickerDelegate, null);
                return t;
            }).Unwrap();
        }

        private void CheckCameraUsageDescription()
        {
            var info = NSBundle.MainBundle.InfoDictionary;

            if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                return;
            }

            if (!info.ContainsKey(new NSString("NSCameraUsageDescription")))
                throw new UnauthorizedAccessException("On iOS 10 and higher you must set NSCameraUsageDescription in your Info.plist file to enable Authorization Requests for Camera access!");
        }

        private void CheckPhotoUsageDescription()
        {
            var info = NSBundle.MainBundle.InfoDictionary;

            if (!UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                return;
            }

            if (!info.ContainsKey(new NSString("NSPhotoLibraryUsageDescription")))
                throw new UnauthorizedAccessException("On iOS 10 and higher you must set NSPhotoLibraryUsageDescription in your Info.plist file to enable Authorization Requests for Photo Library access!");
        }

        #endregion

        #region Private Static Methods
        private static MediaPickerController SetupController(MediaPickerDelegate mpDelegate, UIImagePickerControllerSourceType sourceType, string mediaType, StoreCameraMediaOptions options = null)
        {
            var picker = new MediaPickerController(mpDelegate)
            {
                MediaTypes = new[] { mediaType },
                SourceType = sourceType
            };

            if (sourceType != UIImagePickerControllerSourceType.Camera)
            {
                return picker;
            }

            picker.CameraDevice = GetUiCameraDevice(options.DefaultCamera);
            picker.AllowsEditing = options?.AllowCropping ?? false;

            var overlay = options.OverlayViewProvider?.Invoke();
            if (overlay is UIView)
            {
                picker.CameraOverlayView = overlay as UIView;
            }
            switch (mediaType)
            {
                case TypeImage:
                    picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;
                    break;
                case TypeMovie:
                    var voptions = (StoreVideoOptions)options;

                    picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Video;
                    picker.VideoQuality = GetQuailty(voptions.Quality);
                    picker.VideoMaximumDuration = voptions.DesiredLength.TotalSeconds;
                    break;
            }

            return picker;
        }

        private static UIImagePickerControllerCameraDevice GetUiCameraDevice(CameraDevice device)
        {
            switch (device)
            {
                case CameraDevice.Front:
                    return UIImagePickerControllerCameraDevice.Front;
                case CameraDevice.Rear:
                    return UIImagePickerControllerCameraDevice.Rear;
                default:
                    throw new NotSupportedException();
            }
        }

        private static UIImagePickerControllerQualityType GetQuailty(VideoQuality quality)
        {
            switch (quality)
            {
                case VideoQuality.Low:
                    return UIImagePickerControllerQualityType.Low;
                case VideoQuality.Medium:
                    return UIImagePickerControllerQualityType.Medium;
                default:
                    return UIImagePickerControllerQualityType.High;
            }
        }

        #endregion
    }
}
