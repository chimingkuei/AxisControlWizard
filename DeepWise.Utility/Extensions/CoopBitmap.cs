using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static WindowsAPI.Kernal32;
namespace DeepWise.Controls
{
    public class CoopBitmap : IDisposable
    {
        public CoopBitmap(System.Drawing.Size size) : this(size.Width, size.Height) { }
        public CoopBitmap(int width, int height)
        {
            if (width <= 0 || height <= 0) throw new ArgumentException();
            Width = width;
            Height = height;
            var format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            uint byteCount = (uint)(format.GetStride(width) * height);

            sectionPointer = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, PAGE_READWRITE, 0, byteCount, null);
            mapPointer = MapViewOfFile(sectionPointer, FILE_MAP_ALL_ACCESS, 0, 0, byteCount);

            //create the InteropBitmap
            interopBitmap = Imaging.CreateBitmapSourceFromMemorySection(sectionPointer, (int)width, (int)height, PixelFormats.Pbgra32, format.GetStride(width), 0) as InteropBitmap;
            gdiBitmap = new System.Drawing.Bitmap((int)width, (int)height, format.GetStride(width), format, mapPointer);
        }
        public unsafe CoopBitmap(Bitmap oriMask,System.Drawing.Color overlayColor) : this(oriMask.Width, oriMask.Height) 
        {
            if (oriMask != null)
            {
                if (oriMask.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    var srcData = oriMask.LockBits(new Rectangle(0, 0, oriMask.Width, oriMask.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    var destData = gdiBitmap.LockBits(new Rectangle(0, 0, oriMask.Width, oriMask.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    int mask = overlayColor.ToArgb();
                    int blank = 0;
                    var w = oriMask.Width;
                    Parallel.For(0, oriMask.Height, y =>
                    {
                        byte* src = (byte*)srcData.Scan0;
                        int* dest = (int*)destData.Scan0;
                        src += srcData.Stride * y;
                        dest += destData.Stride / 4 * y;
                        for (int x = w - 1; x >= 0; x--)
                        {
                            *dest = *src != 0 ? mask : blank;
                            dest++;
                            src++;
                        }
                    });
                    oriMask.UnlockBits(srcData);
                    gdiBitmap.UnlockBits(destData);
                }
                else if (oriMask.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    var srcData = oriMask.LockBits(new Rectangle(0, 0, oriMask.Width, oriMask.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    var destData = gdiBitmap.LockBits(new Rectangle(0, 0, oriMask.Width, oriMask.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    int mask = overlayColor.ToArgb();
                    int blank = 0;
                    var w = oriMask.Width;
                    Parallel.For(0, oriMask.Height, y =>
                    {
                        int* src = (int*)srcData.Scan0;
                        int* dest = (int*)destData.Scan0;
                        src += srcData.Stride/4 * y;
                        dest += destData.Stride / 4 * y;
                        for (int x = w - 1; x >= 0; x--)
                        {
                            *dest = *src != 0 ? mask : blank;
                            dest++;
                            src++;
                        }
                    });
                    oriMask.UnlockBits(srcData);
                    gdiBitmap.UnlockBits(destData);
                }
                else
                    throw new ArgumentException();
            }
        }
        public int Width { get; }
        public int Height { get; }
        public System.Drawing.Size Size=>new System.Drawing.Size(Width, Height);
        public void Draw(Action<Graphics> action, Int32Rect? invalidateRect = null)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            using (var g = Graphics.FromImage(gdiBitmap))
            {
                action(g);
            }
            if (invalidateRect == null)
                interopBitmap.Invalidate();
            else
                interopBitmap.Invalidate(invalidateRect);
        }
        public void Clear()
        {
            Draw(g =>
            {
                g.Clear(System.Drawing.Color.Transparent);
            });
        }
        public void Clear(System.Drawing.Color color)
        {
            Draw(g =>
            {
                g.Clear(color);
            });
        }
        
        public InteropBitmap InteropBitmap
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
                return interopBitmap;
            }
        }

        public Bitmap GDIBitmap
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
                return gdiBitmap;
            }
        }

        InteropBitmap interopBitmap;
        Bitmap gdiBitmap;
        IntPtr sectionPointer;
        IntPtr mapPointer;
        const int PAGE_READWRITE = 0x04;
        const int FILE_MAP_ALL_ACCESS = 0xF001F;

        bool isDisposed = false;
        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            gdiBitmap.Dispose();
            UnmapViewOfFile(mapPointer);
            CloseHandle(sectionPointer);
            isDisposed = true;
        }
    }
}
