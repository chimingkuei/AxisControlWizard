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
    [DebuggerDisplay("P0={P0}, P1={P1},Length={Length},Angle={Angle}")]
    [LocalizedDisplayName(nameof(Segment), typeof(Resources))]
    public struct Segment : IShape, ISerializable, IScannable, IEquatable<Segment>
    {
        public Point P0 { get; set; }
        public Point P1 { get; set; }
        [LocalizedDisplayName(nameof(Length), typeof(Resources))]
        public double Length => (P1 - P0).Length;
        [LocalizedDisplayName(nameof(Angle), typeof(Resources))]
        public double Angle => Math.Atan2(Y1 - Y0, X1 - X0);

        [Browsable(false)]
        public double X0
        {
            get => P0.X;
            set => P0 = new Point(value, Y0);
        }
        [Browsable(false)]
        public double Y0
        {
            get => P0.Y;
            set => P0 = new Point(X0, value);
        }
        [Browsable(false)]
        public double X1
        {
            get => P1.X;
            set => P1 = new Point(value, Y1);
        }
        [Browsable(false)]
        public double Y1
        {
            get => P1.Y;
            set => P1 = new Point(X1, value);
        }
        public bool OnContour(Point p) => Geometry.IsZero(Geometry.GetDistance(p, this, out _));
        IShape IShape.Move(Vector offset) => new Segment(P0 += offset, P1 += offset);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public Segment RelativeTo(Point origin, double angle)
        {
            Segment seg = this;
            seg.P0 = seg.P0.RelativeTo(origin, angle);
            seg.P1 = seg.P1.RelativeTo(origin, angle);
            return seg;
        }
        public Segment AppendOn(Point origin, double angle)
        {
            Segment seg = this;
            seg.P0 = seg.P0.AppendOn(origin, angle);
            seg.P1 = seg.P1.AppendOn(origin, angle);
            return seg;
        }
        public bool InRect(Rect rect, out Segment intersection)
        {
            if (rect.Contains(this.P0))
            {
                if (rect.Contains(this.P1))
                {
                    intersection = this;
                    return true;
                }
                else
                {
                    if (P1.X > rect.Right)
                    {
                        if (Geometry.Intersect(this, rect.RightEdge, out Point p))
                        {
                            intersection = new Segment(this.P0, p);
                            return true;
                        }
                    }
                    else if (P1.X < rect.Left)
                    {
                        if (Geometry.Intersect(this, rect.LeftEdge, out Point p))
                        {
                            intersection = new Segment(this.P0, p);
                            return true;
                        }
                    }
                    if (P1.X > rect.Bottom)
                    {
                        if (Geometry.Intersect(this, rect.BottomEdge, out Point p))
                        {
                            intersection = new Segment(this.P0, p);
                            return true;
                        }
                    }
                    else if (P1.X < rect.Top)
                    {
                        if (Geometry.Intersect(this, rect.TopEdge, out Point p))
                        {
                            intersection = new Segment(this.P0, p);
                            return true;
                        }
                    }
                }
            }
            else if (rect.Contains(this.P1))
            {
                if (P0.X > rect.Right)
                {
                    if (Geometry.Intersect(this, rect.RightEdge, out Point p))
                    {
                        intersection = new Segment(p, this.P1);
                        return true;
                    }
                }
                else if (P0.X < rect.Left)
                {
                    if (Geometry.Intersect(this, rect.LeftEdge, out Point p))
                    {
                        intersection = new Segment(p, this.P1);
                        return true;
                    }
                }
                if (P0.X > rect.Bottom)
                {
                    if (Geometry.Intersect(this, rect.BottomEdge, out Point p))
                    {
                        intersection = new Segment(p, this.P1);
                        return true;
                    }
                }
                else if (P0.X < rect.Top)
                {
                    if (Geometry.Intersect(this, rect.TopEdge, out Point p))
                    {
                        intersection = new Segment(p, this.P1);
                        return true;
                    }
                }
            }
            intersection = Segment.NaN;
            return false;
        }
        public Segment(double x0, double y0, double x1, double y1)
        {
            P0 = new Point(x0, y0);
            P1 = new Point(x1, y1);
        }
        public Segment(Point p0, Point p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public Segment(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            P0 = (Point)info.GetValue(nameof(P0), typeof(Point));
            P1 = (Point)info.GetValue(nameof(P1), typeof(Point));

        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(P0), P0);
            info.AddValue(nameof(P1), P1);
        }
        public override string ToString() => X0 + "," + Y0 + "," + X1 + "," + Y1;


        [Browsable(false)]
        public Point MidPoint => new Point((X0 + X1) / 2, (Y0 + Y1) / 2);
        [Browsable(false)]
        public Vector Forward
        {
            get
            {
                Vector v = P1 - P0;
                v.Normalize();
                return v;
            }
        }
        [Browsable(false)]
        public Vector Right => Vector.FormAngle(Angle + Math.PI / 2);
        [Browsable(false)]
        public Vector Left => Vector.FormAngle(Angle - Math.PI / 2);
        [Browsable(false)]
        public Vector Back => Vector.FormAngle(Angle + Math.PI);
        public static Segment NaN => new Segment(double.NaN, double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Segment segment) => double.IsNaN(segment.X0) || double.IsNaN(segment.X1) || double.IsNaN(segment.Y0) || double.IsNaN(segment.Y1);
        public static explicit operator Line(Segment segment) => new Line(segment.P0, segment.P1);
        public static explicit operator Vector(Segment segment) => segment.P1 - segment.P0;
        public static Segment operator +(Segment line, Vector v) => new Segment(line.P0 + v, line.P1 + v);
        public static Segment operator -(Segment line, Vector v) => new Segment(line.P0 - v, line.P1 - v);

        (Point from, Point to)[] IScannable.GetLines()
        {
            return new (Point, Point)[] { (P0, P1) };
        }

        public override bool Equals(object obj)
        {
            return obj is Segment segment && Equals(segment);
        }

        public bool Equals(Segment other)
        {
            return X0 == other.X0 &&
                   Y0 == other.Y0 &&
                   X1 == other.X1 &&
                   Y1 == other.Y1;
        }

        public override int GetHashCode()
        {
            var hashCode = 1435102314;
            hashCode = hashCode * -1521134295 + X0.GetHashCode();
            hashCode = hashCode * -1521134295 + Y0.GetHashCode();
            hashCode = hashCode * -1521134295 + X1.GetHashCode();
            hashCode = hashCode * -1521134295 + Y1.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Segment a, Segment b) => a.Equals(b);
        public static bool operator !=(Segment a, Segment b) => !a.Equals(b);
    }
}
