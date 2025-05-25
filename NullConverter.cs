using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AudioPlayer
{
    public class NullConverter : IValueConverter
    {
        public bool Invert { get; set; } // True for IsNotNull, False for IsNull

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;
            return Invert ? !isNull : isNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}