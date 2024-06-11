using ADLINK_DEVICE;
using MotionControllers.Motion;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MotionControllers.UI
{
    /// <summary>
    /// IOIndicator.xaml 的互動邏輯
    /// </summary>
    public partial class MotionStatusIndicator : UserControl
    {
        public MotionStatusIndicator()
        {
            InitializeComponent();
            this.ToolTip = GetDescription(MotionStatus);
            this.DataContextChanged += IOIndicator_DataContextChanged;
        }

        public static readonly DependencyProperty MotionStatusProperty = DependencyProperty.Register(nameof(MotionStatus), typeof(MotionStatus), typeof(MotionStatusIndicator), new PropertyMetadata(MotionStatus.VM));
        public MotionStatus MotionStatus { get => (MotionStatus)GetValue(MotionStatusProperty); set => SetValue(MotionStatusProperty, value); }

        public DeviceName Model => controller != null ? controller.Model : DeviceName.NULL;

        public static readonly DependencyProperty AxisIDProperty = DependencyProperty.Register(nameof(AxisID), typeof(int), typeof(MotionStatusIndicator), new PropertyMetadata(-1));
        public int AxisID { get => (int)GetValue(AxisIDProperty); set => SetValue(AxisIDProperty, value); }

        [Obsolete("Use AxisID instead.")]
        public int Axis { get => AxisID; set => AxisID = value; }

        ADLINK_Motion controller;
        private void IOIndicator_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(controller !=null)
            {
                controller.MotionStatusChanged -= StatusChanged;
            }

            if (DataContext is ADLINK_Motion cntlr)
            {
                controller = cntlr;
                cntlr.MotionStatusChanged += StatusChanged; 
                if(controller.IsInitialized && AxisID!=-1)
                    indicator.LightColor = controller.GetMotionStatus(AxisID, MotionStatus) ? Color.FromRgb(0,255,0) : Colors.DarkGreen;
                else
                    indicator.LightColor = Colors.Gray;
            }
            else
                indicator.LightColor = Colors.Gray;
            this.ToolTip = GetDescription(MotionStatus);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (controller != null)
                switch (e.Property.Name)
                {
                    case nameof(MotionStatus):
                    case nameof(AxisID):
                        if (AxisID != -1)
                            indicator.LightColor = controller.GetMotionStatus(AxisID, MotionStatus) ? Color.FromRgb(0, 255, 0) : Colors.DarkGreen;
                        else
                            indicator.LightColor = Colors.Gray;
                        this.ToolTip = GetDescription(MotionStatus);
                        break;
                    default:
                        break;
                }

        }
        string GetDescription(Enum source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
        private void StatusChanged(object sender, MotionStatusEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.AxisID == AxisID && e.Status == MotionStatus)
                    indicator.LightColor = e.Value ? Color.FromRgb(0,255,0) : Colors.DarkGreen;
                
            });
        }
    }
}
