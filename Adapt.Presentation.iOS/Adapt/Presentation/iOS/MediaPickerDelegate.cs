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

using System;
using System.IO;
using System.Threading.Tasks;

using CoreGraphics;
using AssetsLibrary;
using Foundation;
using UIKit;
using NSAction = System.Action;
using System.Globalization;

namespace Adapt.Presentation.iOS
{
    internal class MediaPickerDelegate
        : UIImagePickerControllerDelegate
    {
        internal MediaPickerDelegate(UIViewController viewController, UIImagePickerControllerSourceType sourceType, StoreCameraMediaOptions options)
        {
            _ViewController = viewController;
            _Source = sourceType;
            _Options = options ?? new StoreCameraMediaOptions();

            if (viewController == null)
            {
                return;
            }

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            _Observer = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, DidRotate);
        }

        public UIPopoverController Popover
        {
            get;
            set;
        }

        public UIView View => _ViewController.View;

        public Task<MediaFile> Task => _Tcs.Task;

        public override async void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
        {
            RemoveOrientationChangeObserverAndNotifications();

            MediaFile mediaFile;
            switch ((NSString)info[UIImagePickerController.MediaType])
            {
                case Media.TypeImage:
                    mediaFile = await GetPictureMediaFile(info);
                    break;

                case Media.TypeMovie:
                    mediaFile = await GetMovieMediaFile(info);
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                UIApplication.SharedApplication.SetStatusBarStyle(Media.StatusBarStyle, false);
            }

            Dismiss(picker, () =>
            {


                _Tcs.TrySetResult(mediaFile);
            });
        }

        public override void Canceled(UIImagePickerController picker)
        {
            RemoveOrientationChangeObserverAndNotifications();

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                UIApplication.SharedApplication.SetStatusBarStyle(Media.StatusBarStyle, false);
            }

            Dismiss(picker, () =>
            {


                _Tcs.SetResult(null);
            });
        }

        public void DisplayPopover(bool hideFirst = false)
        {
            if (Popover == null)
                return;

            var swidth = UIScreen.MainScreen.Bounds.Width;
            var sheight = UIScreen.MainScreen.Bounds.Height;

            nfloat width = 400;
            nfloat height = 300;


            if (_Orientation == null)
            {
                _Orientation = IsValidInterfaceOrientation(UIDevice.CurrentDevice.Orientation) ? UIDevice.CurrentDevice.Orientation : GetDeviceOrientation(_ViewController.InterfaceOrientation);
            }

            nfloat x, y;
            if (_Orientation == UIDeviceOrientation.LandscapeLeft || _Orientation == UIDeviceOrientation.LandscapeRight)
            {
                y = swidth / 2 - height / 2;
                x = sheight / 2 - width / 2;
            }
            else
            {
                x = swidth / 2 - width / 2;
                y = sheight / 2 - height / 2;
            }

            if (hideFirst && Popover.PopoverVisible)
                Popover.Dismiss(false);

            Popover.PresentFromRect(new CGRect(x, y, width, height), View, 0, true);
        }

        private UIDeviceOrientation? _Orientation;
        private readonly NSObject _Observer;
        private readonly UIViewController _ViewController;
        private readonly UIImagePickerControllerSourceType _Source;
        private TaskCompletionSource<MediaFile> _Tcs = new TaskCompletionSource<MediaFile>();
        private readonly StoreCameraMediaOptions _Options;

        private bool IsCaptured => _Source == UIImagePickerControllerSourceType.Camera;

        private void Dismiss(UIImagePickerController picker, NSAction onDismiss)
        {
            if (_ViewController == null)
            {
                onDismiss();
                _Tcs = new TaskCompletionSource<MediaFile>();
            }
            else
            {
                if (Popover != null)
                {
                    Popover.Dismiss(true);
                    Popover.Dispose();
                    Popover = null;

                    onDismiss();
                }
                else
                {
                    picker.DismissViewController(true, onDismiss);
                    picker.Dispose();
                }
            }
        }

        private void RemoveOrientationChangeObserverAndNotifications()
        {
            if (_ViewController == null)
            {
                return;
            }

            UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.RemoveObserver(_Observer);
            _Observer.Dispose();
        }

        private void DidRotate(NSNotification notice)
        {
            var device = (UIDevice)notice.Object;
            if (!IsValidInterfaceOrientation(device.Orientation) || Popover == null)
                return;
            if (_Orientation.HasValue && IsSameOrientationKind(_Orientation.Value, device.Orientation))
                return;

            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
            {
                if (!GetShouldRotate6(device.Orientation))
                    return;
            }
            else if (!GetShouldRotate(device.Orientation))
                return;

            var co = _Orientation;
            _Orientation = device.Orientation;

            if (co == null)
                return;

            DisplayPopover(true);
        }

        private bool GetShouldRotate(UIDeviceOrientation orientation)
        {
            var iorientation = UIInterfaceOrientation.Portrait;
            switch (orientation)
            {
                case UIDeviceOrientation.LandscapeLeft:
                    iorientation = UIInterfaceOrientation.LandscapeLeft;
                    break;

                case UIDeviceOrientation.LandscapeRight:
                    iorientation = UIInterfaceOrientation.LandscapeRight;
                    break;

                case UIDeviceOrientation.Portrait:
                    iorientation = UIInterfaceOrientation.Portrait;
                    break;

                case UIDeviceOrientation.PortraitUpsideDown:
                    iorientation = UIInterfaceOrientation.PortraitUpsideDown;
                    break;

                default: return false;
            }

            return _ViewController.ShouldAutorotateToInterfaceOrientation(iorientation);
        }

        private bool GetShouldRotate6(UIDeviceOrientation orientation)
        {
            if (!_ViewController.ShouldAutorotate())
                return false;

            var mask = UIInterfaceOrientationMask.Portrait;
            switch (orientation)
            {
                case UIDeviceOrientation.LandscapeLeft:
                    mask = UIInterfaceOrientationMask.LandscapeLeft;
                    break;

                case UIDeviceOrientation.LandscapeRight:
                    mask = UIInterfaceOrientationMask.LandscapeRight;
                    break;

                case UIDeviceOrientation.Portrait:
                    mask = UIInterfaceOrientationMask.Portrait;
                    break;

                case UIDeviceOrientation.PortraitUpsideDown:
                    mask = UIInterfaceOrientationMask.PortraitUpsideDown;
                    break;

                default: return false;
            }

            return _ViewController.GetSupportedInterfaceOrientations().HasFlag(mask);
        }

        private async Task<MediaFile> GetPictureMediaFile(NSDictionary info)
        {
            var image = (UIImage)info[UIImagePickerController.EditedImage] ?? (UIImage)info[UIImagePickerController.OriginalImage];

            var meta = info[UIImagePickerController.MediaMetadata] as NSDictionary;


            var path = GetOutputPath(Media.TypeImage,
                _Options.Directory ?? (IsCaptured ? string.Empty : "temp"),
                _Options.Name);

            var cgImage = image.CGImage;

            if (_Options.PhotoSize != PhotoSize.Full)
            {
                try
                {
                    var percent = 1.0f;
                    switch (_Options.PhotoSize)
                    {
                        case PhotoSize.Large:
                            percent = .75f;
                            break;
                        case PhotoSize.Medium:
                            percent = .5f;
                            break;
                        case PhotoSize.Small:
                            percent = .25f;
                            break;
                        case PhotoSize.Custom:
                            percent = _Options.CustomPhotoSize / 100f;
                            break;
                    }

                    //calculate new size
                    var width = image.CGImage.Width * percent;
                    var height = image.CGImage.Height * percent;

                    //begin resizing image
                    image = image.ResizeImageWithAspectRatio(width, height);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to compress image: {ex}");
                }
            }

            //iOS quality is 0.0-1.0
            var quality = _Options.CompressionQuality / 100f;
            image.AsJPEG(quality).Save(path, true);

            string aPath = null;
            if (_Source != UIImagePickerControllerSourceType.Camera)
            {

                //try to get the album path's url
                var url = (NSUrl)info[UIImagePickerController.ReferenceUrl];
                aPath = url?.AbsoluteString;
            }
            else
            {
                if (!_Options.SaveToAlbum)
                {
                    return new MediaFile(path, () => File.OpenRead(path), aPath);
                }

                try
                {
                    var library = new ALAssetsLibrary();
                    var albumSave = await library.WriteImageToSavedPhotosAlbumAsync(cgImage, meta);
                    aPath = albumSave.AbsoluteString;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("unable to save to album:" + ex);
                }
            }

            return new MediaFile(path, () => File.OpenRead(path), aPath);
        }



        private async Task<MediaFile> GetMovieMediaFile(NSDictionary info)
        {
            var url = (NSUrl)info[UIImagePickerController.MediaURL];

            var path = GetOutputPath(Media.TypeMovie,
                      _Options.Directory ?? (IsCaptured ? string.Empty : "temp"),
                      _Options.Name ?? Path.GetFileName(url.Path));

            File.Move(url.Path, path);

            string aPath = null;
            if (_Source != UIImagePickerControllerSourceType.Camera)
            {
                //try to get the album path's url
                var url2 = (NSUrl)info[UIImagePickerController.ReferenceUrl];
                aPath = url2?.AbsoluteString;
            }
            else
            {
                if (!_Options.SaveToAlbum)
                {
                    return new MediaFile(path, () => File.OpenRead(path), aPath);
                }

                try
                {
                    var library = new ALAssetsLibrary();
                    var albumSave = await library.WriteVideoToSavedPhotosAlbumAsync(new NSUrl(path));
                    aPath = albumSave.AbsoluteString;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("unable to save to album:" + ex);
                }
            }

            return new MediaFile(path, () => File.OpenRead(path), aPath);
        }

        private static string GetUniquePath(string type, string path, string name)
        {
            var isPhoto = type == Media.TypeImage;
            var ext = Path.GetExtension(name);
            if (ext == string.Empty)
                ext = isPhoto ? ".jpg" : ".mp4";

            name = Path.GetFileNameWithoutExtension(name);

            var nname = name + ext;
            var i = 1;
            while (File.Exists(Path.Combine(path, nname)))
                nname = name + "_" + i++ + ext;

            return Path.Combine(path, nname);
        }

        private static string GetOutputPath(string type, string path, string name)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path);
            Directory.CreateDirectory(path);

            if (!string.IsNullOrWhiteSpace(name))
            {
                return Path.Combine(path, GetUniquePath(type, path, name));
            }

            var timestamp = DateTime.Now.ToString("yyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            if (type == Media.TypeImage)
                name = "IMG_" + timestamp + ".jpg";
            else
                name = "VID_" + timestamp + ".mp4";

            return Path.Combine(path, GetUniquePath(type, path, name));
        }

        private static bool IsValidInterfaceOrientation(UIDeviceOrientation self)
        {
            return self != UIDeviceOrientation.FaceUp && self != UIDeviceOrientation.FaceDown && self != UIDeviceOrientation.Unknown;
        }

        private static bool IsSameOrientationKind(UIDeviceOrientation o1, UIDeviceOrientation o2)
        {
            if (o1 == UIDeviceOrientation.FaceDown || o1 == UIDeviceOrientation.FaceUp)
                return o2 == UIDeviceOrientation.FaceDown || o2 == UIDeviceOrientation.FaceUp;
            if (o1 == UIDeviceOrientation.LandscapeLeft || o1 == UIDeviceOrientation.LandscapeRight)
                return o2 == UIDeviceOrientation.LandscapeLeft || o2 == UIDeviceOrientation.LandscapeRight;
            if (o1 == UIDeviceOrientation.Portrait || o1 == UIDeviceOrientation.PortraitUpsideDown)
                return o2 == UIDeviceOrientation.Portrait || o2 == UIDeviceOrientation.PortraitUpsideDown;

            return false;
        }

        private static UIDeviceOrientation GetDeviceOrientation(UIInterfaceOrientation self)
        {
            switch (self)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                    return UIDeviceOrientation.LandscapeLeft;
                case UIInterfaceOrientation.LandscapeRight:
                    return UIDeviceOrientation.LandscapeRight;
                case UIInterfaceOrientation.Portrait:
                    return UIDeviceOrientation.Portrait;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return UIDeviceOrientation.PortraitUpsideDown;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
