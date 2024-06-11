using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DeepWise.Shapes
{
    /// <summary>
    /// 用來計算長度、距離等特徵。
    /// </summary>
    public static class Geometry
    {
        private const double Epsilon = 1e-10;
        [DebuggerStepThrough]
        public static bool IsZero(this double value) => Math.Abs(value) < Epsilon;

        public static Point GetCenterBulge(Point p0, Point p1, double bulge)
        {
            double x0 = p0.X;
            double y0 = p0.Y;
            double x1 = p1.X;
            double y1 = p1.Y;
            double b = bulge;

            var norm = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            var s = norm / 2;
            var d = s * (1 - b * b) / (2 * b);

            var u = (x1 - x0) / norm;
            var v = (y1 - y0) / norm;

            var c1 = -v * d + (x0 + x1) / 2;
            var c2 = u * d + (y0 + y1) / 2;

            return new Point(c1, c2);
        }

        #region [Distance]
        public static double GetDistance(this Point p, Arc arc, out Point closest)
        {
            double angle = (p - arc.Center).Angle;
            if (Geometry.IsAngleInRange(angle, arc.StartAngle, arc.EndAngle))
            {
                Vector v = (p - arc.Center).UnitVector;
                closest = arc.Center + v * arc.Radius;
                return (closest - p).Length;
            }
            else
            {
                double strSwp = GetSweepAngle(angle, arc.StartAngle);
                double endSwp = GetSweepAngle(angle, arc.EndAngle);
                if (strSwp > Math.PI) strSwp -= Math.PI;
                if (endSwp > Math.PI) endSwp -= Math.PI;

                if (strSwp < endSwp)
                    closest = arc.Center + arc.Radius * Vector.FormAngle(arc.StartAngle);
                else
                    closest = arc.Center + arc.Radius * Vector.FormAngle(arc.EndAngle);
                return (closest - p).Length;
            }
        }
        public static double GetDistance(this Point p, Arc arc) => GetDistance(p, arc, out _);
        public static double GetDistance(this Point p, Circle circle, out Point closest)
        {
            Vector v = (p - circle.Center).UnitVector;
            closest = circle.Center + v * circle.Radius;
            return (closest - p).Length;
        }
        public static double GetDistance(this Point p, Ellipse ellipse, out Point closest)
        {
            //https://stackoverflow.com/questions/22959698/distance-from-given-point-to-given-ellipse
            throw new NotImplementedException();


        }
        public static double GetDistance(this Point p, Line line, out Point closest)
        {
            double A = line.A, B = line.B, C = line.C;
            if (A == 0 && B == 0)
            {
                closest = Point.NaN;
                return double.NaN;
            }
            double sum = A * A + B * B;
            double x = (B * (B * p.X - A * p.Y) - A * C) / sum;
            double y = (A * (-B * p.X + A * p.Y) - B * C) / sum;
            closest = new Point(x, y);
            return Math.Abs((A * p.X + B * p.Y + C) / Math.Sqrt(sum));
        }
        public static double GetDistance(this Point p, Segment segment, out Point closest)
        {
            Vector b = segment.P1 - segment.P0;
            if (b == Vector.Empty)
            {
                closest = segment.P0;
                return (p - closest).Length;
            }
            Vector a = p - segment.P0;
            double t = (a * b) / b.LengthSquared;

            if (t >= 1)
                closest = segment.P1;
            else if (t <= 0)
                closest = segment.P0;
            else
                closest = segment.P0 + t * (Vector)segment;

            return (p - closest).Length;
        }
        public static double GetDistanceSquared(this Point p, Line line)
        {
            double A = line.A, B = line.B, C = line.C;
            if (A == 0 && B == 0)
            {
                return double.NaN;
            }
            double sum = A * A + B * B;
            double x = (B * (B * p.X - A * p.Y) - A * C) / sum;
            double y = (A * (-B * p.X + A * p.Y) - B * C) / sum;
            x -= p.X;
            y -= p.Y;
            return x * x + y * y;
        }
        #endregion

        #region [Intersection]
        //public static bool ShapesIntersect(IShape shape1, IShape shape2, out Point[] hits)
        //{
        //    switch (shape1.GetType().Name)
        //    {
        //        case nameof(Arc):
        //            {
        //                switch (shape2.GetType().Name)
        //                {
        //                    case nameof(Arc): return Intersect((Arc)shape1, (Arc)shape2, out hits);
        //                    case nameof(Circle): return Intersect((Arc)shape1, (Circle)shape2, out hits);
        //                    case nameof(Line): return Intersect((Arc)shape1, (Line)shape2, out hits);
        //                    case nameof(Segment): return Intersect((Arc)shape1, (Segment)shape2, out hits);
        //                    default: throw new NotImplementedException();
        //                }
        //            }
        //        case nameof(Circle):
        //            {
        //                switch (shape2.GetType().Name)
        //                {
        //                    case nameof(Arc): return Intersect((Arc)shape2, (Circle)shape1, out hits);
        //                    case nameof(Circle): return Intersect((Circle)shape1, (Circle)shape2, out hits);
        //                    case nameof(Line): return Intersect((Circle)shape1, (Line)shape2, out hits);
        //                    case nameof(Segment): return Intersect((Circle)shape1, (Segment)shape2, out hits);
        //                    default: throw new NotImplementedException();
        //                }
        //            }
        //        case nameof(Line):
        //            {
        //                switch (shape2.GetType().Name)
        //                {
        //                    case nameof(Arc): return Intersect((Arc)shape2, (Line)shape1, out hits);
        //                    case nameof(Circle): return Intersect((Circle)shape2, (Line)shape1, out hits);
        //                    case nameof(Line): return Intersect((Line)shape2, (Line)shape1, out hits);
        //                    case nameof(Segment): return Intersect((Line)shape1, (Segment)shape2, out hits);
        //                    default: throw new NotImplementedException();
        //                }
        //            }
        //        case nameof(Segment):
        //            {
        //                switch (shape2.GetType().Name)
        //                {
        //                    case nameof(Arc): return Intersect((Arc)shape2, (Segment)shape1, out hits);
        //                    case nameof(Circle): return Intersect((Circle)shape2, (Segment)shape1, out hits);
        //                    case nameof(Line): return Intersect((Line)shape2, (Segment)shape1, out hits);
        //                    case nameof(Segment): return Intersect((Segment)shape2, (Segment)shape1, out hits);
        //                    default: throw new NotImplementedException();
        //                }
        //            }
        //        default: throw new NotImplementedException();
        //    }
        //}
        //public static bool Intersect<T,W>(T a,W b,out Point[] intxns) where T : IShape where W : IShape
        //{
        //    var typeA = a.GetType();
        //    var typeB = b.GetType();
        //    MethodInfo method = typeof(Geometry).GetMethod("Intersect", new Type[] { typeA, typeB });
        //    if (method != null)
        //    {
        //        object[] args = new object[] { a, b, null };
        //        var result = (bool)method.Invoke(null, args);
        //            intxns = (Point[])args[2];
        //        return result;
        //    }
        //    method = typeof(Geometry).GetMethod("Intersect", new Type[] { typeB , typeA});
        //    if (method != null)
        //    {
        //        object[] args = new object[] { b, a, null };
        //        var result = (bool)method.Invoke(null, args);
        //        intxns = (Point[])args[2];
        //        return result;
        //    }
        //    throw new Exception($"無法計算類型{typeof(T).Name}以及{typeof(W).Name}的交點");
        //}
        public static bool Intersect(Arc arc1, Arc arc2, out Point[] hits)
        {
            if (Intersect((Circle)arc1, (Circle)arc2, out hits))
                hits = hits.Where(p => IsAngleInRange((p - arc1.Center).Angle, arc1.StartAngle, arc1.EndAngle) && IsAngleInRange((p - arc2.Center).Angle, arc1.StartAngle, arc1.EndAngle)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Arc arc, Circle circle, out Point[] hits)
        {
            if (Intersect((Circle)arc, circle, out hits))
                hits = hits.Where(p => IsAngleInRange(Math.Atan2(p.Y - arc.Center.Y, p.X - arc.Center.X), arc.StartAngle, arc.EndAngle)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Arc arc, Line line, out Point[] hits)
        {
            if (Intersect(new Circle(arc.Center, arc.Radius), line, out hits))
                hits = hits.Where(p => IsAngleInRange(Math.Atan2(p.Y - arc.Center.Y, p.X - arc.Center.X), arc.StartAngle, arc.EndAngle)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Arc arc, Segment segment, out Point[] hits)
        {
            if (Intersect((Circle)arc, segment, out hits))
                hits = hits.Where(p => IsAngleInRange((p - arc.Center).Angle, arc.StartAngle, arc.EndAngle)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Circle c1, Circle c2, out Point[] hits)
        {
            double distance = (c1.Center - c2.Center).Length;
            if (IsZero(distance - c1.Radius - c2.Radius))
            {
                Vector v = c2.Center - c1.Center;
                v.Normalize();
                hits = new Point[] { c1.Center + v * c1.Radius };
            }
            else if (distance > c1.Radius + c2.Radius)
                hits = new Point[0];
            else
            {

                Point c = (Point)(c2.Center - c1.Center);
                double cx2 = c.X * c.X, cy2 = c.Y * c.Y;
                double m = cx2 + cy2 + c1.Radius * c1.Radius - c2.Radius * c2.Radius;
                double A = cx2 + cy2;
                double B = -m * c.X;
                double C = m * m / 4 - c1.Radius * c1.Radius * c.Y * c.Y;
                double D = B * B - 4 * A * C;
                if (D.IsZero())
                {
                    double x = -B / (2 * A);
                    D = Math.Sqrt(c1.Radius * c1.Radius - x * x);
                    hits = new Point[] { new Point(x + c1.Center.X, c1.Center.Y + D), new Point(x + c1.Center.X, c1.Center.Y - D) };
                }
                else if (D > 0)
                {
                    D = Math.Sqrt(D);
                    double x1 = (-B + D) / (2 * A);
                    double y1 = (c1.Radius * c1.Radius - c2.Radius * c2.Radius + cx2 + cy2 - 2 * c.X * x1) / (2 * c.Y);
                    double x2 = (-B - D) / (2 * A);
                    double y2 = (c1.Radius * c1.Radius - c2.Radius * c2.Radius + cx2 + cy2 - 2 * c.X * x2) / (2 * c.Y);
                    hits = new Point[] { new Point(x1 + c1.Center.X, y1 + c1.Center.Y), new Point(x2 + c1.Center.X, y2 + c1.Center.Y) };
                }
                else
                    throw new Exception(" Intersect(Circle c1,Circle c2,out Point[] hits) => D < 0");
            }
            return hits.Length > 0;
        }
        public static bool Intersect(Circle circle, Line line, out Point[] hits)
        {
            if(Line.IsVertical(line,out double lineX))
            {
                double t = circle.Radius * circle.Radius - (lineX - circle.Center.X) * (lineX - circle.Center.X);
                if (Geometry.IsZero(t))
                {
                    hits = new Point[] { new Point(lineX, circle.Center.Y) };
                    return true;
                }
                else if(t>0)
                {
                    t = Math.Sqrt(t);
                    hits = new Point[] { new Point(lineX, circle.Center.Y + t), new Point(lineX, circle.Center.Y - t) };
                    if(GetInferiorAngle((hits[1] - hits[0]).Angle, line.Angle)>Math.PI/8) hits = hits.Reverse().ToArray();
                    return true;
                }
                else
                {
                    hits = new Point[0];
                    return false;
                }
            }
            else
            {
                double A = 1 + line.A * line.A / (line.B * line.B);
                double B = -2 * circle.Center.X + 2 * line.A * line.C / (line.B * line.B) + 2 * circle.Center.Y * line.A / line.B;
                double C = circle.Center.X * circle.Center.X + line.C * line.C / (line.B * line.B) + 2 * line.C * circle.Center.Y / line.B + circle.Center.Y * circle.Center.Y - circle.Radius * circle.Radius;
                double d = B * B - 4 * A * C;
                if (A.IsZero() || d < 0)
                    hits = new Point[0];
                else if (d.IsZero())
                {
                    double x = -B / (2 * A);
                    double y = (line.A * x + line.C) / -line.B;
                    hits = new Point[] { new Point(x, y) };
                }
                else
                {
                    d = Math.Sqrt(d);
                    double x1 = (-B + d) / (2 * A);
                    double x2 = (-B - d) / (2 * A);
                    hits = new Point[] { new Point(x1, (line.A * x1 + line.C) / -line.B), new Point(x2, (line.A * x2 + line.C) / -line.B) };
                    if (GetInferiorAngle((hits[1] - hits[0]).Angle, line.Angle) > Math.PI / 8) hits = hits.Reverse().ToArray();
                }
                return hits.Length > 0;
            }
        }
        public static bool Intersect(Circle circle, Segment segment, out Point[] hits)
        {
            if (Intersect(circle, (Line)segment, out hits))
                hits = hits.Where(p => segment.OnContour(p)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Circle circle, Ellipse ellipse, out Point[] hits)
        {
            throw new NotImplementedException();
            //circle.Center -= (Vector)ellipse.Center;
            //circle.Center = circle.Center.RotateAround(Point.Zero, -ellipse.Angle);
            //double[] roots = new double[4];
            //try
            //{
            //    roots[0] = MathNet.Numerics.RootFinding.Bisection.FindRoot(new Func<double, double>(x => G(x) - F(x)), -ellipse.SemiMajorAxis, ellipse.SemiMajorAxis);
            //}
            //catch { roots[0] = double.NaN; }
            //try
            //{
            //    roots[1] = MathNet.Numerics.RootFinding.Bisection.FindRoot(new Func<double, double>(x => G(x) - nF(x)), -ellipse.SemiMajorAxis, ellipse.SemiMajorAxis);
            //}
            //catch { roots[1] = double.NaN; }
            //try
            //    {
            //        roots[2] = MathNet.Numerics.RootFinding.Bisection.FindRoot(new Func<double, double>(x => -G(x) - F(x)), -ellipse.SemiMajorAxis, ellipse.SemiMajorAxis);
            //}
            //catch { roots[2] = double.NaN; }
            //try
            //{
            //    roots[3] = MathNet.Numerics.RootFinding.Bisection.FindRoot(new Func<double, double>(x => -G(x) - nF(x)), -ellipse.SemiMajorAxis, ellipse.SemiMajorAxis);
            //}
            //catch { roots[3] = double.NaN; }

            //List<Point> intxns = new List<Point>();
            //foreach(double root in roots)
            //{
            //    if (!double.IsNaN(root))
            //        intxns.Add(new Point(root, G(root)));
            //}
            //hits = intxns.ToArray();
            //return hits.Length > 0;

            //double G(double x)
            //{//ax^2 + by^2 + c = 0
            //    double a = 1/(ellipse.SemiMajorAxis * ellipse.SemiMajorAxis);
            //    double b = 1/(ellipse.SemiMinorAxis * ellipse.SemiMinorAxis);
            //    double c = -1;
            //    return Math.Sqrt(-4 * b * (a * x * x + c)) / (2 * b);
            //}
            //double dG(double x)
            //{//ax^2 + by^2 + c = 0
            //    double a = ellipse.SemiMinorAxis * ellipse.SemiMinorAxis;
            //    double b = ellipse.SemiMajorAxis * ellipse.SemiMajorAxis;
            //    double c = -a * b;
            //    return (2 * a * x) / Math.Sqrt(-4 * b * (a * x * x + c));
            //}
            //double F(double x)=> Math.Sqrt(circle.Radius * circle.Radius - (x - circle.Center.X) * (x - circle.Center.X)) + circle.Center.Y;
            //double nF(double x)=> -Math.Sqrt(circle.Radius * circle.Radius - (x - circle.Center.X) * (x - circle.Center.X)) + circle.Center.Y;
            //double dF(double x) => (circle.Center.X - x) / Math.Sqrt(circle.Radius * circle.Radius - (x - circle.Center.X) * (x - circle.Center.X));
            
        }
        public static bool Intersect(Line line1, Line line2, out Point[] hits)
        {
            double delta = line1.A * line2.B - line2.A * line1.B;
            if (delta.IsZero())
            {
                if ((Line.IsHorizontal(line1, out double y) && Line.IsVertical(line2, out double x)) ||
                    Line.IsVertical(line1, out x) && Line.IsHorizontal(line2, out y))
                    hits = new Point[] { new Point(x, y) };
                else
                    hits = new Point[0];
            }
            else
                hits = new Point[] { new Point((line1.B * line2.C - line2.B * line1.C) / delta, (line2.A * line1.C - line1.A * line2.C) / delta) };
            return hits.Length > 0;
        }
        public static bool Intersect(Line line1, Line line2, out Point hit)
        {
            double delta = line1.A * line2.B - line2.A * line1.B;
            if (delta.IsZero())
            {
                if ((Line.IsHorizontal(line1, out double y) && Line.IsVertical(line2, out double x)) || Line.IsVertical(line1, out x) && Line.IsHorizontal(line2, out y))
                {
                    hit = new Point(x, y);
                    return true;
                }
                else
                {
                    hit = Point.NaN;
                    return false;
                }
            }
            else
            {
                hit = new Point((line1.B * line2.C - line2.B * line1.C) / delta, (line2.A * line1.C - line1.A * line2.C) / delta);
                return true;
            }
            
        }
        public static bool Intersect(Line line, Segment segment, out Point[] hits)
        {
            if (Intersect((Shapes.Line)segment, line, out hits))
                hits = hits.Where(p => segment.OnContour(p)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Segment segment1, Segment segment2, out Point[] hits)
        {
            if (Intersect((Line)segment1, (Line)segment2, out hits))
                hits = hits.Where(p => segment1.OnContour(p) && segment2.OnContour(p)).ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(Segment segment1, Segment segment2, out Point hits)
        {
            if (Intersect((Line)segment1, (Line)segment2, out hits) && segment1.OnContour(hits) && segment2.OnContour(hits))
                return true;
            else
                return false;
        }
        public static bool Intersect(Rect rect, Line line, out Point[] hits)
        {
            List<Point> tmp = new List<Point>();
            foreach (Segment edge in new Segment[] { new Segment(rect.Left, rect.Top, rect.Right, rect.Top), new Segment(rect.Right, rect.Top, rect.Right, rect.Bottom), new Segment(rect.Right, rect.Bottom, rect.Left, rect.Bottom), new Segment(rect.Left, rect.Bottom, rect.Left, rect.Top), })
            {
                if (Intersect(line, edge, out Point[] intxs))
                    tmp.AddRange(intxs);
            }

            for (int j = tmp.Count - 1; j >= 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    if ((tmp[i] - tmp[j]).Length.IsZero())
                    {
                        tmp.RemoveAt(j);
                        break;
                    }
                }
            }
            hits = tmp.ToArray();
            return hits.Length > 0;
        }
        public static bool Intersect(RectRotatable rect, Line line, out Point[] hits)
        {
            List<Point> tmp = new List<Point>();
            foreach (Segment edge in new Segment[] { rect.TopEdge, rect.RightEdge, rect.BottomEdge, rect.LeftEdge })
            {
                if (Intersect(line, edge, out Point[] intxs))
                    tmp.AddRange(intxs);
            }

            for (int j = tmp.Count - 1; j >= 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    if ((tmp[i] - tmp[j]).Length.IsZero())
                    {
                        tmp.RemoveAt(j);
                        break;
                    }
                }
            }
            hits = tmp.ToArray();
            return hits.Length > 0;
        }
        #endregion

        #region [Angle]
        /// <summary>
        /// 取得幅角之主角，回傳值介於(-pi, pi]之間。
        /// </summary>
        public static double Arg(double angle)
        {
            angle %= Math.PI * 2;
            if (angle > Math.PI) angle -= 2 * Math.PI;
            else if (angle <= -Math.PI) angle += 2 * Math.PI;
            return angle;
        }
        /// <summary>
        /// 判斷角度是否在指定的範圍內。
        /// </summary>
        public static bool IsAngleInRange(double angle, double strAngle, double endAngle)
        {
            strAngle = Arg(strAngle);
            endAngle = Arg(endAngle);
            angle = Arg(angle);
            if (endAngle > strAngle) return endAngle >= angle && angle >= strAngle;
            else if (strAngle > endAngle) return !(strAngle > angle && angle > endAngle);
            else return angle == strAngle;
        }
        /// <summary>
        /// 判斷該弧長是否大於PI並且小於2PI。
        /// </summary>
        /// <param name="strAngle"></param>
        /// <param name="endAngle"></param>
        /// <returns></returns>
        public static bool IsReflexAngle(double strAngle, double endAngle)
        {
            if(Geometry.IsZero(strAngle - endAngle))
            {
                return false;
            }
            else if (endAngle > strAngle)
            {
                return endAngle - strAngle > Math.PI;
            }
            else
            {
                return strAngle - endAngle <= Math.PI;
            }
        }
        /// <summary>
        /// 判斷該弧長是否大於0並且小於PI。
        /// </summary>
        /// <param name="strAngle"></param>
        /// <param name="endAngle"></param>
        /// <returns></returns>
        public static bool IsInferiorAngle(double strAngle, double endAngle) => !IsReflexAngle(strAngle, endAngle);
        public static double AngleFormPoints(Point a, Point b, Point c)
        {
            Vector ab = b - a;
            Vector cb = b - c;
            return Math.Acos((ab * cb) / (ab.Length * cb.Length));
        }
        /// <summary>
        /// 判斷角度是否是平緩的(靠近x軸)，也就是角度介於 (-π/4,π/4) 、 (3/4π,π) 或 (-3/4π,-π) 之間。
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static bool IsGentle(double angle)
        {
            angle = Arg(angle);
            return (angle > -Math.PI / 4 && angle < Math.PI / 4) || (angle > Math.PI * 3 / 4 || angle < -Math.PI * 3 / 4);
        }
        /// <summary>
        /// 取得順時針方向掃過的角度。
        /// </summary>
        public static double GetSweepAngle(double strAngle, double endAngle)
        {
            strAngle = Arg(strAngle);
            endAngle = Arg(endAngle);
            if (IsZero(endAngle - strAngle)) return 0;
            if (endAngle > strAngle) return endAngle - strAngle;
            else return 2 * Math.PI + endAngle - strAngle;
        }
        public static double GetInferiorAngle(double ang1,double ang2)
        {
            if (Geometry.IsReflexAngle(ang1, ang2)) 
                return Geometry.GetSweepAngle(ang2, ang1);
            else
                return Geometry.GetSweepAngle(ang1, ang2);
        }
        #endregion

        public static int[][] GetIndexesOrderByDirection(this IEnumerable<DeepWise.Shapes.Point> ps, double right, double newRowDistance)
        {
            var vR = Vector.FormAngle(right);
            var vU = Vector.FormAngle(right + Math.PI / 2);

            var yOrder = ps.Select((p, i) => ((Vector p, int i))((Vector)p, i)).OrderBy(g => g.p * vU).ToArray();
            var rowStarts = new List<int>();
            for (int i = 1; i < yOrder.Length; i++)
            {
                var v = yOrder[i].p - yOrder[i - 1].p;
                var diff = v * vU;
                if (v * vU >= newRowDistance)
                    rowStarts.Add(i);
            }

            var result = new List<int[]>();

            //firstRow
            result.Add(yOrder.Take(rowStarts[0]).OrderBy(g => g.p * vR).Select(x => x.i).ToArray());

            for (int i = 1; i < rowStarts.Count; i++)
            {
                result.Add(yOrder.Skip(rowStarts[i - 1]).Take(rowStarts[i] - rowStarts[i - 1]).OrderBy(g => (DeepWise.Shapes.Vector)g.p * vR).Select(x => x.i).ToArray());

            }

            result.Add(yOrder.Skip(rowStarts.Last()).OrderBy(g => g.p * vR).Select(x => x.i).ToArray());
            return result.ToArray();
        }
    }
}
