using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using PortToNet.Model;

namespace PortToNet
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");
            return !(bool)value;
        }
    }

    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class Bool2ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                return new SolidColorBrush(Colors.Gray);
            bool val = (bool)value;
            if (val)
                return new SolidColorBrush(Colors.Green);
            else
                return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Thickness))]
    public class Bool2BorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                return new Thickness(2);
            bool val = (bool)value;
            if (val)
                return new Thickness(0);
            else
                return new Thickness(2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class Double2doubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(double))
                return 0.0;
            double val = (double)value;
            double pa = (double)parameter;
            return val + pa;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(double))
                return 0.0;
            double val = (double)value;
            double pa = (double)parameter;
            return val - pa;
        }
    }


    [ValueConversion(typeof(int), typeof(SolidColorBrush))]
    public class Value2ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = 0;
            int pa = 0;
            if (value.GetType() != typeof(int))
            {
                val = int.Parse((string)value);
            }
            else
            {
                val = (int)value;
            }
            if (parameter.GetType() != typeof(int))
            {
                pa = int.Parse((string)parameter);
            }
            else
            {
                pa = (int)parameter;
            }

            SolidColorBrush br = new SolidColorBrush(Colors.OldLace);
            if (val == pa)
                br = new SolidColorBrush(Colors.DarkCyan);
            return br;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class Bool2BorderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                return Visibility.Collapsed;
            bool val = (bool)value;
            if (val)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(NetProtocolTypeDescription), typeof(Visibility))]
    public class TcpProtocol2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            if (value.GetType() != typeof(NetProtocolTypeDescription))
                return Visibility.Collapsed;
            List<NetProtocolType> pars = new List<NetProtocolType>();
            if (parameter is Array)
            {
                Array array = (Array)parameter;
                foreach (var item in array)
                {
                    if (item.GetType() == typeof(NetProtocolType))
                    {
                        pars.Add((NetProtocolType)item);
                    }
                }
            }
            else
            {
                if (parameter.GetType() != typeof(NetProtocolType))
                    return Visibility.Collapsed;
                pars.Add((NetProtocolType)parameter);
            }
            NetProtocolTypeDescription val = (NetProtocolTypeDescription)value;
            bool cz = pars.Where(a => a == val.Mode.Value).Count() > 0;
            if (cz)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(long), typeof(string))]
    public class Bytes2HumanStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            long bytes;
            long.TryParse(value.ToString(), out bytes);
            if (bytes > 1024 * 1024 * 1024) // GB
            {
                double gb = (double)bytes / (1024 * 1024 * 1024);
                return $"{gb:F1}GB";
            }
            if (bytes > 1024 * 1024) // MB
            {
                double gb = (double)bytes / (1024 * 1024);
                return $"{gb:F1}MB";
            }
            if (bytes > 1024) // KB
            {
                double gb = (double)bytes / (1024);
                return $"{gb:F1}KB";
            }
            return bytes.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
