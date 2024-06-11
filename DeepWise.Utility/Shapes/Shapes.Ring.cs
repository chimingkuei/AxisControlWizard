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
    [DebuggerDisplay("Center={Center}, StartRadius={StartRadius},EndRadius={EndRadius}")]
    [LocalizedDisplayName(nameof(Ring), typeof(Resources))]
    public struct Ring : IShape, IAreal, ISerializable, IScannable, IEquatable<Ring>
    {
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => (EndRadius * EndRadius - StartRadius * StartRadius) * Math.PI;
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter => 2 * Math.PI * (StartRadius + EndRadius);
        [LocalizedDisplayName(nameof(Center), typeof(Resources))]
        public Point Center { get; set; }
        [LocalizedDisplayName(nameof(StartRadius), typeof(Resources))]
        public double StartRadius { get; set; }
        [LocalizedDisplayName(nameof(EndRadius), typeof(Resources))]
        public double EndRadius { get; set; }

        public static bool IsNaN(Ring ring) => Point.IsNaN(ring.Center) | double.IsNaN(ring.StartRadius) | double.IsNaN(ring.EndRadius);

        [Browsable(false)]
        public double LargeRadius => StartRadius > EndRadius ? StartRadius : EndRadius;
        [Browsable(false)]
        public double SmallRadius => StartRadius > EndRadius ? EndRadius : StartRadius;
        [Browsable(false)]
        public double X
        {
            get => Center.X;
            set => Center = new Point(value, Y);
        }
        [Browsable(false)]
        public double Y
        {
            get => Center.Y;
            set => Center = new Point(X, value);
        }

        public bool Contains(Point p)
        {
            double sqrd = (p - Center).LengthSquared;
            if (EndRadius > StartRadius)
                return sqrd <= EndRadius * EndRadius && sqrd >= StartRadius * StartRadius;
            else
                return sqrd <= StartRadius * StartRadius && sqrd >= EndRadius * EndRadius;

        }
        public bool OnContour(Point p) => new Circle(Center, StartRadius).OnContour(p) || new Circle(Center, EndRadius).OnContour(p);
        IShape IShape.Move(Vector offset) => new Ring(Center += offset, StartRadius, EndRadius);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => Append(origin, angle);
        public Ring RelativeTo(Point origin, double angle)
        {
            Ring cricle = this;
            cricle.Center = cricle.Center.RelativeTo(origin, angle);
            return cricle;
        }
        public Ring Append(Point origin, double angle)
        {
            Ring cricle = this;
            cricle.Center = cricle.Center.AppendOn(origin, angle);
            return cricle;
        }
        public Ring(double x, double y, double r1, double r2) { Center = new Point(x, y); StartRadius = r1; EndRadius = r2; }
        public Ring(Point center, double r1, double r2) { Center = center; StartRadius = r1; EndRadius = r2; }
        public Ring(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            Center = (Point)info.GetValue(nameof(Center), typeof(Point));
            StartRadius = (double)info.GetValue(nameof(StartRadius), typeof(double));
            EndRadius = (double)info.GetValue(nameof(EndRadius), typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Center), Center);
            info.AddValue(nameof(StartRadius), StartRadius);
            info.AddValue(nameof(EndRadius), EndRadius);
        }

        public override string ToString() => X + "," + Y + "," + StartRadius + "," + EndRadius;

        (Point from, Point to)[] IScannable.GetLines()
        {
            double length = 2 * Math.PI * LargeRadius;
            (Point, Point)[] lines = new (Point, Point)[(int)length];
            for (int i = 0; i < lines.Length; i++)
            {
                Vector v = Vector.FormAngle(i / LargeRadius);
                lines[i].Item1 = Center + StartRadius * v;
                lines[i].Item2 = Center + EndRadius * v;
            }
            return lines;
        }

        public override bool Equals(object obj)
        {
            return obj is Ring ring && Equals(ring);
        }

        public bool Equals(Ring other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   StartRadius == other.StartRadius &&
                   EndRadius == other.EndRadius;
        }

        public Circle GetOuterCircle() => new Circle(Center, LargeRadius);
        public Circle GetInnerCircle() => new Circle(Center, SmallRadius);

        public override int GetHashCode()
        {
            var hashCode = -131269964;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + StartRadius.GetHashCode();
            hashCode = hashCode * -1521134295 + EndRadius.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Ring a, Ring b) => a.Equals(b);
        public static bool operator !=(Ring a, Ring b) => !a.Equals(b);
    }
}
