
using Adapt.Presentation;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Adapt.PresentationSamples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();

            TakePhotoButton.Clicked += TakePhotoButton_Clicked;
        }

        private async void TakePhotoButton_Clicked(object sender, System.EventArgs e)
        {
            const string defaultFileName = "New Photo.jpg";
            var media = App.PresentationFactory.CreateMedia(App.CurrentPermissions);
            var filePicker = App.PresentationFactory.CreateFilePicker();

            var isCameraAvailable = await media.GetIsCameraAvailable();

            if (!isCameraAvailable || !media.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            using (var mediaFile = await media.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Adapt Sample App",
                Name = defaultFileName
            }))
            {
                using (var readFileStream = mediaFile.GetStream())
                {
                    var fileTypes = new Dictionary<string, IList<string>> { { "Jpeg Image", new List<string> { ".jpg" } } };

                    using (var fileData = await filePicker.PickAndOpenFileForWriting(fileTypes, defaultFileName))
                    {
                        var readBuffer = new byte[readFileStream.Length];
                        await readFileStream.ReadAsync(readBuffer, 0, (int)readFileStream.Length);
                        fileData.FileStream.Write(readBuffer, 0, readBuffer.Length);
                    }
                }
            }
        }
    }
}