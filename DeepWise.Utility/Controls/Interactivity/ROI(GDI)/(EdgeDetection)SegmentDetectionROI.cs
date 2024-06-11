using System;
using System.Drawing;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;
using Size = DeepWise.Shapes.Size;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class LineDetectionROIGDI : InteractiveObjectGDI , IScannable
    {
        
        public LineDetectionROIGDI(RectRotatable rect) : this()
        {
            lp = rect.Location + rect.Height / 2 * Vector.FormAngle(rect.Angle + Math.PI / 2);
            rp = lp + rect.Width * Vector.FormAngle(rect.Angle);
            _angle = rect.Angle;
            radius = rect.Height / 2;
        }
        public LineDetectionROIGDI(Point l, Point r, double radius) : this()
        {
            lp = l; rp = r; this.radius = radius;
        }
        public LineDetectionROIGDI() 
        {
            var rect = new RectRotatable(10, 10, 100, 50, 0);
            lp = rect.Location + rect.Height / 2 * Vector.FormAngle(rect.Angle + Math.PI / 2);
            rp = lp + rect.Width * Vector.FormAngle(rect.Angle);
            radius = rect.Height / 2;

        }

        public override string[] MenuItems => new string[] { "方向對調" };

        public override void OnMenuItemsClicked(object sender, EventArgs e)
        {
            switch(((dynamic)sender).Text as string)
            {
                case "方向對調":
                    var tmp = lp;
                    lp = rp;
                    rp = tmp;
                    break;
            }
        }




        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            //Edges
            e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
            e.Draw((RectRotatable)this);
            e.Color = (Focused ? ROIColors.Highlight : ROIColors.Default).MakeTransparent();
            e.Fill((RectRotatable)this);
            e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
            e.DrawArrow(lb, Angle + Math.PI / 2);
            e.DrawArrow(rb, Angle + Math.PI / 2);
            e.pen.DashPattern = new float[] { 5, 5 };
            e.DrawLine(lp, rp);
            e.pen.DashPattern = new float[] { 1};
            if (!Focused) return;
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            e.Color = Color.Transparent;
            e.Fill(new Circle(lp, e.DraggingPointRadius));
            e.Fill(new Circle(rp, e.DraggingPointRadius));
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
            e.Draw(new Circle(lp, e.DraggingPointRadius));
            e.Draw(new Circle(rp, e.DraggingPointRadius));
        }

        public Point lp, rp;
        double Angle
        {
            get
            {
                double angle = Math.Atan2(rp.Y - lp.Y, rp.X - lp.X);
                if(_angle !=angle)
                {
                    _angle = angle;
                    OnAngleChanged();
                }
                return angle;
            }
        }

        private void OnAngleChanged()
        {
            //SizeHCursor = CustomCursor.SizeLarget(Angle);
            //SizeVCursor = CustomCursor.SizeLarget(Angle +Math.PI/2);
        }
        //Cursor SizeHCursor = CustomCursor.SizeLarget(0);
        //Cursor SizeVCursor = CustomCursor.SizeLarget(Math.PI / 2);

        double _angle;
        Segment TopEdge => new Segment(lt, rt);
        Segment BottomEdge => new Segment(lb, rb);
        Segment LeftEdge => new Segment(lt, lb);
        Segment RightEdge => new Segment(rt, rb);
        Point lt => lp - radius * Vector.FormAngle(Angle + Math.PI / 2);
        Point rt => rp - radius * Vector.FormAngle(Angle + Math.PI / 2);
        Point lb => lp + radius * Vector.FormAngle(Angle + Math.PI / 2);
        Point rb => rp + radius * Vector.FormAngle(Angle + Math.PI / 2);
        Vector Right => Vector.FormAngle(Angle);
        Vector Left => Vector.FormAngle(Angle + Math.PI);
        double radius;
        public static explicit operator RectRotatable(LineDetectionROIGDI roi) => new RectRotatable(roi.lt, new Size((roi.rp - roi.lp).Length, roi.radius * 2), roi.Angle);
        private enum Element
        {
            None,LeftPoint,RightPoint,Edge_Top,Edge_Bottom,Edge_Right,Edge_Left,Face
        }
        private Element selected;

        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            if (Focused)
            {
                if (e.IsMouseOver(lp))
                {
                    selected = Element.LeftPoint;
                    return true;
                }
                else if (e.IsMouseOver(rp))
                {
                    selected = Element.RightPoint;
                    return true;
                }
                else if (e.IsMouseOver(TopEdge))
                {
                    selected = Element.Edge_Top;
                    return true;
                }
                else if (e.IsMouseOver(BottomEdge))
                {
                    selected = Element.Edge_Bottom;
                    return true;
                }
                else if (e.IsMouseOver(LeftEdge))
                {
                    selected = Element.Edge_Left;
                    return true;
                }
                else if (e.IsMouseOver(RightEdge))
                {
                    selected = Element.Edge_Right;
                    return true;
                }
            }

            if(((RectRotatable)this).Contains(e.Location))
            {
                selected = Element.Face;
                return true;
            }
            return false;
        }
        public override Cursor Cursor
        {
            get
            {
                switch (selected)
                {
                    case Element.Face: return Cursors.SizeAll;
                    case Element.Edge_Top:
                    case Element.Edge_Bottom:
                        //return SizeVCursor;
                    case Element.Edge_Left:
                    case Element.Edge_Right:
                        //return SizeHCursor;
                    default:
                        return Cursors.Hand;
                }

            }
        }
        protected Segment oriPos;
        public override void OnMouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            oriPos = new Segment(lp, rp);
            if (selected == Element.LeftPoint || selected == Element.RightPoint) Cursor.Hide();
            BeginDrag(sender as DisplayGDI,e.Location);
            //if(selected == Element.Edge_Left)
            //{
            //    (sender as Control).Cursor = CustomCursor.SizeLarget(Angle);
            //}

            base.OnMouseDown(sender, e);
        }
        public override void OnMouseUp(object sender, DisplayGDIMouseEventArgs e)
        {
            if (selected == Element.LeftPoint || selected == Element.RightPoint) Cursor.Show();
            base.OnMouseUp(sender, e);
        }
        public override void OnMouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            Point p = e.Location;
            switch (selected)
            {
                case Element.Face:
                    {
                        lp = p - (Vector)MouseDownPosition + (Vector)oriPos.P0;
                        rp = p - (Vector)MouseDownPosition + (Vector)oriPos.P1;
                        break;
                    }
                case Element.LeftPoint:
                    {
                        lp = e.Location;
                        break;
                    }
                case Element.RightPoint:
                    {
                        rp = e.Location;
                        break;
                    }
                case Element.Edge_Bottom:
                case Element.Edge_Top:
                    {
                        radius =  Vector.FormAngle(Angle + Math.PI / 2)* (e.Location - lp);
                        if (selected == Element.Edge_Top) radius = -radius;
                        if(radius<0)
                        {
                            Point tmp = lp;
                            lp = rp;
                            rp = tmp;
                            radius = -radius;
                        }
                        if (radius < 0.5) radius = 0.5;
                        break;
                    }
                case Element.Edge_Left:
                    {
                        double l = Left * (p - rp);
                        lp = rp + Left * l;
                        if(l<0)
                        {
                            Point tmp = lp;
                            lp = rp;
                            rp = tmp;
                            selected = Element.Edge_Right;
                        }
                    }
                    break;
                case Element.Edge_Right:
                    {
                        double l = Right * (p - lp);
                        rp = lp + Right * l;
                        if (l < 0)
                        {
                            Point tmp = lp;
                            lp = rp;
                            rp = tmp;
                            selected = Element.Edge_Left;
                        }
                    }
                    break;

            }
            base.OnMouseMove(sender, e);
        }

        public (Point from, Point to)[] GetLines()
        {
            return ((RectRotatable)this as IScannable).GetLines();
        }
    }
}
