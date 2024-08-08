using System;
using System.Globalization;
using System.Windows.Data;

namespace dosymep.WPF.Converters {
    [ValueConversion(typeof(bool), typeof(bool))]
    [ValueConversion(typeof(bool), typeof(object))]
    public class InverseBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(targetType != typeof(bool) && targetType != typeof(bool?) && targetType != typeof(object)) {
                throw new InvalidOperationException("The target must be a boolean or an object");
            }

            bool? b = (bool?) value;
            return b.HasValue && !b.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(value as bool?);
        }
    }
}
