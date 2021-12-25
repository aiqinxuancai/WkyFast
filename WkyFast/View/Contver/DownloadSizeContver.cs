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
    [ValueConversion(typeof(int), typeof(string))]
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

    [ValueConversion(typeof(long), typeof(string))]
    public class DownloadStatusContver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return (long)value switch
            {
                0 => "添加中",
                8 => "等待中",
                9 => "已暂停",
                1 => "下载中",
                11 => "已完成",
                _ => value,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }


    /// <summary>
    /// 当前下载进度转换
    /// </summary>
    public class DownloadProgressContver : IMultiValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values">参数1进度 万分之，参数2 总大小</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            long fullSize = 0;
            double progress = 0;
            if (values.Length == 2)
            {

                if (values[0] is string)
                {
                    progress = long.Parse((string)values[0]) / 10000d;
                }
                else if (values[0] is long)
                {
                    progress = (long)values[0] / 10000d;
                }

                if (values[1] is string)
                {
                    fullSize = (long)(progress * long.Parse((string)values[1]));
                }
                else if (values[1] is long)
                {
                    fullSize = (long)(progress * (long)values[1]);
                }
                
            }


            return FileSizeContver.GetSizeString(fullSize);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(uint), typeof(Visibility))]
    public class DownloadProgressVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (long)value == 0 || (long)value == 10000)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
