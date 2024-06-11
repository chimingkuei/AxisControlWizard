using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionControllers.UI
{
    /// <summary>
    /// GeneralAnalogInputPanel.xaml 的互動邏輯
    /// </summary>
    public partial class IOPanel : UserControl
    {
        public static void Show(ADLINK_Motion  cntlr)
        {
       
            var wind = new Window() { SizeToContent = SizeToContent.WidthAndHeight, };
            var grid = new StackPanel() { Background = new SolidColorBrush(Colors.Black) };
            grid.Children.Add(new GroupBox() { Foreground = new SolidColorBrush(Colors.White), Header = "General Analog Output Table", Content = new IOPanel(IOTypes.General | IOTypes.Analog | IOTypes.Output)  });
            grid.Children.Add(new GroupBox() { Foreground = new SolidColorBrush(Colors.White), Header = "General Analog Input Table", Content = new IOPanel(IOTypes.General | IOTypes.Analog | IOTypes.Input) });
            wind.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wind.Content = grid;
            wind.DataContext = cntlr;
            
            wind.Show();
        }
        public IOPanel()
        {
            InitializeComponent();
            
            DataContextChanged += (s,e)=> InitializeGrid();

            Loaded += IOPanel_Loaded;
        }

        private void IOPanel_Loaded(object sender, RoutedEventArgs e)
        {
            dgc_slaveID.Visibility = PortType.HasFlag(IOTypes.EtherCAT) ? Visibility.Visible : Visibility.Collapsed;
            if(PortType.HasFlag(IOTypes.Input))
            {
                dgc_Value.IsReadOnly = PortType.HasFlag(IOTypes.Input);
                
            }
            else
            {
                ValueTextBoxCursor = Cursors.Hand;
            }
            if(PortType.HasFlag(IOTypes.Digital))
            {
                dataGrid.Columns.Remove(dgc_Value);
            }
            else if (PortType.HasFlag(IOTypes.EtherCAT))
            {
                dataGrid.Columns.Remove(dgc_BoolValue);

            }
        }

        public static readonly DependencyProperty ValueTextBoxCursorProperty = DependencyProperty.Register(nameof(ValueTextBoxCursor), typeof(Cursor), typeof(IOPanel));
        public Cursor ValueTextBoxCursor { get => (Cursor)GetValue(ValueTextBoxCursorProperty); set => SetValue(ValueTextBoxCursorProperty, value); }


        public IOPanel(IOTypes type) : this()
        {
            PortType = type;
        }

        public static readonly DependencyProperty PortTypeProperty = DependencyProperty.Register(nameof(PortType), typeof(IOTypes),typeof(IOPanel), new UIPropertyMetadata(IOTypes.General | IOTypes.Digital | IOTypes.Input));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch(e.Property.Name)
            {
                case nameof(PortType):
                    dgc_slaveID.Visibility = PortType.HasFlag(IOTypes.EtherCAT) ? Visibility.Visible : Visibility.Collapsed;
                    dgc_Value.IsReadOnly = PortType.HasFlag(IOTypes.Input);
                    InitializeGrid();
                    break;
            }
        }

        public IOTypes PortType
        {
            get => (IOTypes)GetValue(PortTypeProperty);
            set => SetValue(PortTypeProperty, value);
        }

        public bool IsAnalog => PortType.HasFlag(IOTypes.Analog);
        ADLINK_Motion controller;

        void InitializeGrid()
        {
            if(controller!=null)
            {
                controller.IOStatusChanged -= Controller_IOStatusChanged;
                try
                {
                }
                catch { }
            }
            
            if (DataContext is ADLINK_Motion cntlr)
            {
                controller = cntlr;
                try
                {
                    var match = PortType;
                    if (IsAnalog)
                    {
                        tmpA = new ObservableCollection<IOPort<double>>();
                        tmpD = null;
                        foreach (var item in controller.IOTable.Where(x => (x.Value.Type & match) == match).Select(x => new IOPort<double>(cntlr, x.Value)))
                            tmpA.Add(item);
                        //dataGrid.DataContext = tmpA;
                        dataGrid.ItemsSource = tmpA;
                    }
                    else
                    {
                        tmpA = null;
                        tmpD = new ObservableCollection<IOPort<bool>>();
                        foreach (var item in controller.IOTable.Where(x => (x.Value.Type & match) == match).Select(x => new IOPort<bool>(cntlr, x.Value)))
                            tmpD.Add(item);
                        //dataGrid.DataContext = tmpD;
                        dataGrid.ItemsSource = tmpD;
                    }
                    controller.IOStatusChanged += Controller_IOStatusChanged;
                }
                catch { }
            }
            else
                dataGrid.DataContext = null;
        }

        private void Controller_IOStatusChanged(object sender, IOEventArgs e)
        {
            if (tmpD != null)
                foreach (var item in tmpD.Where(x => e.Match(x.Info)))
                    item.NotifyPropertyChanged("Value");
            if (tmpA != null)
                foreach (var item in tmpA.Where(x => e.Match(x.Info)))
                    item.NotifyPropertyChanged("Value");
        }


        ObservableCollection<IOPort<double>> tmpA;
        ObservableCollection<IOPort<bool>> tmpD;

        

        private void Btn_DO_Switch(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && sender is TextBlock)
            {

                if (((FrameworkElement)sender).DataContext is IOPort<bool> port && port.Info.Type.HasFlag(IOTypes.Output))
                {
                    port.Value = !port.Value;
                }
            }
        }

        private void Btn_Rules_Clicked(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is IOPort<bool> port && port.Info.Type.HasFlag(IOTypes.Output))
            {
            }
        }
    }

    public class BooleanToOnOffConverter : IValueConverter
    {
        public static BooleanToOnOffConverter Instance { get; } = new BooleanToOnOffConverter();
        const string OnText = "ON";
        const string OffText = "OFF";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? OnText : OffText;
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                text = text.ToLower();
                if (text == OnText.ToLower())
                    return true;
                else if (text == OffText.ToLower())
                    return false;
                else
                    return false;
            }
            else
                return false;
        }
    }

    public class BooleanToColorConverter : IValueConverter
    {
        public static BooleanToColorConverter Instance { get; } = new BooleanToColorConverter();
        static System.Windows.Media.Brush OnColor = System.Windows.Media.Brushes.Green;
        static System.Windows.Media.Brush OffText = System.Windows.Media.Brushes.Red;

        public BooleanToColorConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? OnColor : OffText;
            else
                return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
