using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MotionControllers.UI
{
    /// <summary>
    /// IOIndicator.xaml 的互動邏輯
    /// </summary>
    public partial class MotionIOStatusIndicator : UserControl
    {
        public MotionIOStatusIndicator()
        {
            InitializeComponent();
            this.ToolTip = GetDescription(MotionIOStatus);
            this.DataContextChanged += IOIndicator_DataContextChanged;
        }

        public static readonly DependencyProperty MotionIOStatusProperty = DependencyProperty.Register(nameof(MotionIOStatus), typeof(MotionIOStatus), typeof(MotionIOStatusIndicator), new PropertyMetadata(MotionIOStatus.SVON));
        public MotionIOStatus MotionIOStatus { get => (MotionIOStatus)GetValue(MotionIOStatusProperty); set => SetValue(MotionIOStatusProperty, value); }

        public static readonly DependencyProperty AxisIDProperty = DependencyProperty.Register(nameof(AxisID), typeof(int), typeof(MotionIOStatusIndicator), new PropertyMetadata(-1));
        public int AxisID { get => (int)GetValue(AxisIDProperty); set => SetValue(AxisIDProperty, value); }

        [Obsolete("Use AxisID instead.")]
        public int Axis { get => AxisID; set => AxisID =value; }



        ADLINK_Motion controller;
        private void IOIndicator_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(controller !=null)
            {
                controller.MotionIOStatusChanged -= StatusChanged;
            }

            if (DataContext is ADLINK_Motion cntlr)
            {
                controller = cntlr;
                if (cntlr.IsInitialized && AxisID != -1)
                    indicator.LightColor = controller.GetMotionIOStatus(AxisID, MotionIOStatus) ? Colors.Red : Colors.DarkRed;
                else
                    indicator.LightColor = Colors.Gray;
                cntlr.MotionIOStatusChanged += StatusChanged; 
            }
            else
                indicator.LightColor = Colors.Gray;
            this.ToolTip = GetDescription(MotionIOStatus);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (controller != null)
                switch (e.Property.Name)
                {
                    case nameof(MotionIOStatus):
                    case nameof(AxisID):
                        if (AxisID != -1)
                            indicator.LightColor = controller.GetMotionIOStatus(AxisID, MotionIOStatus) ? Colors.Red : Colors.DarkRed;
                        else
                            indicator.LightColor = Colors.Gray;
                        this.ToolTip = GetDescription(MotionIOStatus);
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

        private void StatusChanged(object sender, MotionIOStatusEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.AxisID == AxisID && e.Status == MotionIOStatus)
                {
                    indicator.LightColor = e.Value ? Colors.Red : Colors.DarkRed;
                }
            });
        }
    }

}
