using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class DateTimePicker : WrapLayout, IDisposable
    {
        #region Fields
        private readonly DatePicker _Date;
        private readonly TimePicker _Time;
        private readonly Button _ClearButton;
        private readonly Button _NowButton;
        private bool _IsChanging;
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty ValueProperty =
        BindableProperty.Create<DateTimePicker, DateTime>
        (
            p => p.Value,
            defaultValue: default(DateTime),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                ValueChanging(bindable, oldValue, newValue);
            }
        );

        private static void ValueChanging(BindableObject bindable, DateTime oldValue, DateTime newValue)
        {
            var ctrl = (DateTimePicker)bindable;
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
            Padding = 2;
            _Date = new DatePicker { Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern };
            _Time = new TimePicker { Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern };
            _ClearButton = new Button { Text = "Clear" };
            _NowButton = new Button { Text = "Now" };
            Children.Add(_ClearButton);
            Children.Add(_NowButton);
            Children.Add(_Date);
            Children.Add(_Time);
            _Date.PropertyChanged += PropertyChanged;
            _Time.PropertyChanged += PropertyChanged;
            _ClearButton.Clicked += ClearButton_Clicked;
            _NowButton.Clicked += NowButton_Clicked;
            _Date.Date = new DateTime();
            _Time.Time = new DateTime().TimeOfDay;
        }
        #endregion

        #region Event Handlers

        private void NowButton_Clicked(object sender, EventArgs e)
        {
            Value = DateTime.Now;
        }

        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            Value = DateTime.MinValue;
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (_IsChanging)
            {
                return;
            }

            if (!new List<string> { nameof(TimePicker.Time), nameof(DatePicker.Date) }.Contains(propertyChangedEventArgs.PropertyName))
            {
                return;
            }

            Value = _Date.Date.Add(_Time.Time);
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            _Date.PropertyChanged -= PropertyChanged;
            _Time.PropertyChanged -= PropertyChanged;
            _ClearButton.Clicked -= ClearButton_Clicked;
            _NowButton.Clicked -= NowButton_Clicked;
        }
        #endregion
    }
}
