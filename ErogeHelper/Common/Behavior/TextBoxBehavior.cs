using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ErogeHelper.Common.Behavior
{
    public class TextBoxBehavior
    {
        public static string GetSelectedText(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectedTextProperty);
        }

        public static void SetSelectedText(DependencyObject obj, string value)
        {
            obj.SetValue(SelectedTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedText. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.RegisterAttached(
                "SelectedText",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedTextChanged));

        private static void SelectedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is not TextBox tb)
                return;

            if (e.OldValue == null && e.NewValue != null)
            {
                tb.SelectionChanged += Tb_SelectionChanged;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                tb.SelectionChanged -= Tb_SelectionChanged;
            }

            if (e.NewValue is string newValue && !newValue.Equals(tb.SelectedText))
            {
                tb.SelectedText = newValue;
            }
        }

        private static void Tb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                SetSelectedText(tb, tb.SelectedText);
            }
        }
    }
}