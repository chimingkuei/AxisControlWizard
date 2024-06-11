using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionControllers.UI
{
    /// <summary>
    /// IOIndicator.xaml 的互動邏輯
    /// </summary>
    //TODO : Change name from IOIndicator to DIOIndicator
    public partial class IOIndicator : UserControl
    {
        public IOIndicator()
        {
            InitializeComponent();
            var bd = new Binding("PortInfo")
            {
                Converter = PortInfoToNameConverter._instance,
                ConverterParameter = this,
                Source = this,
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, bd);
            
            //textBlock.GetBindingExpression(TextBlock.TextProperty).BindingGroup
            this.DataContextChanged += IOIndicator_DataContextChanged;
        }

        ADLINK_Motion controller;
        private void IOIndicator_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(controller !=null)
            {
                controller.DigitalSignalChanged -= Cntlr_IOStatusChanged;
            }

            if(DataContext is ADLINK_Motion cntlr)
            {
                controller = cntlr;
                textBlock.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                cntlr.DigitalSignalChanged += Cntlr_IOStatusChanged;
            }
            Update();
            
        }


        public static DependencyProperty OnColorProperty = DependencyProperty.Register(nameof(OnColor), typeof(Color), typeof(IOIndicator),new PropertyMetadata(Color.FromRgb(0,255,0)));
        public static DependencyProperty OffColorProperty = DependencyProperty.Register(nameof(OffColor), typeof(Color), typeof(IOIndicator), new PropertyMetadata(Color.FromRgb(0, 120, 0)));
        public Color OnColor
        {
            get => (Color)GetValue(OnColorProperty);
            set => SetValue(OnColorProperty, value);
        }
        public Color OffColor
        {
            get => (Color)GetValue(OffColorProperty);
            set => SetValue(OffColorProperty, value);
        }

        private void Cntlr_IOStatusChanged(object sender, IOEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Match(PortInfo))
                    indicator.LightColor = (bool)e.Value ? OnColor : OffColor;
            });
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(IOIndicator));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void Update()
        {
            if (controller != null && PortInfo != null)
            {
                if(!controller.IsInitialized)
                {
                    indicator.LightColor = Colors.Gray;
                    return;
                }
                try
                {
                    indicator.LightColor = controller.GetIOValue<bool>(PortInfo) ? OnColor : OffColor;
                }
                catch
                {
                    indicator.LightColor = Colors.Gray;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if(e.Property == PortInfoProperty)
            {
                if (PortInfo != null)
                    this.Cursor = PortInfo.Type.HasFlag(IOTypes.Output) ? Cursors.Hand : Cursors.Arrow;
                else
                    this.Cursor = Cursors.Arrow;
            }
        }

        public static readonly DependencyProperty PortInfoProperty = DependencyProperty.Register("PortInfo", typeof(IOPortInfo), typeof(IOIndicator));
        public IOPortInfo PortInfo
        {
            get => (IOPortInfo)GetValue(PortInfoProperty);
            set => SetValue(PortInfoProperty, value);
        }

        private void indicator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (controller != null && PortInfo != null && PortInfo.Type.HasFlag(IOTypes.Output))
                {
                    if (controller.IsInitialized)
                    {
                        controller.SetOutputValue(PortInfo, !controller.GetIOValue<bool>(PortInfo));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }

    internal class PortInfoToNameConverter : MarkupExtension, IValueConverter
    {
        internal static PortInfoToNameConverter _instance = new PortInfoToNameConverter();

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is IOIndicator indicator)
            {
                if (string.IsNullOrEmpty(indicator.Text))
                {
                    if (value is IOPortInfo info)
                    {
                        if (indicator.DataContext is ADLINK_Motion motion)
                        {
                            try
                            {
                                var pair = motion.IOTable.FirstOrDefault(x => info == x.Value);
                                if (pair.Value != null)
                                {
                                    return pair.Key;
                                }
                            }
                            catch { }
                        }

                        string name = "";
                        if (info.Type.HasFlag(IOTypes.Digital)) name = "D";
                        else name = "A";

                        if (info.Type.HasFlag(IOTypes.Input)) name += "I";
                        else name += "O";

                        if(info.Type.HasFlag(IOTypes.EtherCAT))
                        {
                            name += info.SlaveID + "," + info.Channel+"(Ether)";
                        }
                        else
                        {
                            name += info.Channel;
                        }

                        return name;
                    }
                }
                else
                    return indicator.Text;
            }
            return "error";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PortInfoToNameConverter());
        }
    }
}
