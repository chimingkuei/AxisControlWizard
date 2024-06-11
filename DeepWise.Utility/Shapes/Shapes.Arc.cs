using DeepWise.Localization;
using DeepWise.Properties;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DeepWise.Shapes
{
    [Serializable]
    [DebuggerDisplay("Center={Center}, Radius={Radius}, SweepAngle={SweepAngle}")]
    [LocalizedDisplayName(nameof(Arc), typeof(Resources))]
    public struct Arc : IShape, ISerializable, IEquatable<Arc>
    {
        //Main Properties 
        [LocalizedDisplayName(nameof(Center), typeof(Resources))]
        public Point Center { get; set; }
        [LocalizedDisplayName(nameof(Radius), typeof(Resources))]
        public double Radius { get; set; }
        [LocalizedDisplayName(nameof(StartAngle), typeof(Resources))]
        public double StartAngle { get; set; }
        [LocalizedDisplayName(nameof(EndAngle), typeof(Resources))]
        public double EndAngle { get; set; }
        [LocalizedDisplayName(nameof(SweepAngle), typeof(Resources))]
        public double SweepAngle => Geometry.GetSweepAngle(StartAngle, EndAngle);

        //Methods
        public bool OnContour(Point p) => Geometry.IsZero((p - Center).Length) && Geometry.IsAngleInRange(Math.Atan2(p.Y - Center.Y, p.X - Center.X), StartAngle, EndAngle);

        IShape IShape.Move(Vector offset) => new Arc(Center += offset, Radius, StartAngle, EndAngle);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public Arc RelativeTo(Point origin, double angle)
        {
            Arc arc = this;
            arc.Center = Center.RelativeTo(origin, angle);
            arc.StartAngle = Geometry.GetSweepAngle(angle, arc.StartAngle);
            arc.EndAngle = Geometry.GetSweepAngle(angle, arc.EndAngle);
            return arc;
        }
        public Arc AppendOn(Point origin, double angle)
        {
            Arc arc = this;
            arc.Center = arc.Center.AppendOn(origin, angle);
            arc.StartAngle = Geometry.Arg(angle + arc.StartAngle);
            arc.EndAngle = Geometry.Arg(angle + arc.EndAngle);
            return arc;
        }

        public override string ToString() => Center.X + "," + Center.Y + "," + Radius + "," + StartAngle + "," + EndAngle;
        public static explicit operator Circle(Arc arc) => new Circle(arc.Center, arc.Radius);

        //Constructs
        public Arc(Point center, double radius, double strAngle, double endAngle) { Center = center; Radius = radius; StartAngle = strAngle; EndAngle = endAngle; }
        public Arc(double x, double y, double r, double strAngle, double endAngle) { Center = new Point(x, y); Radius = r; StartAngle = strAngle; EndAngle = endAngle; }
        public Arc(Point p0, Point p1, Point p2, bool clockwise = true)
        {
            Line a = new Line(p1.X - p0.X, p1.Y - p0.Y, (p0.X * p0.X - p1.X * p1.X + p0.Y * p0.Y - p1.Y * p1.Y) / 2);
            Line b = new Line(p2.X - p0.X, p2.Y - p0.Y, (p0.X * p0.X - p2.X * p2.X + p0.Y * p0.Y - p2.Y * p2.Y) / 2);
            if (Geometry.Intersect(a, b, out Point[] center))
            {
                Center = center[0];
                Radius = (p0 - center[0]).Length;
                StartAngle = (p0 - Center).Angle;
                EndAngle = (p2 - Center).Angle;
            }
            else
            {
                Center = Point.NaN;
                Radius = double.NaN;
                StartAngle = double.NaN;
                EndAngle = double.NaN;
            }
        }
        public Arc(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            Center = (Point)info.GetValue(nameof(Center), typeof(Point));
            Radius = (double)info.GetValue(nameof(Radius), typeof(double));
            StartAngle = (double)info.GetValue(nameof(StartAngle), typeof(double));
            EndAngle = (double)info.GetValue(nameof(EndAngle), typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Center), Center);
            info.AddValue(nameof(Radius), Radius);
            info.AddValue(nameof(StartAngle), StartAngle);
            info.AddValue(nameof(EndAngle), EndAngle);
        }

        public static Arc NaN => new Arc(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Arc arc) => Point.IsNaN(arc.Center) | double.IsNaN(arc.Radius) | double.IsNaN(arc.StartAngle) | double.IsNaN(arc.EndAngle);

        public override bool Equals(object obj)
        {
            return obj is Arc arc && Equals(arc);
        }

        public bool Equals(Arc other)
        {
            return Center == other.Center &&
                   Radius == other.Radius &&
                   StartAngle == other.StartAngle &&
                   EndAngle == other.EndAngle;
        }

        public override int GetHashCode()
        {
            var hashCode = 1840041826;
            hashCode = hashCode * -1521134295 + Center.GetHashCode();
            hashCode = hashCode * -1521134295 + Radius.GetHashCode();
            hashCode = hashCode * -1521134295 + StartAngle.GetHashCode();
            hashCode = hashCode * -1521134295 + EndAngle.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Arc a, Arc b) => a.Equals(b);
        public static bool operator !=(Arc a, Arc b) => !a.Equals(b);
    }
}
