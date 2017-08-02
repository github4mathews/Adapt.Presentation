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
using Android.App;
using app = Android.App;
using diag = System.Diagnostics;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Environment = Android.OS.Environment;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;
//using Android.Support.V4.Content;
using Android.Content.PM;
using System.Globalization;

namespace Adapt.Presentation.AndroidPlatform
{
    /// <summary>
    /// Picker
    /// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MediaPickerActivity
        : Activity, Android.Media.MediaScannerConnection.IOnScanCompletedListener
    {
        internal const string ExtraPath = "path";
        internal const string ExtraLocation = "location";
        internal const string ExtraType = "type";
        internal const string ExtraId = "id";
        internal const string ExtraAction = "action";
        internal const string ExtraTasked = "tasked";
        internal const string ExtraSaveToAlbum = "album_save";
        internal const string ExtraFront = "android.intent.extras.CAMERA_FACING";

        internal static event EventHandler<MediaPickedEventArgs> MediaPicked;

        private int _Id;
        private int _Front;
        private string _Title;
        private string _Description;
        private string _Type;

        /// <summary>
        /// The user's destination path.
        /// </summary>
        private Uri _Path;
        private bool _IsPhoto;
        private bool _SaveToAlbum;
        private string _Action;

        private int _Seconds;
        private VideoQuality _Quality;

        private bool _Tasked;
        /// <summary>
        /// OnSaved
        /// </summary>
        /// <param name="outState"></param>
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean("ran", true);
            outState.PutString(MediaStore.MediaColumns.Title, _Title);
            outState.PutString(MediaStore.Images.ImageColumns.Description, _Description);
            outState.PutInt(ExtraId, _Id);
            outState.PutString(ExtraType, _Type);
            outState.PutString(ExtraAction, _Action);
            outState.PutInt(MediaStore.ExtraDurationLimit, _Seconds);
            outState.PutInt(MediaStore.ExtraVideoQuality, (int)_Quality);
            outState.PutBoolean(ExtraSaveToAlbum, _SaveToAlbum);
            outState.PutBoolean(ExtraTasked, _Tasked);
            outState.PutInt(ExtraFront, _Front);

            if (_Path != null)
            {
                outState.PutString(ExtraPath, _Path.Path);
            }

            base.OnSaveInstanceState(outState);
        }



        /// <summary>
        /// OnCreate
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var b = savedInstanceState ?? Intent.Extras;

            var ran = b.GetBoolean("ran", false);

            _Title = b.GetString(MediaStore.MediaColumns.Title);
            _Description = b.GetString(MediaStore.Images.ImageColumns.Description);

            _Tasked = b.GetBoolean(ExtraTasked);
            _Id = b.GetInt(ExtraId, 0);
            _Type = b.GetString(ExtraType);
            _Front = b.GetInt(ExtraFront);
            if (_Type == "image/*")
            {
                _IsPhoto = true;
            }

            _Action = b.GetString(ExtraAction);
            Intent pickIntent = null;
            try
            {
                pickIntent = new Intent(_Action);
                if (_Action == Intent.ActionPick)
                {
                    pickIntent.SetType(_Type);
                }
                else
                {
                    if (!_IsPhoto)
                    {
                        _Seconds = b.GetInt(MediaStore.ExtraDurationLimit, 0);
                        if (_Seconds != 0)
                        {
                            pickIntent.PutExtra(MediaStore.ExtraDurationLimit, _Seconds);
                        }
                    }

                    _SaveToAlbum = b.GetBoolean(ExtraSaveToAlbum);
                    pickIntent.PutExtra(ExtraSaveToAlbum, _SaveToAlbum);

                    _Quality = (VideoQuality)b.GetInt(MediaStore.ExtraVideoQuality, (int)VideoQuality.High);
                    pickIntent.PutExtra(MediaStore.ExtraVideoQuality, GetVideoQuality(_Quality));

                    if (_Front != 0)
                    {
                        pickIntent.PutExtra(ExtraFront, (int)Android.Hardware.CameraFacing.Front);
                    }

                    if (!ran)
                    {
                        _Path = GetOutputMediaFile(this, b.GetString(ExtraPath), _Title, _IsPhoto, false);

                        Touch();

                        bool targetsNOrNewer;

                        try
                        {
                            targetsNOrNewer = (int)app.Application.Context.ApplicationInfo.TargetSdkVersion >= 24;
                        }
                        catch (Exception appInfoEx)
                        {
                            System.Diagnostics.Debug.WriteLine("Unable to get application info for targetSDK, trying to get from package manager: " + appInfoEx);
                            targetsNOrNewer = false;

                            var appInfo = PackageManager.GetApplicationInfo(app.Application.Context.PackageName, 0);
                            if (appInfo != null)
                            {
                                targetsNOrNewer = (int)appInfo.TargetSdkVersion >= 24;
                            }
                        }

                        if (targetsNOrNewer && _Path.Scheme == "file")
                        {
                            throw new NotImplementedException();
                        }
                        pickIntent.PutExtra(MediaStore.ExtraOutput, _Path);
                    }
                    else
                    {
                        _Path = Uri.Parse(b.GetString(ExtraPath));
                    }
                }

                if (!ran)
                {
                    StartActivityForResult(pickIntent, _Id);
                }
            }
            catch (Exception ex)
            {
                OnMediaPicked(new MediaPickedEventArgs(_Id, ex));
                //must finish here because an exception has occured else blank screen
                Finish();
            }
            finally
            {
                pickIntent?.Dispose();
            }
        }

        private void Touch()
        {
            if (_Path.Scheme != "file")
            {
                return;
            }

            var newPath = GetLocalPath(_Path);
            try
            {
                var stream = File.Create(newPath);
                stream.Close();
                stream.Dispose();

            }
            catch (Exception ex)
            {
                diag.Debug.WriteLine("Unable to create path: " + newPath + " " + ex.Message + "This means you have illegal characters");
                throw ;
            }
        }

        private void DeleteOutputFile()
        {
            try
            {
                if (_Path?.Scheme != "file")
                {
                    return;
                }

                var localPath = GetLocalPath(_Path);

                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to delete file: " + ex.Message);
            }
        }

        private void GrantUriPermissionsForIntent(Intent intent, Uri uri)
        {
            var resInfoList = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            foreach (var resolveInfo in resInfoList)
            {
                var packageName = resolveInfo.ActivityInfo.PackageName;
                GrantUriPermission(packageName, uri, ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);
            }
        }

        internal static Task<MediaPickedEventArgs> GetMediaFileAsync(Context context, int requestCode, string action, bool isPhoto, ref Uri path, Uri data)
        {
            Task<Tuple<string, bool>> pathFuture;

            string originalPath = null;

            if (action != Intent.ActionPick)
            {

                originalPath = path.Path;


                // Not all camera apps respect EXTRA_OUTPUT, some will instead
                // return a content or file uri from data.
                if (data != null && data.Path != originalPath)
                {
                    originalPath = data.ToString();
                    var currentPath = path.Path;
                    pathFuture = TryMoveFileAsync(context, data, path, isPhoto).ContinueWith(t =>
                        new Tuple<string, bool>(t.Result ? currentPath : null, false));
                }
                else
                {
                    pathFuture = TaskFromResult(new Tuple<string, bool>(path.Path, false));

                }
            }
            else if (data != null)
            {
                originalPath = data.ToString();
                path = data;
                pathFuture = GetFileForUriAsync(context, path, isPhoto);
            }
            else
            {
                pathFuture = TaskFromResult<Tuple<string, bool>>(null);
            }

            return pathFuture.ContinueWith(t =>
            {

                var resultPath = t.Result.Item1;
                var aPath = originalPath;
                if (resultPath == null || !File.Exists(t.Result.Item1))
                {
                    return new MediaPickedEventArgs(requestCode, new MediaFileNotFoundException(originalPath));
                }

                var mf = new MediaFile(resultPath, () => File.OpenRead(resultPath), aPath);
                return new MediaPickedEventArgs(requestCode, false, mf);
            });
        }

        private bool _Completed;

        /// <summary>
        /// OnActivity Result
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _Completed = true;
            base.OnActivityResult(requestCode, resultCode, data);



            if (_Tasked)
            {
                if (resultCode == Result.Canceled)
                {
                    //delete empty file
                    DeleteOutputFile();

                    var future = TaskFromResult(new MediaPickedEventArgs(requestCode, true));

                    Finish();
                    await Task.Delay(50);

                    //TODO: Await this?

                    future.ContinueWith(t => OnMediaPicked(t.Result));
                }
                else
                {

                    var e = await GetMediaFileAsync(this, requestCode, _Action, _IsPhoto, ref _Path, data?.Data);
                    Finish();
                    await Task.Delay(50);
                    OnMediaPicked(e);

                }
            }
            else
            {
                if (resultCode == Result.Canceled)
                {
                    //delete empty file
                    DeleteOutputFile();

                    SetResult(Result.Canceled);
                }
                else
                {
                    var resultData = new Intent();
                    resultData.PutExtra("MediaFile", data?.Data);
                    resultData.PutExtra("path", _Path);
                    resultData.PutExtra("isPhoto", _IsPhoto);
                    resultData.PutExtra("action", _Action);
                    resultData.PutExtra(ExtraSaveToAlbum, _SaveToAlbum);
                    SetResult(Result.Ok, resultData);
                }

                Finish();
            }
        }

        public static Task<bool> TryMoveFileAsync(Context context, Uri url, Uri path, bool isPhoto)
        {
            var moveTo = GetLocalPath(path);
            return GetFileForUriAsync(context, url, isPhoto).ContinueWith(t =>
            {
                if (t.Result.Item1 == null)
                {
                    return false;
                }

                try
                {
                    if (url.Scheme == "content")
                    {
                        context.ContentResolver.Delete(url, null, null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to delete content resolver file: " + ex.Message);
                }

                try
                {
                    File.Delete(moveTo);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to delete normal file: " + ex.Message);
                }

                try
                {
                    File.Move(t.Result.Item1, moveTo);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to move files: " + ex.Message);
                }

                return true;
            }, TaskScheduler.Default);
        }

        private static int GetVideoQuality(VideoQuality videoQuality)
        {
            switch (videoQuality)
            {
                case VideoQuality.Medium:
                case VideoQuality.High:
                    return 1;

                default:
                    return 0;
            }
        }

        private static string GetUniquePath(string folder, string name, bool isPhoto)
        {
            var ext = Path.GetExtension(name);
            if (ext == string.Empty)
            {
                ext = isPhoto ? ".jpg" : ".mp4";
            }

            name = Path.GetFileNameWithoutExtension(name);

            var nname = name + ext;
            var i = 1;
            while (File.Exists(Path.Combine(folder, nname)))
            {
                nname = name + "_" + i++ + ext;
            }

            return Path.Combine(folder, nname);
        }

        public static Uri GetOutputMediaFile(Context context, string subdir, string name, bool isPhoto, bool saveToAlbum)
        {
            subdir = subdir ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                if (isPhoto)
                {
                    name = "IMG_" + timestamp + ".jpg";
                }
                else
                {
                    name = "VID_" + timestamp + ".mp4";
                }
            }

            var mediaType = isPhoto ? Environment.DirectoryPictures : Environment.DirectoryMovies;
            var directory = saveToAlbum ? Environment.GetExternalStoragePublicDirectory(mediaType) : context.GetExternalFilesDir(mediaType);
            using (var mediaStorageDir = new Java.IO.File(directory, subdir))
            {
                if (mediaStorageDir.Exists())
                {
                    return Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name, isPhoto)));
                }

                if (!mediaStorageDir.Mkdirs())
                {
                    throw new IOException("Couldn't create directory, have you added the WRITE_EXTERNAL_STORAGE permission?");
                }

                if (saveToAlbum)
                {
                    return Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name, isPhoto)));
                }

                // Ensure this media doesn't show up in gallery apps
                using (var nomedia = new Java.IO.File(mediaStorageDir, ".nomedia"))
                {
                    nomedia.CreateNewFile();
                }

                return Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name, isPhoto)));
            }
        }

        private static Task<Tuple<string, bool>> GetFileForUriAsync(Context context, Uri uri, bool isPhoto)
        {
            var tcs = new TaskCompletionSource<Tuple<string, bool>>();

            switch (uri.Scheme)
            {
                case "file":
                    tcs.SetResult(new Tuple<string, bool>(new System.Uri(uri.ToString()).LocalPath, false));
                    break;
                case "content":
                    Task.Factory.StartNew(() =>
                    {
                        ICursor cursor = null;
                        try
                        {
                            string[] proj = null;
                            if ((int)Build.VERSION.SdkInt >= 22)
                            {
                                proj = new[] { MediaStore.MediaColumns.Data };
                            }

                            cursor = context.ContentResolver.Query(uri, proj, null, null, null);
                            if (cursor == null || !cursor.MoveToNext())
                            {
                                tcs.SetResult(new Tuple<string, bool>(null, false));
                            }
                            else
                            {
                                var column = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
                                string contentPath = null;

                                if (column != -1)
                                {
                                    contentPath = cursor.GetString(column);
                                }



                                // If they don't follow the "rules", try to copy the file locally
                                if (contentPath == null || !contentPath.StartsWith("file", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string fileName = null;
                                    try
                                    {
                                        fileName = Path.GetFileName(contentPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Unable to get file path name, using new unique " + ex);
                                    }


                                    var outputPath = GetOutputMediaFile(context, "temp", fileName, isPhoto, false);

                                    try
                                    {
                                        using (var input = context.ContentResolver.OpenInputStream(uri))
                                        using (Stream output = File.Create(outputPath.Path))
                                        {
                                            input.CopyTo(output);
                                        }

                                        contentPath = outputPath.Path;
                                    }
                                    catch (Java.IO.FileNotFoundException fnfEx)
                                    {
                                        // If there's no data associated with the uri, we don't know
                                        // how to open this. contentPath will be null which will trigger
                                        // MediaFileNotFoundException.
                                        System.Diagnostics.Debug.WriteLine("Unable to save picked file from disk " + fnfEx);
                                    }
                                }

                                tcs.SetResult(new Tuple<string, bool>(contentPath, false));
                            }
                        }
                        finally
                        {
                            if (cursor != null)
                            {
                                cursor.Close();
                                cursor.Dispose();
                            }
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                    break;
                default:
                    tcs.SetResult(new Tuple<string, bool>(null, false));
                    break;
            }

            return tcs.Task;
        }

        private static string GetLocalPath(Uri uri)
        {
            return new System.Uri(uri.ToString()).LocalPath;
        }

        private static Task<T> TaskFromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(result);
            return tcs.Task;
        }

        private static void OnMediaPicked(MediaPickedEventArgs e)
        {
            MediaPicked?.Invoke(null, e);
        }

        public void OnScanCompleted(string path, Uri uri)
        {
            Console.WriteLine("scan complete: " + path);
        }

        protected override void OnDestroy()
        {
            if (!_Completed)
            {
                DeleteOutputFile();
            }
            base.OnDestroy();
        }
    }
}
