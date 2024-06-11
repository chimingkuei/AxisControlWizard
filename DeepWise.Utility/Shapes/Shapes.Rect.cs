using DeepWise.Localization;
using DeepWise.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DeepWise.Shapes
{
    [DebuggerDisplay("Location={Location}, Size={Size}")]
    [Serializable]
    [LocalizedDisplayName(nameof(Rect), typeof(Resources))]
    public struct Rect : IShape, IAreal, ISerializable
    {
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => Width * Height;
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter => 2 * (Width + Height);
        [LocalizedDisplayName(nameof(Location), typeof(Resources))]
        public Point Location { get; set; }
        [LocalizedDisplayName(nameof(Width), typeof(Resources))]
        public double Width { get; set; }
        [LocalizedDisplayName(nameof(Height), typeof(Resources))]
        public double Height { get; set; }

        [Browsable(false)]
        public Size Size
        {
            get => new Size(Width, Height);
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }
        [Browsable(false)]
        public double X
        {
            get => Location.X;
            set => Location = new Point(value, Y);
        }
        [Browsable(false)]
        public double Y
        {
            get => Location.Y;
            set => Location = new Point(X, value);
        }
        [Browsable(false)]
        public double Left => X;
        [Browsable(false)]
        public double Right => X + Width;
        [Browsable(false)]
        public double Top => Y;
        [Browsable(false)]
        public double Bottom => Y + Height;
        [Browsable(false)]
        public Segment TopEdge => new Segment(Left, Top, Right, Top);
        [Browsable(false)]
        public Segment RightEdge => new Segment(Right, Top, Right, Bottom);
        [Browsable(false)]
        public Segment BottomEdge => new Segment(Right, Bottom, Left, Bottom);
        [Browsable(false)]
        public Segment LeftEdge => new Segment(Left, Bottom, Left, Top);

        public bool Contains(Point p) => p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;
        public bool OnContour(Point p)
        {
            if (p.X == X || p.X == Right)
                return p.Y >= Y && p.Y <= Bottom;
            else if (p.Y == Y || p.Y == Bottom)
                return p.X >= X && p.X <= Right;
            else
                return false;
        }
        public static Rect FromDrawingRect(System.Drawing.Rectangle rect) => new Rect(rect.X - 0.5, rect.Y - 0.5, rect.Width, rect.Height);
        public static implicit operator RectRotatable(Rect rect) => new RectRotatable(rect.X, rect.Y, rect.Width, rect.Height, 0);
        public Rect Dilate(double r) => new Rect(Left - r, Top - r, Width + 2 * r, Height + 2 * r);
        public Rect Erode(double r) => new Rect(Left + r, Top + r, Width - 2 * r, Height - 2 * r);
        IShape IShape.Move(Vector offset) => new Rect(Location += offset, Size);
        IShape IShape.AppendOn(Point origin, double angle) => throw new Exception($"{nameof(Rect)}不支援旋轉變換");
        IShape IShape.RelatvieTo(Point origin, double angle) => throw new Exception($"{nameof(Rect)}不支援旋轉變換");

        public static Rect NaN => new Rect(double.NaN, double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Rect rect) => double.IsNaN(rect.X) || double.IsNaN(rect.Y) || double.IsNaN(rect.Width) || double.IsNaN(rect.Height);

        public override string ToString() => X + "," + Y + "," + Width + "," + Height;

        public static implicit operator Rect(System.Windows.Rect rect) => new Rect(rect.X, rect.Y, rect.Width, rect.Height);

        public static Rect Intersect(Rect r1, Rect r2)
        {
            var top = r1.Top> r2.Top? r1.Top : r2.Top;
            var bottom = r1.Bottom < r2.Bottom ? r1.Bottom : r2.Bottom;
            var left = r1.Left > r2.Left ? r1.Left : r2.Left;
            var right = r1.Right < r2.Right ? r1.Right : r2.Right;
            return (top < bottom && left < right) ? new Rect(left, top, right - left, bottom - top) : new Rect();
        }

        public static bool TryIntersect(Rect r1, Rect r2,out Rect result)
        {
            var top = r1.Top > r2.Top ? r1.Top : r2.Top;
            var bottom = r1.Bottom < r2.Bottom ? r1.Bottom : r2.Bottom;
            var left = r1.Left > r2.Left ? r1.Left : r2.Left;
            var right = r1.Right < r2.Right ? r1.Right : r2.Right;
            if(top < bottom && left < right)
            {
                result = new Rect(left, top, right - left, bottom - top);
                return true;
            }
            else
            {
                result = new Rect();
                return false;
            }
        }
        public static Rect operator +(Rect rect, Vector v) => new Rect(rect.X + v.X, rect.Y + v.Y, rect.Width, rect.Height);
        public static Rect operator -(Rect rect, Vector v) => new Rect(rect.X - v.X, rect.Y - v.Y, rect.Width, rect.Height);
        public Rect(double x, double y, double width, double height)
        {
            if (width < 0 || height < 0) throw new ArgumentOutOfRangeException("\"Width\" or \"Height\" of rectangle can not less than zero");
            Location = new Point(x, y);
            Width = width;
            Height = height;
        }
        public Rect(Point location, Size size)
        {
            Location = location;
            Width = size.Width;
            Height = size.Height;
        }
        public Rect(SerializationInfo info, StreamingContext context)
        {
            Location = (Point)info.GetValue(nameof(Location), typeof(Point));
            Width = (double)info.GetValue(nameof(Width), typeof(double));
            Height = (double)info.GetValue(nameof(Height), typeof(double));
        }

        public static Rect FromBoundary(double left, double top,double right, double bottom)=> new Rect(left, top, right - left, bottom - top);

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Location), Location);
            info.AddValue(nameof(Width), Width);
            info.AddValue(nameof(Height), Height);
        }
    }
}