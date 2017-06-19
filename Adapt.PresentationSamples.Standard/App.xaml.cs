using Adapt.Presentation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Adapt.PresentationSamples
{
    public partial class App
    {
        public static IPresentationFactory PresentationFactory { get; private set; }
        public static IPermissions CurrentPermissions { get; private set; }

        public App(IPresentationFactory presentationFactory, IPermissions currentPermissions)
        {
            PresentationFactory = presentationFactory;
            CurrentPermissions = currentPermissions;

            var button = new Button { Text = "Take Photo" };
            button.Clicked += Button_Clicked;
            //var mainPage = new MainPage();
            //mainPage.DisplayAlert("test", "test", "test");

            mainPage = new ContentPage { Title = "test", Content = button };

            InitializeComponent();
            MainPage = mainPage;
        }


        ContentPage mainPage;


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            const string defaultFileName = "New Photo.jpg";
            var media = App.PresentationFactory.CreateMedia(App.CurrentPermissions);
            var filePicker = App.PresentationFactory.CreateFilePicker();

            var isCameraAvailable = await media.GetIsCameraAvailable();

            if (!isCameraAvailable || !media.IsTakePhotoSupported)
            {
                await mainPage.DisplayAlert("No Camera", ":( No camera available.", "OK");
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
