using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Adapt.Presentation.UWP.Adapt.Presentation.UWP
{
    [TemplateVisualState(Name = StateContentVisible, GroupName = GroupContent)]
    [TemplateVisualState(Name = StateContentCollapsed, GroupName = GroupContent)]
    [TemplatePart(Name = DismissButtonPart, Type = typeof(Button))]
    public sealed class InAppNotification : ContentControl, IInAppNotification
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="ShowDismissButton"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowDismissButtonProperty = DependencyProperty.Register(nameof(ShowDismissButton), typeof(bool), typeof(InAppNotification), new PropertyMetadata(true, OnShowDismissButtonChanged));

        /// <summary>
        /// Gets or sets a value indicating whether to show the Dismiss button of the control.
        /// </summary>
        public bool ShowDismissButton
        {
            get => (bool)GetValue(ShowDismissButtonProperty);
            set => SetValue(ShowDismissButtonProperty, value);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Key of the UI Element that dismiss the control
        /// </summary>
        private const string DismissButtonPart = "PART_DismissButton";

        /// <summary>
        /// Key of the VisualStateGroup that show/dismiss content
        /// </summary>
        private const string GroupContent = "State";

        /// <summary>
        /// Key of the VisualState when content is dismissed
        /// </summary>
        private const string StateContentCollapsed = "Collapsed";

        /// <summary>
        /// Key of the VisualState when content is showed
        /// </summary>
        private const string StateContentVisible = "Visible";

        private readonly DispatcherTimer _Timer = new DispatcherTimer();

        private Button _DismissButton;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InAppNotification"/> class.
        /// </summary>
        public InAppNotification()
        {
            DefaultStyleKey = typeof(InAppNotification);

            _Timer.Tick += (sender, e) =>
            {
                Dismiss();
            };
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Event raised when the notification is dismissed
        /// </summary>
        public event EventHandler Dismissed;

        #endregion

        #region Public Methods

        /// <summary>
        /// Dismiss the notification
        /// </summary>
        public void Dismiss()
        {
            if (Visibility == Visibility.Visible)
            {
                VisualStateManager.GoToState(this, StateContentCollapsed, true);
                Dismissed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Show notification using the current template
        /// </summary>
        /// <param name="duration">Displayed duration of the notification in ms (less or equal 0 means infinite duration)</param>
        public void Show(int duration = 0)
        {
            _Timer.Stop();

            Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, StateContentVisible, true);

            if (duration > 0)
            {
                _Timer.Interval = TimeSpan.FromMilliseconds(duration);
                _Timer.Start();
            }
        }

        public void Show(string text)
        {
            Show(text, 10);
        }

        /// <summary>
        /// Show notification using text as the content of the notification
        /// </summary>
        /// <param name="text">Text used as the content of the notification</param>
        /// <param name="duration">Displayed duration of the notification in ms (less or equal 0 means infinite duration)</param>
        public void Show(string text, int duration)
        {
            ContentTemplate = null;
            Content = text;
            Show(duration);
        }

        /// <summary>
        /// Show notification using UIElement as the content of the notification
        /// </summary>
        /// <param name="element">UIElement used as the content of the notification</param>
        /// <param name="duration">Displayed duration of the notification in ms (less or equal 0 means infinite duration)</param>
        public void Show(UIElement element, int duration = 0)
        {
            ContentTemplate = null;
            Content = element;
            Show(duration);
        }

        /// <summary>
        /// Show notification using DataTemplate as the content of the notification
        /// </summary>
        /// <param name="dataTemplate">DataTemplate used as the content of the notification</param>
        /// <param name="duration">Displayed duration of the notification in ms (less or equal 0 means infinite duration)</param>
        public void Show(DataTemplate dataTemplate, int duration = 0)
        {
            ContentTemplate = dataTemplate;
            Content = null;
            Show(duration);
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            if (_DismissButton != null)
            {
                _DismissButton.Click -= DismissButton_Click;
            }

            _DismissButton = (Button)GetTemplateChild(DismissButtonPart);

            if (_DismissButton != null)
            {
                _DismissButton.Visibility = ShowDismissButton ? Visibility.Visible : Visibility.Collapsed;
                _DismissButton.Click += DismissButton_Click;
            }

            VisualStateManager.GoToState(this, Visibility == Visibility.Visible ? StateContentVisible : StateContentCollapsed, true);

            base.OnApplyTemplate();
        }

        #endregion

        #region Private Methods

        private static void OnShowDismissButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inApNotification = (InAppNotification)d;

            var showDismissButton = (bool)e.NewValue;

            if (inApNotification._DismissButton != null)
            {
                inApNotification._DismissButton.Visibility = showDismissButton ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            Dismiss();
        }

        #endregion
    }
}