using System;
using System.Drawing;
using System.Windows.Forms;
using Point = DeepWise.Shapes.Point;

namespace DeepWise.Controls.Interactivity
{
    /// <summary>
    /// 表示可在控制項<see cref="DisplayGDI"/>中進行互動的物件。
    /// </summary>
    [Serializable]
    public abstract partial class InteractiveObjectGDI 
    {

        internal const double resizeBlockSize = 8;
        public object Tag { get; set; } = null;
        public string Name { get; set; } = null;
        public bool Focused { get; internal set; } = false;
        public static Brush TransparentBrush = new SolidBrush(Color.Black);
        public virtual Cursor Cursor => null;
        public abstract void Paint(DisplayGDIPaintEventArgs e);
        public abstract bool IsMouseOver(DisplayGDIMouseEventArgs e);

        public virtual string ToolTips { get; } = null;

        public virtual string[] MenuItems { get; } = new string[0];
        public virtual void OnMenuItemsClicked(object sender,EventArgs e) { }

        public virtual void OnMouseMove(object sender, DisplayGDIMouseEventArgs e) 
        {
            OnPropertyChanged(sender);
            e.Cancel = true;
        }
        public virtual void OnMouseUp(object sender, DisplayGDIMouseEventArgs e)
        {
            //doesn't need other judgement cause Cause OnMouseUp only invoke when roi object selected
            if (e.Button == MouseButtons.Left)
            {
                DisplayGDI display = sender as DisplayGDI;
                OnPropertyChanged(sender);
                ValueChanged?.Invoke(this, EventArgs.Empty);
                display.DragginItem = Dragging= false;
            }
        }
        public virtual void OnMouseDown(object sender, DisplayGDIMouseEventArgs e) 
        {
            if(e.Button == MouseButtons.Left)
            {
                MouseDownPosition = e.Location;
                DisplayGDI display = sender as DisplayGDI;
                display.DragginItem = Dragging = true;
            }
        }

        internal void BeginDrag(DisplayGDI display,Point location)
        {
            MouseDownPosition = location;
            display.DragginItem = Dragging = true;
        }

        public void Focus(DisplayGDI display)
        {
            if (display.ActiveObject == this) return;
            if (display.ActiveObject != null && display.ActiveObject != this)
                display.ActiveObject.Focused = false;
            display.ActiveObject = this;
            Focused = true;
        }

      





        public bool Dragging { get; private set; } = false;
        public Point MouseDownPosition { get; private set; }

        public event EventHandler PropertyChanged;
        public event EventHandler ValueChanged;
        protected void OnPropertyChanged(object display)
        {
            //DZ.Measurement.ROIAttribute.SetShapeValue(this, new Measurement.ROIAttribute.ROIEventEventArgs(display));
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    
}
