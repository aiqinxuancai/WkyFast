using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WkyFast.Utils;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

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
                14 => "准备添加中",
                38 => "磁盘写入异常",
                _ => value,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    [ValueConversion(typeof(long), typeof(SolidColorBrush))]
    public class DownloadStatusBrushContver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //ColorConverter colorConverter = new ColorConverter();

            var color = (long)value switch
            {
                0 => (Color)ColorConverter.ConvertFromString("#2d8cf0"), //"添加中", //蓝色
                8 =>  (Color)ColorConverter.ConvertFromString("#2d8cf0"),//"等待中", //蓝色
                9 => (Color)ColorConverter.ConvertFromString("#ff9900"),//"已暂停", //橙色
                1 => (Color)ColorConverter.ConvertFromString("#2d8cf0"),//"下载中", //蓝色
                11 => (Color)ColorConverter.ConvertFromString("#19be6b"), //已完成 //绿色
                14 => (Color)ColorConverter.ConvertFromString("#2d8cf0"),//"准备添加中", //蓝色
                38 => (Color)ColorConverter.ConvertFromString("#ed4014"),//"磁盘写入异常", //红色
                _ => (Color)ColorConverter.ConvertFromString("#f8f8f9"), //value, //灰色
            };


            return new SolidColorBrush(color);
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


    [ValueConversion(typeof(uint), typeof(Visibility))]
    public class DownloadSpeedVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (long)value switch
            {
                0 => Visibility.Visible,
                1 => Visibility.Visible,
                _ => Visibility.Collapsed
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
