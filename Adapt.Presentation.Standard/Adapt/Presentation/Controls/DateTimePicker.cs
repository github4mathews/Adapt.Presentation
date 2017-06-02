using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class DateTimePicker : StackLayout
    {
        #region Fields
        private readonly DatePicker _Date;
        private readonly TimePicker _Time;
        private bool _IsChanging;
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

            if (ctrl._IsChanging)
            {
                return;
            }

            ctrl._IsChanging = true;
            ctrl._Date.Date = newValue;
            ctrl._Time.Time = newValue.TimeOfDay;
            ctrl._IsChanging = false;
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
            _Date.PropertyChanged += PropertyChanged;
            _Time.PropertyChanged += PropertyChanged;
            _Date.Date = new DateTime();
            _Time.Time = new DateTime().TimeOfDay;
        }
        #endregion

        #region Event Handlers
        private void PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (new List<string> { nameof(TimePicker.Time), nameof(DatePicker.DateProperty) }.Contains(propertyChangedEventArgs.PropertyName))
            {
                Value = _Date.Date.Add(_Time.Time);
            }
        }
        #endregion
    }
}
