using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if AForge
    using AForge.Video.DirectShow;
#endif
//需要 AForge 套件
namespace DeepWise.Devices
{
#if !AForge
    //安裝套件後請移除該類別
    public class VideoCaptureDevice
    {
        public VideoCaptureDevice() 
        {
            throw new Exception("需要安裝AForge套件");
        }
        public void Start() { }
        public void Stop() { }
        public bool IsRunning { get; set; }
        public event EventHandler<NewFrameEventArgs> NewFrame;
    }
    public class NewFrameEventArgs : EventArgs
    {
        public PixelFormat PixelFormat { get; set; }
        public Size Size { get; set; }
        public Bitmap Frame { get; set; }
    }
#endif

    //public class WebCamGrabber : ICamera
    //{
    //    VideoCaptureDevice Device;
    //    public WebCamGrabber(VideoCaptureDevice device)
    //    {
    //        Device = device;
    //        Device.Start();

    //        var _event = new ManualResetEvent(false);
    //        device.NewFrame += GetSize;
    //        _event.WaitOne();
    //        void GetSize(object sender, NewFrameEventArgs eventArgs)
    //        {
    //            Size = eventArgs.Size;
    //            Format = eventArgs.PixelFormat;
    //            Device.NewFrame -= GetSize;
    //            _event.Set();
    //        }

    //        device.NewFrame += Device_NewFrame;
    //    }

    //    private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
    //    {
    //        var frame = eventArgs.Frame;
    //        var bData = frame.LockBits(new Rectangle(Point.Empty, frame.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, frame.PixelFormat);

    //        ReceivedImage?.Invoke(sender, new ImageRecievedEventArgs(bData.Scan0, frame.Size, Format));
    //        frame.UnlockBits(bData);
    //    }

    //    public Size Size { get; private set; } = Size.Empty;
    //    public PixelFormat Format { get; private set; } = PixelFormat.Undefined;

    //    public bool IsOpened => IsRunning;

    //    public bool IsRunning => Device.IsRunning;

    //    public event EventHandler<ImageRecievedEventArgs> ReceivedImage;

    //    public void Close()
    //    {
    //        Device.Stop();
    //    }

    //    public bool Open()
    //    {
    //        Device.Start();
    //        return IsRunning;
    //    }

    //    public void Start() => Open();

    //    public void Stop() => Close();

    //    public Bitmap Capture()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
