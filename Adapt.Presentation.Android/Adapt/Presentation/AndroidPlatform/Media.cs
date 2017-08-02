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
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Media;
using Android.Graphics;
using System.Text.RegularExpressions;

namespace Adapt.Presentation.AndroidPlatform
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    [Android.Runtime.Preserve(AllMembers = true)]
    public class Media : MediaBase, IMedia
    {
        #region Fields
        private readonly bool _IsCameraAvailable;
        private readonly Context _Context;
        private int _RequestId;
        private TaskCompletionSource<MediaFile> _CompletionSource;
        private const string IllegalCharacters = "[|\\?*<\":>/']";
        #endregion

        #region Constructor
        /// <summary>
        /// Implementation
        /// </summary>
        public Media(IPermissions currentPermissions) : base(currentPermissions)
        {

            _Context = Android.App.Application.Context;
            _IsCameraAvailable = _Context.PackageManager.HasSystemFeature(PackageManager.FeatureCamera);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Gingerbread)
            {
                _IsCameraAvailable |= _Context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraFront);
            }
        }
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

        #region Public Methods

        public async Task<bool> GetIsCameraAvailable()
        {
            return await Task.FromResult(_IsCameraAvailable);
        }

        ///<inheritdoc/>
        public Task InitializeAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Picks a photo from the default gallery
        /// </summary>
        /// <returns>Media file or null if canceled</returns>
        public async Task<MediaFile> PickPhotoAsync(PickMediaOptions options = null)
        {
            if (!await RequestStoragePermission())
            {
                return null;
            }
            var media = await TakeMediaAsync("image/*", Intent.ActionPick, null);

            if (options == null)
            {
                options = new PickMediaOptions();
            }

            //check to see if we picked a file, and if so then try to fix orientation and resize
            if (string.IsNullOrWhiteSpace(media?.Path))
            {
                return media;
            }

            try
            {
                await FixOrientationAndResizeAsync(media.Path, options.PhotoSize, options.CompressionQuality, options.CustomPhotoSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to check orientation: " + ex);
            }

            return media;
        }


        /// <summary>
        /// Take a photo async with specified options
        /// </summary>
        /// <param name="options">Camera Media Options</param>
        /// <returns>Media file of photo or null if canceled</returns>
        public async Task<MediaFile> TakePhotoAsync(StoreCameraMediaOptions options)
        {
            if (!_IsCameraAvailable)
            {
                throw new NotSupportedException();
            }

            if (!await RequestStoragePermission())
            {
                return null;
            }


            VerifyOptions(options);

            var media = await TakeMediaAsync("image/*", MediaStore.ActionImageCapture, options);

            if (string.IsNullOrWhiteSpace(media?.Path))
            {
                return media;
            }

            if (options.SaveToAlbum)
            {
                try
                {
                    var fileName = System.IO.Path.GetFileName(media.Path);
                    var publicUri = MediaPickerActivity.GetOutputMediaFile(_Context, options.Directory ?? "temp", fileName, true, true);
                    using (System.IO.Stream input = File.OpenRead(media.Path))
                    using (System.IO.Stream output = File.Create(publicUri.Path))
                    {
                        input.CopyTo(output);
                    }

                    media.AlbumPath = publicUri.Path;

                    var f = new Java.IO.File(publicUri.Path);

                    //MediaStore.Images.Media.InsertImage(context.ContentResolver,
                    //    f.AbsolutePath, f.Name, null);

                    try
                    {
                        MediaScannerConnection.ScanFile(_Context, new[] { f.AbsolutePath }, null, _Context as MediaPickerActivity);

                        var values = new ContentValues();
                        values.Put(MediaStore.Images.Media.InterfaceConsts.Title, System.IO.Path.GetFileNameWithoutExtension(f.AbsolutePath));
                        values.Put(MediaStore.Images.Media.InterfaceConsts.Description, string.Empty);
                        values.Put(MediaStore.Images.Media.InterfaceConsts.DateTaken, Java.Lang.JavaSystem.CurrentTimeMillis());
                        values.Put(MediaStore.Images.ImageColumns.BucketId, f.ToString().ToLowerInvariant().GetHashCode());
                        values.Put(MediaStore.Images.ImageColumns.BucketDisplayName, f.Name.ToLowerInvariant());
                        values.Put("_data", f.AbsolutePath);

                        var cr = _Context.ContentResolver;
                        cr.Insert(MediaStore.Images.Media.ExternalContentUri, values);
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine("Unable to save to scan file: " + ex1);
                    }

                    var contentUri = Android.Net.Uri.FromFile(f);
                    var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile, contentUri);
                    _Context.SendBroadcast(mediaScanIntent);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Unable to save to gallery: " + ex2);
                }
            }

            //check to see if we need to rotate if success

            try
            {
                await FixOrientationAndResizeAsync(media.Path, options.PhotoSize, options.CompressionQuality, options.CustomPhotoSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to check orientation: " + ex);
            }


            return media;
        }

        /// <summary>
        /// Picks a video from the default gallery
        /// </summary>
        /// <returns>Media file of video or null if canceled</returns>
        public async Task<MediaFile> PickVideoAsync()
        {

            if (!await RequestStoragePermission())
            {
                return null;
            }

            return await TakeMediaAsync("video/*", Intent.ActionPick, null);
        }

        /// <summary>
        /// Take a video with specified options
        /// </summary>
        /// <param name="options">Video Media Options</param>
        /// <returns>Media file of new video or null if canceled</returns>
        public async Task<MediaFile> TakeVideoAsync(StoreVideoOptions options)
        {
            if (!_IsCameraAvailable)
            {
                throw new NotSupportedException();
            }

            if (!await RequestStoragePermission())
            {
                return null;
            }

            VerifyOptions(options);

            return await TakeMediaAsync("video/*", MediaStore.ActionVideoCapture, options);
        }

        /// <summary>
        /// Resize Image Async
        /// </summary>
        public Task<bool> ResizeAsync(string filePath, PhotoSize photoSize, int quality, int customPhotoSize)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult(false);
            }

            try
            {
                return Task.Run(() =>
                {
                    try
                    {


                        if (photoSize == PhotoSize.Full)
                        {
                            return false;
                        }

                        var percent = 1.0f;
                        switch (photoSize)
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
                                percent = customPhotoSize / 100f;
                                break;
                        }


                        //First decode to just get dimensions
                        var options = new BitmapFactory.Options
                        {
                            InJustDecodeBounds = true
                        };

                        //already on background task
                        BitmapFactory.DecodeFile(filePath, options);

                        var finalWidth = (int)(options.OutWidth * percent);
                        var finalHeight = (int)(options.OutHeight * percent);

                        //calculate sample size
                        options.InSampleSize = CalculateInSampleSize(options, finalWidth, finalHeight);

                        //turn off decode
                        options.InJustDecodeBounds = false;


                        //this now will return the requested width/height from file, so no longer need to scale
                        using (var originalImage = BitmapFactory.DecodeFile(filePath, options))
                        {

                            //always need to compress to save back to disk
                            using (var stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
                            {

                                originalImage.Compress(Bitmap.CompressFormat.Jpeg, quality, stream);
                                stream.Close();
                            }

                            originalImage.Recycle();

                            // Dispose of the Java side bitmap.
                            GC.Collect();
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        throw ex;
#else
                        return false;
#endif
                    }
                });
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#else
                return Task.FromResult(false);
#endif
            }
        }


        #endregion

        #region Private Methods
        private async Task<bool> RequestStoragePermission()
        {
            //We always have permission on anything lower than marshmallow.
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return true;
            }

            var status = await CurrentPermissions.CheckPermissionStatusAsync(Permission.Storage);
            if (status == PermissionStatus.Granted)
            {
                return true;
            }

            Console.WriteLine("Does not have storage permission granted, requesting.");
            var results = await CurrentPermissions.RequestPermissionsAsync(Permission.Storage);
            if (!results.ContainsKey(Permission.Storage) || results[Permission.Storage] == PermissionStatus.Granted)
            {
                return true;
            }

            Console.WriteLine("Storage permission Denied.");
            return false;
        }

        private void VerifyOptions(StoreMediaOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (System.IO.Path.IsPathRooted(options.Directory))
            {
                throw new ArgumentException("options.Directory must be a relative path", nameof(options));
            }

            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                options.Name = Regex.Replace(options.Name, IllegalCharacters, string.Empty).Replace(@"\", string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(options.Directory))
            {
                options.Directory = Regex.Replace(options.Directory, IllegalCharacters, string.Empty).Replace(@"\", string.Empty);
            }
        }

        private Intent CreateMediaIntent(int id, string type, string action, StoreMediaOptions options, bool tasked = true)
        {
            var pickerIntent = new Intent(_Context, typeof(MediaPickerActivity));
            pickerIntent.PutExtra(MediaPickerActivity.ExtraId, id);
            pickerIntent.PutExtra(MediaPickerActivity.ExtraType, type);
            pickerIntent.PutExtra(MediaPickerActivity.ExtraAction, action);
            pickerIntent.PutExtra(MediaPickerActivity.ExtraTasked, tasked);

            if (options != null)
            {
                pickerIntent.PutExtra(MediaPickerActivity.ExtraPath, options.Directory);
                pickerIntent.PutExtra(MediaStore.Images.ImageColumns.Title, options.Name);

                var cameraOptions = options as StoreCameraMediaOptions;
                if (cameraOptions != null)
                {
                    if (cameraOptions.DefaultCamera == CameraDevice.Front)
                    {
                        pickerIntent.PutExtra("android.intent.extras.CAMERA_FACING", 1);
                    }
                    pickerIntent.PutExtra(MediaPickerActivity.ExtraSaveToAlbum, cameraOptions.SaveToAlbum);
                }
                var vidOptions = options as StoreVideoOptions;
                if (vidOptions != null)
                {
                    if (vidOptions.DefaultCamera == CameraDevice.Front)
                    {
                        pickerIntent.PutExtra("android.intent.extras.CAMERA_FACING", 1);
                    }
                    pickerIntent.PutExtra(MediaStore.ExtraDurationLimit, (int)vidOptions.DesiredLength.TotalSeconds);
                    pickerIntent.PutExtra(MediaStore.ExtraVideoQuality, (int)vidOptions.Quality);
                }
            }
            //pickerIntent.SetFlags(ActivityFlags.ClearTop);
            pickerIntent.SetFlags(ActivityFlags.NewTask);
            return pickerIntent;
        }

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

        private Task<MediaFile> TakeMediaAsync(string type, string action, StoreMediaOptions options)
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<MediaFile>(id);
            if (Interlocked.CompareExchange(ref _CompletionSource, ntcs, null) != null)
            {
                throw new InvalidOperationException("Only one operation can be active at a time");
            }

            _Context.StartActivity(CreateMediaIntent(id, type, action, options));

            void Handler(object s, MediaPickedEventArgs e)
            {
                var tcs = Interlocked.Exchange(ref _CompletionSource, null);

                MediaPickerActivity.MediaPicked -= Handler;

                if (e.RequestId != id)
                {
                    return;
                }

                if (e.IsCanceled)
                {
                    tcs.SetResult(null);
                }
                else if (e.Error != null)
                {
                    tcs.SetException(e.Error);
                }
                else
                {
                    tcs.SetResult(e.Media);
                }
            }

            MediaPickerActivity.MediaPicked += Handler;

            return _CompletionSource.Task;
        }

        /// <summary>
        ///  Rotate an image if required and saves it back to disk.
        /// </summary>
        private Task<bool> FixOrientationAndResizeAsync(string filePath, PhotoSize photoSize, int quality, int customPhotoSize)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult(false);
            }

            try
            {
                return Task.Run(() =>
                {
                    try
                    {
                        //First decode to just get dimensions
                        var options = new BitmapFactory.Options
                        {
                            InJustDecodeBounds = true
                        };

                        //already on background task
                        BitmapFactory.DecodeFile(filePath, options);

                        var rotation = GetRotation(filePath);

                        // if we don't need to rotate, aren't resizing, and aren't adjusting quality then simply return
                        if (rotation == 0 && photoSize == PhotoSize.Full && quality == 100)
                        {
                            return false;
                        }

                        var percent = 1.0f;
                        switch (photoSize)
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
                                percent = customPhotoSize / 100f;
                                break;
                        }


                        var finalWidth = (int)(options.OutWidth * percent);
                        var finalHeight = (int)(options.OutHeight * percent);

                        //calculate sample size
                        options.InSampleSize = CalculateInSampleSize(options, finalWidth, finalHeight);

                        //turn off decode
                        options.InJustDecodeBounds = false;


                        //this now will return the requested width/height from file, so no longer need to scale
                        var originalImage = BitmapFactory.DecodeFile(filePath, options);

                        if (finalWidth != originalImage.Width || finalHeight != originalImage.Height)
                        {
                            originalImage = Bitmap.CreateScaledBitmap(originalImage, finalWidth, finalHeight, true);
                        }
                        //if we need to rotate then go for it.
                        //then compresse it if needed
                        if (rotation != 0)
                        {
                            var matrix = new Matrix();
                            matrix.PostRotate(rotation);
                            using (var rotatedImage = Bitmap.CreateBitmap(originalImage, 0, 0, originalImage.Width, originalImage.Height, matrix, true))
                            {

                                //always need to compress to save back to disk
                                using (var stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    rotatedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, stream);



                                    stream.Close();
                                }
                                rotatedImage.Recycle();
                            }
                            originalImage.Recycle();
                            originalImage.Dispose();
                            // Dispose of the Java side bitmap.
                            GC.Collect();

                            //Save out new exif data
                            SetExifData(filePath, Orientation.Normal);
                            return true;
                        }



                        //always need to compress to save back to disk
                        using (var stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            originalImage.Compress(Bitmap.CompressFormat.Jpeg, quality, stream);
                            stream.Close();
                        }



                        originalImage.Recycle();
                        originalImage.Dispose();
                        // Dispose of the Java side bitmap.
                        GC.Collect();
                        return true;

                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        throw ex;
#else
                        return false;
#endif
                    }
                });
            }
            catch (Exception ex)
            {
#if DEBUG
                throw;
#else
                return Task.FromResult(false);
#endif
            }

        }

        private int CalculateInSampleSize(
            BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            var height = options.OutHeight;
            var width = options.OutWidth;
            var inSampleSize = 1;

            if (height <= reqHeight && width <= reqWidth)
            {
                return inSampleSize;
            }

            var halfHeight = height / 2;
            var halfWidth = width / 2;

            // Calculate the largest inSampleSize value that is a power of 2 and keeps both
            // height and width larger than the requested height and width.
            while (halfHeight / inSampleSize >= reqHeight
                   && halfWidth / inSampleSize >= reqWidth)
            {
                inSampleSize *= 2;
            }

            return inSampleSize;
        }

        private void SetExifData(string filePath, Orientation orientation)
        {
            try
            {
                using (var ei = new ExifInterface(filePath))
                {

                    ei.SetAttribute(ExifInterface.TagOrientation, Java.Lang.Integer.ToString((int)orientation));
                    ei.SaveAttributes();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#endif
            }
        }

        private static int GetRotation(string filePath)
        {
            try
            {
                using (var ei = new ExifInterface(filePath))
                {
                    var orientation = (Orientation)ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);

                    switch (orientation)
                    {
                        case Orientation.Rotate90:
                            return 90;
                        case Orientation.Rotate180:
                            return 180;
                        case Orientation.Rotate270:
                            return 270;
                        default:
                            return 0;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#else
                return 0;
#endif
            }
        }

        #endregion
    }
}
