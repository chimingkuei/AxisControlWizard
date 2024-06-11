using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Geometry = DeepWise.Shapes.Geometry;
namespace DeepWise.Controls.Interactivity
{
    public class RotatableRectROI : InteractiveROI
    {
        public RotatableRectROI(RectRotatable rect) : this()
        {
            SetRegion(rect);
        }

        private RotatableRectROI()
        {
            Rect.RenderTransformOrigin = new System.Windows.Point(0,0);
            Rect.RenderTransform = rotateTransform;


            ShapeElements.Add(Rect);
            ShapeElements.Add(rotatePoint);


            foreach (var shape in ShapeElements)
            {
                shape.Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                shape.Fill = new SolidColorBrush(Color.FromArgb(80,0, 120, 215));
            }
        }
        
        #region UI
        void UpdatePosition(double zoomLevel)
        {
            Rect.SetLocation(lt.X, lt.Y);
            Rect.Width = Width;
            Rect.Height = Height;
            rotateTransform.Angle = Angle / Math.PI * 180;

            UpdateLayout(zoomLevel);

        }
        RotateTransform rotateTransform = new RotateTransform();
        public override void UpdateLayout(double zoomLevel)
        {
            var r = rotRadius / zoomLevel;
            var offset = rotOffset / zoomLevel;
            rotatePoint.Width = rotatePoint.Height = 2 * r;
            var p = this.r + Right * offset;
            p.X -= r;
            p.Y -= r;
            rotatePoint.SetLocation(p);
        }
        protected override void OnFocusedChanged()
        {
            base.OnFocusedChanged();
            rotatePoint.Visibility = Focused ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        System.Windows.Shapes.Ellipse rotatePoint = new System.Windows.Shapes.Ellipse() { Visibility = System.Windows.Visibility.Hidden, Width = 2 * rotRadius, Height= 2 * rotRadius };
        System.Windows.Shapes.Rectangle Rect = new System.Windows.Shapes.Rectangle();

        #endregion UI

        public string[] MenuItems;
        public void OnMenuItemsClicked(object sender, EventArgs e)
        {
            switch (((dynamic)sender).Text as string)
            {
                case "方向對調":
                    throw new NotImplementedException();
                    break;
            }
        }

        #region Data and Critical Points
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
        protected Segment RightEdge => new Segment(rt, rb);
        protected Segment LeftEdge => new Segment(lt, lb);
        protected const double  rotRadius = 5, rotOffset = 16;
        private enum Element
        {
            Block_R = 0b0001, Block_L = 0b0010, Block_B = 0b0100, Block_T = 0b1000,
            Block_RT = Block_R + Block_T, 
            Block_LB = Block_L + Block_B, 
            Block_RB = Block_R + Block_B, 
            Block_LT = Block_L + Block_T,
            Face = 16, RotateNode
        }
        #endregion

        public static explicit operator RectRotatable(RotatableRectROI roi) => new RectRotatable(roi.lt, new Size(roi.Width, roi.Height), roi.Angle);

        private Element selected;

        public override bool IsMouseOver(DisplayMouseEventArgs e)
        {
            var p = (DeepWise.Shapes.Point)e.Location;
            if (Focused)
            {
                var tmp = new Point(r.X + rotOffset / e.ZoomLevel * Math.Cos(Angle), r.Y + rotOffset / e.ZoomLevel * Math.Sin(Angle));
                if ((tmp - p).LengthSquared < rotRadius * rotRadius / e.ZoomLevel / e.ZoomLevel)
                {
                    selected = Element.RotateNode;
                    return true;
                }
                else if (e.IsMouseOver(lt))
                {
                    selected = Element.Block_LT;
                    return true;
                }
                else if (e.IsMouseOver(rt))
                {
                    selected = Element.Block_RT;
                    return true;
                }
                else if (e.IsMouseOver(rb))
                {
                    selected = Element.Block_RB;
                    return true;
                }
                else if (e.IsMouseOver(lb))
                {
                    selected = Element.Block_LB;
                    return true;
                }
                else if (e.IsMouseOver(RightEdge))
                {
                    selected = Element.Block_R;
                    return true;
                }
                else if (e.IsMouseOver(TopEdge))
                {
                    selected = Element.Block_T;
                    return true;
                }
                else if (e.IsMouseOver(LeftEdge))
                {
                    selected = Element.Block_L;
                    return true;
                }
                else if (e.IsMouseOver(BottomEdge))
                {
                    selected = Element.Block_B;
                    return true;
                }
        
            }


            if (((RectRotatable)this).Contains(e.Location))
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
            
                    default:
                        return Cursors.Hand;
                }
            }
        }
        protected Point anchor;
        public override void MouseDown(DisplayMouseEventArgs e)
        {

            switch (selected)
            {
                case Element.Block_RT: anchor = lb; break;
                case Element.Block_RB: anchor = lt; break;
                case Element.Block_LT: anchor = rb; break;
                case Element.Block_LB: anchor = rt; break;
                case Element.Block_R: anchor = l; break;
                case Element.Block_L: anchor = r; break;
                case Element.Block_T: anchor = b; break;
                case Element.Block_B: anchor = t; break;
                case Element.Face: anchor = Center; break;
            }
            base.MouseDown(e);
        }
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            base.MouseUp(e);
        }
        public override void MouseMove(DisplayMouseEventArgs e)
        {
            Point p = e.Location;
            switch (selected)
            {
                case Element.Face:
                    {
                        Center = p - (Vector)(Point)StartLocation + (Vector)anchor;
                        break;
                    }
                case Element.Block_R:
                case Element.Block_L:
                    {
                        //using points (Center, Center + Right) instead of (l,r) to ensure pair of points aren't the same
                        Geometry.GetDistance(p, new DeepWise.Shapes.Line(Center, Center + Right), out Point p1);
                        Center = (Point)(((Vector)anchor + (Vector)p1) / 2);
                        double w = (p1 - anchor) * Right;
                        Width = w > 0 ? w : -w;

                        if (selected == Element.Block_R && w < 0) selected = Element.Block_L;
                        else if (selected == Element.Block_L && w > 0) selected = Element.Block_R;
                        break;
                    }
                case Element.Block_B:
                case Element.Block_T:
                    {
                        //using points (Center, Center + Right) instead of (l,r) to ensure pair of points aren't the same
                        Geometry.GetDistance(p, new DeepWise.Shapes.Line(Center, Center + Down), out Point p1);
                        Center = (Point)(((Vector)anchor + (Vector)p1) / 2);
                        double h = (p1 - anchor) * Down;
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
                        Vector p0 = (Vector)anchor;
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
            UpdatePosition(e.ZoomLevel);
            base.MouseMove(e);
        }

        public override IShape GetRegion() => (RectRotatable)this;
        
        public override void SetRegion(IShape shape)
        {
            var rect = (DeepWise.Shapes.RectRotatable)shape;
            Size = rect.Size;
            Angle = rect.Angle;
            Location = rect.Location;
            UpdatePosition(1 / Rect.StrokeThickness);
        }

        public override void SetRegion(System.Windows.Point from, System.Windows.Point to)
        {
            Location = new Point(from.X < to.X ? from.X : to.X, from.Y < to.Y ? from.Y : to.Y);
            Width = Math.Abs(from.X - to.X);
            Height = Math.Abs(from.Y - to.Y);
            UpdatePosition(1 / Rect.StrokeThickness);
        }
    }
}
