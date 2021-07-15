using System;
using System.Globalization;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace ErogeHelper.Common.Converter
{
    public class TextEvaluateConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string input)
            {
                var textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap
                };

                var escapedXml = SecurityElement.Escape(input);

                while (escapedXml?.IndexOf("|~S~|") != -1)
                {
                    //up to |~S~| is normal
                    textBlock.Inlines.Add(new Run(escapedXml?.Substring(0,
                        escapedXml.IndexOf("|~S~|", StringComparison.Ordinal))));

                    //between |~S~| and |~E~| is highlighted
                    textBlock.Inlines.Add(new Run(escapedXml?[
                            (escapedXml.IndexOf("|~S~|", StringComparison.Ordinal) + 5)..escapedXml.IndexOf("|~E~|", StringComparison.Ordinal)])
                    { TextDecorations = TextDecorations.Strikethrough, Background = Brushes.Red });

                    //the rest of the string (after the |~E~|)
                    escapedXml = escapedXml?[(escapedXml.IndexOf("|~E~|", StringComparison.Ordinal) + 5)..];
                }

                if (escapedXml.Length > 0)
                {
                    textBlock.Inlines.Add(new Run(escapedXml));
                }
                return textBlock;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("This converter cannot be used in two-way binding.");
        }
    }
}