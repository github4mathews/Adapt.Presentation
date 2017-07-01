
using Adapt.Presentation;
using Model;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Adapt.PresentationSamples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            TakePhotoButton.Clicked += TakePhotoButton_Clicked;
            RenderButton.Clicked += RenderButton_Clicked;

            DateTimePickerTab.BindingContext = new DateTimeModel { TheDateTime = DateTime.Now };

            XAMLBox.Text= "<?xml version=\"1.0\" encoding=\"UTF - 8\"?>\r\n<ContentView xmlns = \"http://xamarin.com/schemas/2014/forms\" xmlns:x = \"http://schemas.microsoft.com/winfx/2009/xaml\" >\r\n\t<ContentView.Content>\r\n\t\t<StackLayout VerticalOptions=\"Center\" HorizontalOptions=\"Center\" BackgroundColor=\"LightBlue\">\r\n\t\t\t<Label Text=\"Hello Xamarin.Forms!\" />\r\n\t\t</StackLayout>\r\n\t</ContentView.Content>\r\n</ContentView>";
        }

        private void RenderButton_Clicked(object sender, EventArgs e)
        {
            ContentBox.Content = XamlReader.Load<ContentView>(XAMLBox.Text);
        }

        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
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