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
            if (value != null && parameter is IList collection)
            {
                int index = collection.IndexOf(value);
                return $"{index + 1:D2}. "; 
            }
            return "01.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}