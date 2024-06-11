using DeepWise.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InteropBitmap = System.Windows.Interop.InteropBitmap;
namespace System.Drawing
{
    public static class BitmapExtensions
    {
        public unsafe static void Add(IntPtr scan0, Size size, int stride, byte value)
        {
            byte* str = (byte*)scan0;
            var w = size.Width;
            Parallel.For(0, size.Height, (y =>
            {
                byte* head = str + y * stride;
                for (int i = 0; i < w; i++)
                {
                    var pixel = head[i];
                    var sum = pixel + value;
                    if (sum > 255)
                        head[i] = 255;
                    else
                        head[i] = (byte)sum;
                }
            }));
        }

        public unsafe static void Add(IntPtr scan0, Size size, int stride, byte value, Rectangle roi)
        {
            byte* str = (byte*)scan0;
            var w = size.Width;
            var r = roi.Right + 1;
            Parallel.For(roi.Top, roi.Bottom + 1, (y =>
            {
                byte* head = str + y * stride;
                for (int i = roi.Left; i < r; i++)
                {

                    var pixel = head[i];
                    var sum = pixel + value;
                    if (sum > 255)
                        head[i] = 255;
                    else
                        head[i] = (byte)sum;
                }
            }));

        }

        public static void Add(this Bitmap bitmap, int value)
        {
            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            Add(data.Scan0, bitmap.Size, bitmap.GetStride(), (byte)value);
            bitmap.UnlockBits(data);
        }

        public static void Add(this Bitmap bitmap, int value, Rectangle roi)
        {
            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            Add(data.Scan0, bitmap.Size, bitmap.GetStride(), (byte)value, roi);
            bitmap.UnlockBits(data);
        }

        public static void Add(this BitmapData data, int value)
        {
            Add(data.Scan0, new System.Drawing.Size(data.Width, data.Height), data.Stride, (byte)value);

        }

        public static void Add(this BitmapData data, int value, Rectangle roi)
        {
            Add(data.Scan0, new System.Drawing.Size(data.Width, data.Height), data.Stride, (byte)value, roi);
        }

        static ColorPalette _mono8Palette;
        public static ColorPalette Mono8Palette
        {
            get
            {
                if (_mono8Palette == null) Ini();
                return _mono8Palette;
                void Ini()
                {
                    using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                    {
                        _mono8Palette = bmp.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            _mono8Palette.Entries[i] = Color.FromArgb(i, i, i);
                        }
                    }
                }
            }
        }

        public static BitmapData GetBitmapData(Bitmap bmp) => datas[bmp];
        public static void UnlockBitmapFromInterop(this Bitmap bmp)
        {
            if (ints.ContainsKey(bmp))
            {
                bmp.UnlockBits(datas[bmp]);
                ints.Remove(bmp);
                datas.Remove(bmp);
            }
            else
                throw new Exception();
        }
        private static Dictionary<Bitmap, BitmapData> datas = new Dictionary<Bitmap, BitmapData>();
        private static Dictionary<Bitmap, Windows.Media.Imaging.CachedBitmap> ints = new Dictionary<Bitmap, Windows.Media.Imaging.CachedBitmap>();
        public static Windows.Media.Imaging.CachedBitmap CreateInteropBitmapFromBitmap(this Bitmap bmp)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            datas[bmp] = data;
            var intBmp = (Windows.Media.Imaging.CachedBitmap)InteropBitmap.Create(data.Width, data.Height, 96, 96, bmp.PixelFormat.ToMediaFormat(), bmp.Palette.ToBitmapPalette(), data.Scan0, data.Stride * data.Height, data.Stride);
            ints[bmp] = intBmp;
            return intBmp;
        }

        public static Windows.Media.Imaging.BitmapPalette ToBitmapPalette(this ColorPalette palette)
        {
            if (palette == null | palette.Entries.Length == 0) return null;
            return new Windows.Media.Imaging.BitmapPalette(palette.Entries.Select(ToMediaColor).ToArray());
        }

        public static Windows.Media.Color ToMediaColor(this Color c) => Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);

        public static Bitmap CreateMono8Bitmap(int width,int height)
        {
            var bmp = new Bitmap(width,height,PixelFormat.Format8bppIndexed);
            bmp.Palette = Mono8Palette;
            return bmp;
        }

        [Obsolete("Use BitmapExtensions.Mono8Palette property instead")]
        public static ColorPalette GetMono8Palette()
        {
            ColorPalette palette;
            using(var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
            }
            return palette;
        }
        public static int GetBytesCount(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                default: throw new NotImplementedException();
            }
        }
        
        public static int GetStride(this Bitmap bitmap)
        {
            int Stride = 4 * ((bitmap.Width * bitmap.PixelFormat.GetBytesCount() * 8 + 31) / 32);
            return Stride;
        }

        public static int GetStride(this System.Drawing.Imaging.PixelFormat format,int width)
        {
            int Stride = 4 * ((width * format.GetBytesCount() * 8 + 31) / 32);
            return Stride;
        }
        public static Windows.Media.ImageSource ToImageSource(this Icon icon)
        {
            Windows.Media.ImageSource imageSource = Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Windows.Int32Rect.Empty,
                Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
               bitmap.PixelFormat);

            try
            {
                return System.Windows.Media.Imaging.BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    bitmap.PixelFormat.ToMediaFormat(),
                    null,
                    bitmapData.Scan0,
                     bitmapData.Stride * rect.Height,
                    bitmapData.Stride);
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static Bitmap ToBitmap(this Windows.Media.Imaging.BitmapSource bitmapsource)
        {
            
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new Windows.Media.Imaging.BmpBitmapEncoder();
                enc.Frames.Add(Windows.Media.Imaging.BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        public static Image GetDisabledImage(this Image bmp)
        {
            var newBmp = new Bitmap(bmp.Width, bmp.Height);
            newBmp.MakeTransparent();
            using (Graphics g = Graphics.FromImage(newBmp))
            {
                ControlPaint.DrawImageDisabled(g, bmp, 0, 0, Color.Transparent);
            }
            return newBmp;
        }

        public static Bitmap GetDisabledBitmap(this Bitmap bmp)
        {
            var newBmp = new Bitmap(bmp.Width, bmp.Height);
            newBmp.MakeTransparent();
            using (Graphics g = Graphics.FromImage(newBmp))
            {
                ControlPaint.DrawImageDisabled(g, bmp, 0, 0, Color.Transparent);
            }
            return newBmp;
        }

        public static System.Windows.Media.PixelFormat ToMediaFormat(this System.Drawing.Imaging.PixelFormat format)
        {
            switch (format)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return System.Windows.Media.PixelFormats.Pbgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return System.Windows.Media.PixelFormats.Bgr32;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return System.Windows.Media.PixelFormats.Rgb24;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return System.Windows.Media.PixelFormats.Gray8;
                default:
                    return System.Windows.Media.PixelFormats.Default;
            }
        }

        public static Bitmap UnlockLoad(string path)
        {
            Bitmap result;
            GC.Collect();
            using (var bmp = Bitmap.FromFile(path) as Bitmap)
            {
                result = (Bitmap)bmp.Clone(new Rectangle(0,0,bmp.Width,bmp.Height), bmp.PixelFormat);
            }
            return result;
        }

        public static unsafe Bitmap ConvertToMono8(this Bitmap srcBmp)
        {
            Bitmap destBmp;
            switch (srcBmp.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format8bppIndexed:
                    {
                        int offset = GetBytesCount(srcBmp.PixelFormat);
                        destBmp = new Bitmap(srcBmp.Size.Width, srcBmp.Size.Height, PixelFormat.Format8bppIndexed);
                        destBmp.Palette = Mono8Palette;
                        var dataDest = destBmp.LockBits(new Rectangle(Point.Empty, srcBmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                        var height = srcBmp.Height;
                        var width = srcBmp.Width;
                        var srcData = srcBmp.LockBits(new Rectangle(Point.Empty, srcBmp.Size), ImageLockMode.ReadWrite, srcBmp.PixelFormat);
                        byte* src = (byte*)srcData.Scan0;
                        if(srcBmp.PixelFormat != PixelFormat.Format8bppIndexed )
                        src++;
                        byte* dest = (byte*)dataDest.Scan0;
                        Parallel.For(0, height, j =>
                        {
                            int srcIndex = srcData.Stride * j;
                            int destIndex = dataDest.Stride * j;
                            for (int i = 0; i < width; i++)
                            {
                                dest[destIndex] = src[srcIndex];
                                srcIndex += offset;
                                destIndex++;
                            }
                        });
                        srcBmp.UnlockBits(srcData);
                        destBmp.UnlockBits(dataDest);
                    }
                    break;
                //case PixelFormat.Format8bppIndexed:
                //    return (Bitmap)srcBmp.Clone();
                default: throw new NotSupportedException();
            }


            return destBmp;
        }
    }

    public static class GraphicsExtensions
    {
        public static void DrawPolyline(this Graphics g, Pen pen, Vertex[] ps, bool isClose = false)
        {
            var count = ps.Length;
            for (int i = 1; i < count; i++)
            {
                var str = ps[i - 1];
                var end = ps[i];

                var b = ps[i - 1].Bulge;
                if (b == 0)
                    g.DrawLine(pen, (float)str.X, (float)str.Y, (float)end.X, (float)end.Y);
                else
                {
                    double x0 = str.X;
                    double y0 = str.Y;
                    double x1 = end.X;
                    double y1 = end.Y;

                    var norm = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
                    var s = norm / 2;
                    var d = s * (1 - b * b) / (2 * b);

                    var u = (x1 - x0) / norm;
                    var v = (y1 - y0) / norm;

                    var cx = -v * d + (x0 + x1) / 2;
                    var cy = u * d + (y0 + y1) / 2;
                    
                    var r = (end.Location - new DeepWise.Shapes.Point(cx, cy)).Length;

                    var strAng = Math.Atan2(str.Y - cy, str.X - cx);
                    strAng = strAng / Math.PI * 180;

                    double sweep = Math.Atan(b) * 4;
                    sweep = sweep / Math.PI * 180;
                    float rr = (float)(r + r);
                    g.DrawArc(pen, (float)(cx - r), (float)(cy - r), rr, rr, (float)strAng, (float)sweep);
                }
            }

            if (isClose)
            {
                var end = ps.First();
                var str = ps.Last();
                var b = str.Bulge;
                if (b == 0)
                {
                    g.DrawLine(pen, (float)end.X, (float)end.Y, (float)str.X, (float)str.Y);
                }
                else
                {
                    double x0 = str.X;
                    double y0 = str.Y;
                    double x1 = end.X;
                    double y1 = end.Y;

                    var norm = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
                    var s = norm / 2;
                    var d = s * (1 - b * b) / (2 * b);

                    var u = (x1 - x0) / norm;
                    var v = (y1 - y0) / norm;

                    var cx = -v * d + (x0 + x1) / 2;
                    var cy = u * d + (y0 + y1) / 2;

                    var r = (end.Location - new DeepWise.Shapes.Point(cx, cy)).Length;

                    var strAng = Math.Atan2(str.Y - cy, str.X - cx);
                    strAng = strAng / Math.PI * 180;

                    double sweep = Math.Atan(b) * 4;
                    sweep = sweep / Math.PI * 180;
                    float rr = (float)(r + r);
                    g.DrawArc(pen, (float)(cx - r), (float)(cy - r), rr, rr, (float)strAng, (float)sweep);
                }
            }
        }
    }

    public class ImageNotFoundException : Exception
    {
        public ImageNotFoundException()
        {

        }

        public ImageNotFoundException(string msg) : base(msg)
        {

        }
    }

    public class BitmapJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Bitmap);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string jstring)
            {
                var m = new MemoryStream(Convert.FromBase64String(jstring));
                return (Bitmap)Bitmap.FromStream(m);
            }
            else
                return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Bitmap bmp = (Bitmap)value;
            MemoryStream m = new MemoryStream();
            bmp.Save(m, System.Drawing.Imaging.ImageFormat.Png);
            writer.WriteValue(Convert.ToBase64String(m.ToArray()));
        }
    }
}
