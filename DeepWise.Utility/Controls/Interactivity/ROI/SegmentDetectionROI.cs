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

namespace DeepWise.Controls.Interactivity
{
    public class LineDetectionROI : InteractiveROI
    {
        public LineDetectionROI(RectRotatable rect) : this()
        {
            lp = rect.Location + rect.Height / 2 * Vector.FormAngle(rect.Angle + Math.PI / 2);
            rp = lp + rect.Width * Vector.FormAngle(rect.Angle);
            _angle = rect.Angle;
            radius = rect.Height / 2;
            UpdatePosition(1);
        }
        public LineDetectionROI(Point l, Point r, double radius) : this()
        {
            lp = l; rp = r; this.radius = radius;
            UpdatePosition(1);
        }
        private LineDetectionROI()
        {
            Rect.RenderTransformOrigin = new System.Windows.Point(0, 0);
            Rect.RenderTransform = rotateTransform;

            ShapeElements.Add(Rect);
            ShapeElements.Add(leftPoint);
            ShapeElements.Add(rightPoint);
            ShapeElements.Add(LeftMark);
            ShapeElements.Add(RightMark);
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
            Rect.Width = (rp - lp).Length;
            Rect.Height = radius * 2;
            rotateTransform.Angle = Angle / Math.PI * 180;

            UpdateLayout(zoomLevel);

        }
        RotateTransform rotateTransform = new RotateTransform();
        public override void UpdateLayout(double zoomLevel)
        {
            leftPoint.Width = leftPoint.Height = rightPoint.Width = rightPoint.Height = DisplayMouseEventArgs.DefaultDraggingPointRadius * 2 / zoomLevel;
            leftPoint.SetLocation(lp.X - leftPoint.Width / 2, lp.Y - leftPoint.Height / 2);
            rightPoint.SetLocation(rp.X - leftPoint.Width / 2, rp.Y - leftPoint.Height / 2);

            double arrowLength = 8 / zoomLevel;
            var dir = Angle - Math.PI / 2;
            var wl = lb + new Vector(0.5, 0.5) + arrowLength * Vector.FormAngle(dir + 0.643501109);
            var wr = lb + new Vector(0.5, 0.5) + arrowLength * Vector.FormAngle(dir - 0.643501109);
            Path orangePath = new Path();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = wl;
            LineSegment lineSegment1 = new LineSegment();
            lineSegment1.Point = lb;
            pathFigure.Segments.Add(lineSegment1);
            LineSegment lineSegment2 = new LineSegment();
            lineSegment2.Point = wr;
            pathFigure.Segments.Add(lineSegment2);
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures = new PathFigureCollection();
            pathGeometry.Figures.Add(pathFigure);
            LeftMark.Data = pathGeometry;

            var v = rp - lp;
            wl += v;
            wr += v;
            orangePath = new Path();
            pathFigure = new PathFigure();
            pathFigure.StartPoint = wl;
            lineSegment1 = new LineSegment();
            lineSegment1.Point = rb;
            pathFigure.Segments.Add(lineSegment1);
            lineSegment2 = new LineSegment();
            lineSegment2.Point = wr;
            pathFigure.Segments.Add(lineSegment2);
            pathGeometry = new PathGeometry();
            pathGeometry.Figures = new PathFigureCollection();
            pathGeometry.Figures.Add(pathFigure);
            RightMark.Data = pathGeometry;
        }
        protected override void OnFocusedChanged()
        {
            base.OnFocusedChanged();
            leftPoint.Visibility =
            rightPoint.Visibility =
            Focused ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            LeftMark.Fill = RightMark.Fill = null;
        }

        System.Windows.Shapes.Ellipse leftPoint = new System.Windows.Shapes.Ellipse() { Visibility = System.Windows.Visibility.Hidden };
        System.Windows.Shapes.Ellipse rightPoint = new System.Windows.Shapes.Ellipse() { Visibility = System.Windows.Visibility.Hidden };
        System.Windows.Shapes.Rectangle Rect = new System.Windows.Shapes.Rectangle();
        System.Windows.Shapes.Path LeftMark = new System.Windows.Shapes.Path();
        System.Windows.Shapes.Path RightMark = new System.Windows.Shapes.Path();
        #endregion UI

        public string[] MenuItems;
        public void OnMenuItemsClicked(object sender, EventArgs e)
        {
            switch (((dynamic)sender).Text as string)
            {
                case "方向對調":
                    var tmp = lp;
                    lp = rp;
                    rp = tmp;
                    break;
            }
        }

        public Point lp, rp;
        double Angle
        {
            get
            {
                double angle = Math.Atan2(rp.Y - lp.Y, rp.X - lp.X);
                if (_angle != angle)
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
        public static explicit operator RectRotatable(LineDetectionROI roi) => new RectRotatable(roi.lt, new Size((roi.rp - roi.lp).Length, roi.radius * 2), roi.Angle);
        private enum Element
        {
            None, LeftPoint, RightPoint, Edge_Top, Edge_Bottom, Edge_Right, Edge_Left, Face
        }
        private Element selected;

        public override bool IsMouseOver(DisplayMouseEventArgs e)
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
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            oriPos = new Segment(lp, rp);
            //BeginDrag(sender as DisplayGDI, e.Location);
            //if(selected == Element.Edge_Left)
            //{
            //    (sender as Control).Cursor = CustomCursor.SizeLarget(Angle);
            //}

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
                        lp = p - (Vector)(DeepWise.Shapes.Point)StartLocation + (Vector)oriPos.P0;
                        rp = p - (Vector)(DeepWise.Shapes.Point)StartLocation + (Vector)oriPos.P1;
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
                        radius = Vector.FormAngle(Angle + Math.PI / 2) * ((Point)e.Location - lp);
                        if (selected == Element.Edge_Top) radius = -radius;
                        if (radius < 0)
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
                        if (l < 0)
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
            UpdatePosition(e.ZoomLevel);
            base.MouseMove(e);
        }

        public override IShape GetRegion() => (RectRotatable)this;
        
        public override void SetRegion(IShape shape)
        {
            var rect = (DeepWise.Shapes.RectRotatable)shape;
            lp = rect.Location + rect.Height / 2 * Vector.FormAngle(rect.Angle + Math.PI / 2);
            rp = lp + rect.Width * Vector.FormAngle(rect.Angle);
            _angle = rect.Angle;
            radius = rect.Height / 2;
            UpdatePosition(1 / Rect.StrokeThickness);
        }

        public override void SetRegion(System.Windows.Point from, System.Windows.Point to)
        {
            lp = from;
            rp = to;
            var v = rp - lp;
            _angle = v.Angle;
            radius = Math.Min(v.Length / 4, 10.0);
        }
    }
}
