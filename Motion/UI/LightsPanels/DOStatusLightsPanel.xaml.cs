using DeepWise.Controls;
using MotionControllers.Motion;
using System;
using System.Collections.Generic;
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
    /// DigitalOutputPanel.xaml 的互動邏輯
    /// </summary>
    [Obsolete("控制項已經過時")]
    public partial class DOStatusLightsPanel : UserControl
    {
        static Color activeColor = Colors.Green;
        static Color deactiveColor = Colors.Gray;

        public DOStatusLightsPanel()
        {
            InitializeComponent();
            DataContextChanged += DOStatusLightsPanel_DataContextChanged;
        }

        private void DOStatusLightsPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.controller != null) this.controller.IOStatusChanged -= Controller_IOStatusChanged;

            if (DataContext is ADLINK_Motion cntlr)
            {
                this.controller = cntlr;
                if (indicators.Count == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var l = this.GetType().GetField("Output" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as StatusIndicator;
                        indicators.Add(i, l);
                        l.ToolTip = i.ToString();
                    }
                    for (int i = 8; i < 24; i++)
                    {
                        var l = this.GetType().GetField("Output" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as StatusIndicator;
                        indicators.Add(i, l);
                        l.ToolTip = i.ToString();
                    }
                }

                foreach (var item in indicators)
                {
                    if (controller.Model != ADLINK_DEVICE.DeviceName.AMP_20408C && item.Key > 4) continue;
                    item.Value.LightColor = (cntlr.IsInitialized && cntlr.GetIOValue<bool>(new IOPortInfo(item.Key, IOTypes.Digital| IOTypes.Output)) ? Colors.Green : Colors.Gray);
                }
                this.controller.IOStatusChanged += Controller_IOStatusChanged;
            }
            else
            {
                this.controller = null;
                foreach (var item in indicators)
                    item.Value.LightColor = Colors.Gray;
            }
        }

        private void Controller_IOStatusChanged(object sender, IOEventArgs e)
        {
            if (controller != null && e.Type == (IOTypes.General | IOTypes.Digital | IOTypes.Output))
            {
                var l = this.GetType().GetField("Output" + e.Channel, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as StatusIndicator;
                Dispatcher.Invoke(() => l.LightColor = (bool)e.Value ? activeColor : deactiveColor);
            }
        }

        Dictionary<int, StatusIndicator> indicators = new Dictionary<int, StatusIndicator>();

        ADLINK_Motion controller;

        private void Output0_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(controller!=null)
            {
                if(controller.IsInitialized)
                {
                    var name = (sender as Control).Name;
                    int index = int.Parse(name.Replace("Output", ""));
                    var info = new IOPortInfo(index, IOTypes.Output | IOTypes.Digital);
                    controller.SetOutputValue(info, !controller.GetIOValue<bool>(info));
                }
            }
        }
    }
}
