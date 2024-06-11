using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;
using System.ComponentModel;
using static ADLINK_DEVICE.APS168;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Concurrent;
using ADLINK_DEVICE;
using Microsoft.VisualStudio.Threading;

namespace MotionControllers.Motion
{
    public partial class ADLINK_Motion : MotionController
    {
        //TODO : 考慮移除AIO事件，只保留DIO事件
        //建立AIO專屬控制向 、在介面關注時才更新其數值
        [Obsolete("若監聽的訊號為Digital訊號，請使用DigitalSignalChanged。若監聽的訊號為Analog類型，請嘗試使用WaitAIO方法")]
        public event EventHandler<IOEventArgs> IOStatusChanged;

        /// <summary>
        /// 此事件主要用於更新UI，請考慮使用WaitDIO方法。
        /// </summary>
        public event EventHandler<IOEventArgs> DigitalSignalChanged;
        
        private void InitializeIOList(IDictionary<int, EC_MODULE_INFO> slaveInfo)
        {
            #region Create General I/O
            switch (Model)
            {
                case DeviceName.AMP_20408C:
                    for (int i = 0; i < 24; i++)
                    {
                        if (i == 4) i = 8;
                        var port = new IOPortInfo(i, IOTypes.General | IOTypes.Digital | IOTypes.Input);
                        IOTable.Add(port.ToString(), port);
                        port = new IOPortInfo(i, IOTypes.General | IOTypes.Digital | IOTypes.Output);
                        IOTable.Add(port.ToString(), port);
                    }
                    break;
                case DeviceName.PCIE_8332:
                case DeviceName.PCIE_8334:
                case DeviceName.PCIE_8338:
                    for (int i = 0; i < 4; i++)
                    {
                        var port = new IOPortInfo(i, IOTypes.General | IOTypes.Digital | IOTypes.Input);
                        IOTable.Add(port.ToString(), port);
                        port = new IOPortInfo(i, IOTypes.General | IOTypes.Digital | IOTypes.Output);
                        IOTable.Add(port.ToString(), port);
                    }
                    break;
            }
            #endregion

            #region Create EtherCAT IO
            if (slaveInfo!= null)
            {
                int AICount = 0;
                int AOCount = 0;
                int DICount = 0;
                int DOCount = 0;
                foreach (var item in slaveInfo)
                {
                    for (int i = 0; i < item.Value.AI_ModuleNum; i++)
                    {
                        var port = new IOPortInfo(item.Key, i, IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Input);
                        IOTable.Add(port.ToString(), port);
                        AICount++;
                    }
                    for (int i = 0; i < item.Value.AO_ModuleNum; i++)
                    {
                        var port = new IOPortInfo(item.Key, i, IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output);
                        IOTable.Add(port.ToString(), port);
                        AOCount++;
                    }
                    for (int i = 0; i < item.Value.DI_ModuleNum; i++)
                    {
                        for (int p = 0; p < 8; p++)
                        {

                            var port = new IOPortInfo(item.Key, DICount, IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input);
                            IOTable.Add(port.ToString(), port);
                            DICount++;
                        }
                    }
                    for (int i = 0; i < item.Value.DO_ModuleNum; i++)
                    {
                        for (int p = 0; p < 8; p++)
                        {
                            var port = new IOPortInfo(item.Key, DOCount, IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output);
                            IOTable.Add(port.ToString(), port);
                            DOCount++;
                        }
                    }
                }
            }

            IOTable.Sort();
            #endregion

            //Resolve Contrast
            try
            {
                IOTable.Load();
            }
            catch (FileNotFoundException ex)
            {
                //resolve old version
                string path = @"D:\mcpInfo";
                if (File.Exists(path))
                {
                    var jObj = JObject.Parse(File.ReadAllText(path));
                    var ioTable = jObj["IOList"];
                    //Load IO
                    foreach (var item in ioTable.ToObject<Dictionary<string, IOPortInfo>>())
                    {
                        var match = IOTable.FirstOrDefault(x => x.Value == item.Value);
                        if (match.Value != null)
                        {
                            IOTable.Remove(match.Key);
                            IOTable.Add(item.Key, item.Value);
                        }
                        else
                        {
                            IOTable.Add(item.Key, item.Value);
                        }
                    }
                    IOTable.Sort();
                    IOTable.Save();
                }
            }
            catch (Exception ex)
            {
                WriteMessage(ex);
            }
        }

        public IODictionary IOTable { get; } = new IODictionary(@"D:\ioConfig");

        public void SetOutputValue(string name, object value)
        {
            var port = IOTable[name];
            switch (port.Type)
            {
                case IOTypes.General | IOTypes.Digital | IOTypes.Output:
                    SetDigitalOutputValue(port, (bool)value);
                    return;
                case IOTypes.General | IOTypes.Analog | IOTypes.Output:
                    SetAnalogOutputValue(port, (double)value);
                    return;
                case IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output:
                    SetDigitalOutputValueEtherCAT(port, (bool)value);
                    return;
                case IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output:
                    SetAnalogOutputValueEtherCAT(port, (double)value);
                    return;
                default:
                    if (port.Type.HasFlag(IOTypes.Input))
                        throw new Exception("'Input' signal value can not set be set");
                    else
                        throw new ArgumentException();
            }
        }
        public void SetOutputValue(IOPortInfo port, object value)
        {
            switch (port.Type)
            {
                case IOTypes.General | IOTypes.Digital | IOTypes.Output:
                    SetDigitalOutputValue(port, (bool)value);
                    return;
                case IOTypes.General | IOTypes.Analog | IOTypes.Output:
                    SetAnalogOutputValue(port, (double)value);
                    return;
                case IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output:
                    SetDigitalOutputValueEtherCAT(port, (bool)value);
                    return;
                case IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output:
                    SetAnalogOutputValueEtherCAT(port, (double)value);
                    return;
                default:
                    if (port.Type.HasFlag(IOTypes.Input))
                        throw new Exception("'Input' signal value can not set be set");
                    else
                        throw new ArgumentException();
            }
        }
        private object GetIOValue(string name)
        {
            var port = IOTable[name];
            return port.Type switch
            {
                IOTypes.General | IOTypes.Digital | IOTypes.Input => GetDigitalInputValue(port),
                IOTypes.General | IOTypes.Analog | IOTypes.Input => GetAnalogInputValue(port),
                IOTypes.General | IOTypes.Digital | IOTypes.Output => GetDigitalOutputValue(port),
                IOTypes.General | IOTypes.Analog | IOTypes.Output => GetAnalogOutputValue(port),
                IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input => GetDigitalInputValueEtherCAT(port),
                IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Input => GetAnalogInputValueEtherCAT(port),
                IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output => GetDigitalOutputValueEtherCAT(port),
                IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output => GetAnalogOutputValueEtherCAT(port),
                _ => throw new ArgumentException(),
            };
        }
        private object GetIOValue(IOPortInfo port)
        {
            return port.Type switch
            {
                IOTypes.General  | IOTypes.Digital | IOTypes.Input  => GetDigitalInputValue(port),
                IOTypes.General  | IOTypes.Analog  | IOTypes.Input  => GetAnalogInputValue(port),
                IOTypes.General  | IOTypes.Digital | IOTypes.Output => GetDigitalOutputValue(port),
                IOTypes.General  | IOTypes.Analog  | IOTypes.Output => GetAnalogOutputValue(port),
                IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input  => GetDigitalInputValueEtherCAT(port),
                IOTypes.EtherCAT | IOTypes.Analog  | IOTypes.Input  => GetAnalogInputValueEtherCAT(port),
                IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output => GetDigitalOutputValueEtherCAT(port),
                IOTypes.EtherCAT | IOTypes.Analog  | IOTypes.Output => GetAnalogOutputValueEtherCAT(port),
                _ => throw new ArgumentException(),
            };
        }

        public  T GetIOValue<T>(string name) => (T)GetIOValue(name);
        public T GetIOValue<T>(IOPortInfo port) => (T)GetIOValue(port);

        public Task Wait() => Monitor.Wait();

        public async Task WaitDIO(string name, bool value)
        {
            var dic = IOTable;
            if (dic.ContainsKey(name))
                await WaitDIO(dic[name], value);
            else
                throw new IONotFoundException(name);
        }
        public async Task WaitDIO(string name, bool value, int timeout)
        {
            if (IOTable.ContainsKey(name))
                await WaitDIO(IOTable[name], value, timeout);
            else
                throw new IONotFoundException(name);
        }
        public async Task WaitDIO(string name, bool value, CancellationToken token)
        {
            if (IOTable.ContainsKey(name))
                await WaitDIO(IOTable[name], value, token);
            else
                throw new IONotFoundException(name);
        }

        public async Task WaitAIO(string name, Predicate<double> condiction)
        {
            if (IOTable.ContainsKey(name))
                await WaitAIO(IOTable[name], condiction);
            else
                throw new IONotFoundException(name);
        }
        public async Task WaitAIO(string name, Predicate<double> condiction, int timeout)
        {
            if (IOTable.ContainsKey(name))
                await WaitAIO(IOTable[name], condiction, timeout);
            else
                throw new IONotFoundException(name);
        }
        public async Task WaitAIO(string name, Predicate<double> condiction, CancellationToken token)
        {
            if (IOTable.ContainsKey(name))
                await WaitAIO(IOTable[name], condiction, token);
            else
                throw new IONotFoundException(name);
        }

        #region Obsolete
        [Obsolete("Use GetIOValue(string) instead")]
        public object GetInputValue(string name)
        {
            var matchs = IOTable.Where(x => x.Key == name && x.Value.Type.HasFlag(IOTypes.Input));
            switch (matchs.Count())
            {
                case 0:
                    throw new IONotFoundException(name, IOTypes.Input);
                case 1:
                    var info = matchs.First().Value;
                    return info.Type switch
                    {
                        IOTypes.General | IOTypes.Digital | IOTypes.Input => GetDigitalInputValue(info),
                        IOTypes.General | IOTypes.Analog | IOTypes.Input => GetAnalogInputValue(info),
                        IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input => GetDigitalInputValueEtherCAT(info),
                        IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Input => GetAnalogInputValueEtherCAT(info),
                        _ => throw new ArgumentException(),
                    };
                default:
                    throw new MemberAccessException($"存在多於一個名稱為\"{name}\"的點位");

            }
        }
        [Obsolete("Use GetIOValue(string) instead")]
        public object GetOutputValue( string name)
        {
            var matchs = IOTable.Where(x => x.Key == name && x.Value.Type.HasFlag(IOTypes.Output));
            switch (matchs.Count())
            {
                case 0:
                    throw new IONotFoundException(name, IOTypes.Output);
                case 1:
                    var info = matchs.First().Value;
                    return info.Type switch
                    {
                        IOTypes.General | IOTypes.Digital | IOTypes.Output => GetDigitalOutputValue(info),
                        IOTypes.General | IOTypes.Analog | IOTypes.Output => GetAnalogOutputValue(info),
                        IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output => GetDigitalOutputValueEtherCAT(info),
                        IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output => GetAnalogOutputValueEtherCAT(info),
                        _ => throw new ArgumentException(),
                    };
                default:
                    throw new MemberAccessException($"存在多於一個名稱為\"{name}\"的Output點位");
            }
        }
        [Obsolete("Use GetIOValue<T>(string) instead")]
        public  T GetInputValue<T>( string name) => (T)GetInputValue(name);
        [Obsolete("Use GetIOValue<T>(string) instead")]
        public  T GetOutputValue<T>(string name) => (T)GetOutputValue(name);

        //TODO : 移除牽動的方法
        public async Task WaitDIO(IOPortInfo info, bool target)
        {
            if (!info.Type.HasFlag(IOTypes.Digital)) throw new ArgumentException();

            if (GetIOValue<bool>(info) == target) return;

            //listening event
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            DigitalSignalChanged += SetEvent;
            
            await Task.Run(resetEvent.WaitOne);

            void SetEvent(object sender, IOEventArgs e)
            {
                if (e.Match(info) && (bool)e.Value == target)
                {
                    DigitalSignalChanged -= SetEvent;
                    resetEvent.Set();
                }
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="target"></param>
        /// <param name="timeout"></param>
        /// <returns>return false when time out</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task WaitDIO(IOPortInfo info, bool target, int timeout)
        {
            if (!info.Type.HasFlag(IOTypes.Digital)) throw new ArgumentException();

            //check current value
            if (GetIOValue<bool>(info) == target) return;

            //listening event
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            DigitalSignalChanged += SetEvent;

            if (!await Task.Run(() => resetEvent.WaitOne(timeout)))
            {
                DigitalSignalChanged -= SetEvent;
                throw new TimeoutException();
            }

            void SetEvent(object sender, IOEventArgs e)
            {
                if (e.Match(info) && (bool)e.Value == target)
                {
                    resetEvent.Set();
                    DigitalSignalChanged -= SetEvent;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns>return <see cref="false"/> when cancellation is requested.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task WaitDIO(IOPortInfo info, bool target, CancellationToken token)
        {
            if (!info.Type.HasFlag(IOTypes.Digital)) throw new ArgumentException();

            if (GetIOValue<bool>(info) == target) return ;

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            DigitalSignalChanged += SetEvent;

            await Task.Run(() =>
            {
                WaitHandle.WaitAny(new[] { resetEvent, token.WaitHandle });
            });
            DigitalSignalChanged -= SetEvent;

            void SetEvent(object sender, IOEventArgs e)
            {
                if (e.Match(info) && (bool)e.Value == target)
                {
                    DigitalSignalChanged -= SetEvent;
                    resetEvent.Set();
                }
            }
        }

        public async Task WaitAIO(IOPortInfo info, Predicate<double> condition)
        {
            if (!info.Type.HasFlag(IOTypes.Analog)) throw new ArgumentException();

            while (!condition(GetIOValue<double>(info)))
                await Task.Delay(50);
        }
        public async Task WaitAIO(IOPortInfo info, Predicate<double> condition, int timeout)
        {
            if (!info.Type.HasFlag(IOTypes.Analog)) throw new ArgumentException();
            while (!condition(GetIOValue<double>(info)))
            {
                await Task.Delay(50);
                timeout -= 50;
                if (timeout <= 0) throw new TimeoutException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="condition"></param>
        /// <param name="token"></param>
        /// <returns>return false if cancellation token is requested.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task WaitAIO(IOPortInfo info, Predicate<double> condition, CancellationToken token)
        {
            if (!info.Type.HasFlag(IOTypes.Analog)) throw new ArgumentException();
            while (!condition(GetIOValue<double>(info)))
            {
                await Task.Delay(50);
                if (token.IsCancellationRequested) return ;
            }
        }
        #endregion

        #region Native Generic I/O
        bool[] DIData
        {
            get
            {
                if (_diData == null)
                {
                    try
                    {
                        GetDigitalInputValue(new IOPortInfo(0, IOTypes.Input | IOTypes.Digital | IOTypes.General));
                    }
                    catch
                    {
                        return null;
                    }

                    _diData = new bool[32];
                    switch (Model)
                    {
                        case DeviceName.AMP_20408C:
                            {
                                int value = 0;
                                APS_read_d_input(this.BoardID, 0, out value);
                                for (int i = 0; i < 32; i++)
                                {
                                    var tmp = (((value >> i) & 1) == 1) ? 1 : 0;
                                    switch (Model)
                                    {
                                        case DeviceName.AMP_20408C:

                                            break;
                                        default:

                                            break;
                                    }
                                    _diData[i] = tmp != 0;
                                }
                                break;
                            }
                        case DeviceName.PCIE_8332:
                        case DeviceName.PCIE_8334:
                        case DeviceName.PCIE_8338:
                            for (int i = 0; i < 4; i++)
                            {
                                APS_read_d_channel_input(this.BoardID, 0, i, out var value);
                                _diData[i] = value != 0;
                            }
                            break;

                    }

                }
                return _diData;
            }
        }
        bool[] _diData;
        Dictionary<(int slaveID, int port), bool> diEtherData = new Dictionary<(int slaveID, int port), bool>();
        //[Digital]
        private bool GetDigitalInputValue(IOPortInfo port)
        {
            int value;
            int result;
            switch (Model)
            {
                case DeviceName.AMP_20408C:
                    result = APS_read_d_input(this.BoardID, 0, out value);
                    value = (((value >> port.Channel) & 1) == 1) ? 1 : 0;
                    break;
                default:
                    result = APS_read_d_channel_input(this.BoardID, 0, port.Channel, out value);
                    break;
            }
            if (result == 0)
                return value == 1;
            else
                throw new APSException(result);
        }
        private bool GetDigitalOutputValue(IOPortInfo port)
        {
            int value = 0;
            var result = APS_read_d_channel_output(this.BoardID, 0, port.Channel, ref value);
            if (result == 0)
                return value == 1;
            else
                throw new APSException(result);
        }
        private void SetDigitalOutputValue(IOPortInfo port, bool value)
        {
            //if (IOTable.Rules.ContainsKey(port) && IOTable.Rules[port].Any(x => !x.Validate(this, value))) throw new IOOperationInvalidException(port.ToString(this));

            if (GetDigitalOutputValue(port) == value) return;
            var result = APS_write_d_channel_output(this.BoardID, 0, port.Channel, value ? 1 : 0);
            if (result == 0)
            {
#if RunIOEventInTask
                Task.Run(() =>
                {
                    IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Digital | IOTypes.Output, channel, value));
                });
#else
                IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Digital | IOTypes.Output, port.Channel, value));
                DigitalSignalChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Digital | IOTypes.Output, port.Channel, value));
#endif
            }
            else
                throw new APSException(result);
        }

        //[Analog]
        private double GetAnalogInputValue(IOPortInfo port)
        {
            double value = 0;
            var result = Model switch
            {
                DeviceName.AMP_20408C => throw new Exception($"不支援該操作"),
                _ => APS_read_a_input_value(this.BoardID, port.Channel, ref value),
            };
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        private double GetAnalogOutputValue(IOPortInfo port)
        {
            double value = 0;
            var result = Model switch
            {
                DeviceName.AMP_20408C => throw new Exception($"不支援該操作"),
                _ => APS_read_a_output_value(this.BoardID, port.Channel, ref value),
            };
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }

        private void SetAnalogOutputValue(IOPortInfo port, double value)
        {
            //if (IOTable.Rules.ContainsKey(port) && IOTable.Rules[port].Any(x => !x.Validate(this, value))) throw new IOOperationInvalidException(port.ToString(this));

            double oldValue = GetAnalogOutputValue(port);
            var result = Model switch
            {
                DeviceName.AMP_20408C => throw new Exception($"不支援該操作"),
                _ => APS_write_a_output_value(this.BoardID, port.Channel, value),
            };
            double newValue = GetAnalogOutputValue(port);
            if (result == 0)
            {
                if (oldValue != newValue)
                {
#if RunIOEventInTask
                    Task.Run(() =>
                    {
                        IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Analog | IOTypes.Output, channel, value));
                    });
#else
                    IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Analog | IOTypes.Output, port.Channel, value));
#endif
                }
            }
            else
                throw new APSException(result);
        }

        public bool IsEtherCATSupported
        {
            get
            {
                switch (this.Model)
                {
                    case DeviceName.NULL:
                    case DeviceName.AMP_20408C:
                        return false;
                    case DeviceName.PCI_825458:
                    case DeviceName.PCIE_8332:
                    case DeviceName.PCIE_8334:
                    case DeviceName.PCIE_8338:
                        return true;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        //[Digital]
        private bool GetDigitalInputValueEtherCAT(IOPortInfo port)
        {
            int result = APS_get_field_bus_d_channel_input(this.BoardID, 0, port.SlaveID, port.Channel, out var value);
            if (result == 0) //success
                return value != 0;
            else
                throw new APSException(result);
        }

        private void SetDigitalOutputValueEtherCAT(IOPortInfo port, bool value)
        {
            //if (IOTable.Rules.ContainsKey(port) && IOTable.Rules[port].Any(x => !x.Validate(this,value))) throw new IOOperationInvalidException(port.ToString(this));

            if (GetDigitalOutputValueEtherCAT(port) == value) return;
            int result = APS_set_field_bus_d_channel_output(this.BoardID, 0, port.SlaveID, port.Channel, value ? 1 : 0);
            if (result == 0)
            {
#if RunIOEventInTask
                Task.Run(() =>
                {
                    IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output, slave_id, channel, value));
                });
#else
                IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output, port.SlaveID, port.Channel, value));
                DigitalSignalChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output, port.SlaveID, port.Channel, value));
#endif
            }
            else
                throw new APSException(result);
        }
        private bool GetDigitalOutputValueEtherCAT(IOPortInfo port)
        {
            int value = 0;
            int result = APS_get_field_bus_d_channel_output(this.BoardID, 0, port.SlaveID, port.Channel, ref value);
            if (result == 0)
                return value == 1;
            else
                throw new APSException(result);
        }
        //[Analog]
        private double GetAnalogInputValueEtherCAT(IOPortInfo port)
        {
            int result = APS_get_field_bus_a_input(this.BoardID, 0, port.SlaveID, port.Channel, out var value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        private void SetAnalogOutputValueEtherCAT(IOPortInfo port, double value)
        {
            //if (IOTable.Rules.ContainsKey(port) && IOTable.Rules[port].Any(x => !x.Validate(this, value))) throw new IOOperationInvalidException(port.ToString(this));

            if (GetAnalogOutputValueEtherCAT(port) == value) return;
            int result = APS_set_field_bus_a_output(this.BoardID, 0, port.SlaveID, port.Channel, value);
            if (result == 0)
            {

#if RunIOEventInTask
                Task.Run(() =>
                {
                    IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Output, port.SlaveID, port.Channel, value));
                });
#else
                IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Output, port.SlaveID, port.Channel, value));
#endif
            }
            else
                throw new APSException(result);
        }
        private double GetAnalogOutputValueEtherCAT(IOPortInfo port)
        {
            double value = 0;
            int result = APS_get_field_bus_a_output(this.BoardID, 0, port.SlaveID, port.Channel, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }

        Dictionary<int, double> aiData = new Dictionary<int, double>();
        Dictionary<(int slaveID, int port), double> aiEtherData = new Dictionary<(int slaveID, int port), double>();
        //public double AnalogInputError { get; set; } = 0.1;
        #endregion

        private void UpdateEtherCATInput(object sender, EventArgs e)
        {
            foreach (var item in this.diDataEther)
            {
                for (int i = 0; i < item.Value.Length; i++)
                {
                    APS_get_field_bus_d_channel_input(this.BoardID, 0, item.Key, i, out var value);
                    bool tmp = value != 0;
                    if (tmp != diDataEther[item.Key][i])
                    {
                        diDataEther[item.Key][i] = tmp;
#if RunIOEventInTask
                        Task.Run(() =>
                        {
                            IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input, item.Key, i, tmp));
                        });
#else
                        IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input, item.Key, i, tmp));
                        DigitalSignalChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Digital | IOTypes.Input, item.Key, i, tmp));
#endif


                    }
                }
            }

            foreach (var item in this.aiDataEther)
            {
                for (int i = 0; i < item.Value.Length; i++)
                {
                    APS_get_field_bus_a_input(this.BoardID, 0, item.Key, i, out var tmp);
                    if (tmp != aiDataEther[item.Key][i])
                    {
                        aiDataEther[item.Key][i] = tmp;
#if RunIOEventInTask
                        Task.Run(() =>
                        {
                            IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Input, item.Key, i, tmp));
                        });
#else
                        IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.EtherCAT | IOTypes.Analog | IOTypes.Input, item.Key, i, tmp));
#endif

                    }
                }
            }
        }
        private void UpdateGeneralInput(object sender, EventArgs e)
        {

            for (int i = 0; i < 24; i++)
            {
                if (i == 4)
                {
                    switch (Model)
                    {
                        case DeviceName.AMP_20408C:
                            i = 8;
                            break;
                        case DeviceName.PCIE_8332:
                        case DeviceName.PCIE_8334:
                        case DeviceName.PCIE_8338:
                            i = 24;
                            continue;
                        default:
                            throw new NotImplementedException();
                    }
                }

                bool value = default;
                switch (Model)
                {
                    case DeviceName.AMP_20408C:
                        APS_read_d_input(this.BoardID, i, out var tmp);
                        value = tmp != 0;
                        break;
                    case DeviceName.PCIE_8332:
                    case DeviceName.PCIE_8334:
                    case DeviceName.PCIE_8338:
                        APS_read_d_channel_input(this.BoardID, 0,i, out tmp);
                        value = tmp != 0;
                        continue;
                    default:
                        throw new NotImplementedException();
                }
               
                if (DIData[i] != value)
                {
                    DIData[i] = value;
#if RunIOEventInTask
                            Task.Run(() => 
                            {
                                IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Digital | IOTypes.Input, i, value));
                            });
#else
                    IOStatusChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Digital | IOTypes.Input, i, value));
                    DigitalSignalChanged?.Invoke(this, new IOEventArgs(IOTypes.General | IOTypes.Digital | IOTypes.Input, i, value));
#endif
                }
            }
           
            //Analog
            switch (Model)
            {
                case DeviceName.AMP_20408C:
                case DeviceName.PCIE_8332:
                case DeviceName.PCIE_8334:
                case DeviceName.PCIE_8338:
                    break;//不支援
                default:
                    throw new NotImplementedException();
            }
        }
    }
}