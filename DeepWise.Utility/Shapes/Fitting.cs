using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace DeepWise.Shapes
{
    /// <summary>
    /// 擬合、回歸的方法庫。
    /// </summary>
    public static class Fitting
    {
        /// <summary>
        /// 計算點群的回歸值線。(最小平方法)
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Line GetRegressedLine(this IEnumerable<Point> ps)
        {
            if (ps == null || ps.Count() < 2) return Line.NaN;
            double dx = ps.Max(p => p.X) - ps.Min(p => p.X);
            double dy = ps.Max(p => p.Y) - ps.Min(p => p.Y);
            if (dx == 0 && dy == 0) return Shapes.Line.NaN;
            if (dx >= dy)
            {
                LineH(ps, out _, out double y0, out double slope);
                return new Line(slope, -1, y0);
                //[Old]
                //LineH(ps, out _, out double y0, out double slope);
                //var line = new Line(slope, -1, y0);
                //LineH(ps.Where(p => p.GetDistanceSquared(line) < 16), out _, out y0, out slope);
                //return new Line(slope, -1, y0);
            }
            else
            {
                LineV(ps, out _, out double x0, out double slope);
                return new Line(-1, slope, x0);
                //[Old]
                //LineV(ps, out _, out double x0, out double slope);
                //var line = new Line(-1, slope, x0);
                //LineV(ps.Where(p => p.GetDistanceSquared(line) < 16), out _, out x0, out slope);
                //return new Line(-1, slope, x0);
            }
        }

        /// <summary>
        /// 計算兩組點群的回歸平行線。(最小平方法)
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Line[] GetRegressedLinesParallel(this IEnumerable<Point> _L1ps, IEnumerable<Point> _L2ps)
        {
            Line[] lines = new Line[2];
            double dx = _L1ps.Max(p => p.X) - _L1ps.Min(p => p.X);
            double dy = _L1ps.Max(p => p.Y) - _L1ps.Min(p => p.Y);
            if (dx >= dy)
                ParalleLineH(_L1ps, _L2ps, out lines[0], out lines[1]);
            else
                ParalleLineV(_L1ps, _L2ps, out lines[0], out lines[1]);
            return lines;
        }

        /// <summary>
        /// 計算點群的回歸線段。(最小平方法)
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Segment GetRegressedSegment(this IEnumerable<Point> ps)
        {
            Line line = ps.GetRegressedLine();
            
            if (Line.IsNaN(line))
            {
                return Segment.NaN;
            }
            else
            {
                Geometry.GetDistance(ps.First(), line, out Point p0);
                Geometry.GetDistance(ps.Last(), line, out Point p1);
                return new Segment(p0, p1);
            }
        }

        /// <summary>
        /// 計算點群的回歸圓。(最小平方法)
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Circle GetRegressedCircle(this IEnumerable<Point> ps)
        {
            int n = ps.Count();
            if (n <= 2) return Shapes.Circle.NaN;
            Point centroid = new Point(ps.Select(p => p.X).Average(), ps.Select(p => p.Y).Average());
            // computing moments(note: all moments will be normed, i.e.divided by n)

            double Mxx = 0, Myy = 0, Mxy = 0, Mxz = 0, Myz = 0, Mzz = 0;

            double[] X = new double[n], Y = new double[n], Z = new double[n];
            int i = 0;
            foreach(Point p in ps)
            {
                X[i] = p.X - centroid.X;
                Y[i] = p.Y - centroid.Y;
                Z[i] = X[i] * X[i] + Y[i] * Y[i];
                Mxy = Mxy + X[i] * Y[i];
                Mxx = Mxx + X[i] * X[i];
                Myy = Myy + Y[i] * Y[i];
                Mxz = Mxz + X[i] * Z[i];
                Myz = Myz + Y[i] * Z[i];
                Mzz = Mzz + Z[i] * Z[i];
                i++;
            }

            Mxx = Mxx / n;
            Myy = Myy / n;
            Mxy = Mxy / n;
            Mxz = Mxz / n;
            Myz = Myz / n;
            Mzz = Mzz / n;

            // computing the coefficients of the characteristic polynomial

            double Mz = Mxx + Myy;
            double Cov_xy = Mxx * Myy - Mxy * Mxy;
            double Mxz2 = Mxz * Mxz;
            double Myz2 = Myz * Myz;
            double A2 = 4 * Cov_xy - 3 * Mz * Mz - Mzz;
            double A1 = Mzz * Mz + 4 * Cov_xy * Mz - Mxz2 - Myz2 - Mz * Mz * Mz;
            double A0 = Mxz2 * Myy + Myz2 * Mxx - Mzz * Cov_xy - 2 * Mxz * Myz * Mxy + Mz * Mz * Cov_xy;
            double A22 = A2 + A2;
            double epsilon = 1e-12;
            double ynew = 1e+20;
            double IterMax = 20;
            double xnew = 0;

            // Newton's method starting at x=0

            for (int iter = 0; iter < IterMax; iter++)
            {
                double yold = ynew;
                ynew = A0 + xnew * (A1 + xnew * (A2 + 4 * xnew * xnew));
                if (Math.Abs(ynew) > Math.Abs(yold))
                {
                    //System.Windows.Forms.MessageBox.Show("Newton-Pratt goes wrong direction: |" + ynew + "| > |" + yold + "|");
                    xnew = 0;
                    break;
                }
                double Dy = A1 + xnew * (A22 + 16 * xnew * xnew);
                double xold = xnew;
                xnew = xold - ynew / Dy;
                if (Math.Abs((xnew - xold) / xnew) < epsilon) break;
                if (iter > IterMax)
                {
                    //System.Windows.Forms.MessageBox.Show("Newton-Pratt will not converge");
                    xnew = 0;
                }
                if (xnew < 0)
                {
                    //System.Windows.Forms.MessageBox.Show("Newton-Pratt negative root:  x=" + xnew);
                    xnew = 0;
                }

            }

            // computing the circle parameters
            double DET = xnew * xnew - xnew * Mz + Cov_xy;
            Point Center = (Point)(new Vector(Mxz * (Myy - xnew) - Myz * Mxy, Myz * (Mxx - xnew) - Mxz * Mxy) / DET / 2);
            return new Circle(Center + (Vector)centroid, Math.Sqrt(Center.X * Center.X + Center.Y * Center.Y + Mz + 2 * xnew));

        }

        /// <summary>
        /// 計算兩點群的同心圓。
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="ps2"></param>
        /// <returns></returns>
        public static Circle[] GetRegressedCirclesConcentric(this IEnumerable<Point> ps,IEnumerable<Point> ps2)
        {
            var c = ps.Concat(ps2).GetRegressedCircle().Center;
            return new Circle[] 
            {
                new Circle(c, ps.Select(p => (p - c).Length).Average()),
                new Circle(c, ps2.Select(p => (p - c).Length).Average()),
            };
        }

        /// <summary>
        /// 獲得點群資料的最小平方法回歸圓弧。
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Arc GetRegressedArc(this IEnumerable<Point> ps)
        {
            Circle circle = GetRegressedCircle(ps);
            double strAngle = (ps.First() - circle.Center).Angle;
            double endAngle = (ps.Last() - circle.Center).Angle;
            double passthrough = (ps.ElementAt(ps.Count() / 2) - circle.Center).Angle;
            if (Geometry.IsAngleInRange(passthrough, strAngle, endAngle))
                return new Arc(circle.Center, circle.Radius, strAngle, endAngle);
            else
                return new Arc(circle.Center, circle.Radius, endAngle, strAngle);
        }

        /// <summary>
        /// 慟！！！！此方法還沒實作！！！！！
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        internal static Ellipse GetRegressedEllipse(this IEnumerable<Point> ps)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 拋物線擬合。(y = ax^2 + bx + c)
        /// </summary>
        /// <param name="ps">datas point</param>
        /// <param name="a">f(x) = ax^2 + bx + c</param>
        /// <param name="b">f(x) = ax^2 + bx + c</param>
        /// <param name="c">f(x) = ax^2 + bx + c</param>
        public static bool GetParabolaCurve(this IEnumerable<Point> ps, out double a, out double b, out double c)
        {
            a = b = c = double.NaN;
            int n = ps.Count();
            if (n < 2) return false;

            //a + bx + cx^2 = f(x)
            double sx = ps.Sum(p => p.X);
            double sx2 = ps.Sum(p => Math.Pow(p.X, 2));
            double sx3 = ps.Sum(p => Math.Pow(p.X, 3));
            double sx4 = ps.Sum(p => Math.Pow(p.X, 4));
            double sy = ps.Sum(p => p.Y);
            double sxy = ps.Sum(p => p.X * p.Y);
            double sx2y = ps.Sum(p => Math.Pow(p.X, 2) * p.Y);

            //solve linear equation
            var result = Gauss(new double[,] { { n, sx, sx2, sy }, { sx, sx2, sx3, sxy }, { sx2, sx3, sx4, sx2y } });

            //get parameters of para
            if (result != null && result.Length >= 2)
            {
                a = result[2];
                b = result[1];
                c = result[0];
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 獲得擬合的拋物線方程式。
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Func<double, double> GetParabolaCurve(this IEnumerable<Point> ps)
        {
            GetParabolaCurve(ps, out double a, out double b, out double c);
            return new Func<double, double>(x => a * x * x + b * x + c);
        }

        #region Native
        internal static void LineH(IEnumerable<Point> ps, out double rSquared, out double yIntercept, out double slope)
        {
            double sumOfX = 0, sumOfY = 0, sumOfXSq = 0, sumOfYSq = 0, sumCodeviates = 0;

            int count = ps.Count();
            foreach (Point p in ps)
            {
                double x = p.X, y = p.Y;
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            double ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            double ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            double rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            double sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR;
            yIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
        internal static void LineV(IEnumerable<Point> ps, out double rSquared, out double xIntercept, out double slope)
        {
            double sumOfX = 0, sumOfY = 0, sumOfXSq = 0, sumOfYSq = 0, sumCodeviates = 0;
            int count = ps.Count();
            foreach(Point p in ps)
            {
                double x = p.Y, y = p.X;
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            double ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            double ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            double rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            double sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = rNumerator / Math.Sqrt(rDenom);

            rSquared = dblR * dblR;
            xIntercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }
        internal static void ParalleLineH(IEnumerable<Point> _L1ps, IEnumerable<Point> _L2ps, out Line line1, out Line line2)
        {
            double _L1_x2 = _L1ps.Sum(p => p.X * p.X);
            double _L1_x = _L1ps.Sum(p => p.X);
            double _L1_y = _L1ps.Sum(p => p.Y);
            double _L1_xy = _L1ps.Sum(p => p.X * p.Y);

            double _L2_x2 = _L2ps.Sum(p => p.X * p.X);
            double _L2_x = _L2ps.Sum(p => p.X);
            double _L2_y = _L2ps.Sum(p => p.Y);
            double _L2_xy = _L2ps.Sum(p => p.X * p.Y);

            double a0 = _L1_x2 + _L2_x2, b0 = _L1_x, c0 = _L2_x, d0 = _L1_xy + _L2_xy;
            double a1 = _L1_x, b1 = _L1ps.Count(), d1 = _L1_y;
            double a2 = _L2_x,  c2 = _L2ps.Count(), d2 = _L2_y;

            double a = (d0 - d1 / b1 * b0 - d2 / c2 * c0) / (a0 - a1 / b1 * b0 - a2 / c2 * c0);
            double b = (d1 - a1 * a) / b1;
            double c = (d2 - a2 * a) / c2;
            line1 = new Line(a, -1, b);
            line2 = new Line(a, -1, c);
        }
        internal static void ParalleLineV(IEnumerable<Point> _L1ps, IEnumerable<Point> _L2ps, out Line line1, out Line line2)
        {
            double _L1_x2 = _L1ps.Sum(p => p.Y * p.Y);
            double _L1_x = _L1ps.Sum(p => p.Y);
            double _L1_y = _L1ps.Sum(p => p.X);
            double _L1_xy = _L1ps.Sum(p => p.X * p.Y);

            double _L2_x2 = _L2ps.Sum(p => p.Y * p.Y);
            double _L2_x = _L2ps.Sum(p => p.Y);
            double _L2_y = _L2ps.Sum(p => p.X);
            double _L2_xy = _L2ps.Sum(p => p.X * p.Y);

            double a0 = _L1_x2 + _L2_x2, b0 = _L1_x, c0 = _L2_x, d0 = _L1_xy + _L2_xy;
            double a1 = _L1_x, b1 = _L1ps.Count(),  d1 = _L1_y;
            double a2 = _L2_x, c2 = _L2ps.Count(), d2 = _L2_y;

            double a = (d0 - d1 / b1 * b0 - d2 / c2 * c0) / (a0 - a1 / b1 * b0 - a2 / c2 * c0);
            double b = (d1 - a1 * a) / b1;
            double c = (d2 - a2 * a) / c2;
            line1 = new Line(-1, a, b);
            line2 = new Line(-1, a, c);
        }

        static double[] Gauss(double[,] matrix)
        {
            if (matrix == null) throw new ArgumentNullException("matrix");

            //无解
            int cols = matrix.GetLength(1);
            if (cols <= 1) return null;

            //有无穷解
            int rows = matrix.GetLength(0);
            if (rows < cols - 1) return new double[] { };

            //转换为行阶梯
            for (int i = 0; i < rows - 1; i++)
            {
                //选取主元（提高计算精度）
                GaussPivoting(matrix, i, rows, cols);

                //消去一列
                for (int k = i + 1; k < rows; k++)
                {
                    if (matrix[k, i] != 0)
                    {
                        double t = matrix[i, i] / matrix[k, i];
                        for (int j = i; j < cols - 1; j++)
                        {
                            matrix[k, j] = matrix[i, j] - matrix[k, j] * t;
                        }
                        matrix[k, cols - 1] = matrix[i, cols - 1] - matrix[k, cols - 1] * t;
                    }
                }
            }

            //检查秩，判断是否有唯一解
            int rank1 = 0, rank2 = 0;
            for (int i = 0; i < rows; i++)
            {
                bool isZeroRow = true;
                for (int j = i; j < cols - 1; j++)
                {
                    if (matrix[i, j] != 0)
                    {
                        isZeroRow = false;
                        break;
                    }
                }
                if (!isZeroRow) rank1++;
                if (!isZeroRow || matrix[i, cols - 1] != 0) rank2++;
            }

            //如果矩阵的秩小于增广矩阵的秩，则无解
            if (rank1 < rank2) return null;

            //如果矩阵的秩小于方程组的数量，则有无穷解
            if (rank1 < cols - 1) return new double[] { };

            //从底往上依次求解
            double[] result = new double[cols - 1];
            for (int i = rows - 1; i >= 0; i--)
            {
                double y = matrix[i, cols - 1];
                for (int j = i + 1; j < cols - 1; j++)
                {
                    y -= matrix[i, j] * result[j];
                }
                result[i] = matrix[i, i] != 0 ? Math.Round(y / matrix[i, i], 3) : 0;
            }

            return result;
        }
        static void GaussPivoting(double[,] matrix, int i, int rows, int cols)
        {
            //选主元
            int pivotRow = i;
            for (int k = i + 1; k < rows; k++)
            {
                if (Math.Abs(matrix[k, i]) > Math.Abs(matrix[pivotRow, i]))
                    pivotRow = k;
            }

            //交换行
            if (pivotRow != i)
            {
                double tmp = 0;
                for (int j = 0; j < cols; j++)
                {
                    tmp = matrix[i, j];
                    matrix[i, j] = matrix[pivotRow, j];
                    matrix[pivotRow, j] = tmp;
                }
            }

            //除以主元，当前主元变为1
            double pivot = matrix[i, i];
            if (pivot != 0)
            {
                for (int j = i; j < cols; j++)
                    matrix[i, j] /= pivot;
            }
        }
        #endregion


    }
}
