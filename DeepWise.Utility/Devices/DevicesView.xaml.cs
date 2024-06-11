using DeepWise.Controls;
using DeepWise.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

namespace DeepWise.Devices
{
    /// <summary>
    /// Interaction logic for DevicesView.xaml
    /// </summary>
    public partial class DevicesView : ItemsControl
    {
        public DevicesView()
        {
            InitializeComponent();
            Items.Clear();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.Source as Button;
            if (btn.DataContext is IDevice device)
            {
                btn.Opacity = 0.5;
                btn.IsEnabled = true;
                var parent = btn.TemplatedParent as ContentPresenter;//indicator
                var tbx_exclamation = parent.ContentTemplate.FindName("tbx_exclamation", parent) as TextBlock;
                var light = parent.ContentTemplate.FindName("indicator", parent) as StatusIndicator;
                var success = await Task.Run(device.Initialize);
                light.LightColor = success? Colors.Green : Colors.Red;
                tbx_exclamation.Visibility = success? Visibility.Collapsed: Visibility.Visible;
                btn.IsEnabled = false;
                btn.Opacity = 1;
            }
        }

    }

    public class InvBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DeviceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                if (value.GetType().GetProperty("Name") is PropertyInfo info)
                {
                    var name = info.GetValue(value) as string;
                    if(!string.IsNullOrEmpty(name))
                        return info.GetValue(value) as string;
                }
                return value.GetType().GetDisplayName();
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
