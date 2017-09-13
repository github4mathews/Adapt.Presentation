using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Adapt.Presentation.UWP.Adapt.Presentation.UWP
{
    public sealed partial class InAppNotification : ContentControl, IInAppNotification
    {
        #region Dependency Properties

        public DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(InAppNotification), null);

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        #region Private Fields

        private readonly DispatcherTimer _Timer = new DispatcherTimer();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InAppNotification"/> class.
        /// </summary>
        public InAppNotification()
        {
            //Prepare timer
            _Timer.Tick += (sender, e) =>
            {
                Dismiss();
            };

            //Setup control
            InitializeComponent();
            DataContext = this;

            //Setup flyout
            TheFlyout = new Flyout { Content = this };
            FlyoutBase.SetAttachedFlyout((Frame)Window.Current.Content, TheFlyout);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Event raised when the notification is dismissed
        /// </summary>
        public event EventHandler Dismissed;

        #endregion

        #region Public Properties
        public Flyout TheFlyout { get; set; }

        #endregion

        #region Private Properties

        private static FrameworkElement Frame => (FrameworkElement)Window.Current.Content;

        #endregion

        #region Public Methods

        /// <summary>
        /// Dismiss the notification
        /// </summary>
        public void Dismiss()
        {
            TheFlyout.Hide();
            Dismissed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Show notification using the current template
        /// </summary>
        /// <param name="duration">Displayed duration of the notification in ms (less or equal 0 means infinite duration)</param>
        public void Show(int duration = 0)
        {
            _Timer.Stop();

            TheFlyout.ShowAt(Frame);

            if (duration > 0)
            {
                _Timer.Interval = TimeSpan.FromMilliseconds(duration);
                _Timer.Start();
            }
        }

        public void Show(string text)
        {
            Show(text, 3000);
        }

        /// <summary>
        /// Show notification using text as the content of the notification
        /// </summary>
        /// <param name="text">Text used as the content of the notification</param>
        /// <param name="duration">Displayed duration of the notification in ms (less or equal 0 means infinite duration)</param>
        public void Show(string text, int duration)
        {
            Text = text;
            Show(duration);
        }

        #endregion

        #region Event Handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dismiss();
        }

        #endregion
    }
}