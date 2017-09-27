using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Adapt.Presentation.Behaviours
{
    public class EntryPatternMatchValidationBehaviour : Behavior<Entry>
    {
        #region Fields
        private Entry Entry;
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

        #region Protected Overrides
        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            Entry = bindable;
            Entry.TextChanged += Entry_TextChanged;
            Refresh();
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
        }
        #endregion

        #region Event Handlers
        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }
        #endregion

        #region Private Methods
        private void Refresh()
        {
            if (Entry == null)
            {
                _LastText = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(Entry.Text) && !string.IsNullOrEmpty(Default))
            {
                if (IsMatching(Default, Mask))
                {
                    Entry.Text = Default;
                }
                else if (!string.IsNullOrEmpty(Mask))
                {
                    throw new Exception($"The Default of {Default} does not match the Mask {Mask}.");
                }
            }

            if (string.IsNullOrEmpty(Mask) || string.IsNullOrEmpty(Entry.Text))
            {
                _LastText = string.Empty;
                return;
            }

            var isMatch = IsMatching(Entry.Text, Mask);
            if (!isMatch)
            {
                Entry.Text = _LastText;
                return;
            }

            _LastText = Entry.Text;
        }
        #endregion

        #region Private Static Methods
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
