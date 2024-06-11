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
    /// DigitalInputPanel.xaml 的互動邏輯
    /// </summary>
    [Obsolete("控制項已經過時")]
    public partial class DIStatusLightsPanel : UserControl
    {
        public DIStatusLightsPanel()
        {
            InitializeComponent();
            DataContextChanged += DIStatusLightsPanel_DataContextChanged;    
        }

        private void DIStatusLightsPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.controller != null) this.controller.IOStatusChanged -= Cntlr_IOStatusChanged;

            if (DataContext is ADLINK_Motion cntlr)
            {
                this.controller = cntlr;
                if (indicators.Count == 0)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        var l = this.GetType().GetField("Input" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as StatusIndicator;
                        indicators.Add(i, l);
                        l.ToolTip = i.ToString();
                    }
                    if(controller.Model == ADLINK_DEVICE.DeviceName.AMP_20408C)
                    {
                        for (int i = 8; i < 24; i++)
                        {
                            var l = this.GetType().GetField("Input" + i, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as StatusIndicator;
                            indicators.Add(i, l);
                            l.ToolTip = i.ToString();
                        }
                    }
                }

                foreach (var item in indicators)
                    item.Value.LightColor = (cntlr.IsInitialized && cntlr.GetIOValue<bool>(new IOPortInfo(item.Key, IOTypes.Digital| IOTypes.Input)) ? Colors.Green : Colors.Gray);
                cntlr.IOStatusChanged += Cntlr_IOStatusChanged;
            }
            else
            {
                this.controller = null;
                foreach (var item in indicators)
                {
                    item.Value.LightColor = Colors.Gray;
                }
            }
        }

        private void Cntlr_IOStatusChanged(object sender, IOEventArgs e)
        {
            if(e.Type == (IOTypes.General | IOTypes.Digital | IOTypes.Input))
                Dispatcher.Invoke(() => indicators[e.Channel].LightColor = (bool)e.Value ? Colors.Green : Colors.Gray);
        }

        ADLINK_Motion controller;

        Dictionary<int, StatusIndicator> indicators = new Dictionary<int, StatusIndicator>();




    }
}
