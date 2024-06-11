using ADLINK_DEVICE;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static ADLINK_DEVICE.APS168;

namespace MotionControllers.Motion
{
    public class AxisMotionController : INotifyPropertyChanged
    {
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? "Axis" + AxisID : Name;
        }
        ADLINK_Motion _cntlr;
        [JsonIgnore]
        public AxisMotorPosition Position { get; }
        public string Name
        {
            get => _name;
            set
            {
                if (Name == value) return;
                if (_cntlr.Axes.Any(x => x.Name == value))
                    throw new Exception($"name \"{value}\" already exists");
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        public int AxisID { get; }
        public AxisMotionController(ADLINK_Motion cntlr, int axisID)
        {
            _cntlr = cntlr;
            AxisID = axisID;
            Position = new AxisMotorPosition(cntlr,axisID);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void SetParameters(AxisSpeedSetting config)
        {
            if (config.Speed.HasValue) VM = config.Speed.Value;
            if (config.Acceleration.HasValue) ACC = config.Acceleration.Value;
            if (config.Deacceleration.HasValue) DEC = config.Deacceleration.Value;
            if (config.SpeedJog.HasValue) JogVM = config.SpeedJog.Value;
            if (config.AccelerationJog.HasValue) JogACC = config.AccelerationJog.Value;
        }

        [Obsolete]
        public void MoveToAbsolutePosition(int destination) => MoveAbsolute(destination);

        public KeyedLocations PointTable { get; } = new KeyedLocations();
        int _distanceRelative = 3000;
        public int DistanceRelative
        {
            get => _distanceRelative;
            set
            {
                if (_distanceRelative == value) return;
                _distanceRelative = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DistanceRelative)));
            }
        }

        public bool IsAt(int position)
        {
            int diff = Position - position;
            if (diff < 0) diff = -diff;
            return diff <= ErrorPos;
        }
        public bool IsAt(string position)
        {
            double diff = Position - PointTable[position];
            if (diff < 0) diff = -diff;
            return diff <= ErrorPos;
        }

        [Obsolete("Use IsAt(...) instead")]
        public bool IsInLocation(string v) => IsAt(v);

        public bool GetMotionIOStatus(MotionIOStatus status)
        {
            return _cntlr.GetMotionIOStatus(AxisID, status);
        }

        public bool GetMotionStatus(MotionStatus status)
        {
            return _cntlr.GetMotionStatus(AxisID, status);
        }

        public async Task WaitArrived(int position, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var diff = Position  - position;
                if (diff < 0) diff = -diff;
                if (diff <= ErrorPos) break;
                await Task.Delay(50);
            }
        }
        public async Task WaitArrived(string position, CancellationToken token)
        {
            int _location = PointTable[position];
            while (!token.IsCancellationRequested)
            {
                var diff = Position - _location;
                if (diff < 0) diff = -diff;
                if (diff <= ErrorPos) break;
                await Task.Delay(50);
            }
        }
        public async Task WaitArrived(int position, int timeout)
        {
            while (timeout > 0)
            {
                var diff = Position - position;
                if (diff < 0) diff = -diff;
                if (diff <= ErrorPos) break;
                await Task.Delay(50);
                timeout -= 50;
            }
            if (timeout < 0) throw new TimeoutException();
        }
        public async Task WaitArrived(string position, int timeout)
        {
            int _location = PointTable[position];
            while (timeout > 0)
            {
                var diff = Position - _location;
                if (diff < 0) diff = -diff;
                if (diff <= ErrorPos) break;
                await Task.Delay(50);
                timeout -= 50;
            }
            if (timeout < 0) throw new TimeoutException();
        }

        #region AxisParameters
        /// <summary>
        /// maximum speed
        /// </summary>
        [ParameterID(APS_Define.PRA_VM)]
        public double VM { get => GetValue(); set => SetValue(value); }
        [ParameterID(APS_Define.PRA_ACC)]
        public double ACC { get => GetValue(); set => SetValue(value); }
        [ParameterID(APS_Define.PRA_DEC)]
        public double DEC { get => GetValue(); set => SetValue(value); }

        [ParameterID(APS_Define.PRA_HOME_ACC)]
        public double HomeACC { get => GetValue(); set => SetValue(value); }
        [ParameterID(APS_Define.PRA_HOME_VM)]
        public double HomeVM { get => GetValue(); set => SetValue(value); }

        [ParameterID(APS_Define.PRA_JG_ACC)]
        public double JogACC { get => GetValue(); set => SetValue(value); }
        [ParameterID(APS_Define.PRA_JG_VM)]
        public double JogVM { get => GetValue(); set => SetValue(value); }
        #endregion

        #region Motion
        /// <summary>
        /// 該函數用於啟動單軸回Home運動，並且等候直到該軸"<see cref=" MotionStatus.HMV"/>"訊號停止為止。
        /// </summary>
        /// <param name="axisID"></param>
        /// <returns></returns>
        public async Task MoveHome()
        {
            int result = APS_home_move(AxisID);
            if (result != 0)
                throw new APSException(result);
            await Task.Delay(500);
            await _cntlr.WaitMotionStatus(AxisID, MotionStatus.HMV, false);
            return;
        }

        /// <summary>
        /// 該函數用於啟動單軸回Home運動，並且等候直到該軸"<see cref=" MotionStatus.HMV"/>"訊號停止為止。
        /// </summary>
        /// <param name="axisID"></param>
        /// <returns>return false while cancellation is requested.</returns>
        public async Task<bool> MoveHome(CancellationToken token)
        {
            int result = APS_home_move(AxisID);
            if (result != 0)
                throw new APSException(result);
            await Task.Delay(500, token);
            await _cntlr.WaitMotionStatus(AxisID, MotionStatus.HMV, false, token);
            return !token.IsCancellationRequested;
        }

        
        public void MoveAbsolute(int position, int? maxSpeed = null)
        {
            var result = APS_absolute_move(AxisID, position, maxSpeed != null ? maxSpeed.Value : (int)VM);
            if (result != 0) throw new APSException(result);
        }

        public void MoveAbsolute(string position, int? maxSpeed = null)
        {
            var result = APS_absolute_move(AxisID, (int)PointTable[position], maxSpeed != null ? maxSpeed.Value : (int)VM);
            if (result != 0) throw new APSException(result);
        }

        public void MoveRelative(int distance, int? maxSpeed = null)
        {
            var result = APS_relative_move(AxisID, distance, maxSpeed != null ? maxSpeed.Value : (int)VM);
            if (result != 0) throw new APSException(result);
        }


        public void JogStart(AxisDirection direction)
        {
            var result = APS_set_axis_param(AxisID, 65, (int)direction);
            if (result != 0) throw new APSException(result);
            result = APS_jog_start(AxisID, 1);
            if (result != 0) throw new APSException(result);
        }


        public void StopJogMotion()
        {
            var result = APS_jog_start(AxisID, 0);
            if (result != 0) throw new APSException(result);
        }

        public void StopMove()
        {
            var result = APS_stop_move(AxisID);
            if (result != 0) throw new APSException(result);
        }

        public void SetServo(bool state)
        {
            var result = APS_set_servo_on(AxisID, state ? 1 : 0);
            if (result != 0) throw new APSException(result);
        }

        public void ResetAlarm()
        {
            int result = 0;
            if (Math.Abs(Position - Position.Command) > ErrorPos)
            {
                //Set command to feedback position to avoid crash dash
                switch (_cntlr.Model)
                {
                    case DeviceName.AMP_20408C:
                        result = APS_set_command(AxisID, Position);
                        break;
                    case DeviceName.PCIE_8332:
                    case DeviceName.PCIE_8334:
                    case DeviceName.PCIE_8338:
                        result = APS_set_command_f(AxisID, Position);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (result != 0) throw new APSException(result);
            }

            APS_reset_field_bus_alarm(AxisID);
            if (result != 0) throw new APSException(result);
        }
        public static double ErrorPos = 10;

        public async Task WaitMotionStatus(MotionStatus target, bool targetValue)
        {
            await _cntlr.WaitMotionStatus(AxisID, target, targetValue);
        }

        public async Task WaitMotionStatus(MotionStatus target, bool targetValue, int timeout)
        {
            await _cntlr.WaitMotionStatus(AxisID, target, targetValue, timeout);
        }

        public async Task WaitMotionStatus(MotionStatus target, bool targetValue, CancellationToken token)
        {
            await _cntlr.WaitMotionStatus(AxisID, target, targetValue, token);
        }

        public async Task WaitMotionIOStatus(MotionIOStatus target, bool targetValue)
        {
            await _cntlr.WaitMotionIOStatus(AxisID, target, targetValue);
        }

        public async Task WaitMotionIOStatus(MotionIOStatus target, bool targetValue, int timeout)
        {
            await _cntlr.WaitMotionIOStatus(AxisID, target, targetValue, timeout);
        }

        public async Task WaitMotionIOStatus(MotionIOStatus target, bool targetValue, CancellationToken token)
        {
            await _cntlr.WaitMotionIOStatus(AxisID, target, targetValue, token);
        }
        #endregion

        #region Native
        string _name;
        double GetValue([CallerMemberName] string name = "")
        {
            var tmp = 0.0;
            APS_get_axis_param_f(AxisID, GetKey(name), ref tmp);
            return tmp;
        }
        void SetValue(double value, [CallerMemberName] string name = "")
        {
            var key = GetKey(name);
            APS_set_axis_param_f(AxisID, key, value);
            if (key == (int)APS_Define.PRA_JG_ACC)
                APS_set_axis_param_f(AxisID, (int)APS_Define.PRA_JG_DEC, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        int GetKey(string name)
        {
            if (!ParametersID.ContainsKey(name)) ParametersID.Add(name, typeof(AxisMotionController).GetProperty(name).GetCustomAttribute<ParameterIDAttribute>().ID);
            return ParametersID[name];
        }
        private static Dictionary<string, int> ParametersID { get; } = new Dictionary<string, int>();
        [Obsolete("Use Name instead"), JsonIgnore]
        public string AxisName => Name;
        [Obsolete("Use PointTable instead"),JsonIgnore]
        public KeyedLocations ABSLocations => PointTable;
        #endregion

    }
    public class AxesParameterCollection : KeyedCollection<string, AxisMotionController>
    {
        public new AxisMotionController this[int axisID] => this.First(x => x.AxisID == axisID);

        const string path = @"D:\axes";
        public void Save()
        {
            var jString = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, jString);
        }

        public void Load()
        {
            var array = JArray.Parse(File.ReadAllText(path));
            if (array.Count != this.Count) throw new Exception("存檔與當前軸的數量不匹配");
            foreach (var item in array)
            {
                var id = item["AxisID"].ToObject<int>();
                var ctlr = this[id];
                ctlr.PointTable.Clear();
                JsonConvert.PopulateObject(item.ToString(), ctlr);
            }
        }

        protected override string GetKeyForItem(AxisMotionController item) => item.Name;
    }

    [DebuggerDisplay("{Feedback}(Command:{Command})")]
    public class AxisMotorPosition : INotifyPropertyChanged
    {
        public AxisMotorPosition(ADLINK_Motion controller,int axisID)
        {
            AxisID = axisID;
            //controller.Monitor.Update += Update;
        }

        public static implicit operator int(AxisMotorPosition p) => p.Feedback;

        public int AxisID { get; }

        public int Feedback
        {
            get
            {
                var result = APS_get_position(AxisID, out var tmp);
                if (result != 0) throw new APSException(result);
                return tmp;
            }
        }
        public int Command
        {
            get
            {
                var result = APS_get_command(AxisID, out var tmp);
                if (result != 0) throw new APSException(result);
                return tmp;
            }
        }
        public int Error
        {
            get
            {
                var result = APS_get_error_position(AxisID, out var tmp);
                if (result != 0) throw new APSException(result);
                return tmp;
            }
        }
        public void UpdateFeedback(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Feedback)));
        }
        public void Update(object sender = null, EventArgs e = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Feedback)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Command)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
        }

        /// <summary>
        /// Set current feedback position to spcific value. 
        /// (Notice : this will not move the motor
        /// </summary>
        /// <param name="pos"></param>
        /// <exception cref="APSException"></exception>
        public void SetPosition(int pos)
        {
            var result = APS_set_position(AxisID, pos);
            if (result != 0) throw new APSException(result);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public enum AxisDirection
    {
        Plus = 0,
        Minus = 1,
    }

    public class ParameterIDAttribute : Attribute
    {
        public ParameterIDAttribute(int id) => ID = id;
        public ParameterIDAttribute(AxisParameter id) => ID = Convert.ToInt32(id);
        public ParameterIDAttribute(APS_Define id) => ID = Convert.ToInt32(id);
        public int ID { get; }
    }

    public partial class ADLINK_Motion : MotionController
    {
        public AxesParameterCollection Axes { get; } = new AxesParameterCollection();
        void InitializeAxes()
        {
            for (int i = 0; i < CurrentAxisCount; i++)
                Axes.Add(new AxisMotionController(this, this.BoardStartAxisID + i));
            try
            {
                Axes.Load();
            }
            catch (FileNotFoundException ex)
            {
                //resolve old version
                string path = @"D:\mcpInfo";
                if (File.Exists(path))
                {
                    DeepWise.MessageBox.Show("尚未實作Axis資料復原");
                    var jObj = JObject.Parse(File.ReadAllText(path));
                    var ary = jObj["AxisSettings"] as JArray;
                    if(ary.Count != CurrentAxisCount)
                    {
                        DeepWise.MessageBox.Show($"{path} 中的軸數與當前不同", "衝突解決失敗");
                        return;
                    }
                    foreach (JObject axis in jObj["AxisSettings"])
                    {
                        var id = axis["AxisID"].ToObject<int>();
                        Axes[id].Name = axis["AxisName"].ToString();
                        Axes[id].DistanceRelative = axis["DistanceRelative"].ToObject<int>();
                        Axes[id].DistanceRelative = axis["DistanceRelative"].ToObject<int>();
                        Axes[id].VM = axis["Speed"].ToObject<double>();
                        Axes[id].HomeVM = axis["SpeedHome"].ToObject<double>();
                        Axes[id].JogVM = axis["SpeedJog"].ToObject<double>();
                        Axes[id].ACC = axis["Acceleration"].ToObject<double>();
                        Axes[id].HomeACC = axis["AccelerationHome"].ToObject<double>();
                        Axes[id].JogACC = axis["AccelerationJog"].ToObject<double>();
                        foreach(var p in axis["ABSLocations"].ToObject<KeyedLocations>())
                            Axes[id].PointTable.Add(p);
                    }

                    if(DeepWise.MessageBox.Show($"AxisSetting參數已移植,按下[確認]後儲存", "提示", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question)== System.Windows.MessageBoxResult.Yes)
                    {
                        Axes.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                DeepWise.MessageBox.Show(ex);
            }
        }
        
        [Obsolete("Use \"ADLINK_Motion[].WaitArrived(...)\" instead")]
        public async Task WaitArrived(int axisID, int location, CancellationToken token, int error = 50)
        {
            while (!token.IsCancellationRequested)
            {
                var diff = GetPosition(axisID) - location;
                if (diff < 0) diff = -diff;
                if (diff <= error) break;
                await Task.Delay(50);
            }
        }
        [Obsolete("Use \"ADLINK_Motion[].WaitArrived(...)\" instead")]
        public async Task WaitArrived(int axisID, string location, CancellationToken token, int error = 50)
        {
            int _location = this[axisID].PointTable[location];
            while (!token.IsCancellationRequested)
            {
                var diff = GetPosition(axisID) - _location;
                if (diff < 0) diff = -diff;
                if (diff <= error) break;
                await Task.Delay(50);
            }
        }
        [Obsolete("Use \"ADLINK_Motion[].WaitArrived(...)\" instead")]
        public async Task WaitArrived(int axisID, string location, int timeout, int error = 50)
        {
            int _location = this[axisID].PointTable[location];
            while (timeout > 0)
            {
                var diff = GetPosition(axisID) - _location;
                if (diff < 0) diff = -diff;
                if (diff <= error) break;
                await Task.Delay(50);
                timeout -= 50;
            }
            if (timeout < 0) throw new TimeoutException();
        }

        #region (Obsolete)Single Axis Motion
        [Obsolete("Use ADLINK_Motion[axis].Position.Command instead")]
        public int GetCommandPosition(int axisID)
        {
            int result = APS_get_command(axisID, out var tmp);
            if (result != 0) throw new APSException(result);
            return tmp;
        }
        [Obsolete("Use ADLINK_Motion[axis].Position instead")]
        public int GetPosition(int axisID)
        {
            int result = APS_get_position(axisID, out var tmp);
            if (result != 0) throw new APSException(result);
            return tmp;
        }
        [Obsolete("Use ADLINK_Motion[axis].Position.Error instead")]
        public int GetErrorPosition(int axisID)
        {
            int result = APS_get_error_position(axisID, out var tmp);
            if (result != 0) throw new APSException(result);
            return tmp;
        }

        /// <summary>
        /// Dirdirection => (0=pos, 1=neg)
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        [Obsolete("Use ADLINK_Motion[axis].JogStart instead")]
        public int StartJogMotion(int axisID, int direction)
        {
            int _L_errorCode = 0;
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, axisID);
            if (APS_set_axis_param(L_axisID, 65, direction) == 0)
            {
                _L_errorCode = APS_jog_start(L_axisID, 1);
                if (_L_errorCode != 0)
                {
                    this.StopMove(axisID);
                }
            }
            return _L_errorCode;
        }

        [Obsolete("Use ADLINK_Motion[axis].JogStop instead")]
        public int StopJogMotion(int P_boardAxisID)
        {
            int _L_errorCode = 0;
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, P_boardAxisID);
            if (APS_jog_start(L_axisID, 0) == 0)
            {

            }
            else
            {
                this.StopMove(P_boardAxisID);
            }
            return _L_errorCode;
        }

        /// <summary>
        /// 该函数用于启动单轴相对运动。虽然在函数参数中设置了最大速度，但是，由于用户设置为达到最大速度，因此运动距离和加速率可能会不足。速度曲线的加减速率和曲线通过轴参数函数进行设置。
        ///此函数为“发后即忘”的方式。这就意味着在轴运动期间不会挂起用户的程序或过程。用户必
        ///须使用运动状态检查功能或中断事件等待函数来等待它完成。
        ///对于 PCI-8253/56, PCI-8392(H), PCI-8254/58 / AMP-204/8C，用户可以在轴运动期间启动
        ///一个新的运动命令，包括停止命令，以覆盖前一个命令。轴将根据目标位置和新的速度等新设
        ///置，立即切换到新命令。
        ///其他运动模式（如点动，归零，手动脉冲生成，轮廓运动）不能替代此命令。用户必须停止轴
        ///运动才能切换到上述那些模式。
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="distance"></param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        [Obsolete("Use ADLINK_Motion[axis].MoveRelative instead")]
        public void MoveRelative(int axisID, int distance, int maxSpeed = -1)
        {
            int L_axisID = -1;
            int result = this.BoardAxisID_To_AxisID(ref L_axisID, axisID);
            //setting
            if (maxSpeed == -1)
            {
                double tmp = 0;
                APS_get_axis_param_f(L_axisID, (int)APS_Define.PRA_VM, ref tmp);
                maxSpeed = (int)tmp;
            }

            //Run
            if (result >= 0)
            {
                result = APS_relative_move(L_axisID, distance, maxSpeed);
                if (result != 0)
                    throw new APSException(result);
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="P_distance"></param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        [Obsolete("Use ADLINK_Motion[axis].MoveAbsolute instead")]
        public void MoveToAbsolutePosition(int axisID, int position, int maxSpeed = -1)
        {
            int _L_errorCode;
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, axisID);

            if (maxSpeed == -1)
            {
                double tmp = 0;
                this.Get_axis_param(axisID, (int)APS_Define.PRA_VM, ref tmp);
                maxSpeed = (int)tmp;
            }

            if (L_result >= 0)
            {
                _L_errorCode = APS_absolute_move(L_axisID, position, maxSpeed);
                if (_L_errorCode < 0) throw new APSException(_L_errorCode);
            }
            else
                throw new Exception("Method Fail BoardAxisID_To_AxisID");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="P_distance"></param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        [Obsolete("Use ADLINK_Motion[axis].MoveAbsolute instead")]
        public void MoveToAbsolutePosition(int axisID, string position, int maxSpeed = -1) => MoveToAbsolutePosition(axisID, this[axisID].PointTable[position], maxSpeed);
        void MoveToHomeNative(int axisID)
        {
            int L_axisID = -1;
            int result = this.BoardAxisID_To_AxisID(ref L_axisID, axisID);

            if (result >= 0)
            {
                result = APS_home_move(L_axisID);
                if (result != 0)
                    throw new APSException(result);
            }
            else
            {
                throw new APSException(result);
            }
        }

        /// <summary>
        /// 該函數用於啟動單軸回Home運動，並且等候直到該軸"<see cref=" MotionStatus.HMV"/>"訊號停止為止。
        /// </summary>
        /// <param name="axisID"></param>
        /// <returns></returns>
        [Obsolete("Use ADLINK_Motion[axis].MoveHome instead")]
        public async Task MoveToHome(int axisID)
        {
            MoveToHomeNative(axisID);
            await Task.Delay(500);
            await WaitMotionStatus(axisID, MotionStatus.HMV, false);
            return;
        }

        /// <summary>
        /// 該函數用於啟動單軸回Home運動，並且等候直到該軸"<see cref=" MotionStatus.HMV"/>"訊號停止為止。
        /// </summary>
        /// <param name="axisID"></param>
        /// <returns>return false while cancellation is requested.</returns>
        [Obsolete("Use ADLINK_Motion[axis].MoveHome instead")]
        public async Task<bool> MoveToHome(int axisID, CancellationToken token)
        {
            MoveToHomeNative(axisID);
            await Task.Delay(500, token);
            await WaitMotionStatus(axisID, MotionStatus.HMV, false, token);
            return !token.IsCancellationRequested;
        }
        #endregion

        //=============================================================================================================================

        public bool IsAt(int axisID, int position)
        {
            int result = APS168.APS_get_position(axisID, out var tmp);
            if (result != 0) throw new APSException(result);
            var diff = position - tmp;
            if (diff < 0) diff = -diff;
            return diff <= ErrorPos;
        }
        public bool IsAt(int[] axisID, DeepWise.Shapes.Point position)
        {
            int result = APS168.APS_get_position(axisID[0], out var tmp);
            if (result != 0) throw new APSException(result);
            var diff = position.X - tmp;
            if (diff < 0) diff = -diff;
            if (diff > ErrorPos) return false;

            result = APS168.APS_get_position(axisID[1], out tmp);
            if (result != 0) throw new APSException(result);
            diff = position.Y - tmp;
            if (diff < 0) diff = -diff;
            return diff <= ErrorPos;
        }
        public bool IsAt(int[] axisID, DeepWise.Shapes.Point3D position)
        {
            int result = APS168.APS_get_position(axisID[0], out var tmp);
            if (result != 0) throw new APSException(result);
            var diff = position.X - tmp;
            if (diff < 0) diff = -diff;
            if (diff > ErrorPos) return false;

            result = APS168.APS_get_position(axisID[1], out tmp);
            if (result != 0) throw new APSException(result);
            diff = position.Y - tmp;
            if (diff < 0) diff = -diff;
            if (diff > ErrorPos) return false;

            result = APS168.APS_get_position(axisID[2], out tmp);
            if (result != 0) throw new APSException(result);
            diff = position.Z - tmp;
            if (diff < 0) diff = -diff;
            return diff <= ErrorPos;
        }

        public DeepWise.Shapes.Point GetPosition(string axis1, string axis2)
        {
            return new DeepWise.Shapes.Point(this[axis1].Position, this[axis2].Position);
        }
        public DeepWise.Shapes.Point GetPosition(int axis1, int axis2)
        {
            return new DeepWise.Shapes.Point(this[axis1].Position, this[axis2].Position);
        }

        public DeepWise.Shapes.Point3D GetPosition(string axis1, string axis2,string axis3)
        {
            return new DeepWise.Shapes.Point3D(this[axis1].Position, this[axis2].Position, this[axis3].Position);
        }
        public DeepWise.Shapes.Point3D GetPosition(int axis1, int axis2, int axis3)
        {
            return new DeepWise.Shapes.Point3D(this[axis1].Position, this[axis2].Position, this[axis3].Position);
        }

        #region Muti Axes Motion
        public void MoveLineRelative(int[] axes, DeepWise.Shapes.Vector v)
        {
            var result = APS_relative_linear_move(2, axes, new int[] { (int)v.X, (int)v.Y}, (int)this[axes[0]].VM);
            if (result != 0) throw new APSException(result);
        }
        public void MoveLineRelative(string axis1, string axis2, DeepWise.Shapes.Vector v) => MoveLineRelative(new int[] { this[axis1].AxisID, this[axis2].AxisID }, v);
        public void MoveLineAbsolute(int[] axes, DeepWise.Shapes.Point position)
        {
            var result = APS_absolute_linear_move(2, axes, position.Select(x => (int)x).ToArray(), (int)this[axes[0]].VM);
            if (result != 0) throw new APSException(result);
        }
        public void MoveLineAbsolute(string axis1, string axis2, DeepWise.Shapes.Point p) => MoveLineAbsolute(new int[] { this[axis1].AxisID, this[axis2].AxisID }, p);
        public void MoveLineAbsolute(int[] axes, DeepWise.Shapes.Point3D p)
        {
            var result = APS_absolute_linear_move(2,axes, p.Select(x => (int)x).ToArray(), (int)this[axes[0]].VM);
            if (result != 0) throw new APSException(result);
        }
        public void MoveLineAbsolute(string axis1, string axis2, string axis3, DeepWise.Shapes.Point3D p) => MoveLineAbsolute(new int[] { this[axis1].AxisID, this[axis2].AxisID, this[axis3].AxisID }, p);


        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axes"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="APSException"></exception>
        public async Task MovePathAbsolute(int[] axes, IEnumerable<DeepWise.Shapes.Point> path, CancellationToken token)
        {
            int option = 0b0011_0000_0000_0000;//BufferMode
            double tran = 0;
            ADLINK_DEVICE.ASYNCALL sYNCALL = new ADLINK_DEVICE.ASYNCALL();
            foreach (var p in path)
            {
                while (true)
                {
                    if (token.IsCancellationRequested) goto Abort;
                    if (TrySend(APS_line(2, axes, option, p.ToArray(), ref tran, ref sYNCALL))) break;
                    await Task.Delay(100);
                }
            }
            await Monitor.Wait();
            await WaitMotionStatus(axes[0], MotionStatus.MDN, true, token);
            if (token.IsCancellationRequested) goto Abort;
            return;
        Abort:
            this[0].StopMove();
            bool TrySend(int result)
            {
                Debug.WriteLine(result);
                if (result == 0) return true;
                else if (result == -2011) return false;
                else throw new APSException(result);
            }
        }
        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axis1"></param>
        /// <param name="axis2"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task MovePathAbsolute(string axis1, string axis2, IEnumerable<DeepWise.Shapes.Point> path, CancellationToken token) 
            => MovePathAbsolute(new int[] { this[axis1].AxisID, this[axis2].AxisID }, path, token);
        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axes"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="APSException"></exception>
        public async Task MovePathAbsolute(int[] axes, IEnumerable<DeepWise.Shapes.Point3D> path, CancellationToken token)
        {
            int option = 0b0011_0000_0000_0000;//BufferMode
            double tran = 0;
            ADLINK_DEVICE.ASYNCALL sYNCALL = new ADLINK_DEVICE.ASYNCALL();
            foreach (var p in path)
            {
                while (true)
                {
                    if (token.IsCancellationRequested) goto Abort;
                    if (TrySend(APS_line(3, axes, option, p.ToArray(), ref tran, ref sYNCALL))) break;
                    await Task.Delay(100);
                }
            }
            await Monitor.Wait();
            await WaitMotionStatus(axes[0], MotionStatus.MDN, true, token);
            if (token.IsCancellationRequested) goto Abort;
            return;
        Abort:
            this[0].StopMove();
            bool TrySend(int result)
            {
                Debug.WriteLine(result);
                if (result == 0) return true;
                else if (result == -2011) return false;
                else throw new APSException(result);
            }
        }
        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axis1"></param>
        /// <param name="axis2"></param>
        /// <param name="axis3"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task MovePathAbsolute(string axis1, string axis2, string axis3, IEnumerable<DeepWise.Shapes.Point3D> path, CancellationToken token) 
            => MovePathAbsolute(new int[] { this[axis1].AxisID, this[axis2].AxisID, this[axis3].AxisID }, path, token);
        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axes"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <param name="isClose"></param>
        /// <returns></returns>
        /// <exception cref="APSException"></exception>
        public async Task MovePolyAbsolute(int[] axes, IEnumerable<DeepWise.Shapes.Vertex> path, CancellationToken token, bool isClose = false)
        {
            int option = 0b11000000000000;//BufferMode
            double tran = 0;
            var sYNCALL = new ADLINK_DEVICE.ASYNCALL();
            var str = path.First();
            APS_line(axes.Length, axes, option, new double[] { str.X, str.Y }, ref tran, ref sYNCALL);
            foreach (var end in path.Skip(1))
            {
                if (str.Bulge == 0)
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_line(axes.Length, axes, option, new double[] { end.X, end.Y }, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
                else
                {
                    var center = DeepWise.Shapes.Geometry.GetCenterBulge(str.Location, end.Location, str.Bulge);
                    double sweep = Math.Atan(str.Bulge) * 4;
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_arc2_ca(axes, option, new double[] { center.X, center.Y }, sweep, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
                str = end;
            }

            if (isClose)
            {
                str = path.Last();
                var end = path.First();
                if (str.Bulge == 0)
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_line(axes.Length, axes, option, new double[] { end.X, end.Y }, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
                else
                {
                    var center = DeepWise.Shapes.Geometry.GetCenterBulge(str.Location, end.Location, str.Bulge);
                    double sweep = Math.Atan(str.Bulge) * 4;
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_arc2_ca(axes, option, new double[] { center.X, center.Y }, sweep, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
            }

            await Monitor.Wait();
            await WaitMotionStatus(axes[0], MotionStatus.MDN, true, token);
            if (token.IsCancellationRequested) goto Abort;
            return;
        Abort:
            this[axes[0]].StopMove();
            //================================================
            bool TrySend(int result)
            {
                if (result == 0) return true;
                else if (result == -2011) return false;
                else throw new APSException(result);
            }
        }

        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axes"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <param name="isClose"></param>
        /// <returns></returns>
        /// <exception cref="APSException"></exception>
        public async Task MovePolyAbsolute(int[] axes, IEnumerable<DeepWise.Shapes.Vertex> path, IEnumerable<double> z, CancellationToken token, bool isClose = false)
        {
            int option = 0b11000000000000;//BufferMode
            double tran = 0;
            var sYNCALL = new ADLINK_DEVICE.ASYNCALL();
            var str = path.First();
            APS_line(axes.Length, axes, option, new double[] { str.X, str.Y }, ref tran, ref sYNCALL);
            foreach (var end in path.Skip(1))
            {
                if (str.Bulge == 0)
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_line(axes.Length, axes, option, new double[] { end.X, end.Y }, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
                else
                {
                    var center = DeepWise.Shapes.Geometry.GetCenterBulge(str.Location, end.Location, str.Bulge);
                    double sweep = Math.Atan(str.Bulge) * 4;
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_arc2_ca(axes, option, new double[] { center.X, center.Y }, sweep, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
                str = end;
            }

            if (isClose)
            {
                str = path.Last();
                var end = path.First();
                if (str.Bulge == 0)
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_line(axes.Length, axes, option, new double[] { end.X, end.Y }, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
                else
                {
                    var center = DeepWise.Shapes.Geometry.GetCenterBulge(str.Location, end.Location, str.Bulge);
                    double sweep = Math.Atan(str.Bulge) * 4;
                    while (true)
                    {
                        if (token.IsCancellationRequested) goto Abort;
                        if (TrySend(APS_arc2_ca(axes, option, new double[] { center.X, center.Y }, sweep, ref tran, ref sYNCALL))) break;
                        await Task.Delay(100);
                    }
                }
            }

            await Monitor.Wait();
            await WaitMotionStatus(axes[0], MotionStatus.MDN, true, token);
            if (token.IsCancellationRequested) goto Abort;
            return;
        Abort:
            this[axes[0]].StopMove();
            //================================================
            bool TrySend(int result)
            {
                if (result == 0) return true;
                else if (result == -2011) return false;
                else throw new APSException(result);
            }
        }

        /// <summary>
        /// 注意！ : ADLINK_Motion.Stop() 無法正確的停止該運動，請確保使用CancellationToken來終止此運動。
        /// </summary>
        /// <param name="axis1"></param>
        /// <param name="axis2"></param>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <param name="isClose"></param>
        /// <returns></returns>
        public Task MovePolyAbsolute(string axis1, string axis2, IEnumerable<DeepWise.Shapes.Vertex> path, CancellationToken token, bool isClose = false) 
            => MovePolyAbsolute(new int[] { this[axis1].AxisID, this[axis2].AxisID }, path, token, isClose);
        #endregion

        public async Task WaitArrived(int[] axisID, DeepWise.Shapes.Point location, CancellationToken token, int error = 50)
        {
            var p = location.ToArray();
            for (int i = 0; i < p.Length; i++)
            {
                while (!token.IsCancellationRequested)
                {
                    var diff = GetPosition(axisID[i]) - p[i];
                    if (diff < 0) diff = -diff;
                    if (diff <= error) break;
                    await Task.Delay(50);
                }
            }
        }
        public async Task WaitArrived(int[] axisID, DeepWise.Shapes.Point location, int timeout, int error = 50)
        {
            var p = location.ToArray();
            for (int i = 0; i < p.Length; i++)
            {
                while (true)
                {
                    var diff = this[axisID[i]].Position - p[i];
                    if (diff < 0) diff = -diff;
                    if (diff <= error) break;
                    await Task.Delay(50);
                    timeout -= 50;
                    if (timeout < 0) throw new TimeoutException();
                }
            }
        }
        public async Task WaitArrived(int[] axisID, DeepWise.Shapes.Point3D location, CancellationToken token, int error = 50)
        {
            var p = location.ToArray();
            for (int i = 0; i < p.Length; i++)
            {
                while (!token.IsCancellationRequested)
                {
                    var diff = GetPosition(axisID[i]) - p[i];
                    if (diff < 0) diff = -diff;
                    if (diff <= error) break;
                    await Task.Delay(50);
                }
            }
        }
        public async Task WaitArrived(int[] axisID, DeepWise.Shapes.Point3D location, int timeout, int error = 50)
        {
            var p = location.ToArray();
            for (int i = 0; i < p.Length; i++)
            {
                while (true)
                {
                    var diff = this[axisID[i]].Position - p[i];
                    if (diff < 0) diff = -diff;
                    if (diff <= error) break;
                    await Task.Delay(50);
                    timeout -= 50;
                    if (timeout < 0) throw new TimeoutException();
                }
            }
        }
        public static readonly double ErrorPos = AxisMotionController.ErrorPos;

    }
}