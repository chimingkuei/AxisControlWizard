using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using InteropBitmap = System.Windows.Interop.InteropBitmap;
using System.Diagnostics;

namespace DeepWise
{
#if TEST
    public class DImage : IDisposable //主要用途=>取代System.Drawing.Bitmap
    {
        public DImage(System.Drawing.Size size, DPixelFormat format)
        {
            if (size.Width == 0 || size.Height == 0) throw new ArgumentException("width and height cann't be 0");
            _w = size.Width;
            _h = size.Height;
            Format = format;
            Stride = format.GetStride(size.Width);
            _data = Marshal.AllocHGlobal(Stride * size.Height);
        }
        public DImage(System.Drawing.Size size, DPixelFormat format, IntPtr data, int? stride = null)
        {
            if (stride is null) Stride = format.GetStride(size.Width);
            _w = size.Width;
            _h = size.Height;
            Format = format;
            _data = data;
        }
        public DImage(string filename)
        {
            using (var bmp = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(filename))
            {

                _w = bmp.Width;
                _h = bmp.Height;
                Format = DPixelFormat.Mono8;
                Stride = Format.GetStride(bmp.Width);
                var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                _data = Marshal.AllocHGlobal(Stride * _h);
                WindowsAPI.Kernal32.CopyMemory(Data, data.Scan0, (uint)(Stride * _h));
                bmp.UnlockBits(data);
            }
        }
        int _w, _h;
        public int Width => _w;
        public int Height => _h;
        public System.Drawing.Size Size => new System.Drawing.Size(_w, _h);

        public DPixelFormat Format { get; }

        System.Drawing.Bitmap _GDIBitmap;
        public System.Drawing.Bitmap GDIBitmap
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                if (_GDIBitmap == null) throw new InvalidOperationException("GDIBitmap has not been initialized.");
                return _GDIBitmap;
            }
        }

        public System.Drawing.Bitmap InitializeGDIBitmap()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (_GDIBitmap == null)
                switch (Format)
                {
                    case DPixelFormat.RGB32:
                        return _GDIBitmap = new System.Drawing.Bitmap(_w, _h, Stride, System.Drawing.Imaging.PixelFormat.Format32bppRgb, Data);
                    case DPixelFormat.Mono8:
                        return _GDIBitmap = new System.Drawing.Bitmap(_w, _h, Stride, System.Drawing.Imaging.PixelFormat.Format32bppRgb, Data) { Palette = System.Drawing.BitmapExtensions.Mono8Palette };
                    default:
                        throw new NotImplementedException();
                }
            else
                return _GDIBitmap;
        }

        public InteropBitmap _WPFBitmapSource;
        public InteropBitmap WPFBitmapSource
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
                if (_WPFBitmapSource == null) throw new InvalidOperationException("WPFBitmapSource has not been initialized.");
                return _WPFBitmapSource;
            }
        }

        public InteropBitmap CreateWPFBitmapSource()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (_WPFBitmapSource == null)
                switch (Format)
                {
                    case DPixelFormat.RGB32:
                        return _WPFBitmapSource = (InteropBitmap)System.Windows.Interop.Imaging.CreateBitmapSourceFromMemorySection(Data, _w, _h, System.Windows.Media.PixelFormats.Bgr32, Stride, 0);
                    case DPixelFormat.Mono8:
                        return _WPFBitmapSource = (InteropBitmap)System.Windows.Interop.Imaging.CreateBitmapSourceFromMemorySection(Data, _w, _h, System.Windows.Media.PixelFormats.Gray8, Stride, 0);
                    default:
                        throw new NotImplementedException();
                }
            else
                return _WPFBitmapSource;

        }

        public void Invalidate()
        {
            _WPFBitmapSource?.Invalidate();
            _WPFBitmapSource.Palette.Colors.Clear();
        }
        public void Dispose()
        {
            InteropBitmap bmp;
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            _GDIBitmap?.Dispose();
            Marshal.FreeHGlobal(Data);
            IsDisposed = true;
        }
        public bool HadAllocated { get; }
        public bool IsDisposed { get; private set; }
        IntPtr _data;
        public IntPtr Data => IsDisposed ? throw new ObjectDisposedException(GetType().FullName) : _data;
        public int Stride { get; }
    }

    public enum DPixelFormat
    {
        RGB32,
        BGR32,
        Mono8,
    }

    public static class DImageExtensions
    {
        public static int GetStride(this DPixelFormat pixelFormat, int width) => 4 * ((width * pixelFormat.GetBytesCount() * 8 + 31) / 32);

        public static int GetBytesCount(this DPixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case DPixelFormat.RGB32:
                case DPixelFormat.BGR32:
                    return 4;
                case DPixelFormat.Mono8:
                    return 1;
                default:
                    throw new NotImplementedException();
            }
        }
    }
#endif
}
