using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageScraper.Converters
{
    public class PointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var point = (Point)value;
            var roundedPoint = new Point(Math.Round(point.X, 0), Math.Round(point.Y, 0));
            return $"{roundedPoint.X}, {roundedPoint.Y}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}