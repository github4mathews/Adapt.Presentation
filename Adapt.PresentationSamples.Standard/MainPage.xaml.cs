
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
            GetLocationButton.Clicked += GetLocationButton_Clicked;
            ChooseFileButton.Clicked += ChooseFileButton_Clicked;
            RequestPermissionButton.Clicked += RequestPermissionButton_Clicked;

            DateTimePickerTab.BindingContext = new DateTimeModel { TheDateTime = DateTime.Now };

            XAMLBox.Text = "<?xml version=\"1.0\" encoding=\"UTF - 8\"?>\r\n<ContentView xmlns = \"http://xamarin.com/schemas/2014/forms\" xmlns:x = \"http://schemas.microsoft.com/winfx/2009/xaml\" >\r\n\t<ContentView.Content>\r\n\t\t<StackLayout VerticalOptions=\"Center\" HorizontalOptions=\"Center\" BackgroundColor=\"LightBlue\">\r\n\t\t\t<Label Text=\"Hello Xamarin.Forms!\" />\r\n\t\t</StackLayout>\r\n\t</ContentView.Content>\r\n</ContentView>";
        }

        private async void RequestPermissionButton_Clicked(object sender, EventArgs e)
        {
            var permissionStatusDictionary = await App.CurrentPermissions.RequestPermissionsAsync(Permission.Storage);
            if (permissionStatusDictionary.ContainsKey(Permission.Storage) && permissionStatusDictionary[Permission.Storage] != PermissionStatus.Granted)
            {
                await DisplayAlert("No Permission", "Permission to storage not granted", "OK");
            }

            await DisplayAlert("Permission", "Permission to storage granted", "OK");
        }

        private async void ChooseFileButton_Clicked(object sender, EventArgs e)
        {
            var filePicker = App.PresentationFactory.CreateFilePicker();
            using (var fileData = await filePicker.PickAndOpenFileForReading())
            {
                if (fileData == null)
                {
                    await DisplayAlert("No File Selected", "No File Selected", "OK");
                    return;
                }

                if (!fileData.IsPermissionGranted)
                {
                    await DisplayAlert("Permissions", "You must give this app permission to access files", "OK");
                    return;
                }

                await DisplayAlert("File Selected", $"File Name: {fileData.FileName}\r\nFile Size: {fileData.FileStream.Length}", "OK");
            }
        }

        private async void GetLocationButton_Clicked(object sender, EventArgs e)
        {
            var position = await App.Geolocator.GetPositionAsync(null, null, false);
            GeoLocationTab.BindingContext = position;
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
                    FileSelectionDictionary fileTypes = new FileSelectionDictionary { { "Jpeg Image", new List<string> { ".jpg" } } };

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