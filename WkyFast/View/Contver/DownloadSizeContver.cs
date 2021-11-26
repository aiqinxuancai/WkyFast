using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WkyFast.Utils;

namespace WkyFast.View.Contver
{

    public class DownloadSizeContver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var price = FileSizeContver.GetSizeString(long.Parse((string)value));
                return price;
            }
            if (value is long)
            {
                var price = FileSizeContver.GetSizeString((long)value);
                return price;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }


    }
}
