using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class NewArcDetectionROIGDI : InteractiveObjectGDI
    {
        public static explicit operator RingSector(NewArcDetectionROIGDI roi) => new RingSector(roi.Center.X, roi.Center.Y, roi.R_Str, roi.R_End, roi.StrAngle, roi.EndAngle);
        public NewArcDetectionROIGDI(Point p0, Point p1, Point p2, double thickness = 100)
        {
            cps[0] = p0;
            cps[1] = p1;
            cps[2] = p2;
            Thickness = 100;
            GetCircle();
        }
        public NewArcDetectionROIGDI(RingSector rsc)
        {
            if (R_Smaller < 0) throw new ArgumentOutOfRangeException("R1", "環的內徑不可小於0。");
            if (R_Larger < 0) throw new ArgumentOutOfRangeException("R2", "環的外徑不可小於0。");
            if (R_Smaller > R_Larger) throw new ArgumentOutOfRangeException("R1", "環的內徑不可大於外徑。");
            double r = (rsc.EndRadius+ rsc.StartRadius) / 2;
            cps[0] = rsc.Center + r * Vector.FormAngle(rsc.StartAngle);
            cps[1] = rsc.Center + r * Vector.FormAngle(Geometry.Arg(rsc.StartAngle + Geometry.GetSweepAngle(rsc.StartAngle, rsc.EndAngle) / 2));
            cps[2] = rsc.Center + r * Vector.FormAngle(rsc.EndAngle);
            GetCircle();
            Thickness = rsc.EndRadius - rsc.StartRadius;
        }
        public NewArcDetectionROIGDI()
        {
            
        }


        public double R_Str => clockwise ? R_Larger : R_Smaller;
        public double R_End => clockwise ? R_Smaller : R_Larger;
        public Point Center { get; private set; }

        public double HalfThickness { get; set; } = 20;
        public double Thickness
        {
            get => HalfThickness * 2;
            set => HalfThickness = value / 2;
            
        }

        public double R_Smaller => Radius > HalfThickness ? Radius - HalfThickness : 0;
        public double R_Larger => Radius + HalfThickness;
        private double Radius { get; set; }
        public double StrAngle { get; private set; } = 0;
        public double EndAngle { get; private set; } = 0;

        Point strP0 => R_Smaller > 0 ? Center+R_Smaller * Vector.FormAngle(StrAngle) : Center;
        Point strP1 => R_Larger > 0 ? Center+R_Larger * Vector.FormAngle(StrAngle) : Center;
        Point endP0 => R_Smaller > 0 ? Center+R_Smaller * Vector.FormAngle(EndAngle) : Center;
        Point endP1 => R_Larger > 0 ? Center+R_Larger*Vector.FormAngle( EndAngle) : Center;
        Segment StrEdge => new Segment(strP0, strP1);
        Segment EndEdge => new Segment(endP0, endP1);
        private enum Element { Face, P0, P1, P2, Center, Arc_R, Arc_L,Edge_Str,Edge_End }
        private Element selected;

        public bool IsRect { get; private set; }

        void GetCircle()
        {
            Circle c = Circle.NaN;
            if (Geometry.IsZero(Geometry.GetDistance(cps[1], new Segment(cps[0], cps[2]), out _)))
            {
                IsRect = true;
            }
            else
            {
                c = cps.GetRegressedCircle();
                IsRect = Circle.IsNaN(c) || Circle.IsInfinity(c, 10);
            }

            if (IsRect)
            {
                Point tmpP = new Point((cps[0].X + cps[2].X) / 2, (cps[0].Y + cps[2].Y) / 2);
                Vector v = (cps[2] - cps[0]).UnitVector;

                v.Rotate(-Math.PI / 2);
                v *= 2.5;
                tmpP += v;
                //Point[] ps = 
                c = new Point[] { cps[0], cps[2], tmpP }.GetRegressedCircle();


                Center = c.Center;
                Radius = c.Radius;
                StrAngle = (cps[0] - c.Center).Angle;
                double angMid = (tmpP- c.Center).Angle;
                EndAngle = (cps[2] - c.Center).Angle;

                if (!Geometry.IsAngleInRange(angMid, StrAngle, EndAngle))
                {
                    clockwise = false;
                    double tmp = StrAngle;
                    StrAngle = EndAngle;
                    EndAngle = tmp;
                }
                else
                    clockwise = true;
            }
            else
            {
                Center = c.Center;
                Radius = c.Radius;
                StrAngle = (cps[0] - c.Center).Angle;
                double angMid = (cps[1] - c.Center).Angle;
                EndAngle = (cps[2] - c.Center).Angle;

                if (!Geometry.IsAngleInRange(angMid, StrAngle, EndAngle))
                {
                    clockwise = false;
                    double tmp = StrAngle;
                    StrAngle = EndAngle;
                    EndAngle = tmp;
                }
                else
                    clockwise = true;
            }
        }
        protected bool clockwise { get; private set; }

        public Point[] cps = new Point[3];
        Point[] tmpP;
        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
            {
                GetCircle();
                if (!IsRect)
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        if (!R_Str.IsZero())
                            path.AddArc(
                                new RectangleF(e.F(Center - new Vector(R_Str, R_Str)),
                                new SizeF(2 * (float)R_Str * e.ZoomLevel, 2 * (float)R_Str * e.ZoomLevel)),
                                (float)(StrAngle / Math.PI * 180),
                                (float)(Geometry.GetSweepAngle(StrAngle, EndAngle) / Math.PI * 180));
                        path.AddLine(e.F(Center + R_Str * Vector.FormAngle(EndAngle)), e.F(Center + R_End * Vector.FormAngle(EndAngle)));
                        if (!R_End.IsZero())
                            path.AddArc(
                            new RectangleF(e.F(Center - new Vector(R_End, R_End)),
                            new SizeF(2 * (float)R_End * e.ZoomLevel, 2 * (float)R_End * e.ZoomLevel)),
                            (float)(EndAngle / Math.PI * 180),
                            -(float)(Geometry.GetSweepAngle(StrAngle, EndAngle) / Math.PI * 180));
                        path.CloseFigure();
                        e.Graphics.FillPath(Focused ? ROIBrushes.HightlightSt : ROIBrushes.DefaultSt, path);
                        e.Graphics.DrawPath(e.pen, path);
                    }

                    Point strP = clockwise ? strP1 : strP0; Point endP = clockwise ? strP0 : strP1;
                    e.DrawArrow( endP, (endP - strP).Angle);
                    strP = clockwise ? endP1 : endP0; endP = clockwise ? endP0 : endP1;
                    e.DrawArrow( endP, (endP - strP).Angle);
                    e.pen.DashPattern = new float[] { 5, 2 };
                    e.Draw(new Arc(Center, (R_Str + R_End) / 2, StrAngle, EndAngle));
                    e.pen.DashPattern = null;

                }
                else
                {
                    Vector up = (cps[2] - cps[0]).UnitVector;
                    up = up.Rotate(-Math.PI / 2) * HalfThickness;
                    if (Focused)
                    {
                        e.DrawLine( cps[0], cps[2]);
                    }
                    Point tmp1 = cps[0] + up;
                    Point tmp2 = cps[2] + up;
                    e.DrawLine( tmp1, tmp2);
                    tmp1 = cps[0] - up;
                    tmp2 = cps[2] - up;
                    e.DrawLine(tmp1, tmp2);

                    Point strp = cps[0] + up; Point endp = cps[0] - up;
                    e.DrawLine(strp, endp);
                    strp = cps[2] + up; endp = cps[2] - up;
                    e.DrawLine(strp, endp);

                    if (!Focused) return;
                    e.Color = Color.Transparent;
                    e.Fill(new Circle(cps[0], e.DraggingPointRadius));
                    e.Fill(new Circle(cps[1], e.DraggingPointRadius));
                    e.Fill(new Circle(cps[2], e.DraggingPointRadius));
                    e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
                    e.Draw(new Circle(cps[0], e.DraggingPointRadius));
                    e.Draw(new Circle(cps[1], e.DraggingPointRadius));
                    e.Draw(new Circle(cps[2], e.DraggingPointRadius));
                }

                if (Focused)
                    foreach (Point p in cps)
                    {
                        PointF pf = e.F(p);
                        e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        e.Color = Color.Transparent;
                        e.Fill(new Circle(p, e.DraggingPointRadius));
                        e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                        e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
                        e.Draw(new Circle(p, e.DraggingPointRadius));
                    }


            }
        }
        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left) { }
            Point p = e.Location;

            if (Focused)
            {
                double r = e.DraggingPointRadius * e.DraggingPointRadius;
                if ((p - cps[0]).LengthSquared < r)
                {
                    selected = Element.P0;
                    return true;
                }
                else if ((p - cps[2]).LengthSquared < r)
                {
                    selected = Element.P2;
                    return true;
                }
                else if ((p - cps[1]).LengthSquared < r)
                {
                    selected = Element.P1;
                    return true;
                }
                else if(e.IsMouseOver(StrEdge))
                {
                    selected = Element.Edge_Str;
                    return true;
                }
                else if(e.IsMouseOver(EndEdge))
                {
                    selected = Element.Edge_End;
                    return true;
                }

            }

            if (!IsRect)
            {
                double dst = (p - Center).Length;

                if (dst <= R_Larger + DisplayGDIMouseEventArgs.DefaultLineHitRadius / e.ZoomLevel && Geometry.IsAngleInRange(Math.Atan2(e.Y - Center.Y, e.X - Center.X), StrAngle, EndAngle))
                {
                    if (Focused)
                    {
                        if (dst >= R_Larger - DisplayGDIMouseEventArgs.DefaultLineHitRadius / e.ZoomLevel)
                        {
                            selected = clockwise ? Element.Arc_L : Element.Arc_R;
                            return true;
                        }
                        else if (dst <= R_Smaller + DisplayGDIMouseEventArgs.DefaultLineHitRadius / e.ZoomLevel && dst >= R_Smaller - DisplayGDIMouseEventArgs.DefaultLineHitRadius / e.ZoomLevel)
                        {
                            selected = clockwise ? Element.Arc_R : Element.Arc_L;
                            return true;
                        }
                    }

                    if (dst > R_Smaller - DisplayGDIMouseEventArgs.DefaultLineHitRadius / e.ZoomLevel)
                    {
                        selected = Element.Face;
                        return true;
                    }

                }
            }
            else
            {
                Vector right = cps[2] - cps[0];
                Vector up = right.Rotate(-Math.PI / 2); up.Normalize(); up = up * HalfThickness;
                RectRotatable rect = new RectRotatable(cps[0] + up, new DeepWise.Shapes.Size(right.Length, HalfThickness * 2), Math.Atan2(right.Y,right.X));
                if (rect.Contains(e.Location))
                {
                    selected = Element.Face;
                    return true;
                }
            }


            return false;
        }
        public override Cursor Cursor
        {
            get
            {
                switch (selected)
                {
                    case Element.P0:
                    case Element.P1:
                    case Element.P2:
                    case Element.Arc_R:
                    case Element.Arc_L:
                    case Element.Edge_End:
                    case Element.Edge_Str:
                        return Cursors.Hand;
                    case Element.Face:
                        return Cursors.SizeAll;
                    default:
                        return null;
                }
            }
        }
        public override void OnMouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            base.OnMouseDown(sender, e);
            if (selected == Element.Face) tmpP = new Point[] { cps[0], cps[1], cps[2] };
        }
        public override void OnMouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            Point p = e.Location;
            switch (selected)
            {
                case Element.Arc_R:
                    {
                        double d = (p - Center).Length;
                        HalfThickness = Math.Abs(d - Radius);
                        if ((clockwise && d > Radius) || (!clockwise && d < Radius))
                        {
                            Point tmp = cps[0];
                            cps[0] = cps[2];
                            cps[2] = tmp;
                            clockwise = !clockwise;
                        }
                        break;
                    }
                case Element.Arc_L:
                    {
                        double d = (p - Center).Length;
                        HalfThickness = Math.Abs(d - Radius);
                        if ((clockwise && d < Radius) || (!clockwise && d > Radius))
                        {
                            Point tmp = cps[0];
                            cps[0] = cps[2];
                            cps[2] = tmp;
                            clockwise = !clockwise;
                        }
                        break;
                    }
                case Element.Face:
                    {
                        Vector v = p - MouseDownPosition;
                        cps[0] = tmpP[0] + v;
                        cps[1] = tmpP[1] + v;
                        cps[2] = tmpP[2] + v;
                        
                        break;
                    }
                case Element.P0:
                    {
                        cps[0] = p;
                        break;
                    }
                case Element.P1:
                    {
                        cps[1] = p;
                        break;
                    }
                case Element.P2:
                    {
                        cps[2] = p;
                        break;
                    }
                case Element.Edge_Str:
                    {
                        //StrAngle = Math.Atan2(p.Y - Center.Y, p.X - Center.X);
                        Vector v = (p - Center).UnitVector;
                        cps[0] = Center + v * Radius;

                        break;
                    }
                case Element.Edge_End:
                    {
                        //EndAngle = Math.Atan2(p.Y - Center.Y, p.X - Center.X);
                        Vector v = (p - Center).UnitVector;
                        cps[2] = Center + v * Radius;
                        break;
                    }
            }
            GetCircle();
        }
        public override void OnMouseUp(object sender, DisplayGDIMouseEventArgs e)
        {
            base.OnMouseUp(sender, e);
            tmpP = null;
        }

        public IShape ToShape() => (RingSector)this;
    }
}

