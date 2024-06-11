using System;
using System.Drawing;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;
using Size = DeepWise.Shapes.Size;
namespace DeepWise.Controls.Interactivity.ROI
{
    public sealed class RectROIGDI : InteractiveROIGDI
    {
        public static explicit operator Rect(RectROIGDI roi) => new Rect(roi.Location.X, roi.Location.Y, roi.Width, roi.Height);
        public static explicit operator System.Drawing.Rectangle(RectROIGDI roi) => new System.Drawing.Rectangle((int)roi.Location.X, (int)roi.Location.Y, (int)roi.Width, (int)roi.Height);
        public override IShape GetShape() => (Rect)this;
        public override void SetShape(IShape shape)
        {
            if (shape is Rect rect)
            {
                Location = rect.Location;
                Width = rect.Width;
                Height = rect.Height;
            }
            else
                throw new Exception("shape Rect");
        }
        public override void SetShape(Point str, Point end)
        {
            Location = new Point(str.X < end.X ? str.X : end.X, str.Y < end.Y ? str.Y : end.Y);
            Width = Math.Abs(str.X - end.X);
            Height = Math.Abs(str.Y - end.Y);
        }
        public RectROIGDI(Point location, Size size)
        {
            this.Location = location;
            this.Size = size;
        }

        public RectROIGDI(Rect rect)
        {
            this.Location = rect.Location;
            this.Size = rect.Size;
        }
        public bool IsSizeFixed { get; set; } = false;
        public RectROIGDI(System.Drawing.Rectangle rect)
        {
            this.Location = new Point(rect.Left, rect.Top);
            this.Size = new Size(Width, Height);
        }

        public RectROIGDI()
        {

        }

        public RectROIGDI(double left, double top, double width, double height)
        {
            X = left;
            Y = top;
            Width = width;
            Height = height;
        }

        #region Properties
        public double Top => Y;
        public double Bottom => Y + Height;
        public double Left => X;
        public double Right => X + Width;
        public double Width { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Point Location
        {
            get => new Point(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public Size Size
        {
            get => new Size(Width, Height);
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
        public float MaxWidth { get; set; } = float.MaxValue;
        public float MinWidth { get; set; } = 1;
        public float MaxHeight { get; set; } = float.MaxValue;
        public float MinHeight { get; set; } = 1;
        #endregion
        public Size CanvasSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        #region Critical points
        Point rb => new Point(X + Width, Y + Height);
        Point lb => new Point(X, Y + Height);
        Point rt => new Point(X + Width, Y);
        Point lt => Location;
        Point Center
        {
            get => Location + 0.5 * (Vector)Size;
            set
            {
                if (Center == value) return;

                Point tmp = value - (Vector)Center + (Vector)Location;
                //Point tmp = new Point(X + value.X - center.X, Y + value.Y - center.Y);
                if (tmp.Y + Height >= CanvasSize.Height) tmp.Y = CanvasSize.Height - Height - 1;
                else if (tmp.Y < 0) tmp.Y = 0;
                if (tmp.X + Width >= CanvasSize.Width) tmp.X = CanvasSize.Width - Width - 1;
                else if (tmp.X < 0) tmp.X = 0;
                Location = tmp;
            }
        }
        #endregion

        public Point tmpCenter;
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
                case Element.Edge_Bottom:
                    {
                        double lenth = p.Y - Top;
                        if (lenth < MinHeight) lenth = MinHeight;
                        else if (Top + lenth >= CanvasSize.Height) lenth = CanvasSize.Height - Top - 1;
                        if (lenth > MaxHeight) lenth = MaxHeight;
                        Height = lenth;
                        break;
                    }
                case Element.Edge_Left:
                    {
                        double length = Right - p.X;
                        double r = Right;
                        if (length < MinWidth) length = MinWidth;
                        else if (Right - length < 0) length = Right;
                        if (length > MaxWidth) length = MaxWidth;
                        if (p.X + length == r)
                        {
                            X = p.X;
                            Width = length;
                        }
                        else
                        {
                            X = r - length;
                            Width = length;
                        }
                        break;
                    }
                case Element.Edge_Right:
                    {
                        double length = p.X - Left;
                        if (length < MinWidth) length = MinWidth;
                        if (length > MaxWidth) length = MaxWidth;
                        else if (Left + length >= CanvasSize.Width) length = CanvasSize.Width - Left - 1;
                        Width = length;
                        break;
                    }
                case Element.Edge_Top:
                    {
                        double length = Bottom - p.Y;
                        double b = Bottom;
                        if (length < MinHeight) length = MinHeight;
                        else if (Bottom - length < 0) length = Bottom;
                        if (length > MaxHeight) length = MaxHeight;
                        if (p.Y + length == b)
                        {
                            Y = p.Y;
                            Height = length;
                        }
                        else
                        {
                            Y = b - length;
                            Height = length;
                        }
                        break;
                    }
                case Element.Center:
                    {
                        Center = tmpCenter + (p - MouseDownPosition);
                        break;
                    }
            }
        }

        
        public override void Paint(DisplayGDIPaintEventArgs e)
        {
            e.pen = Focused ? ROIPens.Hightlight : ROIPens.Default;
            e.Graphics.FillRectangle(Focused ? ROIBrushes.HightlightSt : ROIBrushes.DefaultSt, new RectangleF(e.F(lt),new SizeF((float)Width * e.ZoomLevel, (float)Height* e.ZoomLevel)));
            e.Draw((Rect)this);
            
        }

        public override bool IsMouseOver(DisplayGDIMouseEventArgs e)
        {
            Point p = e.Location;
            if (!IsSizeFixed && Focused)
            {
                if (Geometry.GetDistance(p, new Segment(lt, lb), out _) <= e.LineHitRadius)//Left
                {
                    selected = Element.Edge_Left;
                    return true;
                }
                else if (Geometry.GetDistance(p, new Segment(lt, rt), out _) <= e.LineHitRadius)//Top
                {
                    selected = Element.Edge_Top;
                    return true;
                }
                else if (Geometry.GetDistance(p, new Segment(lb, rb), out _) <= e.LineHitRadius)//Bottom
                {
                    selected = Element.Edge_Bottom;
                    return true;
                }
                else if (Geometry.GetDistance(p, new Segment(rb, rt), out _) <= e.LineHitRadius)//Right
                {
                    selected = Element.Edge_Right;
                    return true;
                }
            }
            if (((Rect)this).Contains(p))
            {
                selected = Element.Center;
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
                    case Element.Edge_Left:
                    case Element.Edge_Right:
                        return Cursors.SizeWE;
                    case Element.Edge_Bottom:
                    case Element.Edge_Top:
                        return Cursors.SizeNS;
                    case Element.Center:
                        return Cursors.SizeAll;
                    default:
                        return null;
                }
            }
        }

        private enum Element { Center, Edge_Right, Edge_Left, Edge_Top, Edge_Bottom }
        private Element selected;

    }
    //public sealed class RectROI : InteractiveObject
    //{
    //    public static explicit operator Rect(RectROI roi) => throw new NotImplementedException();
    //    public RectROI(Rect rect)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public RectROI(Kuei.Point str,Kuei.Size size)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Paint(DisplayPaintEventArgs e)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override bool IsMouseOver(DisplayMouseEventArgs e)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}

