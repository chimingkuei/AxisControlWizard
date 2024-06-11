using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class RingROIGDI : InteractiveROIGDI
    {
        public static explicit operator Ring(RingROIGDI roi) => new Ring(roi.Center.X, roi.Center.Y, roi.StartRadius, roi.EndRadius);
        public override IShape GetShape() => (Ring)this;
        public override void SetShape(IShape shape)
        {
            if (shape is Ring ring)
            {
                Center = ring.Center;
                StartRadius = ring.StartRadius;
                EndRadius = ring.EndRadius;
            }
            else
                throw new Exception("shape Ring");
        }
        public override void SetShape(Point str, Point end)
        {
            var v = end - str;
            Center = str + v / 2;
            var r = v.Length;
            if (r > 0)
            {
                StartRadius = r / 4;
                EndRadius = r / 2;
            }
        }
        public RingROIGDI(Point center, double radius1, double radius2)
        {
            this.Center = center;
            this.StartRadius = radius1;
            this.EndRadius = radius2;
        }
        public RingROIGDI(double x, double y, double radius1, double radius2)
        {
            this.Center = new Point(x, y);
            this.StartRadius = radius1;
            this.EndRadius = radius2;
        }
        public RingROIGDI(Ring ring)
        {
            this.Center = ring.Center;
            this.StartRadius = ring.StartRadius;
            this.EndRadius = ring.EndRadius;
        }
        public RingROIGDI() { }
        
        #region Properties
        public Point Center { get; set; } = new Point();
        public double StartRadius { get; set; } = 0;
        public double EndRadius { get; set; } = 0;
        public double LargerRadius => StartRadius > EndRadius ? StartRadius : EndRadius;
        public double SmallerRadius => StartRadius > EndRadius ? EndRadius : StartRadius;
        #endregion

        protected enum Element { Center, Circle_Str, Circle_End, Arrow, Face }
        protected Element selected;

        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            Pen pen = Focused ? ROIPens.Hightlight : ROIPens.Default;
            Brush brush = Focused ? ROIBrushes.HightlightSt : ROIBrushes.DefaultSt;

            using (GraphicsPath path = new GraphicsPath())
            {
                if (!StartRadius.IsZero())
                    path.AddArc(
                        new RectangleF(e.F(Center - new Vector(StartRadius, StartRadius)),
                        new SizeF(2 * (float)StartRadius * e.ZoomLevel, 2 * (float)StartRadius * e.ZoomLevel)),
                        0,360);
                if (!EndRadius.IsZero())
                    path.AddArc(
                    new RectangleF(e.F(Center - new Vector(EndRadius, EndRadius)),
                    new SizeF(2 * (float)EndRadius * e.ZoomLevel, 2 * (float)EndRadius * e.ZoomLevel)),
                    0,360);
                //path.CloseFigure();
                e.Graphics.FillPath(Focused ? ROIBrushes.HightlightSt : ROIBrushes.DefaultSt, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        
        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            if (Focused)
            {
                if (e.IsMouseOver(new Circle(Center, EndRadius)))
                {
                    selected = Element.Circle_End;
                    return true;
                }
                else if (e.IsMouseOver(new Circle(Center, StartRadius)))
                {
                    selected = Element.Circle_Str;
                    return true;
                }
            }

            if (((Ring)this).Contains(e.Location))
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
                    case Element.Circle_Str:
                    case Element.Circle_End:
                        return Cursors.Hand;
                    case Element.Face:
                        return Cursors.SizeAll;
                    default:
                        return null;
                }
            }
        }
        private Point tmpCenter;
        public override void OnMouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            base.OnMouseDown(sender, e);
            tmpCenter = Center;
        }
        public override void OnMouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            Point p = e.Location;
            switch (selected)
            {
                case Element.Face:
                    {
                        Center = p - (Vector)MouseDownPosition + (Vector)tmpCenter;
                        break;
                    }
                case Element.Center:
                    {
                        Center = p;
                        break;
                    }
                case Element.Circle_Str:
                    {
                        double r = Math.Sqrt((p.X - Center.X) * (p.X - Center.X) + (p.Y - Center.Y) * (p.Y - Center.Y));
                        if (r < 1) r = 1;
                        StartRadius = r;
                        break;
                    }
                case Element.Circle_End:
                    {
                        double r = Math.Sqrt((p.X - Center.X) * (p.X - Center.X) + (p.Y - Center.Y) * (p.Y - Center.Y));
                        if (r < 1) r = 1;
                        EndRadius = r;
                        break;
                    }
            }
        }
        public IShape ToShape() => (Ring)this;
    }


}
