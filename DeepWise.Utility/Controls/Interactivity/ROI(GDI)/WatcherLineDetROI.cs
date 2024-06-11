using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;
using Size = DeepWise.Shapes.Size;

namespace DeepWise.Controls.Interactivity.ROI
{
    [Obsolete]
    public class WatcherLineDetROI : AnglurRectROIGDI 
    {
        public WatcherLineDetROI(Rect rect) : base(rect) { }
        public WatcherLineDetROI(double x, double y, double width, double height, double angle = 0) : base(x,y,width,height,angle) { }
        public WatcherLineDetROI(Point position, Size size, double angle, PositionType type = PositionType.Location) : base(position, size, angle, type) { }
        public WatcherLineDetROI(RectRotatable rect) : base(rect) { }
        public WatcherLineDetROI() { }
        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            if (!Fixed)
            {
                base.Paint(e);
                e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
                e.DrawArrow(lb, Angle + Math.PI / 2);
                e.DrawArrow(rb, Angle + Math.PI / 2);
            }
            else
            {
                e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
                e.Draw((RectRotatable)this);
                Vector right = Vector.FormAngle(Angle);
                Point p0 = lt + CurrentPosition * right;
                Point p1 = lb + CurrentPosition * right;
                e.Color = Color.Orange;
                e.DrawLine(p0, p1);
                e.DrawArrow(p1, Angle + Math.PI / 2);
            }
        }
        public double CurrentPosition
        {
            get
            {
                if (_currentPosition > Width) _currentPosition = Width;
                return _currentPosition;
            }
            set
            {
                if (value > Width) value = Width;
                else if (value < 0) value = 0;
                if((int)value!= (int)_currentPosition)
                {
                    Vector r = Right;
                    Point p0 = lt + r * (int)value;
                    Point p1 = lb + r * (int)value;
                    PositionChanged?.Invoke(this, new WatcherLineDetROIEventArgs(p0, p1));
                }
                _currentPosition = value;
            }
        }
        private double _currentPosition = 0;
        private enum Elements { Rect , Dial}
        private Elements Selected;
        public event EventHandler<WatcherLineDetROIEventArgs> PositionChanged;
        public override void OnMouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            if(System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                Fixed = !Fixed;
                return;
            }
            if (!Fixed) base.OnMouseDown(sender, e);
        }

        public override void OnMouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            if (!Fixed)
            {
                base.OnMouseMove(sender, e);
            }
            else
            {
                if(Selected == Elements.Dial)
                {
                    double position = (e.Location - lt) * Right /Width;
                    if (position < 0) CurrentPosition = 0;
                    else if (position > 1) CurrentPosition = Width;
                    else CurrentPosition = Width * position;
                }
            }
        }
        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            if(!Fixed)
                return base.IsMouseOver(e);
            else
            {
                Vector right = Vector.FormAngle(Angle);
                if (Focused&& Geometry.GetDistance(e.Location,new Segment(lt + CurrentPosition * right, lb + CurrentPosition * right),out _) <e.LineHitRadius )
                {
                    Selected = Elements.Dial;
                    return true;
                }
       
                if (((RectRotatable)this).Contains(e.Location))
                {
                    Selected = Elements.Rect;
                    return true;
                }
                return false;
            }
        }

        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                if(!Fixed)
                    return base.Cursor;
                else
                {
                    switch(Selected)
                    {
                        case Elements.Dial:
                            return System.Windows.Forms.Cursors.Hand;
                        case Elements.Rect:
                            return System.Windows.Forms.Cursors.SizeAll;
                    }
                }
                return null;
            }
        }

        public bool Fixed { get; set; } = false;

        public class WatcherLineDetROIEventArgs : EventArgs
        {
            public WatcherLineDetROIEventArgs(Point form,Point to)
            {
                Form = form;
                To = to;
            }
            public Point Form;
            public Point To;
        }
    }
}
