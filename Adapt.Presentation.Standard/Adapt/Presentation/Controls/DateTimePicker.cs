using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class DateTimePicker : WrapLayout
    {
        #region Fields
        private readonly DatePicker _Date;
        private readonly TimePicker _Time;
        private bool _IsChanging;
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty ValueProperty =
#pragma warning disable CS0618 // Type or member is obsolete
        BindableProperty.Create<DateTimePicker, DateTime>
        (
            p => p.Value,
            default(DateTime),
            BindingMode.TwoWay,
            propertyChanging: ValueChanging
        );
#pragma warning restore CS0618 // Type or member is obsolete

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
            var clearButton = new Button { Text = "Clear" };
            var nowButton = new Button { Text = "Now" };
            Children.Add(clearButton);
            Children.Add(nowButton);
            Children.Add(_Date);
            Children.Add(_Time);
            _Date.PropertyChanged += DateOrTimePropertyChanged;
            _Time.PropertyChanged += DateOrTimePropertyChanged;
            clearButton.Clicked += ClearButton_Clicked;
            nowButton.Clicked += NowButton_Clicked;
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

        private void DateOrTimePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
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
    }
}
