using DeepWise.Localization;
using DeepWise.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Data;

namespace DeepWise.Shapes
{
    /// <summary>
    /// 表示二維的點。
    /// </summary>
    [DebuggerDisplay("X={X}, Y={Y}")]
    [TypeConverter(typeof(PointConverter))]
    [LocalizedDisplayName(nameof(Point), typeof(Resources)), JsonConverter(typeof(PointJsonConverter))]
    public struct Point : ISerializable, IEquatable<Point>, IShape , IEnumerable<double>  
    {
        public double X { get; set; }
        public double Y { get; set; }
        public static double Distance(Point a, Point b) => (b - a).Length;
        public static double Direction(Point a, Point b) => (b - a).Angle;
        public static Point Offset(Point p, Point offset) => new Point(p.X + offset.X, p.Y + offset.Y);
        public static Point Offset(Point p, double angle, double distance) => p + distance * Vector.FormAngle(angle);
        public static Point OffsetNeg(Point p, Point offset) => new Point(p.X - offset.X, p.Y - offset.Y);
        public static Point Scale(double factor, Point p) => new Point(factor * p.X, factor * p.Y);

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
        public static double Dot(Point a, Point b) => a.X * b.X + a.Y * b.Y;
        public Point(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            X = (double)info.GetValue(nameof(X), typeof(double));
            Y = (double)info.GetValue(nameof(Y), typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(X), X);
            info.AddValue(nameof(Y), Y);
        }

        public static Point Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Point(values[0], values[1]);
        }
        public override string ToString() => X + "," + Y;
        public string ToString(string format) => X.ToString(format) + "," + Y.ToString(format);

        #region Casting
        public static explicit operator Vector(Point p) => new Vector(p.X, p.Y);
        //[GDI]==========================================
        public static explicit operator System.Drawing.Point(Point p) => new System.Drawing.Point((int)(p.X + 0.5), (int)(p.Y + 0.5));
        public static explicit operator System.Drawing.PointF(Point p) => new System.Drawing.PointF((float)p.X, (float)p.Y);
        //[WinBase]======================================
        public static implicit operator Point(System.Windows.Point p) => new Point(p.X, p.Y);
        public static implicit operator System.Windows.Point(Point p) => new System.Windows.Point(p.X, p.Y);
        //[OpenCv]=======================================
        public static explicit operator OpenCvSharp.Point(Point p) => new OpenCvSharp.Point((int)p.X, (int)p.Y);
        public static explicit operator OpenCvSharp.Point2f(Point p) => new OpenCvSharp.Point2f((float)p.X, (float)p.Y);
        public static implicit operator OpenCvSharp.Point2d(Point p) => new OpenCvSharp.Point2d(p.X, p.Y);

        public static explicit operator Point(OpenCvSharp.Point p) => new Point(p.X, p.Y);
        public static explicit operator Point(OpenCvSharp.Point2f p) => new Point(p.X, p.Y);
        public static implicit operator Point(OpenCvSharp.Point2d p) => new Point(p.X, p.Y);
        #endregion

        public static Point Zero => new Point(0, 0);
        public static readonly Point NaN = new Point(double.NaN, double.NaN);
        public static bool IsNaN(Point point) => double.IsNaN(point.X) || double.IsNaN(point.Y);

        public static bool operator ==(Point p1, Point p2) => p1.X == p2.X && p1.Y == p2.Y;
        public static bool operator !=(Point p1, Point p2) => !(p1 == p2);
        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }
        public bool Equals(Point other)
        {
            return X == other.X &&
                   Y == other.Y;
        }
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public bool OnContour(Point p)
        {
            throw new NotImplementedException();
        }

        #region Trasnformation
        public Point RotateAround(Point origin, double angle) =>
     new Point((Math.Cos(angle) * (X - origin.X) - Math.Sin(angle) * (Y - origin.Y) + origin.X),
         (Math.Sin(angle) * (X - origin.X) + Math.Cos(angle) * (Y - origin.Y) + origin.Y));
        IShape IShape.Move(Vector offset) => this + offset;
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public Point RelativeTo(Point origin, double angle)
        {
            Point p = this;
            p = (Point)(p - origin).Rotate(-angle);
            return p;
        }
        public Point AppendOn(Point origin, double angle)
        {
            Point p = this;
            p = origin + ((Vector)p).Rotate(angle);
            return p;
        }
        #endregion

        #region IEnuerable
        public IEnumerator<double> GetEnumerator()
        {
            yield return X;
            yield return Y;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return X;
            yield return Y;
        }

        #endregion
        
        public static Vector operator -(Point p0, Point p1) => new Vector(p0.X - p1.X, p0.Y - p1.Y);
        public static Point operator +(Point p, Vector v) => new Point(p.X + v.X, p.Y + v.Y);
        public static Point operator +(Point p, Size size) => new Point(p.X + size.Width, p.Y + size.Height);
        public static Point operator -(Point p, Vector v) => new Point(p.X - v.X, p.Y - v.Y);
        public static Point operator -(Point p, Size size) => new Point(p.X - size.Width, p.Y - size.Height);
        public static Point operator +(Point p,double x) => new Point(p.X + x, p.Y + x);
        public static Point operator -(Point p,double x) => new Point(p.X - x, p.Y - x);
        public static Point operator *(Point p, double x) => new Point(p.X * x, p.Y * x);
        public static Point operator /(Point p, double x) => new Point(p.X / x, p.Y / x);

        public static explicit operator Point(System.Drawing.Point v)
        {
            return new Point(v.X, v.Y);
        }
    }

    //
    // Summary:
    //     Converts instances of other types to and from a System.Windows.Point.
    public sealed class PointConverter : TypeConverter
    {
        //
        // Summary:
        //     Determines whether an object can be converted from a given type to an instance
        //     of a System.Windows.Point.
        //
        // Parameters:
        //   context:
        //     Describes the context information of a type.
        //
        //   sourceType:
        //     The type of the source that is being evaluated for conversion.
        //
        // Returns:
        //     true if the type can be converted to a System.Windows.Point; otherwise, false.
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        //
        // Summary:
        //     Determines whether an instance of a System.Windows.Point can be converted to
        //     a different type.
        //
        // Parameters:
        //   context:
        //     Describes the context information of a type.
        //
        //   destinationType:
        //     The desired type this System.Windows.Point is being evaluated for conversion.
        //
        // Returns:
        //     true if this System.Windows.Point can be converted to destinationType; otherwise,
        //     false.
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        //
        // Summary:
        //     Attempts to convert the specified object to a System.Windows.Point.
        //
        // Parameters:
        //   context:
        //     Provides contextual information required for conversion.
        //
        //   culture:
        //     Cultural information to respect during conversion.
        //
        //   value:
        //     The object being converted.
        //
        // Returns:
        //     The System.Windows.Point created from converting value.
        //
        // Exceptions:
        //   T:System.NotSupportedException:
        //     Thrown if the specified object is NULL or is a type that cannot be converted
        //     to a System.Windows.Point.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                throw GetConvertFromException(value);
            }

            string text = value as string;
            if (text != null)
            {
                return Point.Parse(text);
            }

            return base.ConvertFrom(context, culture, value);
        }

        //
        // Summary:
        //     Attempts to convert a System.Windows.Point to a specified type.
        //
        // Parameters:
        //   context:
        //     Provides contextual information required for conversion.
        //
        //   culture:
        //     Cultural information to respect during conversion.
        //
        //   value:
        //     The System.Windows.Point to convert.
        //
        //   destinationType:
        //     The type to convert this System.Windows.Point to.
        //
        // Returns:
        //     The object created from converting this System.Windows.Point.
        //
        // Exceptions:
        //   T:System.NotSupportedException:
        //     Thrown if value is null or is not a System.Windows.Point, or if the destinationType
        //     is not one of the valid types for conversion.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != null && value is Point)
            {
                Point point = (Point)value;
                if (destinationType == typeof(string))
                {
                    return point.ToString();
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        //
        // Summary:
        //     Initializes a new instance of the System.Windows.PointConverter class.
        public PointConverter()
        {
        }
    }

    [DebuggerDisplay("X={X}, Y={Y}, Bulge={Bulge}")]
    public struct Vertex
    {
        public Vertex(double x,double y,double bulge)
        {
            X = x;
            Y = y;
            Bulge = bulge;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public Point Location=>new Point(X,Y);
        public double Bulge { get; set; }
    }

    [DebuggerDisplay("X={X}, Y={Y}, Bulge={Bulge}")]
    public struct VertexZ 
    {
        public VertexZ(double x, double y, double bulge,double z)
        {
            X = x;
            Y = y;
            Z = z;
            Bulge = bulge;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Point Location => new Point(X, Y);
        public double Bulge { get; set; }
    }


    /// <summary>
    /// 表示三維的點。
    /// </summary>
    [DebuggerDisplay("X={X}, Y={Y}, Z={Z}")]
    [TypeConverter(typeof(PointConverter))]
    [LocalizedDisplayName(nameof(Point3D), typeof(Resources)), JsonConverter(typeof(PointJsonConverter))]
    public struct Point3D : ISerializable, IEquatable<Point3D>, IEnumerable<double>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// Cross Product
        /// </summary>
        public static Point3D Cross(Point3D a, Point3D b) => new Point3D(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

        public double Magnitude() => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// Inner Product
        /// </summary>
        public static double Dot(Point3D a, Point3D b)=> a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Point3D Offset(Point3D p, Point3D offset) => new Point3D(p.X + offset.X, p.Y + offset.Y, p.Z + offset.Z);
        public static Point3D OffsetNeg(Point3D p, Point3D offset) => new Point3D(p.X - offset.X, p.Y - offset.Y, p.Z - offset.Z);
        public static Point3D Scale(double factor, Point3D p) => new Point3D(factor * p.X, factor * p.Y, factor * p.Z);
        public static double Distance(Point3D a, Point3D b) => (b - a).Length;

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D(Point p, double z)
        {
            X = p.X;
            Y = p.Y;
            Z = z;
        }

        public Point3D(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            X = (double)info.GetValue("X", typeof(double));
            Y = (double)info.GetValue("Y", typeof(double));
            Z = (double)info.GetValue("Z", typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
            info.AddValue("Z", Z);
        }

        public static Point3D Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Point3D(values[0], values[1], values[2]);
        }
        public override string ToString() => X + "," + Y + "," + Z;

        public string ToString(string format) => X.ToString(format) + "," + Y.ToString(format) + "," + Z.ToString(format);

        public static Point3D NaN => new Point3D(double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Point3D point) => double.IsNaN(point.X) || double.IsNaN(point.Y) || double.IsNaN(point.Z);

        public override bool Equals(object obj)
        {
            return obj is Point3D point && Equals(point);
        }
        public bool Equals(Point3D other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public IEnumerator<double> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        //   public Point3D RotateAround(Point3D origin, double angle) =>
        //new Point3D((Math.Cos(angle) * (X- origin.X) - Math.Sin(angle) * (Y - origin.Y) + origin.X),
        //    (Math.Sin(angle) * (X - origin.X) + Math.Cos(angle) * (Y - origin.Y) + origin.Y));




        //public override bool Equals(object obj)
        //{
        //    return obj is Point3D point &&X == point.X &&Y == point.Y;
        //}

        public static explicit operator Point(Point3D p) => new Point(p.X, p.Y);
        public static explicit operator Vector3D(Point3D p) => new Vector3D(p.X, p.Y, p.Z);
        public static Vector3D operator -(Point3D p0, Point3D p1) => new Vector3D(p0.X - p1.X, p0.Y - p1.Y, p0.Z - p1.Z);
        public static Point3D operator +(Point3D p, Vector3D v) => new Point3D(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        public static Point3D operator -(Point3D p, Vector3D v) => new Point3D(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        public static bool operator ==(Point3D p1, Point3D p2) => p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;
        public static bool operator !=(Point3D p1, Point3D p2) => !(p1 == p2);
        public static Point3D Zero => new Point3D(0, 0, 0);
    }

    [DebuggerDisplay("X={X}, Y={Y}, Z={Z},A ={A}, B={B}, C={C}")]
    [LocalizedDisplayName(nameof(Point6), typeof(Resources))]
    public struct Point6 : ISerializable, IEquatable<Point6>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public Point6(decimal x, decimal y, decimal z, decimal a, decimal b, decimal c) : this((double)x, (double)y, (double)z, (double)a, (double)b, (double)c)
        {

        }
        public Point6(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            A = B = C = 0;
        }
        public Point6(Point3D p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
            A = B = C = 0;
        }
        public Point6(Point3D p, double a, double b, double c)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
            A = a;
            B = b;
            C = c;
        }
        public Point6(double x, double y, double z,double a,double b,double c)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
        }
        public Point6(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            X = (double)info.GetValue("X", typeof(double));
            Y = (double)info.GetValue("Y", typeof(double));
            Z = (double)info.GetValue("Z", typeof(double));
            A = (double)info.GetValue("A", typeof(double));
            B = (double)info.GetValue("B", typeof(double));
            C = (double)info.GetValue("C", typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(X), X);
            info.AddValue(nameof(Y), Y);
            info.AddValue(nameof(Z), Z);
            info.AddValue(nameof(A), A);
            info.AddValue(nameof(B), B);
            info.AddValue(nameof(C), C);
        }

        public static Point6 Parse(string s)
        {
            double[] values = s.Split(',').Select(x => double.Parse(x)).ToArray();
            return new Point6(values[0], values[1], values[2], values[3], values[4], values[5]);
        }
        public override string ToString() => X + "," + Y + "," + Z + "," + A + "," + B + "," + C;

        public static Point6 NaN => new Point6(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Point6 pr) => double.IsNaN(pr.X) || double.IsNaN(pr.Y) || double.IsNaN(pr.Z)|| double.IsNaN(pr.A) || double.IsNaN(pr.B) || double.IsNaN(pr.C);

        public static Point6 operator +(Point6 a, Point6 b) => new Point6(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.A + b.A, a.B + b.B, a.C + b.C);
        public static Point6 operator *(Point6 p, double scalar) => new Point6(p.X * scalar, p.Y * scalar, p.Z * scalar, p.A * scalar, p.B * scalar, p.C * scalar);
        public static Point6 operator *(double scalar, Point6 p) => new Point6(p.X * scalar, p.Y * scalar, p.Z * scalar, p.A * scalar, p.B * scalar, p.C * scalar);
        public override bool Equals(object obj)
        {
            return obj is Point6 point && Equals(point);
        }
        public bool Equals(Point6 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z &&
                   A == other.A &&
                   B == other.B &&
                   C == other.C;
        }

        public override int GetHashCode()
        {
            var hashCode = 1727094563;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + C.GetHashCode();
            return hashCode;
        }
        [Browsable(false),JsonIgnore]
        public Point3D Location => new Point3D(X, Y, Z);
        public static explicit operator Point3D(Point6 p) => new Point3D(p.X, p.Y, p.Z);
        public static bool operator ==(Point6 p1, Point6 p2) => p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z && p1.A == p2.A && p1.B == p2.B && p1.C == p2.C;
        public static bool operator !=(Point6 p1, Point6 p2) => !(p1 == p2);
        public static Point6 Zero => new Point6(0, 0, 0);
    }

    [Serializable]
    [DebuggerDisplay("J1={J1}, J2={J2}, J3={J3},J4 ={J4}, J5={J5}, J6={J6}")]
    [LocalizedDisplayName(nameof(Point6Axis), typeof(Resources))]
    public struct Point6Axis : ISerializable, IEquatable<Point6Axis>
    {
        public double J1 { get; set; }
        public double J2 { get; set; }
        public double J3 { get; set; }
        public double J4 { get; set; }
        public double J5 { get; set; }
        public double J6 { get; set; }

        public Point6Axis(double x, double y, double z)
        {
            J1 = x;
            J2 = y;
            J3 = z;
            J4 = J5 = J6 = 0;
        }
        public Point6Axis(Point3D p)
        {
            J1 = p.X;
            J2 = p.Y;
            J3 = p.Z;
            J4 = J5 = J6 = 0;
        }
        public Point6Axis(Point3D p, double a, double b, double c)
        {
            J1 = p.X;
            J2 = p.Y;
            J3 = p.Z;
            J4 = a;
            J5 = b;
            J6 = c;
        }
        public Point6Axis(double x, double y, double z, double a, double b, double c)
        {
            J1 = x;
            J2 = y;
            J3 = z;
            J4 = a;
            J5 = b;
            J6 = c;
        }
        public Point6Axis(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            J1 = (double)info.GetValue("J1", typeof(double));
            J2 = (double)info.GetValue("J2", typeof(double));
            J3 = (double)info.GetValue("J3", typeof(double));
            J4 = (double)info.GetValue("J4", typeof(double));
            J5 = (double)info.GetValue("J5", typeof(double));
            J6 = (double)info.GetValue("J6", typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(J1), J1);
            info.AddValue(nameof(J2), J2);
            info.AddValue(nameof(J3), J3);
            info.AddValue(nameof(J4), J4);
            info.AddValue(nameof(J5), J5);
            info.AddValue(nameof(J6), J6);
        }

        public static Point6Axis Parse(string s)
        {
            double[] values = s.Split(',').Select(x => double.Parse(x)).ToArray();
            return new Point6Axis(values[0], values[1], values[2], values[3], values[4], values[5]);
        }
        public override string ToString() => J1 + "," + J2 + "," + J3 + "," + J4 + "," + J5 + "," + J6;

        public static Point6Axis NaN => new Point6Axis(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Point6Axis pr) => double.IsNaN(pr.J1) || double.IsNaN(pr.J2) || double.IsNaN(pr.J3) || double.IsNaN(pr.J4) || double.IsNaN(pr.J5) || double.IsNaN(pr.J6);

        public static Point6Axis operator +(Point6Axis a, Point6Axis b) => new Point6Axis(a.J1 + b.J1, a.J2 + b.J2, a.J3 + b.J3, a.J4 + b.J4, a.J5 + b.J5, a.J6 + b.J6);
        public static Point6Axis operator *(Point6Axis p, double scalar) => new Point6Axis(p.J1 * scalar, p.J2 * scalar, p.J3 * scalar, p.J4 * scalar, p.J5 * scalar, p.J6 * scalar);
        public static Point6Axis operator *(double scalar, Point6Axis p) => new Point6Axis(p.J1 * scalar, p.J2 * scalar, p.J3 * scalar, p.J4 * scalar, p.J5 * scalar, p.J6 * scalar);
        public override bool Equals(object obj)
        {
            return obj is Point6Axis point && Equals(point);
        }
        public bool Equals(Point6Axis other)
        {
            return J1 == other.J1 &&
                   J2 == other.J2 &&
                   J3 == other.J3 &&
                   J4 == other.J4 &&
                   J5 == other.J5 &&
                   J6 == other.J6;
        }

        public override int GetHashCode()
        {
            var hashCode = 1727094563;
            hashCode = hashCode * -1521134295 + J1.GetHashCode();
            hashCode = hashCode * -1521134295 + J2.GetHashCode();
            hashCode = hashCode * -1521134295 + J3.GetHashCode();
            hashCode = hashCode * -1521134295 + J4.GetHashCode();
            hashCode = hashCode * -1521134295 + J5.GetHashCode();
            hashCode = hashCode * -1521134295 + J6.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Point6Axis p1, Point6Axis p2) => p1.J1 == p2.J1 && p1.J2 == p2.J2 && p1.J3 == p2.J3 && p1.J4 == p2.J4 && p1.J5 == p2.J5 && p1.J6 == p2.J6;
        public static bool operator !=(Point6Axis p1, Point6Axis p2) => !(p1 == p2);
        public static Point6Axis Zero => new Point6Axis(0, 0, 0);
    }

    /// <summary>
    /// 表示二維的向量。
    /// </summary>
    [Serializable]
    [DebuggerDisplay("X={X}, Y={Y}")]
    [LocalizedDisplayName(nameof(Vector), typeof(Resources))]
    public struct Vector : ISerializable, IEquatable<Vector>
    {
        public override string ToString() => X + "," + Y;
        public double X { get; set; }
        public double Y { get; set; }

        [Browsable(false)]
        public double Angle => Math.Atan2(Y, X);
        [Browsable(false)]
        public double Length => Math.Sqrt(X * X + Y * Y);

        [Browsable(false)]
        public double LengthSquared => X * X + Y * Y;
        [Browsable(false)]
        public Vector UnitVector => this / Length;

        [Browsable(false)]
        public double L1Distance => (X > 0 ? X : -X) + (Y > 0 ? Y : -Y);

        public static double Dot(Vector a, Vector b) => a.X * b.X + a.Y * b.Y;
        public void Normalize()
        {
            double l = Length;
            X /= l;
            Y /= l;
        }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }
        public Vector(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            X = (double)info.GetValue("X", typeof(double));
            Y = (double)info.GetValue("Y", typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
        }

        public static Vector FormAngle(double angle) => new Vector(Math.Cos(angle), Math.Sin(angle));
        public static Vector Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Vector(values[0], values[1]);
        }
        public static Vector NaN => new Vector(double.NaN, double.NaN);
        public Vector Rotate(double angle) => new Vector(Math.Cos(angle) * X - Math.Sin(angle) * Y, Math.Sin(angle) * X + Math.Cos(angle) * Y);

        public static Vector Empty => new Vector(0, 0);

        public static bool IsNaN(Vector point) => double.IsNaN(point.X) || double.IsNaN(point.Y);

        public override bool Equals(object obj)
        {
            return obj is Vector vector && Equals(vector);
        }

        public bool Equals(Vector other)
        {
            return X == other.X &&
                   Y == other.Y;
        }
        public static double CrossProduct(Vector u, Vector v) => u.X * v.Y - u.Y * v.X;
        public static double CrossProduct(Point u, Point v) => u.X * v.Y - u.Y * v.X;
        public static double InnerProduct(Vector u, Vector v) => u.X * v.X + u.Y * v.Y;
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public static implicit operator System.Windows.Vector(Vector p) => new System.Windows.Vector(p.X, p.Y);
        public static implicit operator Vector(System.Windows.Vector p) => new Vector(p.X, p.Y);
        public static explicit operator Point(Vector vector) => new Point(vector.X, vector.Y);
        public static explicit operator System.Drawing.SizeF(Vector vector) => new System.Drawing.SizeF((float)vector.X, (float)vector.Y);
        public static double operator *(Vector v0, Vector v1) => v0.X * v1.X + v0.Y * v1.Y;
        public static Vector operator *(double t, Vector v) => new Vector(v.X * t, v.Y * t);
        public static Vector operator /(double t, Vector v) => new Vector(v.X / t, v.Y / t);
        public static Vector operator *(Vector v, double t) => new Vector(v.X * t, v.Y * t);
        public static Vector operator /(Vector v, double t) => new Vector(v.X / t, v.Y / t);
        public static Vector operator +(Vector v1, Vector v2) => new Vector(v1.X + v2.X, v1.Y + v2.Y);
        public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.X - v2.X, v1.Y - v2.Y);
        public static Vector operator -(Vector v) => new Vector(-v.X, -v.Y);
        public static bool operator ==(Vector v1, Vector v2) => v1.X == v2.X && v1.Y == v2.Y;
        public static bool operator !=(Vector v1, Vector v2) => v1.X != v2.X || v1.Y != v2.Y;
    }

    /// <summary>
    /// 表示三維的向量。
    /// </summary>
    [Serializable]
    [DebuggerDisplay("X={X}, Y={Y}, Z={Z}")]
    [LocalizedDisplayName(nameof(Vector3D), typeof(Resources))]
    public struct Vector3D : ISerializable, IEquatable<Vector3D>
    {
        public override string ToString() => X + "," + Y + "," + Z;
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        //public double Angle => Math.Atan2(Y, X);
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        [Browsable(false)]
        public double LengthSquared => X * X + Y * Y + Z * Z;
        [Browsable(false)]
        public Vector3D UnitVector3D => this / Length;

        public void Normalize()
        {
            double l = Length;
            X /= l;
            Y /= l;
        }

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3D(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            X = (double)info.GetValue("X", typeof(double));
            Y = (double)info.GetValue("Y", typeof(double));
            Z = (double)info.GetValue("Z", typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
            info.AddValue("Z", Z);
        }

        //public static Vector3D FormAngle(double angle) => new Vector3D(Math.Cos(angle), Math.Sin(angle));
        public static Vector3D Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Vector3D(values[0], values[1], values[2]);
        }
        public static Vector3D NaN => new Vector3D(double.NaN, double.NaN, double.NaN);
        //public Vector3D Rotate(double angle) => new Vector3D(Math.Cos(angle) * X - Math.Sin(angle) * Y, Math.Sin(angle) * X + Math.Cos(angle) * Y);

        public static Vector3D Empty => new Vector3D(0, 0, 0);

        public static bool IsNaN(Vector3D v) => double.IsNaN(v.X) || double.IsNaN(v.Y) || double.IsNaN(v.Z);

        public override bool Equals(object obj)
        {
            return obj is Vector3D vector && Equals(vector);
        }

        public bool Equals(Vector3D other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }
        public static Vector3D CrossProduct(Vector3D u, Vector3D v) => new Vector3D(u.Y * v.Z - u.Z * v.Y, u.Z * v.X - u.X * v.Z, u.X * v.Y - u.Y * v.X);
        public static double InnerProduct(Vector3D u, Vector3D v) => u.X * v.X + u.Y * v.Y + u.Z * v.Z;

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }
        public static explicit operator Vector(Vector3D v) => new Vector(v.X, v.Y);
        public static explicit operator Point3D(Vector3D v) => new Point3D(v.X, v.Y, v.Z);
        //public static double operator *(Vector3D u, Vector3D v) => u.X * v.X + u.Y * v.Y + u.Z * v.Z;
        public static Vector3D operator *(double t, Vector3D v) => new Vector3D(v.X * t, v.Y * t, v.Z * t);
        public static Vector3D operator /(double t, Vector3D v) => new Vector3D(v.X / t, v.Y / t, v.Z / t);
        public static Vector3D operator *(Vector3D v, double t) => new Vector3D(v.X * t, v.Y * t, v.Z * t);
        public static Vector3D operator /(Vector3D v, double t) => new Vector3D(v.X / t, v.Y / t, v.Z / t);
        public static Vector3D operator +(Vector3D u, Vector3D v) => new Vector3D(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3D operator -(Vector3D u, Vector3D v) => new Vector3D(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3D operator -(Vector3D v) => new Vector3D(-v.X, -v.Y, -v.Z);
        public static bool operator ==(Vector3D u, Vector3D v) => u.X == v.X && u.Y == v.Y && u.Z == v.Z;
        public static bool operator !=(Vector3D u, Vector3D v) => u.X != v.X || u.Y != v.Y || u.Z != v.Z;
    }

    /// <summary>
    /// 表示長寬的資訊。
    /// </summary>
    [TypeConverter(typeof(PointTypeConverter))]
    [DebuggerDisplay("Width={Width}, Height={Height}")]
    [LocalizedDisplayName(nameof(Size), typeof(Resources))]
    public struct Size : ISerializable
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public Size(double x, double y)
        {
            Width = x;
            Height = y;
        }
        public Size(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            Width = (double)info.GetValue("Width", typeof(double));
            Height = (double)info.GetValue("Height", typeof(double));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Width", Width);
            info.AddValue("Height", Height);
        }
        public static Size Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Size(values[0], values[1]);
        }
        public override string ToString() => Width + "," + Height;

        public static Size Empty => new Size(0, 0);

        public static Size NaN => new Size(double.NaN, double.NaN);
        public static bool IsNaN(Size point) => double.IsNaN(point.Width) || double.IsNaN(point.Height);

        public static explicit operator Point(Size vector) => new Point(vector.Width, vector.Height);
        public static explicit operator Vector(Size vector) => new Vector(vector.Width, vector.Height);

    }

    /// <summary>
    /// 表示二維射線。
    /// </summary>
    [Serializable]
    [LocalizedDisplayName(nameof(Ray), typeof(Resources))]
    public struct Ray : IEquatable<Ray>
    {
        public Point Origin { get; set; }
        [LocalizedDisplayName(nameof(Angle), typeof(Resources))]
        public double Angle { get; set; } 
        public bool Contain(Point p)=> (p - Origin) * Vector.FormAngle(Angle) > 0 && Geometry.IsZero(p.GetDistance(new Line(Origin, Angle), out _));

        public Ray(double x, double y, double angle)
        {
            Origin = new Point(x, y);
            Angle = angle;
        }

        public static Ray NaN => new Ray(double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Ray v) => double.IsNaN(v.Origin.X) || double.IsNaN(v.Origin.Y) || double.IsNaN(v.Angle);
        public static Ray Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Ray(values[0], values[1], values[2]);
        }
        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }
        public bool Equals(Ray other) => Origin == other.Origin && Angle == other.Angle;

        public override int GetHashCode()
        {
            var hashCode = 1911993471;
            hashCode = hashCode * -1521134295 + Origin.GetHashCode();
            hashCode = hashCode * -1521134295 + Angle.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Ray p1, Ray p2) => p1.Origin.X== p2.Origin.X&& p1.Origin.Y == p2.Origin.Y && p1.Angle == p2.Angle;
        public static bool operator !=(Ray p1, Ray p2) => !(p1 == p2);
        public static Ray Zero => new Ray(0, 0, 0);
    }

    [Serializable]
    [LocalizedDisplayName(nameof(Datum), typeof(Resources))]
    public struct Datum : IEquatable<Datum>
    {
        public Point Location { get; set; }
        public double Direction { get; set; }

        public Datum(double x, double y, double angle)
        {
            Location = new Point(x, y);
            Direction = angle;
        }

        public static Datum NaN => new Datum(double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Datum v) => double.IsNaN(v.Location.X) || double.IsNaN(v.Location.Y) || double.IsNaN(v.Direction);
        public static Datum Parse(string s)
        {
            string[] tmp = s.Split(',');
            double[] values = new double[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) values[i] = double.Parse(tmp[i]);
            return new Datum(values[0], values[1], values[2]);
        }
        public override bool Equals(object obj)
        {
            return obj is Datum point && Equals(point);
        }
        public bool Equals(Datum other) => Location == other.Location && Direction == other.Direction;

        public override int GetHashCode()
        {
            var hashCode = 1911993471;
            hashCode = hashCode * -1521134295 + Location.GetHashCode();
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Datum p1, Datum p2) => p1.Location == p2.Location && p1.Direction == p2.Direction;
        public static bool operator !=(Datum p1, Datum p2) => !(p1 == p2);
        public static Datum Zero => new Datum(0, 0, 0);
    }



    public class PointTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }
        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue;
            object result;

            result = null;
            stringValue = value as string;

            if (!string.IsNullOrEmpty(stringValue))
            {
                try
                {
                    Type type = (Type)context.GetType().GetProperty("PropertyType",BindingFlags.Public| BindingFlags.Instance).GetValue(context);
                    result = type.GetMethod("Parse").Invoke(null,new object[] { stringValue });
                }
                catch(Exception ex)
                {

                }
            }

            return result ?? base.ConvertFrom(context, culture, value);
        }

    }

    public class PointJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            switch (objectType.Name)
            {
                case nameof(Point):
                case nameof(Point3D):
                case nameof(Point6):
                case nameof(Point6Axis):
                case nameof(Vector):
                case nameof(Vector3D):
                case nameof(Size):
                case nameof(Ray):
                case nameof(Datum):
                    return true;
                default:
                    return false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            switch (objectType.Name)
            {
                case nameof(Point):

                    return new Point(token["X"].ToObject<double>(), token["Y"].ToObject<double>());
                case nameof(Point3D):
                    return new Point3D(token["X"].ToObject<double>(), token["Y"].ToObject<double>(), token["Z"].ToObject<double>());
                case nameof(Point6):
                case nameof(Point6Axis):
                case nameof(Vector):
                case nameof(Vector3D):
                case nameof(Size):
                case nameof(Ray):
                case nameof(Datum):
                default:
                    throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            switch(value)
            {
                case Point p2:
                    jo.Add("X", p2.X);
                    jo.Add("Y", p2.Y);
                    break;
                case Point3D p3:
                    jo.Add("X", p3.X);
                    jo.Add("Y", p3.Y);
                    jo.Add("Z", p3.Z);
                    break;

            }
            jo.WriteTo(writer);
        }
    }


}