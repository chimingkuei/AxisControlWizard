using DeepWise.Localization;
using DeepWise.Properties;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DeepWise.Shapes
{
    [Serializable]
    [DebuggerDisplay("A={A}, B={B}, C={C},Angle={Angle}")]
    [LocalizedDisplayName(nameof(Line), typeof(Resources))]
    public struct Line : IShape, ISerializable
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        [LocalizedDisplayName(nameof(Angle), typeof(Resources))]
        public double Angle => Math.Atan2(A, -B);
        [LocalizedDisplayName(nameof(Slope), typeof(Resources))]
        public double Slope => -A / B;

        public double Func(double x) => -(C + A * x) / B;
        public double InverseFunc(double y) => -(C + B * y) / A;
        public bool OnContour(Point p) => Geometry.IsZero(A * p.X + B * p.Y + C);
        public static Line NaN => new Line(double.NaN, double.NaN, double.NaN);
        public static bool IsNaN(Line line) => double.IsNaN(line.A) || double.IsNaN(line.B) || double.IsNaN(line.C);
        public static bool IsHorizontal(Line line, out double y)
        {
            if (line.A.IsZero() && !line.B.IsZero())
            {
                y = -line.C / line.B;
                return true;
            }
            else
            {
                y = double.NaN;
                return false;
            }
        }
        public static bool IsVertical(Line line, out double x)
        {
            if (line.B.IsZero() && !line.A.IsZero())
            {
                x = -line.C / line.A;
                return true;
            }
            else
            {
                x = double.NaN;
                return false;
            }
        }
        IShape IShape.Move(Vector offset) => new Line(A, B, C -= A * offset.X + B * offset.Y);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => AppendOn(origin, angle);
        public Line RelativeTo(Point origin, double angle)
        {
            if (Line.IsHorizontal(this, out double ly))
            {
                throw new NotImplementedException();
            }
            else if (Line.IsVertical(this, out double lx))
            {
                throw new NotImplementedException();
            }
            else
                return new Line(new Point(this.InverseFunc(0), 0).RelativeTo(origin, angle), new Point(0, this.Func(0)).RelativeTo(origin, angle));
        }
        public Line AppendOn(Point origin, double angle)
        {
            if (Line.IsHorizontal(this, out double y))
                return new Line(new Point(0, y).AppendOn(origin, angle), angle);
            else if (Line.IsVertical(this, out double x))
                return new Line(new Point(x, 0).AppendOn(origin, angle), angle + Math.PI / 2);
            else
                return new Line(new Point(this.InverseFunc(0), 0).AppendOn(origin, angle), new Point(0, this.Func(0)).AppendOn(origin, angle));
        }

        public Line(double a, double b, double c) { A = a; B = b; C = c; }
        public Line(Point p0, Point p1)
        {
            A = p1.Y - p0.Y;
            B = p0.X - p1.X;
            C = -A * p0.X - B * p0.Y;
        }
        public Line(double x0, double y0, double x1, double y1)
        {
            A = y1 - y0;
            B = x0 - x1;
            C = -A * x0 - B * y0;
        }
        public Line(Point p, double angle)
        {
            A = Math.Sin(angle);
            B = -Math.Cos(angle);
            C = -A * p.X - B * p.Y;
        }
        public Line(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            A = (double)info.GetValue(nameof(A), typeof(double));
            B = (double)info.GetValue(nameof(B), typeof(double));
            C = (double)info.GetValue(nameof(C), typeof(double));
        }


        public override string ToString()
        {
            if (IsNaN(this)) return "NaN";
            bool a = !A.IsZero();
            bool b = !B.IsZero();
            if (a & b) return $"{A.ToString("0.##")}X + {B.ToString("0.##")}Y + {C.ToString("0.##")} = 0";
            else if (a) return $"X = {-C / A}";
            else if (b) return $"Y = {-C / B}";
            else return "NaN";
        }
        //public override string ToString() => A.ToString("0.##") + "," + B.ToString("0.##") + "," + C.ToString("0.##");
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(A), A);
            info.AddValue(nameof(B), B);
            info.AddValue(nameof(C), C);
        }
    }
}
