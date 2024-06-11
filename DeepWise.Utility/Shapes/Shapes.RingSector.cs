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
    [DebuggerDisplay("Center={Center}, StartRadius={StartRadius},EndRadius={EndRadius},SweepAngle={SweepAngle}")]
    [LocalizedDisplayName(nameof(RingSector), typeof(Resources))]
    public struct RingSector : IShape, IAreal, ISerializable, IScannable, IEquatable<RingSector>
    {
        [LocalizedDisplayName(nameof(Center), typeof(Resources))]
        public Point Center { get; set; }
        [LocalizedDisplayName(nameof(StartRadius), typeof(Resources))]
        public double StartRadius { get; set; }
        [LocalizedDisplayName(nameof(EndRadius), typeof(Resources))]
        public double EndRadius { get; set; }
        [LocalizedDisplayName(nameof(StartAngle), typeof(Resources))]
        public double StartAngle { get; set; }
        [LocalizedDisplayName(nameof(EndAngle), typeof(Resources))]
        public double EndAngle { get; set; }
        [LocalizedDisplayName(nameof(SweepAngle), typeof(Resources))]
        public double SweepAngle => Geometry.GetSweepAngle(StartAngle, EndAngle);
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => Math.Abs(EndRadius * EndRadius - StartRadius * StartRadius) * Geometry.GetSweepAngle(StartAngle, EndAngle) / 2;
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter => 2 * (SweepAngle * (StartRadius + EndRadius) + Math.Abs(EndRadius - StartRadius));

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
        [Browsable(false)]
        public double LargeRadius => StartRadius > EndRadius ? StartRadius : EndRadius;
        [Browsable(false)]
        public double SmallRadius => StartRadius > EndRadius ? EndRadius : StartRadius;

        public bool Contains(Point p)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            double l = (p - Center).Length;
            if ((StartRadius > EndRadius && l <= StartRadius && l >= EndRadius) || (EndRadius >= StartRadius && l <= EndRadius && l >= StartRadius))
                return Geometry.IsAngleInRange(Math.Atan2(p.Y - Y, p.X - X), StartAngle, EndAngle);
            else
                return false;
        }
        public bool OnContour(Point p) => new Arc(Center, StartRadius, StartAngle, EndAngle).OnContour(p) || new Arc(Center, EndRadius, StartAngle, EndAngle).OnContour(p) || new Segment(Center + StartRadius * Vector.FormAngle(StartAngle), Center + EndRadius * Vector.FormAngle(StartAngle)).OnContour(p) || new Segment(Center + StartRadius * Vector.FormAngle(EndAngle), Center + EndRadius * Vector.FormAngle(EndAngle)).OnContour(p);
        IShape IShape.Move(Vector offset) => new RingSector(X, Y, StartRadius, EndRadius, StartAngle, EndAngle);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public RingSector RelativeTo(Point origin, double angle)
        {
            RingSector rct = this;
            rct.Center = rct.Center.RelativeTo(origin,angle);
            rct.StartAngle = Geometry.GetSweepAngle(angle, rct.StartAngle);
            rct.EndAngle = Geometry.GetSweepAngle(angle, rct.EndAngle);
            return rct;
        }
        public RingSector AppendOn(Point origin, double angle)
        {
            RingSector rct = this;
            rct.Center = rct.Center.AppendOn(origin,angle);
            rct.StartAngle = Geometry.Arg(angle + rct.StartAngle);
            rct.EndAngle = Geometry.Arg(angle + rct.EndAngle);
            return rct;
        }
        (Point from, Point to)[] IScannable.GetLines()
        {
            double length = Geometry.GetSweepAngle(StartAngle, EndAngle) * LargeRadius;
            (Point, Point)[] ps = new (Point, Point)[(int)length+1];
            for (int i = 0; i < ps.Length; i++)
            {
                Vector v = Vector.FormAngle(StartAngle + i / LargeRadius);
                ps[i].Item1 = Center + StartRadius * v;
                ps[i].Item2 = Center + EndRadius * v;
            }
            return ps;
        }

        public RingSector(double x, double y, double r1, double r2, double strAngle, double endAngle) { Center = new Point(x,y); StartRadius = r1; EndRadius = r2; StartAngle = strAngle; EndAngle = endAngle; }
        public RingSector(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            
            Center = (Point)info.GetValue(nameof(Center), typeof(Point));
            StartRadius = (double)info.GetValue(nameof(StartRadius), typeof(double));
            EndRadius = (double)info.GetValue(nameof(EndRadius), typeof(double));
            StartAngle = (double)info.GetValue(nameof(StartAngle), typeof(double));
            EndAngle = (double)info.GetValue(nameof(EndAngle), typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Center), Center);
            info.AddValue(nameof(StartRadius), StartRadius);
            info.AddValue(nameof(EndRadius), EndRadius);
            info.AddValue(nameof(StartAngle), StartAngle);
            info.AddValue(nameof(EndAngle), EndAngle);
        }

        public override string ToString() => X + "," + Y + "," + StartRadius + "," + EndRadius + "," + StartAngle + "," + EndAngle;

        public override bool Equals(object obj)
        {
            return obj is RingSector sector && Equals(sector);
        }

        public bool Equals(RingSector other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   StartRadius == other.StartRadius &&
                   EndRadius == other.EndRadius &&
                   StartAngle == other.StartAngle &&
                   EndAngle == other.EndAngle;
        }

        public override int GetHashCode()
        {
            var hashCode = -772011816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + StartRadius.GetHashCode();
            hashCode = hashCode * -1521134295 + EndRadius.GetHashCode();
            hashCode = hashCode * -1521134295 + StartAngle.GetHashCode();
            hashCode = hashCode * -1521134295 + EndAngle.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(RingSector a, RingSector b) => a.Equals(b);
        public static bool operator !=(RingSector a, RingSector b) => !a.Equals(b);
    }
}
