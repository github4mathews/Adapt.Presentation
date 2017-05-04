using Adapt.Presentation;
using System;
using Xamarin.Forms;

namespace Adapt.PresentationSamples
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            TakePhotoButton.Clicked += TakePhotoButton_Clicked;
        }

        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
        {
            var defaultFileName = $"New Photo.jpg";

            var media = App.PresentationFactory.CreateMedia();

            if (!media.IsCameraAvailable || !media.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            using (var file = await media.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Adapt Sample App",
                Name = defaultFileName
            }))
            {

            }
        }
    }
}
