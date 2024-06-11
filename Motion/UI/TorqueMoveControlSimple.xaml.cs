using ADLINK_DEVICE;
using MotionControllers.Motion;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static ADLINK_DEVICE.APS168;
namespace MotionControllers.UI
{
    /// <summary>
    /// TorqueMovePanel.xaml 的互動邏輯
    /// </summary>
    public partial class TorqueMoveControlSimple : UserControl
    {
        public TorqueMoveControlSimple()
        {
            InitializeComponent();
            this.DataContextChanged += TorqueMovePanel_DataContextChanged;
            if (!DesignerProperties.GetIsInDesignMode(this))
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(60), DispatcherPriority.Normal, Tick, this.Dispatcher);
        }
        void Tick(object sender,EventArgs e)
        {
            int torque = 0;
            APS_get_actual_torque(AxisID, ref torque);
            TBX_ActualTorque.Text = torque.ToString();
            TBX_FeedbackPos.Text = controller.GetPosition(AxisID).ToString();
        }

        DispatcherTimer timer;
        int _axisID = 0;
        public int AxisID
        {
            get => _axisID;
            set
            {
                if (_axisID == value) return;
                _axisID = value;
                InitializeUI();
            }
        }

        private void TorqueMovePanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext is ADLINK_Motion cntlr)
            {
                controller = cntlr;
                InitializeUI();
            }
        }
        ADLINK_Motion controller;

        void InitializeUI(object sender = null, EventArgs e = null)
        {
            APS_get_command_control_mode(AxisID, out var mode);
            rdo_CSPMode.IsChecked = mode == CSP_Mode;
            rdo_CSTMode.IsChecked = mode == CST_Mode;
            int value = 0;
            APS_get_axis_param(AxisID, PRA_TRQ_STP_TIME, ref value);
            TBX_StopTime.Text = value.ToString();
        }

        const byte CSP_Mode = 0;
        const byte CST_Mode = 1;
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == rdo_CSPMode)
                APS_set_command_control_mode(AxisID, CSP_Mode);
            else if (sender == rdo_CSTMode)
                APS_set_command_control_mode(AxisID, CST_Mode);
        }
        public int Torque { get; set; } 
        private void BTN_Stop_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(BTN_Stop):
                    controller.StopMove(AxisID);
                    break;
                case nameof(BTN_EMG_Stop):
                    controller.StopMoveEmergency(AxisID);
                    break;
            }
        }
        const int PRA_INIT_TRQ = 768;
        const int PRA_TRQ_STP_TIME = 769;
        const ushort BySlopeInput = 1;
        const ushort ByReachTime = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            short targetTroque = 0;
            ulong slope = 0;
            int stpTime = 0;
            if (!short.TryParse(TBX_UserTorque.Text, out targetTroque))
            {
                MessageBox.Show("格式錯誤");
                return;
            }
            if (!ulong.TryParse(TBX_Slope.Text, out slope))
            {
                MessageBox.Show("格式錯誤");
                return;
            }
            if (!int.TryParse(TBX_StopTime.Text, out stpTime))
            {
                MessageBox.Show("格式錯誤");
                return;
            }

            APS_set_command_control_mode(AxisID, 1);
            var tmp = new ADLINK_DEVICE.ASYNCALL();
            var crtTroque = 0;
            APS_get_actual_torque(AxisID, ref crtTroque);
            APS_set_axis_param(AxisID, PRA_INIT_TRQ, crtTroque);
            APS_set_axis_param(AxisID, PRA_TRQ_STP_TIME, stpTime);
            APS_torque_move(AxisID, targetTroque, slope, RDO_Slope.IsChecked.Value ? BySlopeInput : ByReachTime, ref tmp);

            TBX_TargetTorque.Text = targetTroque.ToString();
        }

    }
}
