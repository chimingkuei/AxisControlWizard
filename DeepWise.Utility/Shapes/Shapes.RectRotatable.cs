using DeepWise.Localization;
using DeepWise.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DeepWise.Shapes
{
    [Serializable]
    [DebuggerDisplay("Location={Location}, Size={Size},Angle={Angle}")]
    [LocalizedDisplayName(nameof(RectRotatable), typeof(Resources))]
    public struct RectRotatable : IAreal, ISerializable, IEquatable<RectRotatable>, IScannable
    {
        //Main Properties
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter => 2 * (Width + Height);
        [LocalizedDisplayName(nameof(Location), typeof(Resources))]
        public Point Location { get; set; }
        [LocalizedDisplayName(nameof(Width), typeof(Resources))]
        public double Width { get; set; }
        [LocalizedDisplayName(nameof(Height), typeof(Resources))]
        public double Height { get; set; }
        [LocalizedDisplayName(nameof(Angle), typeof(Resources))]
        public double Angle { get; set; }
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => Width * Height;

        [Browsable(false)]
        public Point Center
        {
            get => Location + (Right + Down) / 2;
            set => Location = value + (Left + Up) / 2;
        }
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

        //Properties
        [Browsable(false)]
        public Segment TopEdge => new Segment(Location, Location + Right);
        [Browsable(false)]
        public Segment RightEdge => new Segment(Location + Right, Location + Right + Down);
        [Browsable(false)]
        public Segment BottomEdge => new Segment(Location + Right + Down, Location + Down);
        [Browsable(false)]
        public Segment LeftEdge => new Segment(Location + Down, Location);
        [Browsable(false)]
        public Point LeftTop => Location;
        [Browsable(false)]
        public Point RightTop => Location + Right;
        [Browsable(false)]
        public Point RightBottom => Location + Down + Right;
        [Browsable(false)]
        public Point LeftBottom => Location + Down;
        [Browsable(false)]
        public Vector Right => Width * Vector.FormAngle(Angle);
        [Browsable(false)]
        public Vector Left => -Right;
        [Browsable(false)]
        public Vector Down => Height * Vector.FormAngle(Angle + Math.PI / 2);
        [Browsable(false)]
        public Vector Up => -Down;

        //Methods
        public bool Contains(Point p)
        {
            Vector v = p - Location;

            double x = v * new Vector(Math.Cos(Angle), Math.Sin(Angle));
            double y = v * new Vector(Math.Cos(Angle + Math.PI / 2), Math.Sin(Angle + Math.PI / 2));
            return x <= Width && x >= 0 && y <= Height && y >= 0;
        }
        public bool OnContour(Point p) => TopEdge.OnContour(p) || RightEdge.OnContour(p) || BottomEdge.OnContour(p) || LeftEdge.OnContour(p);

        IShape IShape.Move(Vector offset) => new RectRotatable(Location += offset, Size, Angle);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public RectRotatable RelativeTo(Point origin, double angle)
        {
            RectRotatable rect = this;
            rect.Location = rect.Location.RelativeTo(origin, angle);
            rect.Angle = Geometry.GetSweepAngle(angle, rect.Angle);
            return rect;
        }
        public RectRotatable AppendOn(Point origin, double angle)
        {
            RectRotatable rect = this;
            rect.Location = rect.Location.AppendOn(origin, angle);
            rect.Angle = Geometry.Arg(angle + rect.Angle);
            return rect;
        }

        public override string ToString() => "Location = (" + X.ToString("0.#") + "," + Y.ToString("0.#") + "), Size =(" + Width.ToString("0.#") + "," + Height.ToString("0.#") + "), Angle = " + (Angle * 180 / Math.PI).ToString("0.#");
        //Constructor
        public RectRotatable(double x, double y, double width, double height, double angle) { Location = new Point(x, y); Width = width; Height = height; Angle = angle; }
        public RectRotatable(Point location, Size size, double angle, bool useCenter = false)
        {
            Width = size.Width; Height = size.Height; Angle = angle;
            if (useCenter)
            {
                Location = location + size.Width / 2 * Vector.FormAngle(Angle + Math.PI) + size.Height / 2 * Vector.FormAngle(Angle - Math.PI / 2);
            }
            else
            {
                Location = location;
            }
        }
        public RectRotatable(Point left, Point right, double radius)
        {
            Angle = (right - left).Angle;
            Location = left - radius * Vector.FormAngle(Angle + Math.PI / 2);
            Width = (right - left).Length;
            Height = radius * 2;
        }
        public RectRotatable(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            Location = (Point)info.GetValue(nameof(Location), typeof(Point));
            Width = (double)info.GetValue(nameof(Width), typeof(double));
            Height = (double)info.GetValue(nameof(Height), typeof(double));
            Angle = (double)info.GetValue(nameof(Angle), typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Location), Location);
            info.AddValue(nameof(Width), Width);
            info.AddValue(nameof(Height), Height);
            info.AddValue(nameof(Angle), Angle);
        }

        public static bool IsNaN(RectRotatable rect) => double.IsNaN(rect.Angle) || double.IsNaN(rect.Width) || double.IsNaN(rect.Height) | double.IsNaN(rect.X) | double.IsNaN(rect.Y);

        public override bool Equals(object obj)
        {
            return obj is RectRotatable a && Equals(a);
        }
        public bool Equals(RectRotatable other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Width == other.Width &&
                   Height == other.Height &&
                   Angle == other.Angle;
        }
        public override int GetHashCode()
        {
            var hashCode = 2120356756;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + Angle.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(RectRotatable a, RectRotatable b) => a.Equals(b);
        public static bool operator !=(RectRotatable a, RectRotatable b) => !a.Equals(b);

        (Point, Point)[] IScannable.GetLines()
        {
            (Point, Point)[] ps = new (Point, Point)[(int)Width + 1];
            var l = ps.Length;
            for (int i = 0; i < l; i++)
            {
                ps[i].Item1 = LeftTop + i * Vector.FormAngle(Angle);
                ps[i].Item2 = LeftBottom + i * Vector.FormAngle(Angle);
            }
            return ps;
        }
    }
}
