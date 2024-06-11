using DeepWise.Controls;
using DeepWise.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using uEye.Defines;
using uEye.Defines.Whitebalance;
namespace DeepWise.Devices
{
    using Size = System.Drawing.Size;
    public class CameraIDS : Camera , IDevice
    {
        static CameraIDS()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Split(',')[0] == "uEyeDotNet")
                return Assembly.LoadFile(@"C:\Program Files\IDS\uEye\Develop\DotNet\uEyeDotNet.dll");
            else
                return null;
        }

        public CameraIDS()
        {
            try
            { 
                _cam = new uEye.Camera();
            }
            catch 
            { 
            }
        }

        public CameraIDS(string serialNumber) : this()
        {
            if (serialNumber == null)
                throw new ArgumentNullException();
            else if (serialNumber == string.Empty)
                throw new ArgumentException("序號不能為空字串");
            SerialNumber = serialNumber;
        }

        [Browsable(false)]
        public override string Model
        {
            get
            {
                if (_cam != null)
                {
                    try
                    {
                        _cam.Information.GetSensorInfo(out var info);
                        return info.SensorName;
                    }
                    catch { }
                }
                return "null";
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
        public override bool IsOpened => _cam != null ? this._cam.IsOpened : false;
        public override bool IsRunning
        {
            get
            {
                if (_cam != null)
                {
                    this._cam.Acquisition.HasStarted(out var started);
                    return started;
                }
                else
                    return false;
            }
        }

        public override T Capture<T>(Func<ImageRecievedEventArgs, T> converter)
        {
            if (!IsRunning)
            {
                var captrueResult = _cam.Acquisition.Freeze(DeviceParameter.Wait);
                if (captrueResult != Status.SUCCESS)
                    throw new IDSCameraException(captrueResult);
            }
            _cam.Memory.GetLast(out int id);
            _cam.Memory.GetLast(out IntPtr ptr);
            var args = new ImageRecievedEventArgs(ptr, Size, Format);
            T result = converter(args);
            OnRecievedImage(args);
            _cam.Memory.Unlock(id);
            WriteMessage(new LogMessage(this) { Caption = "相機拍攝影像", Description = $"Single shot on \"{this}\" has been started." });
            return result;
        }

        public override System.Drawing.Size Size
        {
            get
            {
                this._cam.Size.AOI.Get(out Rectangle rect);
                return rect.Size;
            }
            set
            {
                var running = IsRunning;
                if (running) _cam.Acquisition.Stop();
                this._cam.Size.AOI.Set(0, 0, value.Width, value.Height);
                _cam.Memory.GetList(out var list);
                _cam.Memory.Free(list);
                _cam.Memory.Allocate();
                if (running) _cam.Acquisition.Capture();
            }
        }
        public override Size SizeOriginal
        {
            get
            {
                _cam.Size.AOI.GetOriginal(out var rect);
                return rect.Size;
            }
        }
        [Browsable(false)]
        public override System.Drawing.Imaging.PixelFormat PixelFormat
        {
            get
            {
                _cam.PixelFormat.Get(out var format);
                switch (format)
                {
                    case uEye.Defines.ColorMode.RGB8Packed:
                    case uEye.Defines.ColorMode.BGR8Packed:
                        return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    case uEye.Defines.ColorMode.RGBA8Packed:
                    case uEye.Defines.ColorMode.BGRA8Packed:
                        return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                    case uEye.Defines.ColorMode.Mono8:
                        return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    default: throw new NotImplementedException();
                }
            }
            set
            {
                switch (value)
                {
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                        _cam.PixelFormat.Set(uEye.Defines.ColorMode.RGBA8Packed);
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                        _cam.PixelFormat.Set(uEye.Defines.ColorMode.RGB8Packed);
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                        _cam.PixelFormat.Set(uEye.Defines.ColorMode.Mono8);
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }
        public override bool EnableExposure => false;
        public override double Exposure
        {
            get
            {
                _cam.Timing.Exposure.Get(out double value);
                return value;
            }
            set
            {
                _cam.Timing.Exposure.Set(value);
                NotifyExposureAndFrameRate();
            }
        }
        public override double MaximumExposure
        {
            get
            {
                _cam.Timing.Exposure.GetRange(out _, out double tmp, out _);
                return tmp;
            }
        }
        public override double MinimumExposure
        {
            get
            {
                _cam.Timing.Exposure.GetRange(out double tmp, out _, out _);

                return tmp;
            }
        }
        public override bool ExposureAuto
        {
            get
            {
                switch (Model)
                {
                    case "XS":
                    case "UI328xCP-C":
                    default:
                        _cam.AutoFeatures.Sensor.Whitebalance.GetEnable(out WhiteBalanceMode value);
                        return value == WhiteBalanceMode.Automatic;
                }
            }
            set
            {
                switch (Model)
                {
                    case "XS":
                    case "UI328xCP-C":
                    default:
                        _cam.AutoFeatures.Sensor.Whitebalance.SetEnable(value ? WhiteBalanceMode.Automatic : WhiteBalanceMode.Disable);
                        break;
                }
            }
        }

        public override double Framerate
        {
            get
            {
                _cam.Timing.Framerate.Get(out double value);
                return value;
            }
            set
            {
                _cam.Timing.Framerate.Set(value);
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
                _cam.Timing.Framerate.GetFrameRateRange(out _, out double tmp, out _);
                return tmp;
            }
        }
        public override double MinimumFramerate
        {
            get
            {
                _cam.Timing.Framerate.GetFrameRateRange(out double tmp, out _, out _);
                return tmp;
            }
        }


        public override bool Initialize()
        {
            if (_cam.IsOpened)
            {
                WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = $"相機已開啟" });
                return false;
            }
            uEye.Types.CameraInformation[] cameras;
            uEye.Info.Camera.GetCameraList(out cameras);

            uEye.Types.CameraInformation? selected = null;
            //SerialNumber
            
            if (!string.IsNullOrEmpty(SerialNumber))
            {
                selected = cameras.FirstOrDefault(cam => cam.SerialNumber == SerialNumber);
                if (selected != null)
                {
                    if(selected.Value.InUse)
                    {
                        WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = $"相機'{SerialNumber}'正在使用中" });
                        return false;
                    }
                }
                else
                {
                    WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = $"找不到序號為'{SerialNumber}'的相機" });
                    return false;
                }
            }
            else 
            {
                selected = cameras.FirstOrDefault(x => !x.InUse);
                if (selected == null)
                {
                    WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = $"找不到可使用的相機" });
                    return false;
                }
            }
            
            var status = _cam.Init((int)selected.Value.CameraID);
            if(status == Status.Success)
            {
                _cam.Parameter.Load();
                _cam.Memory.Allocate();
                
                _cam.EventFrame += OnFrameGet;
                _serialNumber = selected.Value.SerialNumber;
                NotifyPropertyChanged(nameof(IsOpened));
                
                return true;
            }
            else
            {
                WriteMessage(new LogMessage() { Caption = "相機初始化失敗", Description = status.ToString() });
                return false;
            }
            
        }
      
        public override bool LoadParameters()
        {
            uEye.Defines.Status statusRet = default;
            try
            {
                statusRet = _cam.Parameter.Load();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }
            return (statusRet == uEye.Defines.Status.SUCCESS);
        }

        [DisplayName("儲存設定"), Button]
        public override void SaveParameters()
        {
            try
            {
                _cam.Parameter.Save();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public override void Dispose()
        {
            _cam.Exit();
            base.Dispose();
        }

        public override void Start()
        {
            if (_cam.IsOpened)
            {
                this._cam.Acquisition.Capture();
            }
        }
        public override void Stop()
        {
            if (IsRunning)
                this._cam.Acquisition.Stop();
        }

        private void OnFrameGet(object sender, EventArgs e)
        {
            try
            {
                _cam.Memory.GetLast(out IntPtr ptr);
                _cam.Memory.GetLast(out int id);
                OnRecievedImage(new ImageRecievedEventArgs(ptr, Size, Format));
                _cam.Memory.Unlock(id);
            }
            catch
            {

            }
        }

        private static System.Drawing.Imaging.PixelFormat GetImageFormat(ColorMode colorMode)
        {
            switch (colorMode)
            {
                case ColorMode.RGB8Packed:
                case ColorMode.BGR8Packed:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case ColorMode.BGRA8Packed:
                case ColorMode.RGBA8Packed:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                case ColorMode.Mono8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                default: throw new NotImplementedException();
            }
        }
        public override string ToString()
        {
            if(_cam.IsOpened)
            {
                _cam.Device.GetCameraID(out var id);
                return $"Camera{{{id}}}";
            }
            else
            {
                return base.ToString();
            }
        }
        internal uEye.Camera _cam;
    }

    public class IDSCameraException : Exception
    {
        public IDSCameraException(Status errorCode, [CallerMemberName] string caller = "")
        {
            Status = errorCode;
            CallerName = caller;
        }
        public string CallerName { get; }
        public Status Status { get; }
    }
}
