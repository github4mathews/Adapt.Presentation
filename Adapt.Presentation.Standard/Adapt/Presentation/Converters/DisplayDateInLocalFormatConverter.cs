using System;
using System.Globalization;
using Xamarin.Forms;

namespace Adapt.Presentation.Converters
{
    /// <summary>
    /// This converter simply calls ToString() on DateTimes in order to convert them to the local date format because of this Xamarin Forms binding bug: https://bugzilla.xamarin.com/show_bug.cgi?id=58635
    /// The bug is that Xamarin binding converts DateTimes to en-US format at bind time no matter what the regional settings of the device say.
    /// </summary>
    public class DisplayDateInLocalFormatConverter : IValueConverter
    {
        #region Fields
        private const string NoDateSelected = "No Date Selected";
        #endregion

        #region Public Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var emptyDateDisplay = (string)parameter ?? NoDateSelected;

            if (value is DateTime theDate)
            {
                return theDate == DateTime.MinValue ? emptyDateDisplay : theDate.ToString("dd/MM/yyyy HH:mm");
            }

            return emptyDateDisplay;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
}
