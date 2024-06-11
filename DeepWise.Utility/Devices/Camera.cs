#define ASD
using DeepWise.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Devices
{
    public abstract partial class Camera : IDisposable, ICamera, ICapture , ILogMessageProvider , INotifyPropertyChanged
    {
        public Camera()
        {
        }

        string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual T Capture<T>(Func<ImageRecievedEventArgs, T> converter)
        {
            if (!IsOpened) throw new CameraNotOpenException(this);
            if (!IsRunning) throw new Exception();
            T bmp = default;
            var resetEvent = new System.Threading.ManualResetEvent(false);
            ReceivedImage += Capture;
            if (!resetEvent.WaitOne()) throw new TimeoutException();
            return bmp;

            void Capture(object sender, ImageRecievedEventArgs e)
            {
                ReceivedImage -= Capture;
                bmp = converter.Invoke(e);
                resetEvent.Set();
            }
        }
        public async Task<Bitmap[]> CaptureContinuous(int count)
        {
            if (count < 1) throw new Exception("拍照數量不可小於1");
            if (!IsOpened) throw new CameraNotOpenException(this);
            if (!IsRunning) throw new Exception();
            Bitmap[] bmp = new Bitmap[count];
            using (var resetEvent = new System.Threading.ManualResetEvent(false))
            {
                int tmp = 0;
                ReceivedImage += Camera_ReceivedImage;
                await Task.Run(() =>
                {
                    if (!resetEvent.WaitOne()) throw new TimeoutException("相機沒有回應 : " + ToString());
                });
                return bmp;
                void Camera_ReceivedImage(object sender, ImageRecievedEventArgs e)
                {
                    bmp[tmp++] = e.GetBitmap();
                    if (tmp == count - 1)
                    {
                        resetEvent.Set();
                        ReceivedImage -= Camera_ReceivedImage;
                    }
                }
            }
        }

        

        public abstract bool LoadParameters();
        public abstract void SaveParameters();
        public abstract bool Initialize();
        //public abstract bool Open();
        //public virtual bool Open(string serialNumber) => throw new NotFiniteNumberException();
        //public virtual bool Open(int id) => throw new NotFiniteNumberException();
        //public abstract void Close();
        [Button]
        public abstract void Start();
        [Button]
        public abstract void Stop();
        public virtual void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        protected void OnRecievedImage(ImageRecievedEventArgs args)
        {
            ReceivedImage?.Invoke(this, args);
        }
        protected void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler Disposed;
        public event EventHandler SizeChanged;
        public event EventHandler<ImageRecievedEventArgs> ReceivedImage;
        public event EventHandler<LogMessageEventArgs> MessageWritten;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void WriteMessage(LogMessage msg) => MessageWritten?.Invoke(this, new LogMessageEventArgs(msg));

        #region Properties
        public abstract string Model { get; }
        public abstract string SerialNumber { get; set; }
        [Browsable(false)]
        public abstract bool IsOpened { get; }
        [Browsable(false)]
        public abstract bool IsRunning { get; }

        public abstract Size Size { get; set; }

        public abstract Size SizeOriginal { get; }

        public abstract System.Drawing.Imaging.PixelFormat PixelFormat { get; set; }


        [Browsable(false)]
        public virtual bool EnableExposure => true;

        [Slider("MinimumExposure", "MaximumExposure")]
        public abstract double Exposure { get; set; }

        public abstract bool ExposureAuto { get; set; }

        [Browsable(false)]
        public abstract double MaximumExposure { get; }

        [Browsable(false)]
        public abstract double MinimumExposure { get; }

        [Slider("MinimumFramerate", "MaximumFramerate")]
        public abstract double Framerate { get; set; }

        [Browsable(false)]
        public abstract double MaximumFramerate { get; }

        [Browsable(false)]
        public abstract double MinimumFramerate { get; }
        #endregion Properties


        public PixelFormat Format => this.PixelFormat;

        public override string ToString()
        {
            return $"{Model}:{SerialNumber}";
        }
    }

    public class ImageBag<T> : Queue<T>
    {

    }

    public class CameraNotOpenException : Exception
    {
        public CameraNotOpenException() { }
        public CameraNotOpenException(Camera cam)
        {
            message = "相機 : " + cam.ToString() + " 尚未開啟";
        }

        private string message = "相機尚未開啟";
        public override string Message => message;
    }
}
