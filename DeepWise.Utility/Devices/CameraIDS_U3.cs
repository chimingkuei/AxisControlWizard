using DeepWise.Controls;
using DeepWise.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using peak;
using peak.core;
using System.Diagnostics;
using DeepWise.Windows;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;

namespace DeepWise.Devices
{
    using Size = System.Drawing.Size;

    public class CameraIDS_U3 : Camera
    {
 
        static CameraIDS_U3()
        {
     
            
            //AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            //Assembly.LoadFile(@"C:\Program Files\IDS\ids_peak\generic_sdk\api\binding\dotnet\x86_64\ids_peak_dotnet.dll");
            //Assembly.LoadFile(@"C:\Program Files\IDS\ids_peak\generic_sdk\ipl\binding\dotnet\x86_64\ids_peak_dotnet_interface.dll");
            peak.Library.Initialize();
            Application.Current.Exit += Current_Exit;
        }

        private static void Current_Exit(object sender, ExitEventArgs e)
        {
            peak.Library.Close();
        }

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Split(',')[0] == "ids_peak")
                return Assembly.LoadFile(@"C:\Program Files\IDS\ids_peak\generic_sdk\api\binding\dotnet\x86_64\ids_peak_dotnet.dll");
            else
                return null;
        }
     

        public CameraIDS_U3()
        {

        }

        public CameraIDS_U3(string serialNumber) : this()
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
                if (device != null)
                {
                    try
                    {
                        return device?.ModelName();
                    }
                    catch { }
                }
                return "null";
            }
        }

        string _serialNumber = "";
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
        public override bool IsOpened => device != null;
        public override bool IsRunning => !AquacisionTask.IsCompleted;
        
        public override T Capture<T>(Func<ImageRecievedEventArgs, T> converter)
        {
            if (!IsOpened) throw new CameraNotOpenException(this);
            bool notRunning = !IsRunning;
            if (notRunning)
            {
                
                _dataStream.StartAcquisition( AcquisitionStartMode.Default,1);
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("AcquisitionStart").Execute();
                //nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("AcquisitionStart").WaitUntilDone();
            }
            var buffer = _dataStream.WaitForFinishedBuffer(2500);
            var scan0 = buffer.BasePtr();
            var width = (int)buffer.Width();
            var height = (int)buffer.Height();
            var format = buffer.GetPixelFormate();
            var args = new ImageRecievedEventArgs(scan0, new Size(width, height), format);
            OnRecievedImage(args);
            T result = converter(args);
            _dataStream.QueueBuffer(buffer);
            if (notRunning)
            {
                _dataStream.StopAcquisition();
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").Execute();
                //nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").WaitUntilDone();
            }
            Debug.WriteLine("Image Captured!");
            return result;
        }

        public override System.Drawing.Size Size
        {
            get
            {
                var width = (int)_nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("Width").Value();
                var height = (int)_nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("Height").Value();
                return new Size(width, height);
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
                var width = (int)_nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("WidthMax").Value();
                var height = (int)_nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("HeightMax").Value();
                return new Size(width, height);
            }
        }
        [Browsable(false)]
        public override System.Drawing.Imaging.PixelFormat PixelFormat
        {
            get
            {
                var format = _nodeMapRemoteDevice.Nodes().Where(x => x.Name().Contains("Format")).Select(x=>x.Name()).ToArray();
                var node = _nodeMapRemoteDevice.FindNode<peak.core.nodes.EnumerationNode>("PixelFormat");
                var f = (peak.ipl.PixelFormatName)node.CurrentEntry().Value();
                
                return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
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
                return _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("ExposureTime").Value();
            }
            set => _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("ExposureTime").SetValue(value);
        }
        public override double MaximumExposure
        {
            get
            {
                return _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("ExposureTime").Maximum();
            }
        }
        public override double MinimumExposure
        {
            get
            {
                return _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("ExposureTime").Minimum();
            }
        }
        public override bool ExposureAuto
        {
            get
            {


                var a = _nodeMapRemoteDevice.FindNode<peak.core.nodes.EnumerationNode>("ExposureAuto").CurrentEntry().Value();
                return a != 2;
            }
            set
            {
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.EnumerationNode>("ExposureAuto").SetCurrentEntry(value ? 2 : 0);
                NotifyPropertyChanged();
            }
        }

        public override double Framerate
        {
            get
            {


                return _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("AcquisitionFrameRateTarget").Value();

            }
            set
            {
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("AcquisitionFrameRateTarget").SetValue(value);
            }
        }
        public override double MaximumFramerate
        {
            get
            {
                return _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("AcquisitionFrameRateTarget").Maximum();
            }
        }
        public override double MinimumFramerate
        {
            get
            {
                return _nodeMapRemoteDevice.FindNode<peak.core.nodes.FloatNode>("AcquisitionFrameRateTarget").Minimum();
            }
        }

        public override bool Initialize()
        {
            var deviceManager = peak.DeviceManager.Instance();
            deviceManager.Update();
            if (string.IsNullOrWhiteSpace(SerialNumber))
            {
                if (!deviceManager.Devices().Any()) throw new Exception("can't found any avalible device.(CameraIDS_U3)");

                device = deviceManager.Devices()[0].OpenDevice(peak.core.DeviceAccessType.Control);
                this._serialNumber = device.SerialNumber();
            }
            else
            {
                device = deviceManager.Devices().FirstOrDefault(x => x.SerialNumber() == SerialNumber).OpenDevice(DeviceAccessType.Control);
            }

            if (device != null)
            {
                var dataStreams = device.DataStreams();

                // Open standard data stream
                _dataStream = dataStreams[0].OpenDataStream();

                // Get nodemap of remote device for all accesses to the genicam nodemap tree
                _nodeMapRemoteDevice = device.RemoteDevice().NodeMaps()[0];
            }

            // To prepare for untriggered continuous image acquisition, load the default user set if available
            // and wait until execution is finished
            try
            {
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.EnumerationNode>("UserSetSelector").SetCurrentEntry("UserSet0");
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("UserSetLoad").Execute();
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("UserSetLoad").WaitUntilDone();
            }
            catch(Exception ex)
            {

            }

            //alloc buffer
            UInt32 payloadSize = Convert.ToUInt32(_nodeMapRemoteDevice.FindNode<peak.core.nodes.IntegerNode>("PayloadSize").Value());
            var bufferCountMax = _dataStream.NumBuffersAnnouncedMinRequired();
            for (var bufferCount = 0; bufferCount < bufferCountMax; ++bufferCount)
            {

                var buffer = _dataStream.AllocAndAnnounceBuffer(payloadSize, IntPtr.Zero);

                _dataStream.QueueBuffer(buffer);
            }
            NotifyPropertyChanged(nameof(IsOpened));
            return true;
        }

        public override bool LoadParameters()
        {
            throw new NotImplementedException();
        }
        public override void SaveParameters()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            Debug.WriteLine("--- [BackEnd] Close device");
            // If device was opened, try to stop acquisition
            if (device != null)
            {
                try
                {
                    var remoteNodeMap = device.RemoteDevice().NodeMaps()[0];
                    remoteNodeMap.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").Execute();
                    remoteNodeMap.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").WaitUntilDone();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
                }
            }

            // If data stream was opened, try to stop it and revoke its image buffers
            if (_dataStream != null)
            {
                try
                {
                    _dataStream.KillWait();
                    try
                    {

                    _dataStream.StopAcquisition(peak.core.AcquisitionStopMode.Default);
                    }catch (Exception e)
                    {

                    }
                    _dataStream.Flush(peak.core.DataStreamFlushMode.DiscardAll);

                    foreach (var buffer in _dataStream.AnnouncedBuffers())
                    {
                        _dataStream.RevokeBuffer(buffer);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
                }
            }

            try
            {
                // Unlock parameters after acquisition stop
                _nodeMapRemoteDevice?.FindNode<peak.core.nodes.IntegerNode>("TLParamsLocked").SetValue(0);
            }
            catch (Exception e)
            {
                Debug.WriteLine("--- [BackEnd] Exception: " + e.Message);
            }
            base.Dispose();
        }

        public override void Start()
        {
            if (IsRunning) return;
            Debug.WriteLine("--- [AcquisitionWorker] Start Acquisition");
            try
            {
                _dataStream.StartAcquisition();
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("AcquisitionStart").Execute();
                _nodeMapRemoteDevice.FindNode<peak.core.nodes.CommandNode>("AcquisitionStart").WaitUntilDone();
            }
            catch (Exception e)
            {
                Debug.WriteLine("--- [AcquisitionWorker] Exception: " + e.Message);
            }

            tokenSource = new CancellationTokenSource();
            AquacisionTask = FreeRunGrabeAsync(tokenSource.Token);
        }

        CancellationTokenSource tokenSource; 
        Task AquacisionTask = Task.Delay(0);
        private Task FreeRunGrabeAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        peak.core.Buffer buffer = null;
                        try
                        {

                            buffer = _dataStream.WaitForFinishedBuffer(1500);
                            Debug.WriteLine("--- [FreeRunGrabe] Success: ");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("--- [FreeRunGrabe] Fail: " + ex.Message);
                            if (buffer != null) _dataStream.QueueBuffer(buffer);
                        }
                        finally
                        {

                        }
                        if (buffer != null)
                        {
                            var scan0 = buffer.BasePtr();
                            var width = (int)buffer.Width();
                            var height = (int)buffer.Height();
                            var format = buffer.GetPixelFormate();
                            OnRecievedImage(new ImageRecievedEventArgs(scan0, new Size(width, height), format));
                            _dataStream.QueueBuffer(buffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    var remoteNodeMap = device.RemoteDevice().NodeMaps()[0];
                    _dataStream.StopAcquisition();
                    remoteNodeMap.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").Execute();
                    remoteNodeMap.FindNode<peak.core.nodes.CommandNode>("AcquisitionStop").WaitUntilDone();
                    Debug.WriteLine("--- [FreeRunGrabe] End");
                }
            }, token);
        }

        public override void Stop()
        {
            if (IsRunning)
            {
                tokenSource.Cancel();
                //while (!AquacisionTask.IsCompleted)
                //    Thread.Sleep(50);
            }
        }

        private peak.core.Device device;
        private peak.core.DataStream _dataStream;
        private peak.core.NodeMap _nodeMapRemoteDevice;
    }

    internal static class U3Extensions
    {
        public static System.Drawing.Imaging.PixelFormat GetPixelFormate(this peak.ipl.PixelFormatName format)
        {
            switch (format)
            {
                case peak.ipl.PixelFormatName.RGB8:
                case peak.ipl.PixelFormatName.BGR8:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case peak.ipl.PixelFormatName.Mono8:
                case peak.ipl.PixelFormatName.BayerRG8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                default:
                    throw new NotImplementedException();
            }
        }
        public static System.Drawing.Imaging.PixelFormat GetPixelFormate(this peak.core.Buffer buffer)
        {
            var format = (peak.ipl.PixelFormatName)((int)buffer.PixelFormat());
            switch (format)
            {
                case peak.ipl.PixelFormatName.RGB8:
                case peak.ipl.PixelFormatName.BGR8:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case peak.ipl.PixelFormatName.Mono8:
                case peak.ipl.PixelFormatName.BayerRG8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                default:
                    throw new NotImplementedException();
            }
        }
    }


}
