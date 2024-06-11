using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeepWise.Devices
{

    public interface ICamera : IStreamVideo, ICapture { }

    public class ImageRecievedEventArgs : EventArgs
    {
        public ImageRecievedEventArgs(IntPtr ptr, System.Drawing.Size size, PixelFormat format)
        {
            Scan0 = ptr;
            Size = size;
            Format = format;

        }
        public virtual IntPtr Scan0 { get; }
        public virtual System.Drawing.Size Size { get; }
        public virtual PixelFormat Format { get; }
        public int GetMemorySize()
        {
            switch (Format)
            {
                case PixelFormat.Format8bppIndexed:
                    return Size.Width * Size.Height;
                case PixelFormat.Format24bppRgb:
                    return Size.Width * Size.Height * 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppPArgb:
                    return Size.Width * Size.Height * 4;
                default:
                    throw new NotImplementedException();
            }
        }
        public int Stride
        {
            get
            {
                switch(Format)
                {
                    case PixelFormat.Format8bppIndexed:
                        return Size.Width;
                    case PixelFormat.Format24bppRgb:
                        return Size.Width * 3;
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppRgb:
                    case PixelFormat.Format32bppPArgb:
                        return Size.Width * 4;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public void CopyTo(IntPtr dest) => CopyMemory(dest, Scan0, (uint)(Format.GetBytesPerPixel() * Size.Width * Size.Height));
        public void CopyTo(IntPtr dest, uint count) => CopyMemory(Scan0, dest, count);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }

    public interface IStreamVideo
    {
        event EventHandler<ImageRecievedEventArgs> ReceivedImage;
        System.Drawing.Size Size { get; }
        PixelFormat Format { get; }
    }

    public interface ICapture
    {
        T Capture<T>(Func<ImageRecievedEventArgs, T> func);
    }

    public class ImageGrabber<T> : ConcurrentQueue<T>, IDisposable
    {
        IStreamVideo device { get; }
        Func<ImageRecievedEventArgs, T> Converter;
        public ManualResetEvent _event = new ManualResetEvent(false);
        public void Wait() => _event.WaitOne();
        public Task WaitAsync() => Task.Run(_event.WaitOne);
        public bool IsFinished => _event.WaitOne(0);

        public int TotalCapturedNumber { get; private set; } = 0;

        #region NumberCount
        public ImageGrabber(IStreamVideo device, Func<ImageRecievedEventArgs, T> converter, int count)
        {
            this.device = device;
            this.count = count;
            device.ReceivedImage += Capture_Number;
        }
        int count;
        private void Capture_Number(object sender, ImageRecievedEventArgs e)
        {
            if (count > 0)
            {
                this.Enqueue(Converter(e));
                TotalCapturedNumber++;
                count--;
            }
            else
            {
                device.ReceivedImage -= Capture_Number;
                _event.Set();
            }
        }
        #endregion

        #region Interval
        public ImageGrabber(IStreamVideo device, Func<ImageRecievedEventArgs, T> converter, TimeSpan interval)
        {
            this.device = device;
            this.interval = interval;
            Stopwatch = System.Diagnostics.Stopwatch.StartNew();
            device.ReceivedImage += Capture_Interval;
        }
        TimeSpan interval;
        System.Diagnostics.Stopwatch Stopwatch;
        private void Capture_Interval(object sender, ImageRecievedEventArgs e)
        {
            if (Stopwatch.Elapsed < interval)
            {
                this.Enqueue(Converter(e));
                TotalCapturedNumber++;
            }
            else
            {
                device.ReceivedImage -= Capture_Interval;
                _event.Set();
            }
        }
        #endregion Interval

        #region Signal
        public ImageGrabber(IStreamVideo device, Func<ImageRecievedEventArgs, T> converter, ManualResetEvent resetEvent)
        {
            if (resetEvent.WaitOne(0) == true) throw new Exception();
            this.device = device;
            device.ReceivedImage += Capture_Signal; ;
            _event = resetEvent;
        }
        private void Capture_Signal(object sender, ImageRecievedEventArgs e)
        {
            if (!IsFinished)
            {
                this.Enqueue(Converter(e));
                TotalCapturedNumber++;
            }
            else
            {
                device.ReceivedImage -= Capture_Signal;
                _event.Set();
            }
        }
        #endregion

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.ToString());
            foreach(var item in this)
            {
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }
            device.ReceivedImage -= Capture_Interval;
            device.ReceivedImage -= Capture_Number;
            device.ReceivedImage -= Capture_Signal;
            IsDisposed = true;
        }
    }

    public class ImageGrabber : ImageGrabber<Bitmap>
    {
        public ImageGrabber(IStreamVideo device, int count) : base(device, CameraExtension.GetBitmap, count)
        {
        }

        public ImageGrabber(IStreamVideo device, TimeSpan interval) : base(device, CameraExtension.GetBitmap, interval)
        {

        }

        public ImageGrabber(IStreamVideo device, ManualResetEvent resetEvent) : base(device, CameraExtension.GetBitmap, resetEvent)
        {

        }
    }


    public static class CameraExtension
    {
        public static int GetBytesPerPixel(this PixelFormat imageFormat)
        {
            switch (imageFormat)
            {
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format24bppRgb:
                    return 3;
                default:
                    throw new NotImplementedException();
            }
        }

        public static byte[] GetByteArray(this ImageRecievedEventArgs args)
        {
            var array = new byte[args.Format.GetStride(args.Size.Width) * args.Size.Height];
            Marshal.Copy(args.Scan0, array, 0, array.Length);
            return array;
        }

        public unsafe static T[,] ToMap<T>(IntPtr Scan0, System.Drawing.Size Size, Func<ushort, T> selector)
        {
            var map = new T[Size.Height, Size.Width];
            var p = (ushort*)Scan0.ToPointer();
            Parallel.For(0, Size.Height, y =>
            {
                for (int x = 0, i = y * Size.Width; x < Size.Width; x++, i++)
                    map[y, x] = selector(p[i]);
            });
            return map;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [Obsolete("Use \"CaptureAsBitmap()\" instead.")]
        public static Mat Capture(this ICapture device) => device.Capture(x=>
        {



            switch(x.Format)
            {
                case PixelFormat.Format8bppIndexed:
                    {
                        var tmp = new Mat(x.Size.Height, x.Size.Width, MatType.CV_8UC1);
                        if (tmp.Step() == x.Stride)
                        {
                            CopyMemory(tmp.Data, x.Scan0, (uint)(x.Size.Height * x.Stride));
                            return tmp;
                        }
                        else
                            throw new NotImplementedException();
                    }
                case PixelFormat.Format24bppRgb:

                    {
                        var tmp = new Mat(x.Size.Height, x.Size.Width, MatType.CV_8UC3);
                        if (tmp.Step() == x.Stride)
                        {
                            CopyMemory(tmp.Data, x.Scan0, (uint)(x.Size.Height * x.Stride));
                            return tmp;
                        }
                        else
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        });

        public static Bitmap CaptureAsBitmap(this ICapture device) => device.Capture(GetBitmap);

        public static OpenCvSharp.MatType GetMatType(this System.Drawing.Imaging.PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format8bppIndexed:
                    return OpenCvSharp.MatType.CV_8U;
                case PixelFormat.Format24bppRgb:
                    return OpenCvSharp.MatType.CV_8SC3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppRgb:
                    return OpenCvSharp.MatType.CV_8SC4;
                default:
                    throw new NotImplementedException();
            }
        }

        public unsafe static OpenCvSharp.Mat CaptureAsMat(this ICapture device)
        {
            return device.Capture<OpenCvSharp.Mat>(e =>
            {
                PixelFormat format = e.Format;
                var mat = new OpenCvSharp.Mat(e.Size.Height, e.Size.Width, format.GetMatType());
                int destStride = (int)mat.Step();
                if (e.Stride == destStride)
                    e.CopyTo(mat.Data);
                else
                {
                    Parallel.For(0, e.Size.Height, y =>
                    {
                        var dest = mat.DataPointer;
                        dest += destStride * y;
                        IntPtr src = e.Scan0 + y * e.Stride;
                        Buffer.MemoryCopy((void*)src, (void*)dest, destStride, destStride);
                    });
                }
                return mat;
            });
        }


        public static Bitmap GetBitmap(this ImageRecievedEventArgs args)
        {
            PixelFormat format = args.Format;
            var bmp = new Bitmap(args.Size.Width, args.Size.Height, format);
            if (format == PixelFormat.Format8bppIndexed) bmp.Palette = System.Drawing.BitmapExtensions.Mono8Palette;
            var data = bmp.LockBits(new Rectangle(0, 0, args.Size.Width, args.Size.Height), ImageLockMode.ReadWrite, format);
            args.CopyTo(data.Scan0);
            bmp.UnlockBits(data);
            return bmp;
        }

    }

}
