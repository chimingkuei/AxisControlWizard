using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class CircleROIGDI : InteractiveROIGDI
    {
        public static explicit operator Circle(CircleROIGDI roi) => new Circle(roi.Center.X, roi.Center.Y, roi.Radius);
        public override IShape GetShape() => (Circle)this;
        public override void SetShape(IShape shape)
        {
            if (shape is Circle ring)
            {
                Center = ring.Center;
                Radius = ring.Radius;
            }
            else
                throw new Exception("shape Ring");
        }
        public override void SetShape(Point str, Point end)
        {
            var v = end - str;
            Center = str + v / 2;
            var diameter = v.Length;
            if (diameter > 0)
            {
               
               Radius = diameter / 2;
            }
        }
        public CircleROIGDI(Point center, double radius1)
        {
            this.Center = center;
            this.Radius = radius1;
        }
        public CircleROIGDI(double x, double y, double radius1)
        {
            this.Center = new Point(x, y);
            this.Radius = radius1;
        }
        public CircleROIGDI(Circle ring)
        {
            this.Center = ring.Center;
            this.Radius = ring.Radius;
        }
        public CircleROIGDI() { }
        
        #region Properties
        public Point Center { get; set; } = new Point();
        public double Radius { get; set; } = 0;
        #endregion

        protected enum Element { Center, Circumference, Face }
        protected Element selected;

        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            Pen pen = Focused ? ROIPens.Hightlight : ROIPens.Default;
            Brush brush = Focused ? ROIBrushes.HightlightSt : ROIBrushes.DefaultSt;

            using (GraphicsPath path = new GraphicsPath())
            {
                if (!Radius.IsZero())
                    path.AddArc(
                        new RectangleF(e.F(Center - new Vector(Radius, Radius)),
                        new SizeF(2 * (float)Radius * e.ZoomLevel, 2 * (float)Radius * e.ZoomLevel)),
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
                if (e.IsMouseOver((Circle)this))
                {
                    selected = Element.Circumference;
                    return true;
                }
  
            }

            if (((Circle)this).Contains(e.Location))
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
                    case Element.Circumference:
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
                case Element.Circumference:
                    {
                        double r = Math.Sqrt((p.X - Center.X) * (p.X - Center.X) + (p.Y - Center.Y) * (p.Y - Center.Y));
                        if (r < 1) r = 1;
                        Radius = r;
                        break;
                    }
          
            }
        }
        public IShape ToShape() => (Circle)this;
    }


}
