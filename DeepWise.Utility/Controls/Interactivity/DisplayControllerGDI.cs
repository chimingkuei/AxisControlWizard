
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepWise.Controls.Interactivity
{
    public abstract class DisplayControllerGDI : IInteractiveGDI, IDisposable
    {
        public object Tag { get; set; } = null;

        public virtual void Enter(DisplayGDI display)
        {
            Display = display;
            if (ShowButtons)
            {
                Display.SuspendLayout();
                BTN_Checked = new Button()
                {
                    BackColor = SystemColors.ButtonFace,
                    Size = new System.Drawing.Size(56, 56),
                    
                    Location = (System.Drawing.Point)(Display.Size - new System.Drawing.Size(128, 64)),
                    Image = Properties.Resources.icons8_check_mark_48,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    FlatStyle = FlatStyle.Flat,
                    Name = "BTN_Checked",
                    //ToDo : Cancel this
                    Enabled = false,
                    //==================
                };
                BTN_Checked.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                BTN_Cancel = new Button()
                {
                    BackColor = SystemColors.ButtonFace,
                    Size = new System.Drawing.Size(56, 56),
                    Location = (System.Drawing.Point)(Display.Size - new System.Drawing.Size(64, 64)),
                    Image = Properties.Resources.icons8_cross_mark_48,
                    Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                    FlatStyle = FlatStyle.Flat,
                    Name = "BTN_Cancel",
                };
                BTN_Cancel.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                Display.Controls.Add(BTN_Checked);
                Display.Controls.Add(BTN_Cancel);
                var SetStyle = typeof(Control).GetMethod("SetStyle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                SetStyle.Invoke(BTN_Checked, new object[] { ControlStyles.Selectable, false });
                SetStyle.Invoke(BTN_Cancel, new object[] { ControlStyles.Selectable, false });
                BTN_Checked.Click += OnCheckedButtonClicked;
                BTN_Cancel.Click += OnCancelButtonClicked;
                Display.ResumeLayout();
            }
            waitButtonEvent = new ManualResetEvent(false);
            Started?.Invoke(this, EventArgs.Empty);
        }
        public virtual void Exist()
        {
            if (ShowButtons)
            {
                Display.SuspendLayout();
                BTN_Checked?.Dispose();
                BTN_Cancel?.Dispose();
                BTN_Checked = BTN_Cancel = null;
                Display.ResumeLayout();
            }
            Finished?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler Started;
        public event EventHandler Finished;
        event EventHandler<DisplayGDIControllerButtonEventArgs> _checkedButtonClicked, _cancelButtonClicked;
        public bool UseCustomButtonEvent { get; set; } = false;
        public event EventHandler<DisplayGDIControllerButtonEventArgs> CheckedButtonClicked
        {
            add
            {
                _checkedButtonClicked += value;
                UseCustomButtonEvent = true;
            }
            remove => _checkedButtonClicked -= value;
        }
        public event EventHandler<DisplayGDIControllerButtonEventArgs> CancelButtonClicked
        {
            add
            {
                _cancelButtonClicked += value;
                UseCustomButtonEvent = true;
            }
            remove => _cancelButtonClicked -= value;
        }

        #region Buttons

        /// <summary>
        /// 顯示打勾以及取消的按鈕。
        /// </summary>
        public virtual bool ShowButtons { get; } = false;
        ManualResetEvent waitButtonEvent;
        public async Task<bool> WaitResult()
        {
            await Task.Run(waitButtonEvent.WaitOne);
            return Result;
        }
        bool? _result;
        public bool Result
        {
            get => _result.HasValue ? _result.Value : throw new Exception("尚未設置結果");
            set
            {
                _result = value;
                waitButtonEvent.Set();
            }
        }

        protected Button BTN_Checked { get; private set; }
        protected Button BTN_Cancel { get; private set; }
        protected virtual void OnCheckedButtonClicked(object sender, EventArgs e)
        {
            if (!UseCustomButtonEvent) Result = true;
            _checkedButtonClicked?.Invoke(BTN_Checked, new DisplayGDIControllerButtonEventArgs(this));
        }
        protected virtual void OnCancelButtonClicked(object sender, EventArgs e)
        {
            if (!UseCustomButtonEvent) Result = false;
            _cancelButtonClicked?.Invoke(BTN_Cancel, new DisplayGDIControllerButtonEventArgs(this));
        }

        #endregion

        public DisplayGDI Display { get; private set; }

        public event EventHandler Disposed;
        public virtual void Dispose()
        {
            BTN_Checked?.Dispose();
            BTN_Cancel?.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public abstract void MouseDown(object sender, DisplayGDIMouseEventArgs e);
        public abstract void MouseMove(object sender, DisplayGDIMouseEventArgs e);
        public abstract void MouseUp(object sender, DisplayGDIMouseEventArgs e);
        public abstract void DrawOverlay(DisplayGDIPaintEventArgs e);
    }

    public class DisplayGDIControllerButtonEventArgs : EventArgs
    {
        DisplayControllerGDI controller;
        public DisplayGDIControllerButtonEventArgs(DisplayControllerGDI controller)
        {
            this.controller = controller;

        }
        public bool ButtonResult
        {
            get => controller.Result;
            set => controller.Result = value;
        }
    }

    /// <summary>
    /// 表示可以在控制項上與其進行互動。
    /// </summary>
    public interface IInteractiveGDI
    {
        void MouseDown(object sender, DisplayGDIMouseEventArgs e);
        void MouseMove(object sender, DisplayGDIMouseEventArgs e);
        void MouseUp(object sender, DisplayGDIMouseEventArgs e);
        void DrawOverlay(DisplayGDIPaintEventArgs e);
        void Enter(DisplayGDI display);
        void Exist();
    }
}
