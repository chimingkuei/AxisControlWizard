using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DeepWise.Metrology
{
    public static class EdgeDetection
    {
        #region New
        public static T FindShape<T>(IntPtr scan0, System.Drawing.Size size, int stride, IScannable roi,out double score, EdgeContrast contrast = EdgeContrast.Median, EdgelSearchOptions searchOprations = EdgelSearchOptions.First | EdgelSearchOptions.Crest, float breakDistance = -1, float excludeRadius = -1) where T : IShape
        {
            var ps = GetShpaeEdgels<T>(scan0, size, stride, roi, contrast, searchOprations, breakDistance, excludeRadius);
            score = (double)ps.Length / roi.GetLines().Length;
            switch (typeof(T))
            {
                case Type t when t == typeof(Line):
                    return (T)(object)ps.GetRegressedLine();
                case Type t when t == typeof(Segment):
                    return (T)(object)ps.GetRegressedSegment();
                case Type t when t == typeof(Circle):
                    return (T)(object)ps.GetRegressedCircle();
                case Type t when t == typeof(Arc):
                    return (T)(object)ps.GetRegressedArc();
                default:
                    throw new Exception($"不支援類型'{nameof(T)}'的邊緣偵測");
            }
        }
        public static T FindShape<T>(IntPtr scan0, System.Drawing.Size size, int stride, IScannable roi, EdgeContrast contrast = EdgeContrast.Median, EdgelSearchOptions searchOprations = EdgelSearchOptions.First | EdgelSearchOptions.Crest, float breakDistance = -1, float excludeRadius = -1) where T : IShape
        {
            var ps = GetShpaeEdgels<T>(scan0, size, stride, roi, contrast, searchOprations, breakDistance, excludeRadius);
            switch (typeof(T))
            {
                case Type t when t == typeof(Line):
                    return (T)(object)ps.GetRegressedLine();
                case Type t when t == typeof(Segment):
                    return (T)(object)ps.GetRegressedSegment();
                case Type t when t == typeof(Circle):
                    return (T)(object)ps.GetRegressedCircle();
                case Type t when t == typeof(Arc):
                    return (T)(object)ps.GetRegressedArc();
                default:
                    throw new Exception($"不支援類型'{nameof(T)}'的邊緣偵測");
            }
        }
        public static Point[] GetShpaeEdgels<T>(IntPtr scan0, System.Drawing.Size size, int stride, IScannable roi, EdgeContrast contrast = EdgeContrast.Median, EdgelSearchOptions searchOprations = EdgelSearchOptions.First | EdgelSearchOptions.Crest, float breakDistance = -1, float excludeRadius = -1) where T : IShape
        {
            IEnumerable<Point> ps = FindEdgels(scan0,size,stride,roi,contrast,searchOprations, breakDistance, excludeRadius);
            var psCount = ps.Count();
            var lines = roi.GetLines().Length;
         
            if (typeof(T) == typeof(Line) || typeof(T) == typeof(Segment))
            {
                if (breakDistance!=-1)
                {
                    if (roi is RectRotatable rect)
                    {
                        //Order bottom direction
                        var dir = rect.Down.UnitVector;
                        //var lt = rect.Location;
                        //ps = ps.OrderBy(p => Vector.Dot(p - lt, dir));
                        ps = ps.OrderBy(p => Vector.Dot((Vector)p, dir));
                        List<List<Point>> pps = new List<List<Point>>();
                        List<Point> acc = null;
                        double crt = double.MinValue;
                        foreach (var p in ps)
                        {
                            var tmp = Vector.Dot((Vector)p, dir);
                            //var tmp = Vector.Dot(p - lt, dir);
                            if (Math.Abs(crt - tmp) < breakDistance)
                                acc.Add(p);
                            else
                            {
                                if (acc != null) pps.Add(acc);
                                acc = new List<Point>() { p };
                            }
                            crt = tmp;
                        }
                        pps.Add(acc);
                        ps = pps.OrderByDescending(ts => ts.Count).First();
                    }
                }

                if (excludeRadius!=-1)
                {
                    var line = ps.GetRegressedLine();
                    var ds = excludeRadius * excludeRadius;
                    int oriCount = ps.Count();
                    System.Diagnostics.Debug.WriteLine(oriCount);
                    ps = ps.Where(p => p.GetDistanceSquared(line) < ds);
                    System.Diagnostics.Debug.WriteLine(ps.Count() - oriCount);
                }
            }
            else if (typeof(T) == typeof(Circle) || typeof(T) == typeof(Arc))
            {
                ps = ps.Where(p => !double.IsInfinity(p.X) && !double.IsNaN(p.X));
                var circle = ps.GetRegressedCircle();
                if (excludeRadius!=-1)
                    ps = ps.Where(x => x.GetDistance(circle, out _) > excludeRadius);
            }
            return ps.ToArray();
        }
        public static Point[] FindEdgels(IntPtr scan0, System.Drawing.Size size, int stride, IScannable shape, EdgeContrast contrast = EdgeContrast.Median, EdgelSearchOptions searchOprations = EdgelSearchOptions.First | EdgelSearchOptions.Crest, float breakDistance = -1, float excludeRadius = -1)
        {
            //EdgelSearchMode SearchMode = setting.SearchMode;
            EdgelSearchOptions type = searchOprations;
            if ((type & EdgelSearchOptions.First) != 0 && (type & EdgelSearchOptions.Largest) != 0) throw new Exception($"邊緣搜尋模式不可同時為'{nameof(EdgelSearchOptions.First)}'以及'{nameof(EdgelSearchOptions.Largest)}'");
            if ((type & (EdgelSearchOptions.First | EdgelSearchOptions.Largest)) == 0) throw new Exception($"邊緣搜尋模式必須為'{nameof(EdgelSearchOptions.First)}'或者'{nameof(EdgelSearchOptions.Largest)}'");

            bool first = (type & EdgelSearchOptions.First) != 0;

            float min;
            switch (contrast)
            {
                case EdgeContrast.Small: min = 5; break;
                case EdgeContrast.Median: min = 10; break;
                case EdgeContrast.Larget: min = 15; break;
                default: throw new Exception();
            }
            float max = float.MaxValue;

            //if (!(image.PixelFormat == ImageFormat.Mono8 || image.PixelFormat == ImageFormat.Mono16)) throw new Exception("影像必需為灰階格式");
            var lines = shape.GetLines();
            var rect = new System.Drawing.Rectangle(0, 0, size.Width, size.Height);
            Point[] ps = new Point[lines.Length];
            //New
            Parallel.For(0, lines.Length/3, i =>
            {
                i = i * 3;
                System.Drawing.Point[] line = GetBresenhamLine(lines[i].from, lines[i].to, rect);
                float[] diff = GetDerivative(GetPixelsMono8(scan0,size,stride,line));
                int index = -1;
                //if (SearchMode == EdgelSearchMode.First) GetFirstCrest(diff, out index, type, min, max);
                //else if (SearchMode == EdgelSearchMode.Largest) GetLargestCrest(diff, out index, type, min, max);
                if (first)
                    GetFirstCrest(diff, out index, type, min, max);
                else
                    GetLargestCrest(diff, out index, type, min, max);

                if (index > 1 && index < line.Length - 2)
                {
                    if (SubpixelsMode == SubpixelsInterpolationMode.Parabola) ps[i] = (GetEdgelParabola(index, line, diff));
                    else if (SubpixelsMode == SubpixelsInterpolationMode.Weight) ps[i] = (GetEdgelWeight(index, line, diff));
                    else throw new NotImplementedException();
                }
                else
                {
                    ps[i] = Point.NaN;
                }
            });
            //====================================
            return ps.Where(p => p != Point.Zero && !double.IsNaN(p.X)).ToArray();
        }
        #endregion
        //Setting
        /// <summary>
        /// 取得或設定邊緣偵測的次像素模式。
        /// </summary>
        public static SubpixelsInterpolationMode SubpixelsMode { get; set; } = SubpixelsInterpolationMode.Parabola;

        public static T GetShape<T>(this BitmapData image, IScannable roi, EdgeDetectionSetting setting = null) where T : IShape
        {
            if (setting == null) setting = EdgeDetectionSetting.Default;
            if (image.PixelFormat != PixelFormat.Format8bppIndexed) throw new Exception("data isn't a mono8 format.");
            var ps = image.GetShpaeEdgels<T>(roi, setting);

            T result;
            switch (typeof(T))
            {
                case Type t when t == typeof(Line):
                    result = (T)(object)ps.GetRegressedLine();
                    break;
                case Type t when t == typeof(Segment):
                    if(roi is RectRotatable rect)
                    {
                        var line = ps.GetRegressedLine();
                        Shapes.Geometry.Intersect(rect, line, out var hits);
                        result = (T)(object)new Segment(hits[0], hits[1]);
                    }
                    else
                        result = (T)(object)ps.GetRegressedSegment();
                    break;
                case Type t when t == typeof(Circle):
                    result = (T)(object)ps.GetRegressedCircle();
                    break;
                case Type t when t == typeof(Arc):
                    result = (T)(object)ps.GetRegressedArc();
                    break;
                default:
                    throw new Exception($"不支援類型'{nameof(T)}'的邊緣偵測");
            }
            return result;
        }
        public static T GetShape<T>(this OpenCvSharp.Mat image, IScannable roi, EdgeDetectionSetting setting = null) where T : IShape
        {
            if (setting == null) setting = EdgeDetectionSetting.Default;
            if (image.Type() != OpenCvSharp.MatType.CV_8UC1) throw new Exception("data isn't a mono8 format.");
            var ps = image.GetShpaeEdgels<T>(roi, setting);

            T result;
            switch (typeof(T))
            {
                case Type t when t == typeof(Line):
                    result = (T)(object)ps.GetRegressedLine();
                    break;
                case Type t when t == typeof(Segment):
                    if (roi is RectRotatable rect)
                    {
                        var line = ps.GetRegressedLine();
                        Shapes.Geometry.Intersect(rect, line, out var hits);
                        result = (T)(object)new Segment(hits[0], hits[1]);
                    }
                    else
                        result = (T)(object)ps.GetRegressedSegment();
                    break;
                case Type t when t == typeof(Circle):
                    result = (T)(object)ps.GetRegressedCircle();
                    break;
                case Type t when t == typeof(Arc):
                    result = (T)(object)ps.GetRegressedArc();
                    break;
                default:
                    throw new Exception($"不支援類型'{nameof(T)}'的邊緣偵測");
            }
            return result;
        }
        public static Point[] GetShpaeEdgels<T>(this BitmapData image, IScannable roi, EdgeDetectionSetting setting = null) where T : IShape
        {
            if (setting == null) setting = EdgeDetectionSetting.Default;
            if (image.PixelFormat != PixelFormat.Format8bppIndexed) throw new Exception("data isn't a mono8 format.");
            IEnumerable<Point> ps = image.GetEdgels(roi, setting);
            if (typeof(T) == typeof(Line) || typeof(T) == typeof(Segment))
            {
                if(setting.BreakIntoGroups)
                {
                    if (roi is RectRotatable rect)
                    {
                        //Order bottom direction
                        var dir = rect.Down.UnitVector;
                        //var lt = rect.Location;
                        //ps = ps.OrderBy(p => Vector.Dot(p - lt, dir));
                        ps = ps.OrderBy(p => Vector.Dot((Vector)p , dir));
                        List<List<Point>> pps = new List<List<Point>>();
                        List<Point> acc = null;
                        double crt = double.MinValue;
                        foreach (var p in ps)
                        {
                            var tmp = Vector.Dot((Vector)p , dir);
                            //var tmp = Vector.Dot(p - lt, dir);
                            if (Math.Abs(crt - tmp) < setting.BreakDistance)
                                acc.Add(p);
                            else
                            {
                                if (acc != null) pps.Add(acc);
                                acc = new List<Point>() { p };
                            }
                            crt = tmp;
                        }
                        pps.Add(acc);
                        ps = pps.OrderByDescending(ts => ts.Count).First();
                    }
                }

                if (setting.ExcludeOutliers)
                {
                    var line = ps.GetRegressedLine();
                    var ds = setting.ExcludeRadius * setting.ExcludeRadius;
                    int oriCount = ps.Count();
                    System.Diagnostics.Debug.WriteLine(oriCount);
                    ps = ps.Where(p => p.GetDistanceSquared(line) < ds);
                    System.Diagnostics.Debug.WriteLine(ps.Count() - oriCount);
                }
            }
            else if (typeof(T) == typeof(Circle) || typeof(T) == typeof(Arc))
            {
                ps = ps.Where(p => !double.IsInfinity(p.X) && !double.IsNaN(p.X));
                var circle = ps.GetRegressedCircle();
                if(setting.ExcludeOutliers)
                ps = ps.Where(x => x.GetDistance(circle, out _) > setting.ExcludeRadius);
            }
            return ps.ToArray();
        }
        public static Point[] GetShpaeEdgels<T>(this OpenCvSharp.Mat image, IScannable roi, EdgeDetectionSetting setting = null) where T : IShape
        {
            if (setting == null) setting = EdgeDetectionSetting.Default;
            if (image.Type() != OpenCvSharp.MatType.CV_8UC1) throw new Exception("data isn't a mono8 format.");
            IEnumerable<Point> ps = image.GetEdgels(roi, setting);
            if (typeof(T) == typeof(Line) || typeof(T) == typeof(Segment))
            {
                if (setting.BreakIntoGroups)
                {
                    if (roi is RectRotatable rect)
                    {
                        //Order bottom direction
                        var dir = rect.Down.UnitVector;
                        //var lt = rect.Location;
                        //ps = ps.OrderBy(p => Vector.Dot(p - lt, dir));
                        ps = ps.OrderBy(p => Vector.Dot((Vector)p, dir));
                        List<List<Point>> pps = new List<List<Point>>();
                        List<Point> acc = null;
                        double crt = double.MinValue;
                        foreach (var p in ps)
                        {
                            var tmp = Vector.Dot((Vector)p, dir);
                            //var tmp = Vector.Dot(p - lt, dir);
                            if (Math.Abs(crt - tmp) < setting.BreakDistance)
                                acc.Add(p);
                            else
                            {
                                if (acc != null) pps.Add(acc);
                                acc = new List<Point>() { p };
                            }
                            crt = tmp;
                        }
                        pps.Add(acc);
                        ps = pps.OrderByDescending(ts => ts.Count).First();
                    }
                }

                if (setting.ExcludeOutliers)
                {
                    var line = ps.GetRegressedLine();
                    var ds = setting.ExcludeRadius * setting.ExcludeRadius;
                    int oriCount = ps.Count();
                    System.Diagnostics.Debug.WriteLine(oriCount);
                    ps = ps.Where(p => p.GetDistanceSquared(line) < ds);
                    System.Diagnostics.Debug.WriteLine(ps.Count() - oriCount);
                }
            }
            else if (typeof(T) == typeof(Circle) || typeof(T) == typeof(Arc))
            {
                ps = ps.Where(p => !double.IsInfinity(p.X) && !double.IsNaN(p.X));
                var circle = ps.GetRegressedCircle();
                if (setting.ExcludeOutliers)
                    ps = ps.Where(x => x.GetDistance(circle, out _) > setting.ExcludeRadius);
            }
            return ps.ToArray();
        }
        public static Point[] GetEdgels(this BitmapData image, IScannable shape, EdgeDetectionSetting setting)
        {
            if (setting == null) setting = EdgeDetectionSetting.Default;
            //EdgelSearchMode SearchMode = setting.SearchMode;
            EdgelSearchOptions type = setting.SearchType;
            if ((type & EdgelSearchOptions.First) != 0 && (type & EdgelSearchOptions.Largest) != 0) throw new Exception($"邊緣搜尋模式不可同時為'{nameof(EdgelSearchOptions.First)}'以及'{nameof(EdgelSearchOptions.Largest)}'");
            if ((type & (EdgelSearchOptions.First | EdgelSearchOptions.Largest)) == 0) throw new Exception($"邊緣搜尋模式必須為'{nameof(EdgelSearchOptions.First)}'或者'{nameof(EdgelSearchOptions.Largest)}'");

            bool first = type.HasFlag(EdgelSearchOptions.First);
            bool crest = type.HasFlag(EdgelSearchOptions.Crest);

            float min = setting.MinimumEdgeValue;
            float max = setting.MaximumEdgeValue;

            //if (!(image.PixelFormat == ImageFormat.Mono8 || image.PixelFormat == ImageFormat.Mono16)) throw new Exception("影像必需為灰階格式");
            var lines = shape.GetLines();
            var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
            Point[] ps = new Point[lines.Length];
            //New
            Parallel.For(0, lines.Length, i =>
            {

                System.Drawing.Point[] line = GetBresenhamLine(lines[i].from, lines[i].to, rect);
                float[] diff = GetDerivative(image.GetPixelsMono8(line));
                if (!crest) diff = diff.Select(x => -x).ToArray();
                int index = -1;
                //if (SearchMode == EdgelSearchMode.First) GetFirstCrest(diff, out index, type, min, max);
                //else if (SearchMode == EdgelSearchMode.Largest) GetLargestCrest(diff, out index, type, min, max);
                if (first)
                    GetFirstCrest(diff, out index, type, min, max);
                else
                    GetLargestCrest(diff, out index, type, min, max);

                if (index > 1 && index < line.Length - 2)
                {
                    switch (SubpixelsMode)
                    {
                        case SubpixelsInterpolationMode.Parabola:
                            ps[i] = (GetEdgelParabola(index, line, diff));
                            break;
                        case SubpixelsInterpolationMode.Weight:
                            ps[i] = GetEdgelWeight(index, line, diff);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                
            });
            //====================================
            return ps.Where(p => !Point.IsNaN(p)).ToArray();
        }
        public static Point[] GetEdgels(this OpenCvSharp.Mat image, IScannable shape, EdgeDetectionSetting setting)
        {
            if (setting == null) setting = EdgeDetectionSetting.Default;
            //EdgelSearchMode SearchMode = setting.SearchMode;
            EdgelSearchOptions type = setting.SearchType;
            if ((type & EdgelSearchOptions.First) != 0 && (type & EdgelSearchOptions.Largest) != 0) throw new Exception($"邊緣搜尋模式不可同時為'{nameof(EdgelSearchOptions.First)}'以及'{nameof(EdgelSearchOptions.Largest)}'");
            if ((type & (EdgelSearchOptions.First | EdgelSearchOptions.Largest)) == 0) throw new Exception($"邊緣搜尋模式必須為'{nameof(EdgelSearchOptions.First)}'或者'{nameof(EdgelSearchOptions.Largest)}'");

            bool first = type.HasFlag(EdgelSearchOptions.First);
            bool crest = type.HasFlag(EdgelSearchOptions.Crest);

            float min = setting.MinimumEdgeValue;
            float max = setting.MaximumEdgeValue;

            //if (!(image.PixelFormat == ImageFormat.Mono8 || image.PixelFormat == ImageFormat.Mono16)) throw new Exception("影像必需為灰階格式");
            var lines = shape.GetLines();
            var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
            Point[] ps = new Point[lines.Length];
            //New
            Parallel.For(0, lines.Length, i =>
            {

                System.Drawing.Point[] line = GetBresenhamLine(lines[i].from, lines[i].to, rect);
                float[] diff = GetDerivative(image.GetPixelsMono8(line));
                if (!crest) diff = diff.Select(x => -x).ToArray();
                int index = -1;
                //if (SearchMode == EdgelSearchMode.First) GetFirstCrest(diff, out index, type, min, max);
                //else if (SearchMode == EdgelSearchMode.Largest) GetLargestCrest(diff, out index, type, min, max);
                if (first)
                    GetFirstCrest(diff, out index, type, min, max);
                else
                    GetLargestCrest(diff, out index, type, min, max);

                if (index > 1 && index < line.Length - 2)
                {
                    switch (SubpixelsMode)
                    {
                        case SubpixelsInterpolationMode.Parabola:
                            ps[i] = (GetEdgelParabola(index, line, diff));
                            break;
                        case SubpixelsInterpolationMode.Weight:
                            ps[i] = GetEdgelWeight(index, line, diff);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

            });
            //====================================
            return ps.Where(p => !Point.IsNaN(p)).ToArray();
        }
        public static void GetEdgeValues(this BitmapData data, Point origin, Point to, out float[] pixelsValues, out float[] edgeValues)
        {
            System.Drawing.Point[] line = GetBresenhamLine(origin, to, new System.Drawing.Rectangle(0, 0, data.Width, data.Height));
            byte[] pixels = data.GetPixelsMono8(line);
            pixelsValues = pixels.Select(x => (float)x).ToArray();
            edgeValues = GetDerivative(pixels);
        }
        public unsafe static byte[] GetPixelsMono8(IntPtr _scan0,System.Drawing.Size size,int stride, System.Drawing.Point[] ps)
        {
            int l = ps.Length;
            byte[] pixels = new byte[l];

            byte* scan0 = (byte*)_scan0;
            int w = size.Width;
            int h = size.Height;
            Parallel.For(0, l, i =>
            {
                if (ps[i].X >= 0 && ps[i].X < w && ps[i].Y >= 0 && ps[i].Y < h)
                {
                    pixels[i] = scan0[stride * ps[i].Y + ps[i].X];
                }
                else
                {
                    System.Drawing.Point p = ps[i];
                    throw new Exception("錯誤:像素在圖片之外");
                }
            });
            return pixels;
        }
        public unsafe static byte[] GetPixelsMono8(this BitmapData data, System.Drawing.Point[] ps)
        {
            int l = ps.Length;
            byte[] pixels = new byte[l];

            byte* scan0 = (byte*)data.Scan0;
            int stride = data.Width;
            Parallel.For(0, l, i =>
            {
                if (ps[i].X >= 0 && ps[i].X < data.Width && ps[i].Y >= 0 && ps[i].Y < data.Height)
                {
                    pixels[i] = scan0[stride * ps[i].Y + ps[i].X];
                }
                else
                {
                    System.Drawing.Point p = ps[i];
                    throw new Exception("錯誤:像素在圖片之外");
                }
            });
            return pixels;
        }
        public unsafe static byte[] GetPixelsMono8(this OpenCvSharp.Mat data, System.Drawing.Point[] ps)
        {
            int l = ps.Length;
            byte[] pixels = new byte[l];

            byte* scan0 = (byte*)data.Data;
            int stride = (int)data.Step();
            Parallel.For(0, l, i =>
            {
                if (ps[i].X >= 0 && ps[i].X < data.Width && ps[i].Y >= 0 && ps[i].Y < data.Height)
                {
                    pixels[i] = scan0[stride * ps[i].Y + ps[i].X];
                }
                else
                {
                    System.Drawing.Point p = ps[i];
                    throw new Exception("錯誤:像素在圖片之外");
                }
            });
            return pixels;
        }
        //Interface

        public static System.Drawing.Point[] GetBresenhamLine(int x0, int y0, int x1, int y1)
        {
            if (x0 == x1 && y0 == y1) return new System.Drawing.Point[] { new System.Drawing.Point(x0, y0) };
            List<System.Drawing.Point> ps = new List<System.Drawing.Point>();

            {
                int dx, dy, i, e;
                int incx, incy, inc1, inc2;
                int x, y;

                dx = x1 - x0;
                dy = y1 - y0;

                if (dx < 0) dx = -dx;
                if (dy < 0) dy = -dy;
                incx = 1;
                if (x1 < x0) incx = -1;
                incy = 1;
                if (y1 < y0) incy = -1;
                x = x0; y = y0;
                if (dx > dy)
                {
                    ps.Add(new System.Drawing.Point(x, y));
                    e = 2 * dy - dx;
                    inc1 = 2 * (dy - dx);
                    inc2 = 2 * dy;
                    for (i = 0; i < dx; i++)
                    {
                        if (e >= 0)
                        {
                            y += incy;
                            e += inc1;
                        }
                        else
                            e += inc2;
                        x += incx;
                        ps.Add(new System.Drawing.Point(x, y));
                    }

                }
                else
                {
                    ps.Add(new System.Drawing.Point(x, y));
                    e = 2 * dx - dy;
                    inc1 = 2 * (dx - dy);
                    inc2 = 2 * dx;
                    for (i = 0; i < dy; i++)
                    {
                        if (e >= 0)
                        {
                            x += incx;
                            e += inc1;
                        }
                        else
                            e += inc2;
                        y += incy;
                        ps.Add(new System.Drawing.Point(x, y));
                    }
                }
            }
            return ps.ToArray();
            //List<System.Drawing.Point> ps = new List<System.Drawing.Point>();
            //bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            //if (steep)
            //{
            //    int tmp = x0;
            //    x0 = y0; y0 = tmp;
            //    tmp = x1;
            //    x1 = y1;y1 = tmp;
            //}

            //int deltaX = x1 - x0;
            //int deltaY = y1 > y0 ? y1 - y0 : y0 - y1;
            //int error = deltaX / 2;

            //int y = y0;
            //int ystep = y0 < y1 ? 1 :- 1;
            //if(x1 > x0)
            //    for (int x = x0; x <= x1; x++)
            //    {
            //        if (steep)
            //            ps.Add(new System.Drawing.Point(y, x));
            //        else
            //            ps.Add(new System.Drawing.Point(x, y));
            //        error -= deltaY;
            //        if (error < 0)
            //        {
            //            y += ystep;
            //            error += deltaX;
            //        }
            //    }
            //else
            //    for (int x = x0; x >= x1; x--)
            //    {
            //        if (steep)
            //            ps.Add(new System.Drawing.Point(y, x));
            //        else
            //            ps.Add(new System.Drawing.Point(x, y));
            //        error -= deltaY;
            //        if (error < 0)
            //        {
            //            y += ystep;
            //            error += deltaX;
            //        }
            //    }
            //return ps.ToArray();
        }
        public static System.Drawing.Point[] GetBresenhamLine(Point p0, Point p1)
        {
            int x0 = (int)(p0.X + 0.5), y0 = (int)(p0.Y + 0.5), x1 = (int)(p1.X + 0.5), y1 = (int)(p1.Y + 0.5);
            //int x0 = (int)Math.Round(p0.X), y0 = (int)Math.Round(p0.Y), x1 = (int)Math.Round(p1.X), y1 = (int)Math.Round(p1.Y);
            return GetBresenhamLine(x0, y0, x1, y1);
        }
        public static System.Drawing.Point[] GetBresenhamLine(Point p0, Point p1, System.Drawing.Rectangle roi)
        {
            List<System.Drawing.Point> ps = GetBresenhamLine(p0, p1).ToList();
            bool flag = false;
            for (int i = 0; i < ps.Count; i++)
            {
                if (roi.Contains(ps[i]))
                {
                    flag = true;
                    ps.RemoveRange(0, i);
                    for (int j = ps.Count - 1; j >= i; j--)
                    {
                        if (roi.Contains(ps[j]))
                        {
                            ps.RemoveRange(j + 1, ps.Count - 1 - j);
                            break;
                        }
                    }
                    break;
                }
            }

            if (flag)
                return ps.ToArray();
            else
                return new System.Drawing.Point[0];
        }

        #region Native Methods
        internal static Point GetEdgelParabola(int targetIndex, System.Drawing.Point[] line, float[] dp)
        {
            System.Drawing.Point p0 = line[0];
            System.Drawing.Point p1 = line[line.Length - 1];
            Point[] datasPoint = new Point[5];
            double angle = Math.Atan2(line[line.Length - 1].Y - line[0].Y, line[line.Length - 1].X - line[0].X);
            if (Geometry.IsGentle(angle))
            {
                for (int i = 0; i < 5; i++)
                {
                    datasPoint[i] = new Point(line[targetIndex - 2 + i].X, dp[targetIndex - 2 + i]);
                }
                datasPoint.GetParabolaCurve(out var a, out var b, out _);
                double newX = -b / (2 * a);
                //Test
                double min = datasPoint.Select(x => x.X).Min();
                double max = datasPoint.Select(x => x.X).Max();
                var p = new Point(newX, p0.Y + (newX - p0.X) * (p1.Y - p0.Y) / (p1.X - p0.X));
                return new Point(newX, p0.Y + (newX - p0.X) * (p1.Y - p0.Y) / (p1.X - p0.X));
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    datasPoint[i] = new Point(line[targetIndex - 2 + i].Y, dp[targetIndex - 2 + i]);
                }
                datasPoint.GetParabolaCurve(out double a, out double b, out _);
                double newY = -b / (2 * a);
                var p = new Point(p0.X + (newY - p0.Y) * (p1.X - p0.X) / (p1.Y - p0.Y), newY);
                return new Point(p0.X + (newY - p0.Y) * (p1.X - p0.X) / (p1.Y - p0.Y), newY);
            }
        }
        internal static Point GetEdgelWeight(int targetIndex, System.Drawing.Point[] line, float[] dp)
        {
            int index = targetIndex;
            Vector edgel = new Vector();
            float sum = 0;
            for (int i = 0; i < 5; i++)
            {
                int c = index - 2 + i;
                edgel += dp[c] * new Vector(line[c].X, line[c].Y);
                sum += dp[c];
            }
            return (Point)(edgel / sum);
        }


        internal static float[] GetDerivative(byte[] values)
        {
            int number = values.Length;
            if (number < 2) return new float[0];
            float[] driv = new float[number];
            driv[0] = (values[1] - values[0]);
            for (int i = 1; i < number - 1; i++) driv[i] = (values[i + 1] - values[i - 1]) / 2;
            driv[number - 1] = (values[number - 1] - values[number - 2]);
            return driv;
        }
        internal static float[] GetDerivative(ushort[] values)
        {
            int number = values.Length;
            if (number < 2) return new float[0];
            float[] driv = new float[number];
            driv[0] = (values[1] - values[0]);
            for (int i = 1; i < number - 1; i++) driv[i] = (values[i + 1] - values[i - 1]) / 2;
            driv[number - 1] = (values[number - 1] - values[number - 2]);
            return driv;
        }
        internal static bool GetALLCrests(float[] dv, out int[] indexs, EdgelSearchOptions type = EdgelSearchOptions.Crest | EdgelSearchOptions.Trough, float min = 25, float max = float.PositiveInfinity)
        {
            if (dv.Length < 3)
            {
                indexs = new int[0];
                return false;
            }
            List<int> criticalPoints = new List<int>();

            int n = dv.Length;
            int index = -1;
            float currentMax = min - 0.5f;
            for (int i = 0; i < n; i++)
            {
                float value = dv[i];

                if (type == (EdgelSearchOptions.Crest | EdgelSearchOptions.Trough)) value = value > 0 ? value : -value;
                else if (type == EdgelSearchOptions.Trough) value = -value;

                if (value > currentMax)
                {
                    index = i;
                    currentMax = value;
                }
                else if (value < min)
                {
                    if (index != -1)
                    {
                        criticalPoints.Add(index);
                        index = -1;
                        currentMax = min;
                    }
                }
            }

            for (int i = criticalPoints.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(dv[criticalPoints[i]]) > max)
                    criticalPoints.RemoveAt(i);
            }

            indexs = criticalPoints.ToArray();
            return indexs.Length > 0;
        }
        internal static bool GetLargestCrest(float[] dv, out int index, EdgelSearchOptions type = EdgelSearchOptions.Crest | EdgelSearchOptions.Trough, float min = 25, float max = float.PositiveInfinity)
        {
            GetALLCrests(dv, out int[] indexs, type, min, max);
            if (indexs.Length > 0)
            {
                index = indexs[0];
                if (indexs.Length == 1) return true;
                float tmpMax = dv[index] > 0 ? dv[index] : -dv[index];
                for (int i = 1; i < indexs.Length; i++)
                {
                    float v2 = dv[indexs[i]];
                    if (v2 < 0) v2 = -v2;
                    if (v2 > tmpMax)
                    {
                        tmpMax = v2;
                        index = indexs[i];
                    }
                }
                return true;
            }
            else
            {
                index = -1;
                return false;
            }
        }
        internal static bool GetFirstCrest(float[] dv, out int indexs, EdgelSearchOptions type = EdgelSearchOptions.Crest | EdgelSearchOptions.Trough, float min = 25, float max = float.PositiveInfinity)
        {
            indexs = -1;
            if (dv.Length < 3) return false;

            int n = dv.Length;
            int index = -1;
            float currentMax = min;
            for (int i = 0; i < n; i++)
            {
                float value = dv[i];

                if (type == (EdgelSearchOptions.Crest | EdgelSearchOptions.Trough)) value = value > 0 ? value : -value;
                else if (type == EdgelSearchOptions.Trough) value = -value;

                if (value >= currentMax)
                {
                    index = i;
                    currentMax = value;
                }
                else // dv[i] < currentMax
                {
                    if (index != -1)
                    {
                        if (currentMax < max)
                        {
                            indexs = index;
                            return true;
                        }
                        else
                        {
                            index = -1;
                            currentMax = min;
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}