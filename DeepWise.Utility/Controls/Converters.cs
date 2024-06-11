using DeepWise.Localization;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace DeepWise.Controls
{
    public static class Converters
    {
        public static ObjectIsNotEmptyToBooleanConverter ObjectIsNotEmptyToBooleanConverter { get; } = new ObjectIsNotEmptyToBooleanConverter();
    }
    //TODO : load resource image extension
    public class ImageConverter : IValueConverter
    {
        //public Dispatcher Dispatcher { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (!(value is OpenCvSharp.Mat || value is Bitmap)) throw new ArgumentException();
            if (targetType == typeof(System.Windows.Media.ImageSource))
            {
                if (value is OpenCvSharp.Mat mat)
                    return ConvertToImageSource(mat);
                else if (value is Bitmap bmp)
                    return ConvertToImageSource(bmp);
                else
                    throw new Exception();
            }
            else if (targetType == typeof(Bitmap))
            {
                if (value is Mat mat)
                    return ConvertToBitmap(mat);
                else if (value is Bitmap bmp)
                    return bmp;
                else
                    throw new Exception();
            }
            else if(targetType == typeof(Mat))
            {
                if (value is Mat mat)
                    return mat;
                else if (value is Bitmap bmp)
                    return bmp.ToMat();
                else
                    throw new NotImplementedException();
            }
            else
                throw new NotImplementedException();
        }

        public static ImageSource ConvertToImageSource(Mat mat)
        {
            PixelFormat format;
            var matType = mat.Type();
            if (matType == MatType.CV_8U || matType == MatType.CV_8UC1)
                format = PixelFormats.Gray8;
            else if (matType == MatType.CV_8UC3)
                format = PixelFormats.Rgb24;
            else if (matType == MatType.CV_8UC4)
                format = PixelFormats.Bgr32;
            else
                throw new Exception();
            var stride = (int)mat.Step();
            return InteropBitmap.Create(mat.Width, mat.Height, 96, 96, format, null, mat.Data, mat.Height * stride, stride);
        }
        public static ImageSource ConvertToImageSource(Bitmap bmp)
        {
            PixelFormat format;
            switch (bmp.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    format = PixelFormats.Gray8;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    format = PixelFormats.Rgb24;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    format = PixelFormats.Bgr32;
                    break;
                default:
                    throw new NotImplementedException();
            }
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            var imSrc = InteropBitmap.Create(data.Width, data.Height, 96, 96, format, null, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return imSrc;
        }
        public static Bitmap ConvertToBitmap(Mat mat)
        {
            System.Drawing.Imaging.PixelFormat format;
            var matType = mat.Type();
            if (matType == MatType.CV_8U || matType == MatType.CV_8UC1)
                format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            else if (matType == MatType.CV_8UC3)
                format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            else if (matType == MatType.CV_8UC4)
                format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            else
                throw new Exception();
            var bmp = new Bitmap(mat.Width, mat.Height, (int)mat.Step(), format, mat.Data);
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ColorToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return string.Empty;
            if (value is int || value is int?)
            {
                int color = (int)value;
                if (parameter is string format)
                {
                    format = format.Replace("{0}", GetValue(0).ToString());
                    format = format.Replace("{1}", GetValue(1).ToString());
                    format = format.Replace("{2}", GetValue(2).ToString());
                    format = format.Replace("{3}", GetValue(3).ToString());
                    return format;
                    int GetValue(int channel) => (color & (0xFF << (channel * 8))) >> (channel * 8);
                   
                }
                else
                    return $"{(color & 0xFF000000) >> 24},{(color & 0xFF0000) >> 16},{(color & 0xFF00) >> 8},{color & 0xFF}";

            }
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class VisibilityToBooleanConverter : IValueConverter
    {

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else if (value is bool?)
            {
                bool? flag2 = (bool?)value;
                flag = (flag2.HasValue && flag2.Value);
            }

            return (!flag) ? Visibility.Collapsed : Visibility.Visible;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }

            return false;
        }

  
    }

    public class EnumToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumType = value.GetType();
            var memberInfos =
            enumType.GetMember(value.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m =>
            m.DeclaringType == enumType);
            if (enumValueMemberInfo.TryGetCustomAttribute<ColorAttribute>(out var att))
            {
                var c = att.Color;
                if (targetType == typeof(System.Windows.Media.Color))
                {
                    return System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);
                }
                else if (targetType == typeof(System.Windows.Media.Brush))
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(c.R, c.G, c.B));
                }
                else if (targetType == typeof(System.Drawing.Color))
                {
                    return att.Color;
                }
                else if (targetType == typeof(System.Drawing.Brush))
                {
                    return new System.Drawing.SolidBrush(att.Color);
                }
                else
                    throw new NotImplementedException();
            }
            else
                throw new NotImplementedException();

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                if (targetType == typeof(System.Windows.Media.Color))
                {
                    return b ? System.Windows.Media.Colors.Green : System.Windows.Media.Colors.Red;
                }
                else if (targetType == typeof(System.Windows.Media.Brush))
                {

                    return b ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                }
                else if (targetType == typeof(System.Drawing.Color))
                {

                    return b ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                }
                else if (targetType == typeof(System.Drawing.Brush))
                {
                    return b ? System.Drawing.Brushes.Green : System.Drawing.Brushes.Red;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                if (targetType == typeof(System.Windows.Media.Color))
                {
                    return System.Windows.Media.Colors.Gray;
                }
                else if (targetType == typeof(System.Windows.Media.Brush))
                {
                    return System.Windows.Media.Brushes.Gray;
                }
                else if (targetType == typeof(System.Drawing.Color))
                {
                    return System.Drawing.Color.Gray;
                }
                else if (targetType == typeof(System.Drawing.Brush))
                {
                    return System.Drawing.Brushes.Gray;
                }
                else
                    throw new NotImplementedException();
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Enum enumValue)
                return LocalizedDisplayNameAttributeHelper.GetDisplayName(enumValue);
            else if (value is Type type)
                return LocalizedDisplayNameAttributeHelper.GetDisplayName(type);
            else if (parameter is string propertyName)
                return LocalizedDisplayNameAttributeHelper.GetDisplayName(value.GetType().GetProperty(propertyName));
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(targetType == typeof(Boolean))
                return Object.Equals(value, parameter);
            else if(targetType == typeof(Visibility))
                return Object.Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed;
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HasFlagsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Enum @enum)
            {
                if (targetType == typeof(bool))
                {
                    @enum.GetType();
                    Type enumType = value.GetType();
                    if (parameter is string str)
                    {
                        if (str.Contains(','))
                            return str.Split(',').All(x => @enum.HasFlag((Enum)Enum.Parse(enumType, x)));
                        else
                            return @enum.HasFlag((Enum)Enum.Parse(enumType, parameter.ToString()));
                    }
                    else
                        throw new ArgumentNullException("converter parameter can not be null");
                }
                else if (targetType == typeof(Visibility))
                {
                    @enum.GetType();
                    Type enumType = value.GetType();
                    if (parameter is string str)
                    {
                        if (str.Contains(','))
                            return str.Split(',').All(x => @enum.HasFlag((Enum)Enum.Parse(enumType, x))) ? Visibility.Visible: Visibility.Collapsed;
                        else
                            return @enum.HasFlag((Enum)Enum.Parse(enumType, parameter.ToString())) ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                        throw new ArgumentNullException("converter parameter can not be null");
                }
                else
                    throw new NotImplementedException();

            }
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ObjectIsNotEmptyToBooleanConverter : IValueConverter
    {
        public static ObjectIsNotEmptyToBooleanConverter Instance = new ObjectIsNotEmptyToBooleanConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(targetType == typeof(bool)|| targetType == typeof(bool?))
            {

                if (parameter is bool b && !b)
                    return value is null;
                else if (parameter is string str && str.ToLower() == "false")
                {
                    return value is null;
                }
                else
                    return !(value is null);
            }
            else if (targetType == typeof(Visibility)|| targetType == typeof(Visibility?))
            {
                if (parameter is bool b && !b)
                    return value is null ? Visibility.Visible : Visibility.Collapsed;
                else if (parameter is string str && str.ToLower() == "false")
                {
                    return value is null ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                    return !(value is null) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectIsEmptyToBooleanConverter : IValueConverter
    {
        public static ObjectIsEmptyToBooleanConverter Instance = new ObjectIsEmptyToBooleanConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {

                if (parameter is bool b && !b)
                    return !(value is null);
                else if (parameter is string str && str.ToLower() == "false")
                {
                    return !(value is null);
                }
                else
                    return (value is null);
            }
            else if (targetType == typeof(Visibility))
            {
                if (parameter is bool b && !b)
                    return value is null ? Visibility.Collapsed: Visibility.Visible;
                else if (parameter is string str && str.ToLower() == "false")
                {
                    return value is null ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                    return !(value is null) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStringToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;
                else
                    return str;
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum eValue)
                return eValue.GetDisplayName();
            else
                return value.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EnumBooleanConverter : IValueConverter
    {
        public static EnumBooleanConverter Defualt { get; } = new EnumBooleanConverter();
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = null;
            if (parameter is Enum)
                parameterString = parameter.ToString();
            else if (parameter is string)
                parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = null;
            if (parameter is Enum)
                parameterString = parameter.ToString();
            else if (parameter is string)
                parameterString = parameter as string;

            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }
    public class EnumFlagsBooleanConverter : IValueConverter
    {
        public static EnumFlagsBooleanConverter Defualt { get; } = new EnumFlagsBooleanConverter();
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Enum eValue = (parameter as dynamic).Item1;
            return (value as Enum).HasFlag(eValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            dynamic eValue = (parameter as dynamic).Item1;
            object selectedObj = (parameter as dynamic).Item2;
            PropertyInfo info = (parameter as dynamic).Item3;
            dynamic crt = info.GetValue(selectedObj);
            if ((bool)value)
            {
                crt |= eValue;
            }
            else
            {
                crt &= ~eValue;
            }
            return crt;
        }
        #endregion
    }

    public class ScaleConverter : IValueConverter
    {
        public static ScaleConverter Instance { get; internal set; } = new ScaleConverter();

        public double MinScale { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var scale = (double)value * (double)parameter;
            if (scale < 1) scale = MinScale;
            return scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var scale = (double)value * (double)parameter;
            if (scale < 1) scale = MinScale;
            return (double)value / scale;
        }
    }

    [ContentProperty("Converters")]
    public class ValueConverterGroup : IValueConverter
    {
        #region Data

        private readonly ObservableCollection<IValueConverter> converters = new ObservableCollection<IValueConverter>();
        private readonly Dictionary<IValueConverter, ValueConversionAttribute> cachedAttributes = new Dictionary<IValueConverter, ValueConversionAttribute>();

        #endregion // Data

        #region Constructor

        public ValueConverterGroup()
        {
            this.converters.CollectionChanged += this.OnConvertersCollectionChanged;
        }

        #endregion // Constructor

        #region Converters

        /// <summary>
        /// Returns the list of IValueConverters contained in this converter.
        /// </summary>
        public ObservableCollection<IValueConverter> Converters
        {
            get { return this.converters; }
        }

        #endregion // Converters

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = 0; i < this.Converters.Count; ++i)
            {
                IValueConverter converter = this.Converters[i];
                Type currentTargetType = this.GetTargetType(i, targetType, true);
                output = converter.Convert(output, currentTargetType, parameter, culture);

                // If the converter returns 'DoNothing' then the binding operation should terminate.
                if (output == Binding.DoNothing)
                    break;
            }

            return output;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = this.Converters.Count - 1; i > -1; --i)
            {
                IValueConverter converter = this.Converters[i];
                Type currentTargetType = this.GetTargetType(i, targetType, false);
                output = converter.ConvertBack(output, currentTargetType, parameter, culture);

                // When a converter returns 'DoNothing' the binding operation should terminate.
                if (output == Binding.DoNothing)
                    break;
            }

            return output;
        }

        #endregion // IValueConverter Members

        #region Private Helpers

        #region GetTargetType

        /// <summary>
        /// Returns the target type for a conversion operation.
        /// </summary>
        /// <param name="converterIndex">The index of the current converter about to be executed.</param>
        /// <param name="finalTargetType">The 'targetType' argument passed into the conversion method.</param>
        /// <param name="convert">Pass true if calling from the Convert method, or false if calling from ConvertBack.</param>
        protected virtual Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
        {
            // If the current converter is not the last/first in the list, 
            // get a reference to the next/previous converter.
            IValueConverter nextConverter = null;
            if (convert)
            {
                if (converterIndex < this.Converters.Count - 1)
                {
                    nextConverter = this.Converters[converterIndex + 1];
                    if (nextConverter == null)
                        throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex + 1));
                }
            }
            else
            {
                if (converterIndex > 0)
                {
                    nextConverter = this.Converters[converterIndex - 1];
                    if (nextConverter == null)
                        throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex - 1));
                }
            }

            if (nextConverter != null)
            {
                ValueConversionAttribute conversionAttribute = cachedAttributes[nextConverter];

                // If the Convert method is going to be called, we need to use the SourceType of the next 
                // converter in the list.  If ConvertBack is called, use the TargetType.
                return convert ? conversionAttribute.SourceType : conversionAttribute.TargetType;
            }

            // If the current converter is the last one to be executed return the target type passed into the conversion method.
            return finalTargetType;
        }

        #endregion // GetTargetType

        #region OnConvertersCollectionChanged

        void OnConvertersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // The 'Converters' collection has been modified, so validate that each value converter it now
            // contains is decorated with ValueConversionAttribute and then cache the attribute value.

            IList convertersToProcess = null;
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                convertersToProcess = e.NewItems;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IValueConverter converter in e.OldItems)
                    this.cachedAttributes.Remove(converter);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.cachedAttributes.Clear();
                convertersToProcess = this.converters;
            }

            if (convertersToProcess != null && convertersToProcess.Count > 0)
            {
                foreach (IValueConverter converter in convertersToProcess)
                {
                    object[] attributes = converter.GetType().GetCustomAttributes(typeof(ValueConversionAttribute), false);

                    if (attributes.Length != 1)
                        throw new InvalidOperationException("All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once.");

                    this.cachedAttributes.Add(converter, attributes[0] as ValueConversionAttribute);
                }
            }
        }

        #endregion // OnConvertersCollectionChanged

        #endregion // Private Helpers
    }


    public class BooleanToOpacityConverter : IValueConverter
    {
        public static BooleanToOpacityConverter Instance = new BooleanToOpacityConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean b)
                return b ? 1 : 0.5;
            return value is null ? 0.5 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RangeRule : ValidationRule
    {
        double _min;
        double _max;
        double Min
        {
            get
            {
                if (Target == null)
                    return _min;
                else
                {
                    var property = Target.GetType().GetProperty(MinBindingProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    return Convert.ToDouble(property.GetValue(Target));
                }
            }
        }
        double Max
        {
            get
            {
                if (Target == null)
                    return _max;
                else
                {
                    var property = Target.GetType().GetProperty(MaxBindingProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    return Convert.ToDouble(property.GetValue(Target));
                }
            }
        }
        object Target;
        string MinBindingProperty;
        string MaxBindingProperty;
        public RangeRule(double min, double max)
        {
            _min = min;
            _max = max;
        }

        public RangeRule(object target, string min, string max)
        {
            Target = target;
            MinBindingProperty = min;
            MaxBindingProperty = max;
        }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double newValue = 0;
            try
            {
                if (((string)value).Length > 0)
                    newValue = Double.Parse((String)value);
            }
            catch (Exception e)
            {
                return new System.Windows.Controls.ValidationResult(false, $"Illegal characters or {e.Message}");
            }
            if ((newValue < Min) || (newValue > Max))
            {
                return new System.Windows.Controls.ValidationResult(false, $"Please enter an age in the range: {Min}-{Max}.");

            }
            return System.Windows.Controls.ValidationResult.ValidResult;
        }
    }
}
