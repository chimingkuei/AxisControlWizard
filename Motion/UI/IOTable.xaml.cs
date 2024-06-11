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
    /// IOTable.xaml 的互動邏輯
    /// </summary>
    public partial class IOTable : UserControl
    {
        public IOTable()
        {
            InitializeComponent();
            this.DataContextChanged += IOTable_DataContextChanged;
        }

        private void IOTable_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is ADLINK_Motion cntlr)
            {
                switch(cntlr.Model)
                {
                    case ADLINK_DEVICE.DeviceName.AMP_20408C:
                    default:
                        tabControl.SelectedIndex = 0;
                        break;
                    case ADLINK_DEVICE.DeviceName.PCIE_8332:
                    case ADLINK_DEVICE.DeviceName.PCIE_8334:
                    case ADLINK_DEVICE.DeviceName.PCIE_8338:
                        tabControl.SelectedIndex = 1;
                        break;
                }
            }
        }
    }
}
