using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Views.Converters
{
    public class BoolToColorConverter : MarkupExtension, IValueConverter
    {
        private static BoolToColorConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue)
                return new BrushConverter().ConvertFromString(parameter.ToString()) as SolidColorBrush;
            else
                return new BrushConverter().ConvertFromString("#CC888888") as SolidColorBrush;  // Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush colorBrush = (SolidColorBrush)value;

            if (colorBrush == new BrushConverter().ConvertFromString(parameter.ToString()) as SolidColorBrush)
                return true;
            else
                return false;
        }

        #endregion IValueConverter Members

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolToColorConverter());
        }
    }
}
