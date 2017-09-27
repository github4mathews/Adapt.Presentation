using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace TestXamarinForms
{
    public class PatternMatchEntryBehaviour : Entry
    {
        #region Fields
        private string _LastText = string.Empty;
        private string _Mask;
        private string _Default;
        #endregion

        #region Public Properties
        public string Mask
        {
            get
            {
                return _Mask;
            }
            set
            {
                _Mask = value;
                Refresh();
            }
        }

        public string Default
        {
            get => _Default;
            set
            {
                _Default = value;
                Refresh();
            }
        }
        #endregion

        #region Constructor
        public PatternMatchEntryBehaviour()
        {
            TextChanged += NumericTextBox_TextChanged;
        }
        #endregion

        #region Event Handlers
        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Default))
            {
                if (IsMatching(Default, Mask))
                {
                    Text = Default;
                }
                else if (!string.IsNullOrEmpty(Mask))
                {
                    throw new Exception($"The Default of {Default} does not match the Mask {Mask}.");
                }
            }

            if (string.IsNullOrEmpty(Mask) || string.IsNullOrEmpty(Text))
            {
                _LastText = string.Empty;
                return;
            }

            var isMatch = IsMatching(Text, Mask);
            if (!isMatch)
            {
                Text = _LastText;
                return;
            }

            _LastText = Text;
        }

        private static bool IsMatching(string text, string mask)
        {
            var success = false;
            var matches = Regex.Match(text, mask);
            if (matches.Success)
            {
                var match = matches.Captures.Cast<Capture>().FirstOrDefault();
                if (match.Value == text)
                {
                    success = true;
                }
            }

            return success;
        }
        #endregion
    }
}
