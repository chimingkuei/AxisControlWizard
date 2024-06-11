
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeepWise.Controls.Interactivity
{
    public abstract class DisplayBehavior : IDisplayBehavior , INotifyPropertyChanged
    {
        [Browsable(false)]
        public object Tag { get; set; }
        public DisplayBehavior()
        {
            if(ShowButtons)
            {
                BTN_Checked = new Button() 
                { 
                    Content = new System.Windows.Controls.Image() {Source = Properties.Resources.icons8_check_mark_48.GetDisabledBitmap().ToBitmapSource() } ,
                    Background = System.Windows.SystemColors.ControlBrush,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Width = 48,
                    Height = 48,
                    IsEnabled = false,
                    Margin = new Thickness(0,0,48+20,10)
                };
                BTN_Checked.IsEnabledChanged += (s, e) =>
                {
                    var btn = s as Button;
                    if (btn.IsEnabled)
                        (s as Button).Content = new System.Windows.Controls.Image() { Source = Properties.Resources.icons8_check_mark_48.ToBitmapSource() };
                    else
                        (s as Button).Content = new System.Windows.Controls.Image() { Source = Properties.Resources.icons8_check_mark_48.GetDisabledBitmap().ToBitmapSource() };
                };
                BTN_Checked.Click += OnCheckedButtonClicked;
                BTN_Cancel = new Button()
                {
                    Content = new System.Windows.Controls.Image() { Source = Properties.Resources.icons8_cross_mark_48.ToBitmapSource() },
                    Background = System.Windows.SystemColors.ControlBrush,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Width = 48,
                    Height = 48,
                    Margin = new Thickness(0, 0, 10, 10)
                };
                BTN_Cancel.Click += OnCancelButtonClicked;
            }
        }
        public virtual void Enter(Display display)
        {
            Display = display;
            if(ShowButtons && (display as UserControl).Content is Grid grid)
            {
                grid.Children.Add(BTN_Checked);
                grid.Children.Add(BTN_Cancel);
            }
            Entered?.Invoke(this, EventArgs.Empty);
        }
        public virtual void Exist()
        {
            if (ShowButtons && (Display as UserControl).Content is Grid grid)
            {
                grid.Children.Remove(BTN_Checked);
                grid.Children.Remove(BTN_Cancel);
            }
            Existed?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler Entered;
        public event EventHandler Existed;
        [Browsable(false)]
        public bool UseCustomButtonEvent { get; set; } = false;
        public event EventHandler<CancelEventArgs> CheckedButtonClicked;
        public event EventHandler<CancelEventArgs> CancelButtonClicked;

        protected void NotifyPropertyChanged([CallerMemberName]string propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #region Buttons

        /// <summary>
        /// 顯示打勾以及取消的按鈕。
        /// </summary>
        [Browsable(false)]
        public virtual bool ShowButtons { get; } = false;

        ManualResetEvent resultEvent = new ManualResetEvent(false);
        public async Task<bool> WaitResult()
        {
            await Task.Run(resultEvent.WaitOne);
            return Result;
        }
        bool? _result;
        [Browsable(false)]
        public bool Result
        {
            get => _result.HasValue ? _result.Value : throw new Exception("尚未設置結果");
            set
            {
                if(resultEvent.WaitOne(0)) throw new Exception("結果已設置，無法更改其值");
                _result = value;
                resultEvent.Set();
            }
        }

        protected Button BTN_Checked { get; }
        protected Button BTN_Cancel { get; }
        protected virtual void OnCheckedButtonClicked(object sender, EventArgs e)
        {
            var args = new CancelEventArgs();
            CheckedButtonClicked?.Invoke(BTN_Checked, args);
            if (args.Cancel) return;
            if (!UseCustomButtonEvent) Result = true;
        }
        protected virtual void OnCancelButtonClicked(object sender, EventArgs e)
        {
            var args = new CancelEventArgs();
            CancelButtonClicked?.Invoke(BTN_Cancel, args);
            if (args.Cancel) return;
            if (!UseCustomButtonEvent) Result = false;
        }

        #endregion
        [Browsable(false)]
        public Display Display { get; private set; }

        public abstract void MouseDown(DisplayMouseEventArgs e);
        public abstract void MouseMove(DisplayMouseEventArgs e);
        public abstract void MouseUp(DisplayMouseEventArgs e);
    }

    /// <summary>
    /// 表示可以在控制項上與其進行互動。
    /// </summary>
    public interface IDisplayBehavior
    {
        void MouseDown(DisplayMouseEventArgs e);
        void MouseMove(DisplayMouseEventArgs e);
        void MouseUp(DisplayMouseEventArgs e);
        void Enter(Display display);
        void Exist();
    }
}
