using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using System;

namespace Adapt.Presentation.AndroidPlatform
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Preserve(AllMembers = true)]
    public class FilePickerActivity : Activity
    {
        #region Protected Overrides
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("*/*");

            intent.AddCategory(Intent.CategoryOpenable);
            try
            {
                StartActivityForResult(Intent.CreateChooser(intent, "Select file"), 0);
            }
            catch (Exception exAct)
            {
                System.Diagnostics.Debug.Write(exAct);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Canceled)
            {
                // Notify user file picking was cancelled.
                OnFilePickCancelled();
                Finish();
            }
            else
            {
                System.Diagnostics.Debug.Write(data.Data);
                try
                {
                    var uri = data.Data;

                    var filePath = IoUtil.GetPath(ApplicationContext, uri);

                    if (string.IsNullOrEmpty(filePath))
                    {
                        filePath = uri.Path;
                    }

                    var fileName = GetFileName(ApplicationContext, uri);

                    OnFilePicked(new FilePickerEventArgs(fileName, filePath));
                }
                catch (Exception readEx)
                {
                    // Notify user file picking failed.
                    OnFilePickCancelled();
                    System.Diagnostics.Debug.Write(readEx);
                }
                finally
                {
                    Finish();
                }
            }
        }
        #endregion

        #region Internal Static Events
        internal static event EventHandler<FilePickerEventArgs> FilePicked;
        internal static event EventHandler<EventArgs> FilePickCancelled;
        #endregion

        #region Private Static Methods
        private static string GetFileName(Context ctx, Android.Net.Uri uri)
        {

            string[] projection = { MediaStore.MediaColumns.DisplayName };

            var cr = ctx.ContentResolver;
            var name = "";
            var metaCursor = cr.Query(uri, projection, null, null, null);

            if (metaCursor == null)
            {
                return name;
            }

            try
            {
                if (metaCursor.MoveToFirst())
                {
                    name = metaCursor.GetString(0);
                }
            }
            finally
            {
                metaCursor.Close();
            }

            return name;
        }
        #endregion

        #region Private Static Methods
        private static void OnFilePickCancelled()
        {
            FilePickCancelled?.Invoke(null, null);
        }

        private static void OnFilePicked(FilePickerEventArgs e)
        {
            FilePicked?.Invoke(null, e);
        }
        #endregion
    }
}