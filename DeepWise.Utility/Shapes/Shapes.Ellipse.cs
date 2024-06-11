using DeepWise.Localization;
using DeepWise.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DeepWise.Shapes
{
    [Serializable]
    [DebuggerDisplay("Center={Center}, SemiMajorAxis={SemiMajorAxis}, SemiMinorAxis={SemiMinorAxis}")]
    [LocalizedDisplayName(nameof(Ellipse), typeof(Resources))]
    public struct Ellipse : IShape, IAreal, ISerializable
    {
        [LocalizedDisplayName(nameof(Center), typeof(Resources))]
        public Point Center { get; set; }
        [LocalizedDisplayName(nameof(SemiMajorAxis), typeof(Resources))]
        public double SemiMajorAxis { get; set; }
        [LocalizedDisplayName(nameof(SemiMinorAxis), typeof(Resources))]
        public double SemiMinorAxis { get; set; }
        [LocalizedDisplayName(nameof(Angle), typeof(Resources))]
        public double Angle { get; set; }
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => SemiMajorAxis * SemiMinorAxis * Math.PI;
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter
        {
            get
            {
                double h = ((SemiMajorAxis - SemiMinorAxis) * (SemiMajorAxis - SemiMinorAxis)) /
                        ((SemiMajorAxis + SemiMinorAxis) * (SemiMajorAxis + SemiMinorAxis));
                double sum = 0;
                double error = double.PositiveInfinity;
                int n = 0;
                while (!Geometry.IsZero(error))
                {
                    double hb = HalfBinomial(n);
                    error = hb * hb * Math.Pow(h, n);
                    if (double.IsNaN(error)) break;
                    sum += error;
                    n++;
                }

                return Math.PI * (SemiMajorAxis + SemiMinorAxis) * sum;

                double HalfBinomial(int x)
                {
                    if (x == 0) return 1;
                    double up = 0.5;
                    double total = 1;
                    for (int i = 0; i < x; i++)
                    {
                        total *= up;
                        up--;
                    }
                    return total / Factorial(x);
                }
                double Factorial(int x)
                {
                    if (x == 0) return 1;
                    int tmp = 1;
                    for (int i = x; i > 0; i--) tmp *= i;
                    return tmp;
                }
            }
        }

        public bool Contains(Point p)
        {
            var v = (p - Center).Rotate(-Angle);
            double a2 = SemiMajorAxis * SemiMajorAxis, b2 = SemiMinorAxis * SemiMinorAxis;
            return new Vector(v.X * v.X, v.Y * v.Y) * new Vector(b2, a2) < a2 * b2;
        }
        public bool OnContour(Point p)
        {
            var v = (p - Center).Rotate(-Angle);
            double a2 = SemiMajorAxis * SemiMajorAxis, b2 = SemiMinorAxis * SemiMinorAxis;
            return Geometry.IsZero(new Vector(v.X * v.X, v.Y * v.Y) * new Vector(b2, a2) - a2 * b2);
        }

        public static bool IsNaN(Ellipse ellipse) => Point.IsNaN(ellipse.Center) || double.IsNaN(ellipse.SemiMajorAxis) || double.IsNaN(ellipse.SemiMinorAxis);
        public static bool IsInfinity(Ellipse ellipse, int e)
        {
            double value = Math.Pow(10, e);
            return Math.Abs(ellipse.SemiMajorAxis) > value || Math.Abs(ellipse.SemiMinorAxis) > value || Math.Abs(ellipse.Center.X) > value || Math.Abs(ellipse.Center.Y) > value;
        }

        IShape IShape.Move(Vector offset) => new Ellipse(Center += offset, SemiMajorAxis, SemiMinorAxis, Angle);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public Ellipse RelativeTo(Point origin, double angle)
        {
            return new Ellipse((Point)(Center - origin).Rotate(-angle), SemiMajorAxis, SemiMinorAxis, Angle - angle);
        }
        public Ellipse AppendOn(Point origin, double angle)
        {
            return new Ellipse(origin + ((Vector)Center).Rotate(angle), SemiMajorAxis, SemiMinorAxis, Angle + angle);
        }
        public Ellipse(double x, double y, double major, double minor, double angle)
        {
            Center = new Point(x, y);
            SemiMajorAxis = major;
            SemiMinorAxis = minor;
            Angle = angle;
        }
        public Ellipse(Point center, double major, double minor, double angle)
        {
            Center = center;
            SemiMajorAxis = major;
            SemiMinorAxis = minor;
            Angle = angle;
        }
        public Ellipse(SerializationInfo info, StreamingContext context)
        {
            Center = (Point)info.GetValue(nameof(Center), typeof(Point));
            SemiMajorAxis = (double)info.GetValue(nameof(SemiMajorAxis), typeof(double));
            SemiMinorAxis = (double)info.GetValue(nameof(SemiMinorAxis), typeof(double));
            Angle = (double)info.GetValue(nameof(Angle), typeof(double));

        }
        public static Ellipse NaN => new Ellipse { Center = Point.NaN, SemiMajorAxis = double.NaN, SemiMinorAxis = double.NaN, Angle = double.NaN };

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Center), Center);
            info.AddValue(nameof(SemiMajorAxis), SemiMajorAxis);
            info.AddValue(nameof(SemiMinorAxis), SemiMinorAxis);
            info.AddValue(nameof(Angle), Angle);
        }
        public override string ToString() => $"Center={Center}, SemiMajorAxis={SemiMajorAxis}, SemiMinorAxis={SemiMinorAxis}";

        public override bool Equals(object obj)
        {
            return obj is Ellipse circle && Equals(circle);
        }

        public bool Equals(Ellipse other)
        {
            return Center == other.Center && SemiMajorAxis == other.SemiMajorAxis && SemiMinorAxis == other.SemiMinorAxis;
        }

        public override int GetHashCode()
        {
            var hashCode = 1226546590;
            hashCode *= -1521134295 + Center.GetHashCode();
            hashCode *= -1521134295 + SemiMajorAxis.GetHashCode();
            hashCode *= -1521134295 + SemiMinorAxis.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Ellipse a, Ellipse b) => a.Equals(b);
        public static bool operator !=(Ellipse a, Ellipse b) => !a.Equals(b);
    }
}
