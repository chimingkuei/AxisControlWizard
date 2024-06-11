using DeepWise.Shapes;
using System;
using Pen = System.Drawing.Pen;
using PointF = System.Drawing.PointF;

namespace DeepWise.Controls.Interactivity.ROI
{
    public class AnglurRectROIGDI : InteractiveROIGDI , IScannable
    {
        public static explicit operator RectRotatable(AnglurRectROIGDI roi) => new RectRotatable(roi.Location.X, roi.Location.Y, roi.Width, roi.Height, roi.Angle);
        public enum PositionType { Location, Center }
        public AnglurRectROIGDI(Rect rect)
        {
            Center = rect.Location + new Vector(rect.Width / 2, rect.Height / 2);
            Width = rect.Width;
            Height = rect.Height;
            Angle = 0;
        }
        public AnglurRectROIGDI(double x, double y, double width, double height, double angle = 0)
        {
            Size = new Size(width, height);
            Angle = angle;
            Location = new Point(x, y);
        }
        public AnglurRectROIGDI(Point position, Size size, double angle, PositionType type = PositionType.Location)
        {
            Size = size;
            Angle = angle;
            if (type == PositionType.Location) Location = position;
            else if (type == PositionType.Center) Center = position;
        }
        public AnglurRectROIGDI(RectRotatable rect)
        {
            Size = rect.Size;
            Angle = rect.Angle;
            Location = rect.Location;
        }
        public AnglurRectROIGDI() { }

        #region Properties
        public Point Location
        {
            get => new Point(lt.X, lt.Y);
            set => Center = value + new Vector(Width / 2, Height / 2).Rotate(Angle);
        }
        public Point Center { get; set; } = Point.Zero;
        public Size Size
        {
            get => new Size(Width, Height);
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;
        public double Angle { get; set; } = 0;
        #endregion

        protected Point rb => Center + new Vector(Width / 2, Height / 2).Rotate(Angle);
        protected Point lb => Center + new Vector(-Width / 2, Height / 2).Rotate(Angle);
        protected Point rt => Center + new Vector(Width / 2, -Height / 2).Rotate(Angle);
        protected Point lt => Center + new Vector(-Width / 2, -Height / 2).Rotate(Angle);
        protected Point r => Center + new Vector(Width / 2, 0).Rotate(Angle);
        protected Point l => Center + new Vector(-Width / 2, 0).Rotate(Angle);
        protected Point t => Center + new Vector(0, -Height / 2).Rotate(Angle);
        protected Point b => Center + new Vector(0, Height / 2).Rotate(Angle);
        protected Vector Right => new Vector(Math.Cos(Angle), Math.Sin(Angle));
        protected Vector Down => new Vector(Math.Cos(Angle + Math.PI / 2), Math.Sin(Angle + Math.PI / 2));
        protected Vector Up => -Down;
        protected Vector Left => -Right;
        protected Segment TopEdge => new Segment(lt, rt);
        protected Segment BottomEdge => new Segment(rb, lb);
        protected const double rnode_r1 = 3, rnode_r2 = 5, rnode_gap = 16;
        private enum Element
        {
            Block_R = 0b0001, Block_L = 0b0010, Block_B = 0b0100, Block_T = 0b1000,
            Block_RT = Block_R + Block_T, Block_LB = Block_L + Block_B, Block_RB = Block_R + Block_B, Block_LT = Block_L + Block_T,
            Face = 16, RotateNode
        }
        private Element selected;
        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            Pen pen = Focused ? ROIPens.Hightlight : ROIPens.Default;

            //Edges
            var rect = (RectRotatable)this;
            e.Color = (Focused ? ROIColors.Highlight : ROIColors.Default).MakeTransparent();
            e.Fill(rect);
            e.Color = Focused ? ROIColors.Highlight : ROIColors.Default;
            e.Draw(rect);
            if (!Focused) return;
            double gap = rnode_gap / e.ZoomLevel;
            Point rp = new Point(r.X + gap * Math.Cos(Angle), r.Y + gap * Math.Sin(Angle));
            e.DrawLine(r, rp);
            foreach (Point p in new Point[] { rp, r, l, t, b, lt, rt, rb, lb })
            {
                PointF pf = e.F(p);
                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                e.Fill(new Circle(p, e.DraggingPointRadius));
                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                e.Draw(new Circle(p, e.DraggingPointRadius));
            }

        }

        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                switch (selected)
                {
                    case Element.Face:
                        return System.Windows.Forms.Cursors.SizeAll;
                    case Element.RotateNode:
                    case Element.Block_L:
                    case Element.Block_T:
                    case Element.Block_B:
                    case Element.Block_R:
                    case Element.Block_LT:
                    case Element.Block_RT:
                    case Element.Block_RB:
                    case Element.Block_LB:
                        return System.Windows.Forms.Cursors.Hand;

                    default:
                        return null;
                }
            }
        }
        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            Point p = e.Location;
            Point tmp;

            if (Focused)
            {
                tmp = new Point(r.X + rnode_gap / e.ZoomLevel * Math.Cos(Angle), r.Y + rnode_gap / e.ZoomLevel * Math.Sin(Angle));
                if ((tmp - p).LengthSquared < rnode_r2 * rnode_r2 / e.ZoomLevel / e.ZoomLevel)
                {
                    selected = Element.RotateNode;
                    return true;
                }
                double radSquared = e.DraggingPointRadius * e.DraggingPointRadius;
                if ((r - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_R;
                    return true;
                }
                else if ((t - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_T;
                    return true;
                }
                else if ((l - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_L;
                    return true;
                }
                else if ((b - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_B;
                    return true;
                }
                else if ((lt - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_LT;
                    return true;
                }
                else if ((rt - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_RT;
                    return true;
                }
                else if ((rb - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_RB;
                    return true;
                }
                else if ((lb - p).LengthSquared < radSquared)
                {
                    selected = Element.Block_LB;
                    return true;
                }
            }

            if (((RectRotatable)this).Contains(p))
            {
                selected = Element.Face;
                return true;
            }
            return false;
        }

        protected Point relativeP;
        public override void OnMouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            switch(selected)
            {
                case Element.Block_RT: relativeP = lb; break;
                case Element.Block_RB: relativeP = lt; break;
                case Element.Block_LT: relativeP = rb; break;
                case Element.Block_LB: relativeP = rt; break;
                case Element.Block_R: relativeP = l; break;
                case Element.Block_L: relativeP = r; break;
                case Element.Block_T: relativeP = b; break;
                case Element.Block_B: relativeP = t; break;
                case Element.Face: relativeP = Center; break;
            }
            base.OnMouseDown(sender, e);
        }
        public override void OnMouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            Point p = e.Location;
            switch (selected)
            {
                case Element.Face:
                    {
                        Center = p - (Vector)MouseDownPosition + (Vector)relativeP;
                        break;
                    }
                case Element.Block_R:
                case Element.Block_L:
                    {
                        //using points (Center, Center + Right) instead of (l,r) to ensure pair of points aren't the same
                        Geometry.GetDistance(p, new Line(Center, Center + Right), out Point p1);
                        Center = (Point)(((Vector)relativeP + (Vector)p1) / 2);
                        double w = (p1 - relativeP) * Right;
                        Width = w > 0 ? w : -w;

                        if (selected == Element.Block_R && w < 0) selected = Element.Block_L;
                        else if (selected == Element.Block_L && w > 0) selected = Element.Block_R;
                        break;
                    }
                case Element.Block_B:
                case Element.Block_T:
                    {
                        //using points (Center, Center + Right) instead of (l,r) to ensure pair of points aren't the same
                        Geometry.GetDistance(p, new Line(Center, Center + Down), out Point p1);
                        Center = (Point)(((Vector)relativeP + (Vector)p1) / 2);
                        double h = (p1 - relativeP) * Down;
                        Height = h > 0 ? h : -h;

                        if (selected == Element.Block_B && h < 0) Angle = Geometry.Arg(Angle + Math.PI);
                        else if (selected == Element.Block_T && h > 0) Angle = Geometry.Arg(Angle + Math.PI);
                        break;
                    }
                case Element.Block_RT:
                case Element.Block_RB:
                case Element.Block_LT:
                case Element.Block_LB:
                    {
                        Vector p0 = (Vector)relativeP;
                        Vector p1 = (Vector)p;
                        Center = (Point)((p0 + p1) / 2);
                        double w = (p1 - p0) * Right;
                        double h = (p1 - p0) * Down;
                        Width = w > 0 ? w : -w;
                        Height = h > 0 ? h : -h;

                        if ((selected.HasFlag(Element.Block_R)))
                        {
                            if (w < 0)
                            {
                                selected -= Element.Block_R;
                                selected |= Element.Block_L;
                                //Angle = MathExt.Angle.Arg(Angle + Math.PI);
                            }
                        }
                        else
                        {
                            if (w > 0)
                            {
                                selected -= Element.Block_L;
                                selected |= Element.Block_R;
                                //Angle = MathExt.Angle.Arg(Angle + Math.PI);
                            }
                        }
                        if ((selected.HasFlag(Element.Block_B)))
                        {
                            if (h < 0)
                            {
                                Angle = Geometry.Arg(Angle + Math.PI);
                            }
                        }
                        else
                        {
                            if (h > 0)
                            {
                                Angle = Geometry.Arg(Angle + Math.PI);
                            }
                        }

                        break;
                    }
                case Element.RotateNode:
                    {
                        Angle = Math.Atan2(p.Y - Center.Y, p.X - Center.X);
                        break;
                    }
            }
            base.OnMouseMove(sender, e);
        }

        public IShape ToShape() => (RectRotatable)this;

        public (Point from, Point to)[] GetLines() => ((RectRotatable)this as IScannable).GetLines();

        public override IShape GetShape() => (RectRotatable)this;

        public override void SetShape(IShape shape)
        {
            if (shape is RectRotatable rectA)
            {
                Size = rectA.Size;
                Angle = rectA.Angle;
                Location = rectA.Location;
            }
            else
                throw new ArgumentException(nameof(shape));
        }

        public override void SetShape(Point str, Point end)
        {
            Location = new Point(str.X < end.X ? str.X : end.X, str.Y < end.Y ? str.Y : end.Y);
            Width = Math.Abs(str.X - end.X);
            Height = Math.Abs(str.Y - end.Y);
        }
    }
}
