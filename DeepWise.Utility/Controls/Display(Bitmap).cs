using DeepWise.Devices;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static System.Drawing.BitmapExtensions;
using static WindowsAPI.Kernal32;
namespace DeepWise.Controls
{
    
    public partial class Display
    {
        //TODO : 
#if MAT
        public Mat Image
        {
            get => (Mat)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(Mat), typeof(Display), new PropertyMetadata(null, InitializeDisplay));
#else
        public Bitmap Image
        {
            get => (Bitmap)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(Bitmap), typeof(Display), new PropertyMetadata(null, InitializeDisplay));
        private BitmapData imageBitmapData;
#endif
        public IStreamVideo Camera { get => (IStreamVideo)GetValue(CameraProperty); set => SetValue(CameraProperty, value); }
        public ImageSource OverlayImage
        {
            get => Img_Overlay.Source;
            set => Img_Overlay.Source = value;
        }
        public void InvalidateImage()
        {
            if(Img_Camera.Source is InteropBitmap interopBitmap)
                interopBitmap.Invalidate();
        }
        static void InitializeDisplay(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is Display _this)
            {
                if (e.Property == ImageProperty)
                {
#if MAT
                    if (e.NewValue is Mat value)
                    {
                        _this.Img_Camera.Source = ImageConverter.ConvertToImageSource(value);
                        _this.Img_Camera.Width = value.Width;
                        _this.Img_Camera.Height = value.Height;
                    }
                    else if (e.OldValue != null)
                        _this.Img_Camera.Source = null;
#else
                    if (e.OldValue is Bitmap oldBitmap)
                    {
                        //oldBitmap.UnlockBits(_this.imageBitmapData);
                        _this.imageBitmapData = null;
                    }
                    else if (e.NewValue != null && _this.Camera != null)
                        _this.Camera = null;

                    if (e.NewValue is Bitmap value)
                    {
                        var data = _this.imageBitmapData = value.LockBits(new System.Drawing.Rectangle(0, 0, value.Width, value.Height), ImageLockMode.ReadWrite, value.PixelFormat);
                        _this.Img_Camera.Source = (System.Windows.Media.Imaging.CachedBitmap)InteropBitmap.Create(data.Width, data.Height, 96, 96, value.PixelFormat.ToMediaFormat(), value.Palette.ToBitmapPalette(), data.Scan0, data.Stride * data.Height, data.Stride);

                        value.UnlockBits(data);
                        _this.Img_Camera.Width = value.Width;
                        _this.Img_Camera.Height = value.Height;
                    }
                    else if (e.OldValue != null)
                        _this.Img_Camera.Source = null;
#endif
                }
                else if (e.Property == CameraProperty)
                {
                    if (e.OldValue is IStreamVideo oldCamera)
                    {
                        _this.bufferedBitmap.Dispose();
                        _this.bufferedBitmap = null;
                    }
                    else if (e.NewValue != null && _this.Image != null)
                        _this.Image = null;

                    if (e.NewValue is IStreamVideo newCamera)
                    {
                        _this.bufferedBitmap = new BufferedBitmap(newCamera);
                        _this.Img_Camera.Source = _this.bufferedBitmap.InteropBitmap;
                    }
                    else if (e.OldValue != null)
                        _this.Img_Camera.Source = null;
                }

                if (_this.AutoStretched) _this.Stretch();
                _this.PropertyChanged?.Invoke(_this, new PropertyChangedEventArgs(nameof(ImageWidth)));
                _this.PropertyChanged?.Invoke(_this, new PropertyChangedEventArgs(nameof(ImageHeight)));
            }
        }
        public static readonly DependencyProperty CameraProperty = DependencyProperty.Register(nameof(Camera), typeof(IStreamVideo), typeof(Display), new PropertyMetadata(null, InitializeDisplay));

    
        BufferedBitmap bufferedBitmap;
    }

    public class BufferedBitmap : IDisposable
    {
        public BufferedBitmap(IStreamVideo newCamera)
        {
            this.Target = newCamera;
            const int PAGE_READWRITE = 0x04;
            var format = newCamera.Format;
            var size = newCamera.Size;
            pCount = (uint)(format.GetStride(size.Width) * size.Height);
            section = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, PAGE_READWRITE, 0, pCount, null);
            map = MapViewOfFile(section, 0xF001F, 0, 0, pCount);
            InteropBitmap = Imaging.CreateBitmapSourceFromMemorySection(section, size.Width, size.Height, format.ToMediaFormat(), format.GetStride(size.Width), 0) as System.Windows.Interop.InteropBitmap;
            newCamera.ReceivedImage += OnRecievedCameraImage;
        }
        public void Save(string path)
        {
            InteropBitmap.ToBitmap().Save(path);
            MessageBox.Show("");
            //throw new NotImplementedException();
        }
        public IStreamVideo Target { get; }
        public bool IsDisposed { get; private set; } = false;
        public InteropBitmap InteropBitmap { get; }
        public bool IsPaused { get; set; } = false;
        public static explicit operator InteropBitmap(BufferedBitmap b) => b.InteropBitmap;
        public IntPtr Scan0 => map;
        IntPtr map, section;
        uint pCount;

        private void OnRecievedCameraImage(object sender, ImageRecievedEventArgs e)
        {
            if (IsPaused) return;
            try
            {
                CopyMemory(map, e.Scan0, pCount);
                InteropBitmap.Dispatcher.Invoke(InteropBitmap.Invalidate);
            }
            catch (Exception ex)
            {

            }
        }

        public void Dispose()
        {
            if (IsDisposed) throw new ObjectDisposedException(this.ToString());
            Target.ReceivedImage -= OnRecievedCameraImage;
            UnmapViewOfFile(map);
            CloseHandle(section);
            IsDisposed = true;
        }
    }
}
