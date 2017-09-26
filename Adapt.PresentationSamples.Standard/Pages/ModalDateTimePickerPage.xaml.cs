using Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ModalDateTimePickerPage : ContentPage
	{
		public ModalDateTimePickerPage ()
		{
			InitializeComponent ();
            BindingContext = new TestModel { };
		}

    }
}