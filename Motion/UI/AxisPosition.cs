using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MotionControllers.UI
{
    public class AxisPosition : TextBlock
    {
        #region static

        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(TimeSpan), typeof(AxisPosition), new PropertyMetadata(TimeSpan.FromMilliseconds(60), IntervalChangedCallback));

        private static void IntervalChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AxisPosition wpfTimer = (AxisPosition)d;
            wpfTimer.timer.Interval = (TimeSpan)e.NewValue;
        }

        #endregion

        private void AxisPosition_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ADLINK_Motion cntlr)
                this.cntlr = cntlr;
            else
                this.cntlr = null;
        }
        ADLINK_Motion cntlr;


        private bool isUserVisible;

        private readonly DispatcherTimer timer;

        [Category("Common")]
        public TimeSpan Interval
        {
            get
            {
                return (TimeSpan)this.GetValue(IntervalProperty);
            }
            set
            {
                this.SetValue(IntervalProperty, value);
            }
        }

        public AxisPosition()
        {
            this.timer = new DispatcherTimer(this.Interval, DispatcherPriority.Normal, this.Timer_Tick, this.Dispatcher);
            //this.timer.IsEnabled = true;
            DataContextChanged += AxisPosition_DataContextChanged;
            LayoutUpdated += (s,e)=> timer.IsEnabled = this.IsUserVisible();
        }

        public static readonly DependencyProperty AxisIDProperty = DependencyProperty.Register(nameof(AxisID), typeof(int), typeof(AxisPosition));
        public int AxisID
        {
            get => (int)GetValue(AxisIDProperty);
            set => SetValue(AxisIDProperty, value);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AxisID != -1)
            {
                try
                {
                    Text = "POS : " + cntlr?.GetPosition(AxisID);
                }
                catch
                {
                    Text = "POS : ";
                    timer.IsEnabled = false;
                }
            }
            else
                Text = "";
        }
    }
}
