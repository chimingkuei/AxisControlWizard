using System;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class CircleDetectionROIGDI : RingROIGDI
    {
        public CircleDetectionROIGDI(Point center, double r_str, double r_end) : base(center, r_str, r_end) { }
        public CircleDetectionROIGDI(double x, double y, double r_str, double r_end) : base(x, y, r_str, r_end) { }
        public CircleDetectionROIGDI(Ring ring) : base(ring) { }
        public CircleDetectionROIGDI() { }

        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            base.Paint(e);
            e.Color = Focused ? ROIColors.Highlight: ROIColors.Default;
            e.DrawLine(Center + new Vector(StartRadius, 0), Center + new Vector(EndRadius, 0));
            if (EndRadius > StartRadius)
                e.DrawArrow(Center + new Vector(EndRadius, 0), 0);
            else
                e.DrawArrow(Center + new Vector(EndRadius, 0), Math.PI);
            e.pen.DashPattern = new float[] { 5, 5 };
            e.Draw(new Circle(Center, (StartRadius + EndRadius) / 2));
            e.pen.DashPattern = null;
        }
    }
}

