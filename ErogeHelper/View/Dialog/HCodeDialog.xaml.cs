using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErogeHelper.Common.Constraint;
using ModernWpf.Controls;

namespace ErogeHelper.View.Dialog
{
    /// <summary>
    /// HCodeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HCodeDialog
    {
        public HCodeDialog()
        {
            InitializeComponent();

            Closing += (_, args) =>
            {
                // If the PrimaryButton is disabled, block the "Enter" key
                if (args.Result == ContentDialogResult.Primary && !IsPrimaryButtonEnabled)
                {
                    args.Cancel = true;
                }
            };
        }

        /// <summary>
        /// Dependency property for the <see cref="TextBox.Text"/> property.
        /// </summary>
        public static readonly DependencyProperty InputCodeProperty =
            DependencyProperty.Register(
                "InputCode",
                typeof(string),
                typeof(HCodeDialog),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// InputCode.Text
        /// </summary>
        public string InputCode
        {
            get => (string)GetValue(InputCodeProperty);
            set => SetValue(InputCodeProperty, value);
        }

        #region Customize Search Code Button

        // Declare
        public static readonly RoutedEvent SearchCodeEventRoutedEvent =
            EventManager.RegisterRoutedEvent(
                "SearchCodeClick",                // Event name in xaml
                RoutingStrategy.Bubble,                // Routing strategy
                typeof(EventHandler<RoutedEventArgs>), // Routing type
                typeof(HCodeDialog));         // this.GetType()

        // CLR事件包装
        // Add the declared static `RoutedEvent`
        /// <summary>
        /// The SearchCodeButton.Click event name
        /// </summary>
        public event RoutedEventHandler SearchCodeClick
        {
            add => AddHandler(SearchCodeEventRoutedEvent, value);
            remove => RemoveHandler(SearchCodeEventRoutedEvent, value);
        }

        private void SearchCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new(SearchCodeEventRoutedEvent, this);
            RaiseEvent(args);
        }

        /// <summary>
        /// Dependency property for the <see cref="AppBarButton.IsEnabled"/> property.
        /// </summary>
        public static readonly DependencyProperty CanSearchCodeProperty =
            DependencyProperty.Register(
                "CanSearchCode",
                typeof(bool),
                typeof(HCodeDialog),
                new PropertyMetadata(true, (sender, eventArgs) =>
                {
                    HCodeDialog dialog = (HCodeDialog)sender;
                    dialog.SearchCodeButton.IsEnabled = (bool)eventArgs.NewValue;
                }));

        /// <summary>
        /// SearchCodeButton IsEnable
        /// </summary>
        public bool CanSearchCode
        {
            get => (bool)GetValue(CanSearchCodeProperty);
            set => SetValue(CanSearchCodeProperty, value);
        }

        #endregion

        // if you just want to back a property by dp and provide a default value, use `PropertyMetadata,`
        // if you want to specify animation behavior, use `UIPropertyMetadata`,
        // but if some property affects wpf framework level stuffs eg element layout, parent layout or data binding,
        // use `FrameworkPropertyMetadata`.

        private void CodeTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void CodeTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // see: Common.Validation.InvalidCodeFormatValidationRule
            const string patten = ConstantValues.CodeRegExp;

            if (sender is TextBox codeTextBox)
            {
                if (codeTextBox.Text[^1] == ':')
                {
                    IsPrimaryButtonEnabled = false;
                    return;
                }
                IsPrimaryButtonEnabled = Regex.IsMatch(codeTextBox.Text, patten);
            }
        }
    }
}
