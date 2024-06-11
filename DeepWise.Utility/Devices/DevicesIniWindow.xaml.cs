using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace DeepWise.Devices
{
    /// <summary>
    /// Interaction logic for DevicesInitializeWindow.xaml
    /// </summary>
    public partial class DevicesIniWindow : Window
    {
        public DevicesIniWindow(params IDevice[] devices)
        {
            InitializeComponent();
            this.devices = devices.Select(x => new DeviceInitializer(x)).ToList();
            listView.Items.Clear();
            listView.ItemsSource = this.devices;
            this.Loaded += DevicesInitializeWindow_Loaded;
   
            
        }


        private async void DevicesInitializeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tasks = new List<Task<bool>>();
            foreach (var device in this.devices)
            {
                tasks.Add(device.Initialize());
            }
            await Task.WhenAll(tasks);

            await Task.Delay(1000);
            if (tasks.All(x => x.Result))
                DialogResult = true;
            else
                btnConfirm.IsEnabled = true;
        }

        List<DeviceInitializer> devices;

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = devices.All(x => x.IsDone);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
        
            if((sender as Button).DataContext is DeviceInitializer item)
            {
                await item.Initialize();
            }
        }
    }



    
}
