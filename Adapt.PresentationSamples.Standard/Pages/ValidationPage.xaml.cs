using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ValidationPage : ContentPage
    {
        public ValidationPage()
        {
            InitializeComponent();
            BindingContext = new NumberModel();
        }
    }

    public class NumberModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private decimal _NumberDisplay;

        public decimal NumberDisplay
        {
            get => _NumberDisplay;
            set
            {
                _NumberDisplay = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberDisplay)));
            }
        }
    }

}