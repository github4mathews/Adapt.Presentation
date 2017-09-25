using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class DateTimePicker : Grid
    {
        #region Fields
        private readonly DatePicker _DatePicker;
        private readonly TimePicker _TimePicker;
        private bool _IsChanging;
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty ValueProperty =
        BindableProperty.Create
        (
            nameof(Value),
            typeof(DateTime),
            typeof(DateTimePicker),
            DateTime.MinValue,
            BindingMode.TwoWay,
            propertyChanging: ValueChanging
        );

        private static void ValueChanging(BindableObject bindable, object oldValue, object newValue)
        {
            var date = (DateTime)newValue;
            var thisPicker = (DateTimePicker)bindable;
            thisPicker._IsChanging = true;
            thisPicker._DatePicker.Date = date;
            thisPicker._TimePicker.Time = date.TimeOfDay;
            thisPicker._IsChanging = false;
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

            _DatePicker = new DatePicker { Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, Date = Value };
            _TimePicker = new TimePicker { Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern, Time = Value.TimeOfDay };

            var clearButton = new Button { Text = "Clear" };
            var nowButton = new Button { Text = "Now" };

            Children.Add(clearButton);
            Children.Add(nowButton);
            Children.Add(_DatePicker);
            Children.Add(_TimePicker);

            RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition());

            SetColumn(_DatePicker, 1);
            SetColumn(_TimePicker, 1);
            SetRow(_TimePicker, 1);
            SetRow(clearButton, 1);

            _DatePicker.PropertyChanged += DateOrTimePropertyChanged;
            _TimePicker.PropertyChanged += DateOrTimePropertyChanged;
            clearButton.Clicked += ClearButton_Clicked;
            nowButton.Clicked += NowButton_Clicked;
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

            Value = _DatePicker.Date.Add(_TimePicker.Time);
        }
        #endregion
    }
}
