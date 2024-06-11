using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DeepWise.Controls
{
    public class DigitalClock : TextBlock
    {
        #region static
   
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(TimeSpan), typeof(DigitalClock), new PropertyMetadata(TimeSpan.FromSeconds(1), IntervalChangedCallback));
        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register("IsRunning", typeof(bool), typeof(DigitalClock), new PropertyMetadata(true, IsRunningChangedCallback));
        public static readonly DependencyProperty TimeFormatProperty = DependencyProperty.Register("TimeFormat", typeof(string), typeof(DigitalClock));

        private static void IntervalChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DigitalClock wpfTimer = (DigitalClock)d;
            wpfTimer.timer.Interval = (TimeSpan)e.NewValue;
        }

        private static void IsRunningChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DigitalClock wpfTimer = (DigitalClock)d;
            wpfTimer.timer.IsEnabled = (bool)e.NewValue;
        }

        #endregion

        private readonly DispatcherTimer timer;

        [Category("Common")]
        public TimeSpan Interval
        {
            get => (TimeSpan)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        [Category("Common")]
        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        [Category("Common")]
        public string TimeFormat
        {
            get => (string)GetValue(TimeFormatProperty);
            set => SetValue(TimeFormatProperty, value);
        }

        public DigitalClock()
        {
            this.timer = new DispatcherTimer(this.Interval, DispatcherPriority.Normal, this.Timer_Tick, this.Dispatcher);
            this.timer.IsEnabled = true;

            Text = DateTime.Now.ToString(TimeFormat);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            SetValue(TextProperty, DateTime.Now.ToString(TimeFormat));
        }
    }
}
