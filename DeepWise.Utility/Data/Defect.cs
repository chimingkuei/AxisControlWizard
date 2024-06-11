using DeepWise.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeepWise.Data
{
    public class Defect : IDefect
    {
        public Defect(string name, System.Drawing.Rectangle rect)
        {
            Name = name;
            Region = new Shapes.Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public Defect(string name, DeepWise.Shapes.Rect rect)
        {
            Name = name;
            Region = rect;
        }
        public string Name { get; set; }

        public DeepWise.Shapes.Rect Region { get; set; }

    }

    public interface IDefect
    {
        string Name { get; }
        DeepWise.Shapes.Rect Region { get; }
    }

    public enum DetectionResult
    {
        None,
        Error,
        OK,
        NG,
        A,
        B,
    }
    public class Boolean2DetectionResultStringConverter : IValueConverter
    {
        public static Boolean2DetectionResultConverter Instance = new Boolean2DetectionResultConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? DetectionResult.OK.GetDisplayName() : DetectionResult.NG.GetDisplayName();
            else if (value is null)
                return DetectionResult.None.GetDisplayName();
            else
                throw new Exception();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Boolean2DetectionResultConverter : IValueConverter
    {
        public static Boolean2DetectionResultConverter Instance = new Boolean2DetectionResultConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? DetectionResult.OK : DetectionResult.NG;
            else if (value is null)
                return DetectionResult.None;
            else
                throw new Exception();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DetectionResult2ColorConverter : IValueConverter
    {
        public static DetectionResult2ColorConverter Instance = new DetectionResult2ColorConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool boolValue)
                return boolValue ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            else if(value is null)
                return System.Windows.Media.Brushes.DarkGray;
            else if(value is DetectionResult dr)
            {
                switch (dr)
                {
                    case DetectionResult.None:
                        return System.Windows.Media.Brushes.DarkGray;
                    case DetectionResult.OK:
                    case DetectionResult.A:
                    case DetectionResult.B:
                        return System.Windows.Media.Brushes.Green;
                    case DetectionResult.Error:
                    case DetectionResult.NG:
                        return System.Windows.Media.Brushes.Red;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
                throw new NotImplementedException();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
