using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeepWise.Devices
{
    //此頁測試中，尚未開用！！！

    public interface IDevice
    {
        bool Initialize();
        bool IsOpened { get; }
    }

    public interface ISyncIni
    {
        string SyncKey { get; }
    }

    public interface IReconnectable
    {
        bool IsConnected { get; }
        bool Reconnect();
    }

    public static class DevicesExtension
    {
        public static bool InitializeDevices(IEnumerable<IDevice> devices)
        {
            return new DeepWise.Devices.DevicesIniWindow(devices.ToArray()).ShowDialog().Value;

        }
    }

    public class MainWindowww
    {
        public MainWindowww()
        {
            
            

        }

        void InitializeDevices()
        {
            Devices.Add(nameof(Camera), Camera);
            Devices.Add(nameof(Camera), Camera2, x =>
            {
                CameraIDS cam = x as CameraIDS;
                cam.Start();
            });
            Devices.Initialize();
        }
        public CameraIDS Camera { get; } = new CameraIDS();
        public CameraIDS Camera2 { get; } = new CameraIDS();
        public DeviceResource Devices { get; } = new DeviceResource();
    }


    public class DeviceResource : Dictionary<string, IDevice>
    {
        public bool Initialized { get; private set; } = false;
        public event EventHandler<DeviceInitializeEventArgs> DeviceInitialized;
        internal void OnDeviceInitialized(string name,IDevice device)
        {
            DeviceInitialized?.Invoke(this, new DeviceInitializeEventArgs(name,device));
        }
        private List<IReconnectable> reconnectables { get; } = new List<IReconnectable>();
        private Dictionary<string, Task> PostTask { get; }=new Dictionary<string, Task>();
        private Dictionary<string, Action<IDevice>> PostAction { get; }=new Dictionary<string, Action<IDevice>>();
        public void Add(string key,IDevice device,Action<IDevice> continousWith)
        {
            this.Add(key, device);
            PostAction.Add(key, continousWith);
        }

        public async Task Initialize()
        {
            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach(var key in PostAction.Keys)
            {
                var task = Task.Run(this[key].Initialize);
                PostTask.Add(key, task.ContinueWith(x => PostAction[key](this[key])));
                tasks.Add(task);
            }
            Task t;
            
            await Task.WhenAll(tasks);
            await Task.WhenAll(PostTask.Values);
        }

        void AutoReconnect()
        {
            while(true)
            {
                
                foreach(var item in reconnectables)
                {
                    if(item.IsConnected)
                    {
                        try
                        {
                            item.Reconnect();
                        }
                        catch
                        {

                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        public async Task DisposeAll()
        {
            if (!Initialized) throw new Exception("not initlized yet");
            var tasks = new List<Task>();
            foreach (var device in this.Values)
            {
                if(device is IDisposable disposable)
                    tasks.Add(Task.Run(disposable.Dispose));
            }
            await Task.WhenAll(tasks.ToArray());
        }
    }

    public class DeviceInitializeEventArgs : EventArgs
    {
        public DeviceInitializeEventArgs(string name,IDevice device)
        {
            Device = device;
            DeviceName = name;
        }
        public IDevice Device { get; }
        public string DeviceName { get; }
    }


    internal class DeviceInitializer : INotifyPropertyChanged
    {
        public IDevice Device { get; }
        public DeviceInitializer(IDevice device,Action<IDevice> continuWith = null)
        {
            Device = device;
            this.continuWith = continuWith;
        }
        private Action<IDevice> continuWith;
        public string Status { get; private set; } = "等候";
        public string Name => Device.ToString();
        public async Task<bool> Initialize()
        {
            
            _indicateColor = Colors.Orange;
            Status = "正在連線";
            ShowRestartButton = Visibility.Hidden;
            Update();
            bool result = await Task.Run(Device.Initialize);
            if (result)
            {
                if(continuWith!=null)
                {
                    _indicateColor = Colors.Yellow;
                    Status = "等候中";
                    Update();
                    await Task.Run(() => continuWith.Invoke(Device));
                }
                _indicateColor = Colors.Green;
                IsDone = true;
                Status = "連線成功";
                Update();
            }
            else
            {
                ShowRestartButton = Visibility.Visible;
                _indicateColor = Colors.Red;
                Status = "連線失敗";
                Update();
            }
            
            return result;
        }

        public bool IsDone { get; private set; } = false;
        [Browsable(false)]
        public Visibility ShowRestartButton { get; private set; } = Visibility.Hidden;
        void Update()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IndicateColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowRestartButton)));
        }
        Color _indicateColor = Colors.Orange;

        public event PropertyChangedEventHandler PropertyChanged;

        public Color IndicateColor => _indicateColor;

    }

    public class DevicesInitializer
    {
        public List<IDevice> Devices { get; } = new List<IDevice>();
        public Dictionary<IDevice, string> Queues { get; } = new Dictionary<IDevice, string>();
        public Dictionary<IDevice, Action<IDevice>> Actions { get; } = new Dictionary<IDevice, Action<IDevice>>();
        public void Regist(IDevice device, Action<IDevice> continuWith = null, string queue = null)
        {
            Devices.Add(device);
            if(continuWith != null)
                Actions.Add(device, continuWith);
            if (!string.IsNullOrEmpty(queue))
                Queues.Add(device, queue);
        }
        public void Initialize()
        {
            //foreach(var device in Devices)
            //new DeviceInitializer(device,Actions[device], Queues[device])
        }
    }
}