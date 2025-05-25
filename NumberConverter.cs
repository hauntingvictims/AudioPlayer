using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Collections;

namespace AudioPlayer
{
    public class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                return $"{number:D2}.";
            }
            return "00.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
