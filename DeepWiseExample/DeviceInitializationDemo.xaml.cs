using DeepWise.Devices;
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
using DeepWise.Devices;

namespace DeepWise.Test
{
    /// <summary>
    /// EventLogDemo.xaml 的互動邏輯
    /// </summary>
    [Group("系統"),DisplayName("初始化設備")]
    [Description("此範例展示如何讓物件透過IDevice介面進行初始化，並展示UI。")]
    public partial class DeviceInitializationDemo : Window
    {
        public DeviceInitializationDemo()
        {
            InitializeComponent();
            devicesView.Items.Add(Camera);
            devicesView.Items.Add(MotionController);
            devicesView.Items.Add(Dimmer);
        }

        

        Camera Camera { get; } = new CameraIDS() { Name="TopCamera(IDS)"};
        TestIniDevice MotionController { get; } = new TestIniDevice("MotionController", true,3500);
        TestIniDevice Dimmer { get; } = new TestIniDevice("Dimmer", false,500);
       
    }

    public class TestIniDevice : IDevice,INotifyPropertyChanged
    {
        public TestIniDevice(string name, bool result, int time)
        {
            this.time = time;
            this.Name = name;
            this.result = result;
        }
        public int time;
        public string Name { get; }
        public override string ToString()
        {
            return Name;
        }
        public bool result;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Initialize()
        {
            System.Threading.Thread.Sleep(time);
            IsOpened = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOpened"));
            return result;
        }
        public bool IsOpened { get; private set; } = false;
    }
}
