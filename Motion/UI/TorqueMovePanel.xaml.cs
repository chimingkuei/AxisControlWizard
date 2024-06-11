using MotionControllers.Motion;
using System;
using System.Windows;
using System.Windows.Controls;
using static ADLINK_DEVICE.APS168;
namespace MotionControllers.UI
{
    /// <summary>
    /// TorqueMovePanel.xaml 的互動邏輯
    /// </summary>
    public partial class TorqueMovePanel : UserControl
    {
        public TorqueMovePanel()
        {
            InitializeComponent();
            this.DataContextChanged += TorqueMovePanel_DataContextChanged;
        }

        int _axisID = 0;
        public int AxisID
        {
            get => _axisID;
            set
            {
                _axisID = value;
            }
        }

        private void TorqueMovePanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext is ADLINK_Motion cntlr)
            {
                controller = cntlr;
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
            APS_get_axis_param(AxisID, PRA_INIT_TRQ, ref value);
            TBX_UserTorque.Text = value.ToString();
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
                case nameof(BTN_Set_ActuallTorque):
                    break;
                case nameof(BTN_StopTime):
                    {
                        if (int.TryParse(TBX_StopTime.Text, out var value))
                            APS_set_axis_param(AxisID, PRA_TRQ_STP_TIME, value);
                        else
                            MessageBox.Show("格式錯誤");
                    }
                    break;
                case nameof(BTN_Set_UserTorque):
                    {
                        //if (int.TryParse(TBX_UserTorque.Text, out var value))
                        //    APS_set_axis_param(AxisID, PRA_INIT_TRQ, value);
                        //else
                        //    MessageBox.Show("格式錯誤");
                    }
                    break;
            }

        }
        const int PRA_INIT_TRQ = 768;
        const int PRA_TRQ_STP_TIME = 769;
        const ushort BySlopeInput = 0;
        const ushort ByReachTime = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            short targetTroque = 0;
            if (short.TryParse(TBX_UserTorque.Text, out targetTroque))
                APS_set_axis_param(AxisID, PRA_INIT_TRQ, targetTroque);
            else
            {
                MessageBox.Show("格式錯誤");
                return;
            }

            var tmp = new ADLINK_DEVICE.ASYNCALL();

            var crtTroque = 0;
            APS_get_actual_torque(AxisID, ref crtTroque);
            APS_set_axis_param(AxisID, PRA_INIT_TRQ, crtTroque);
            APS_torque_move(AxisID, targetTroque, 50, BySlopeInput, ref tmp);
        }
    }
}
