using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Views.Converters
{
    class FilterNumberToPolygonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var polygonId = (int)value;
            var currentFilterId = int.Parse((string)parameter);
            if(polygonId != currentFilterId)
                return new BrushConverter().ConvertFromString("#CC888888") as SolidColorBrush;  // Gray
            else
                return new BrushConverter().ConvertFromString("#265b2b") as SolidColorBrush;  // Green


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
