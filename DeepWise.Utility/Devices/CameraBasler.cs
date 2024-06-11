using DeepWise.Controls;
using DeepWise.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using uEye.Defines;
using uEye.Defines.Whitebalance;
using Basler.Pylon;
namespace DeepWise.Devices
{
    using Size = System.Drawing.Size;
    public class CameraBasler : Camera , IDevice
    {
        static CameraBasler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Split(',')[0] == "Basler")
                return Assembly.LoadFile(@"C:\Program Files\Basler\pylon 6\Development\Assemblies\Basler.Pylon\x64\Basler.Pylon.dll");
            else
                return null;
        }

        public CameraBasler()
        {
            try
            {
                _cam = new Basler.Pylon.Camera(DeviceType.GigE, CameraSelectionStrategy.FirstFound);
            }
            catch(Exception ex)
            { 

            }
        }

        public CameraBasler(string serialNumber)
        {
            if (serialNumber == null)
                throw new ArgumentNullException();
            else if (serialNumber == string.Empty)
                throw new ArgumentException("序號不能為空字串");
            _serialNumber = serialNumber;
            _cam = new Basler.Pylon.Camera(serialNumber);
            //_cam.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;
        }

        [Browsable(false)]
        public override string Model
        {
            get
            {
                return _cam.Parameters[PLGigECamera.DeviceModelName].GetValue();
            }
        }

        string _serialNumber="";
        public override string SerialNumber
        {
            get => _serialNumber;
            set
            {
                if (IsOpened)
                {
                    System.Windows.MessageBox.Show("相機已開啟，無法更改其序號");
                }
                else
                    _serialNumber = value;
                NotifyPropertyChanged();
            }
        }
        public override bool IsOpened => _cam != null ? this._cam.IsOpen : false;
        public override bool IsRunning
        {
            get
            {
                if (_cam != null)
                {
                    return this._cam.StreamGrabber.IsGrabbing;
                }
                else
                    return false;
            }
        }
        
        public override T Capture<T>(Func<ImageRecievedEventArgs, T> converter)
        {
            T result;
           
            if (IsRunning)
            {
                _cam.StreamGrabber.Stop();
                using (var gr = _cam.StreamGrabber.GrabOne(2500, TimeoutHandling.ThrowException))
                {
                    var args = new ImageRecievedEventArgs(gr.PixelDataPointer, new Size(gr.Width, gr.Height), Format);
                    OnRecievedImage(args);
                    result = converter(args);
                }
                _cam.StreamGrabber.Start();
            }
            else
            {
                using (var _result = _cam.StreamGrabber.GrabOne(5000, TimeoutHandling.ThrowException))
                {
                    if (_result.GrabSucceeded)
                    {
                        var args = new ImageRecievedEventArgs(_result.PixelDataPointer, Size, Format);
                        result = converter(args);
                        OnRecievedImage(args);
                    }
                    else
                        result = default;
                }
            }
            WriteMessage(new LogMessage(this) { Caption = "相機拍攝影像", Description = $"Single shot on \"{this}\" has been started." });
            return result;
        }

        System.Drawing.Size _size;
        public override System.Drawing.Size Size
        {
            get
            {
                if (_size.IsEmpty)
                {
                    _size.Width = (int)this._cam.Parameters[PLGigECamera.Width].GetValue();
                    _size.Height = (int)this._cam.Parameters[PLGigECamera.Height].GetValue();
                }

                return _size;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override Size SizeOriginal
        {
            get
            {
                var w = (int)this._cam.Parameters[PLGigECamera.WidthMax].GetValue();
                var h = (int)this._cam.Parameters[PLGigECamera.HeightMax].GetValue();
                return new Size(w, h);
            }
        }
        [Browsable(false)]
        public override System.Drawing.Imaging.PixelFormat PixelFormat
        {
            get
            {
               
                var cur = this._cam.Parameters[PLGigECamera.PixelFormat].GetValue();

                switch (cur)
                {
                    case "Mono8":
                        return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    case "BayerRG8":
                        return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                    default:
                        var all = this._cam.Parameters[PLGigECamera.PixelFormat].GetAllValues();
                        throw new NotImplementedException();
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override bool EnableExposure => false;
        public override double Exposure
        {
            get
            {
                return this._cam.Parameters[PLGigECamera.ExposureTimeAbs].GetValue();
            }
            set
            {
                this._cam.Parameters[PLGigECamera.ExposureTimeAbs].SetValue(value);
                NotifyExposureAndFrameRate();
            }
        }
        public override double MaximumExposure
        {
            get
            {
                return this._cam.Parameters[PLGigECamera.ExposureTimeAbs].GetMaximum();
            }
        }
        public override double MinimumExposure
        {
            get
            {
                return this._cam.Parameters[PLGigECamera.ExposureTimeAbs].GetMinimum();
            }
        }
        public override bool ExposureAuto
        {
            get
            {
                var all = this._cam.Parameters[PLGigECamera.ExposureAuto].GetAllValues();
                switch(this._cam.Parameters[PLGigECamera.ExposureAuto].GetValue())
                {
                    case "Continuous":
                        return true;
                    default:
                        return false;
                }
                throw new NotImplementedException();
            }
            set
            {
                if(value)
                    this._cam.Parameters[PLGigECamera.ExposureAuto].SetValue("Continuous");
                else
                    this._cam.Parameters[PLGigECamera.ExposureAuto].SetValue("Off");
            }
        }

        public override double Framerate
        {
            get
            {
                return this._cam.Parameters[PLGigECamera.AcquisitionFrameRateAbs].GetValue();
            }
            set
            {
                this._cam.Parameters[PLGigECamera.AcquisitionFrameRateAbs].SetValue(value);
                NotifyExposureAndFrameRate();
            }
        }
        void NotifyExposureAndFrameRate()
        {
            NotifyPropertyChanged(nameof(Exposure));
            NotifyPropertyChanged(nameof(MaximumExposure));
            NotifyPropertyChanged(nameof(MinimumExposure));
            NotifyPropertyChanged(nameof(Framerate));
            NotifyPropertyChanged(nameof(MaximumFramerate));
            NotifyPropertyChanged(nameof(MinimumFramerate));
        }
        public override double MaximumFramerate
        {
            get
            {
                return this._cam.Parameters[PLGigECamera.AcquisitionFrameRateAbs].GetMaximum();
            }
        }
        public override double MinimumFramerate
        {
            get
            {
                return this._cam.Parameters[PLGigECamera.AcquisitionFrameRateAbs].GetMinimum();
            }
        }


        public override bool Initialize()
        {
            if (_cam.IsOpen)
            {
                WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = $"相機已開啟" });
                return false;
            }

            _cam.CameraOpened += Configuration.AcquireContinuous;

            if (_cam.Open(5000, TimeoutHandling.Return))
            {
                _serialNumber = _cam.CameraInfo["SerialNumber"];
                _cam.Parameters[PLGigECamera.PixelFormat].SetValue("Mono8");
                LoadParameters();
                NotifyPropertyChanged(nameof(IsOpened));
                return true;
            }
            else
            {
                WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = "相機連線逾時" });
                return false;
            }
        }
      
        public override bool LoadParameters()
        {
            if (!IsOpened) throw new Exception("設備尚未初始化");
            try
            {

                _cam.Parameters.Load(SerialNumber + ".psf", ParameterPath.CameraDevice);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [DisplayName("儲存設定"), Button]
        public override void SaveParameters()
        {
            if (!IsOpened) throw new Exception("設備尚未初始化");
            try
            {
                _cam.Parameters.Save(SerialNumber + ".psf", ParameterPath.CameraDevice);
              
            }
            catch (Exception ex)
            {

            }
        }

        public override void Dispose()
        {
            try
            {
                Stop();
               _cam?.Close();
            }
            catch { }

            _cam?.Dispose();
            base.Dispose();
        }

        public override void Start()
        {
            if (_cam.IsOpen )
            {
                _cam.StreamGrabber.ImageGrabbed -= StreamGrabber_ImageGrabbed;
                _cam.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;
                _cam.StreamGrabber.Start( GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                //Task.Run(GrabBack);
            }
        }

        public override void Stop()
        {
            _cam?.StreamGrabber.Stop();
        }


        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            var result = e.GrabResult;
            if (result.GrabSucceeded)
            {
                OnRecievedImage(new ImageRecievedEventArgs(result.PixelDataPointer, new Size(result.Width, result.Height), Format));
            }
            else
            {
                Console.WriteLine("Error: {0} {1}", result.ErrorCode, result.ErrorDescription);
            }
        }
       
        public override string ToString()
        {
            if(_cam.IsOpen)
            {
                var id = _cam.Parameters[PLGigECamera.DeviceID].GetValue();
                return $"Camera{{{id}}}";
            }
            else
            {
                return base.ToString();
            }
        }
        internal Basler.Pylon.Camera _cam;
    }
}
