using System;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class DateTimePicker : StackLayout
    {
        #region Fields
        private readonly DatePicker _Date;
        private readonly TimePicker _Time;
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty ValueProperty =
        BindableProperty.Create<DateTimePicker, DateTime>
        (
            p => p.Value,
            defaultValue: default(DateTime),
            defaultBindingMode: BindingMode.OneWay,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                ValueChanging(bindable, oldValue, newValue);
            }
        );

        private static void ValueChanging(BindableObject bindable, DateTime oldValue, DateTime newValue)
        {
            var ctrl = (DateTimePicker)bindable;
            ctrl._Date.Date = newValue;
        }
        #endregion

        #region Public Properties
        public DateTime Value
        {
            get => (DateTime)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        #endregion

        #region Constructor
        public DateTimePicker()
        {
            Orientation = StackOrientation.Vertical;
            Padding = 2;
            _Date = new DatePicker { Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern };
            _Time = new TimePicker { Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern };
            Children.Add(_Date);
            Children.Add(_Time);
            _Date.PropertyChanged += DateOnPropertyChanged;
            _Time.PropertyChanged += TimeOnPropertyChanged;
            _Date.Date = new DateTime();
            _Time.Time = new DateTime().TimeOfDay;
        }
        #endregion

        #region Event Handlers
        private void TimeOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Time")
            {
                Value = _Date.Date.Add(_Time.Time);
            }
        }
        private void DateOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Date")
            {
                Value = _Date.Date.Add(_Time.Time);
            }
        }
        #endregion
    }
}
