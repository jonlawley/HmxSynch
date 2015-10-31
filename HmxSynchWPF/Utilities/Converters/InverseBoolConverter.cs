using System;
using System.Globalization;
using System.Windows.Data;

namespace HmxSynchWPF.Utilities.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is bool && (bool) value;
            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is bool && (bool)value;
            return !boolValue;
        }
    }
}