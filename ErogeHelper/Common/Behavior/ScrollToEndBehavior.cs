using System.Windows;
using System.Windows.Controls;

namespace ErogeHelper.Common.Behavior
{
    public class ScrollToEndBehavior
    {
        public static readonly DependencyProperty OnTextChangedProperty =
            DependencyProperty.RegisterAttached(
                "OnTextChanged",
                typeof(bool),
                typeof(ScrollToEndBehavior),
                new UIPropertyMetadata(false, OnTextChanged)
            );

        public static bool GetOnTextChanged(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(OnTextChangedProperty);
        }

        public static void SetOnTextChanged(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(OnTextChangedProperty, value);
        }

        private static void OnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;

            if (dependencyObject is not TextBox textBox || (bool)e.OldValue == newValue)
            {
                return;
            }

            static void Handler(object sender, TextChangedEventArgs _) => ((TextBox) sender).ScrollToEnd();

            if (newValue)
            {
                textBox.TextChanged += Handler;
            }
            else
            {
                textBox.TextChanged -= Handler;
            }
        }
    }
}