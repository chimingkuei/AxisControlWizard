using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;
using Size = DeepWise.Shapes.Size;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class PointDetectionROIGDI : InteractiveObjectGDI
    {
        public PointDetectionROIGDI(Point ori ,Point dest)
        {
            Origin = ori;
            Destination = dest;
        }

        public Point Origin { get; set; }
        public Point Destination { get; set; }
        public double Angle => Math.Atan2(Destination.Y - Origin.Y, Destination.X - Origin.X);

        private enum Element
        {
           Face,Origin,Destination
        }
        private Element selected;

        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            if (Focused)
            {
                if (e.IsMouseOver(Destination))
                {
                    selected = Element.Destination;
                    return true;
                }
                else if (e.IsMouseOver(Origin))
                {
                    selected = Element.Origin;
                    return true;
                }
            }

            if (e.IsMouseOver((Segment)this))
            {
                selected = Element.Face;
                return true;
            }
            return false;
        }
        Point p0, p1;
        public override void OnMouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            p0 = Origin;
            p1 = Destination;
            base.OnMouseDown(sender, e);
        }
        public override void OnMouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            switch (selected)
            {
                case Element.Face:
                    {
                        Vector v = e.Location - MouseDownPosition;
                        Origin = p0 + v;
                        Destination = p1 + v;
                        break;
                    }
                case Element.Origin:
                    {
                        Origin = e.Location;
                        break;
                    }
                case Element.Destination:
                    {
                        Destination = e.Location;
                        break;
                    }
            }
        }
        public override Cursor Cursor
        {
            get
            {
                switch (selected)
                {
                    case Element.Origin:
                    case Element.Destination:
                        return Cursors.Hand;
                    case Element.Face:
                        return Cursors.SizeAll;
                    default:
                        return null;
                }
            }
        }

        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
            {
                if(!Focused)
                {
                    e.DrawLine(Origin, Destination);
                    e.DrawArrow(Destination, Angle);
                }
                else
                {
                    e.DrawLine(Origin, Destination);
                    //e.FillCircle(TransparentBrush, new Circle(Origin, e.DraggingPointRadius));
                    e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    e.Color = Color.Transparent;
                    e.Fill(new Circle(Origin, e.DraggingPointRadius));
                    e.Fill(new Circle(Destination, e.DraggingPointRadius));
                    e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
                    e.Draw(new Circle(Origin, e.DraggingPointRadius));
                    e.Draw(new Circle(Destination, e.DraggingPointRadius));
                    e.DrawArrow(Destination - e.DraggingPointRadius * Vector.FormAngle(Angle), Angle);
                }
            }
        }

        public IShape ToShape() => (Segment)this;
        public static explicit operator Segment(PointDetectionROIGDI roi)=> new Segment(roi.Origin,roi.Destination);
    }
}
