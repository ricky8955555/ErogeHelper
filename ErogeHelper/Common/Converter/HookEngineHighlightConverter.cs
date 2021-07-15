using System;
using System.Windows.Data;

namespace ErogeHelper.Common.Converter
{
    public class HookEngineHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string engineName && engineName.Contains("UserHook"))
            {
                return "UserHook";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}