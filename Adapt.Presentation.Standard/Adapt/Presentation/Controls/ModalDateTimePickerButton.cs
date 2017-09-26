using System;
using System.Reflection;
using Xamarin.Forms;

namespace Adapt.Presentation.Controls
{
    public class ModalDateTimePickerButton : Button
    {
        #region Fields
        private readonly ModalDateTimePicker _ModalDateTimePicker;
        #endregion

        #region Public Properties
        public string DateTimePropertyName { get; set; }
        #endregion

        #region Constructor
        public ModalDateTimePickerButton()
        {
            _ModalDateTimePicker = new ModalDateTimePicker();
            Clicked += ModalDateTimePickerButton_Clicked;
            _ModalDateTimePicker.Closing += ModalDateTimePicker_Closing;
        }
        #endregion

        #region Event Handlers
        private async void ModalDateTimePickerButton_Clicked(object sender, EventArgs e)
        {
            var dateTimeProperty = GetDateTimeProperty();
            var value = (DateTime)dateTimeProperty.GetValue(BindingContext);
            _ModalDateTimePicker.SelectedValue = value;
            await Navigation.PushModalAsync(_ModalDateTimePicker);
        }

        private void ModalDateTimePicker_Closing(object sender, EventArgs e)
        {
            if (!_ModalDateTimePicker.IsOkClicked)
            {
                return;
            }

            var dateTimeProperty = GetDateTimeProperty();
            var selectValue = _ModalDateTimePicker.SelectedValue;
            dateTimeProperty.SetValue(BindingContext, selectValue);
        }
        #endregion

        #region Private Methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DateTimePropertyName")]
        private PropertyInfo GetDateTimeProperty()
        {
            if (BindingContext == null)
            {
                throw new Exception("There is no BindingContext");
            }

            if (string.IsNullOrEmpty(DateTimePropertyName))
            {
                throw new Exception($"{nameof(DateTimePropertyName)} must be specified.");
            }

            var bindingContextType = BindingContext.GetType();

            var dateTimeProperty = bindingContextType.GetRuntimeProperty(DateTimePropertyName);
            if (dateTimeProperty == null)
            {
                throw new Exception($"{DateTimePropertyName} does not exist on the type {bindingContextType.FullName}");
            }

            return dateTimeProperty;
        }
        #endregion
    }
}
