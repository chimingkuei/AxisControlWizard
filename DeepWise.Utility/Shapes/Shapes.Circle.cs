using DeepWise.Localization;
using DeepWise.Properties;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DeepWise.Shapes
{
    [Serializable]
    [DebuggerDisplay("Center={Center}, Radius={Radius}")]
    [LocalizedDisplayName(nameof(Circle), typeof(Resources))]
    public struct Circle : IShape, IAreal, ISerializable
    {
        [LocalizedDisplayName(nameof(Center), typeof(Resources))]
        public Point Center { get; set; }
        [LocalizedDisplayName(nameof(Radius), typeof(Resources))]
        public double Radius { get; set; }
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => Radius * Radius * Math.PI;
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter => 2 * Math.PI * Radius;

        public bool Contains(Point p) => (p - Center).LengthSquared <= Radius * Radius;
        public bool OnContour(Point p) => Geometry.IsZero((p - Center).Length);

        public static bool IsNaN(Circle circle) => Point.IsNaN(circle.Center) || double.IsNaN(circle.Radius);
        public static bool IsInfinity(Circle circle, int e)
        {
            double value = Math.Pow(10, e);
            return
                Math.Abs(circle.Radius) > value ||
                Math.Abs(circle.Center.X) > value ||
                Math.Abs(circle.Center.Y) > value;
        }

        IShape IShape.Move(Vector offset) => new Circle(Center += offset, Radius);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public Circle RelativeTo(Point origin, double angle)
        {
            Circle cricle = this;
            cricle.Center = cricle.Center.RelativeTo(origin, angle);
            return cricle;
        }
        public Circle AppendOn(Point origin, double angle)
        {
            Circle cricle = this;
            cricle.Center = cricle.Center.AppendOn(origin, angle);
            return cricle;
        }
        public Circle(double x, double y, double r) { Center = new Point(x, y); Radius = r; }
        public Circle(Point center, double r) { Center = center; Radius = r; }
        public Circle(Point p0, Point p1, Point p2)
        {
            Line a = new Line(p1.X - p0.X, p1.Y - p0.Y, (p0.X * p0.X - p1.X * p1.X + p0.Y * p0.Y - p1.Y * p1.Y) / 2);
            Line b = new Line(p2.X - p0.X, p2.Y - p0.Y, (p0.X * p0.X - p2.X * p2.X + p0.Y * p0.Y - p2.Y * p2.Y) / 2);
            if (Geometry.Intersect(a, b, out Point[] center))
            {
                Center = center[0];
                Radius = (p0 - center[0]).Length;
            }
            else
            {
                Center = Point.NaN;
                Radius = double.NaN;
            }
        }
        public Circle(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            Center = (Point)info.GetValue(nameof(Center), typeof(Point));
            Radius = (double)info.GetValue(nameof(Radius), typeof(double));
        }
        public static Circle NaN => new Circle(double.NaN, double.NaN, double.NaN);

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Center), Center);
            info.AddValue(nameof(Radius), Radius);
        }
        public override string ToString() => Center.X + "," + Center.Y + "," + Radius;

        public override bool Equals(object obj)
        {
            return obj is Circle circle && Equals(circle);
        }

        public bool Equals(Circle other)
        {
            return Center == other.Center &&
                   Radius == other.Radius;
        }

        public override int GetHashCode()
        {
            var hashCode = 1226546590;
            hashCode = hashCode * -1521134295 + Center.GetHashCode();
            hashCode = hashCode * -1521134295 + Radius.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Circle a, Circle b) => a.Equals(b);
        public static bool operator !=(Circle a, Circle b) => !a.Equals(b);

        public System.Drawing.RectangleF GetBoundingBox()
        {
            return new System.Drawing.RectangleF((float)(Center.X - Radius), (float)(Center.Y - Radius), (float)(2 * Radius), (float)(2 * Radius));
        }
    }
}
