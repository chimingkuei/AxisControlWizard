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
using ADLINK_DEVICE;

namespace MotionControllers.Motion
{
    public partial class ADLINK_Motion : MotionController
    {
        #region Motion Status and MotionIO Status

        public event EventHandler<MotionIOStatusEventArgs> MotionIOStatusChanged;
        public event EventHandler<MotionStatusEventArgs> MotionStatusChanged;

        int[] motionIOStatus;
        int[] motionStatus;

        /// <summary>
        /// 等候MotionIO訊號。
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="target"></param>
        /// <param name="targetValue"></param>
        public async Task WaitMotionIOStatus(int axisID, MotionIOStatus target, bool targetValue)
        {
          
            using (var resetEvent = new ManualResetEvent(false))
            {
                MotionIOStatusChanged += Trigger;
                await Task.Delay(10);
                if (GetMotionIOStatus(axisID, target) == targetValue)
                {
                    MotionIOStatusChanged -= Trigger;
                    return;
                }
                await Task.Run(resetEvent.WaitOne);
                void Trigger(object sender, MotionIOStatusEventArgs e)
                {
                    if (e.AxisID == axisID && e.Status == target && e.Value == targetValue)
                    {
                        resetEvent.Set();
                        MotionIOStatusChanged -= Trigger;
                    }
                }
            }
        }

        [Obsolete("Use 'this.Axes or this[axisID] instead")]
        public AxisCollectionWrapper GetSetting() => new AxisCollectionWrapper(this);

        public class AxisCollectionWrapper
        {
            public AxisCollectionWrapper(ADLINK_Motion c)
            {
                AxisSettings = c.Axes;
            }
            public AxesParameterCollection AxisSettings { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="target"></param>
        /// <param name="targetValue"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task WaitMotionIOStatus(int axisID, MotionIOStatus target, bool targetValue, int timeout)
        {
          
            using (var resetEvent = new ManualResetEvent(false))
            {
                MotionIOStatusChanged += Trigger;
                await Task.Delay(10);
                if (GetMotionIOStatus(axisID, target) == targetValue)
                {
                    MotionIOStatusChanged -= Trigger;
                    return;
                }
                var result = await Task.Run(() => resetEvent.WaitOne(timeout));
                MotionIOStatusChanged -= Trigger;
                if (!result) throw new TimeoutException($"第{0}軸等候{nameof(MotionIOStatus)}.{target}訊號等於{targetValue}逾時。");
                //==========================================================
                void Trigger(object sender, MotionIOStatusEventArgs e)
                {
                    if (e.AxisID == axisID && e.Status == target && e.Value == targetValue)
                    {
                        resetEvent.Set();
                        MotionIOStatusChanged -= Trigger;
                    }
                }
            }
        }

 
        public async Task WaitMotionIOStatus(int axisID, MotionIOStatus target, bool targetValue, CancellationToken token)
        {
            
            using (var resetEvent = new ManualResetEvent(false))
            {
                MotionIOStatusChanged += Trigger;
                await Task.Delay(10);
                if (GetMotionIOStatus(axisID, target) == targetValue)
                {
                    MotionIOStatusChanged -= Trigger;
                    return;
                }
                await Task.Run(() =>
                {
                    WaitHandle.WaitAny(new[] { resetEvent, token.WaitHandle });
                });
                MotionIOStatusChanged -= Trigger;
                return;
                void Trigger(object sender, MotionIOStatusEventArgs e)
                {
                    if (e.AxisID == axisID && e.Status == target && e.Value == targetValue)
                    {
                        resetEvent.Set();
                        MotionIOStatusChanged -= Trigger;
                    }
                }
            }
        }


        /// <summary>
        /// 等候Motion訊號。
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="target"></param>
        /// <param name="targetValue"></param>
        /// <param name="timeout">單位:毫秒。(-1代表沒有逾時)</param>
        /// <returns>成功回傳true，逾時則回傳false。</returns>
        public async Task WaitMotionStatus(int axisID, MotionStatus target, bool targetValue)
        {
            using (var resetEvent = new ManualResetEvent(false))
            {

                MotionStatusChanged += Trigger;
                await Task.Delay(10);
                if (GetMotionStatus(axisID, target) == targetValue)
                {
                    MotionStatusChanged -= Trigger;
                    return;
                }
                await Task.Run(resetEvent.WaitOne);

                void Trigger(object sender, MotionStatusEventArgs e)
                {
                    if (e.AxisID == axisID && e.Status == target && e.Value == targetValue)
                    {
                        resetEvent.Set();
                        MotionStatusChanged -= Trigger;
                    }
                }
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="axisID"></param>
       /// <param name="target"></param>
       /// <param name="targetValue"></param>
       /// <param name="timeout"></param>
       /// <returns></returns>
       /// <exception cref="TimeoutException"></exception>
        public async Task WaitMotionStatus(int axisID, MotionStatus target, bool targetValue, int timeout)
        {
            
            using (var resetEvent = new ManualResetEvent(false))
            {
                MotionStatusChanged += Trigger;
                await Task.Delay(10);
                if (GetMotionStatus(axisID, target) == targetValue)
                {
                    MotionStatusChanged -= Trigger;
                    return;
                }
                var result = await Task.Run(() => resetEvent.WaitOne(timeout));
                MotionStatusChanged -= Trigger;
                if (!result) throw new TimeoutException($"第{0}軸等候{nameof(MotionStatus)}.{target}訊號等於{targetValue}逾時。");
                //==========================================================
                void Trigger(object sender, MotionStatusEventArgs e)
                {
                    if (e.AxisID == axisID && e.Status == target && e.Value == targetValue)
                    {
                        resetEvent.Set();
                        MotionStatusChanged -= Trigger;
                    }
                }
            }
        }

        public async Task WaitMotionStatus(int axisID, MotionStatus target, bool targetValue, CancellationToken token)
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                MotionStatusChanged += Trigger;
                await Task.Delay(10);
                if (GetMotionStatus(axisID, target) == targetValue)
                {
                    MotionStatusChanged -= Trigger;
                    return;
                }
                await Task.Run(() =>
                {
                    WaitHandle.WaitAny(new[] { resetEvent, token.WaitHandle });
                });
                MotionStatusChanged -= Trigger;
                return;
                void Trigger(object sender, MotionStatusEventArgs e)
                {
                    if (e.AxisID == axisID && e.Status == target && e.Value == targetValue)
                    {
                        resetEvent.Set();
                        MotionStatusChanged -= Trigger;
                    }
                }
            }
        }

        [Obsolete]
        public void WaitMotionStatusNative(int axisID, MotionStatus target, bool targetValue, int sleepInternal = 10)
        {
            bool tmp = !targetValue;
            while (tmp != targetValue)
            {
                tmp = ((APS_motion_status(axisID) >> ((int)target)) & 1) == 1;
                Thread.Sleep(sleepInternal);
            }
        }

        public bool GetMotionIOStatus(int axisID, MotionIOStatus status) => ((motionIOStatus[axisID] >> (int)status) & 1) != 0;

        public bool GetMotionStatus(int axisID, MotionStatus status) => ((motionStatus[axisID] >> (int)status) & 1) != 0;

        internal void UpdateMotioStatus(object sender = null, EventArgs e = null)
        {
            //Motion I/O Status
            for (int id = 0; id < motionIOStatus.Length; id++)
            {
                var tmp = APS_motion_io_status(id);
                if (motionIOStatus[id] != tmp)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        var newValue = (((tmp >> i) & 1) != 0);
                        var old = (((motionIOStatus[id] >> i) & 1) != 0);
                        if (newValue != old)
                        {
                            try
                            {
                                MotionIOStatusChanged?.Invoke(this, new MotionIOStatusEventArgs(id, (MotionIOStatus)i, newValue));
                            }
                            catch (Exception ex)
                            { 
                            }
                        }
                    }
                    motionIOStatus[id] = tmp;
                }
            }

            //Motion Status
            for (int id = 0; id < motionStatus.Length; id++)
            {
                var tmp = APS_motion_status(id);
                if (motionStatus[id] != tmp)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        var newValue = (((tmp >> i) & 1) != 0);
                        var old = (((motionStatus[id] >> i) & 1) != 0);
                        if (newValue != old)
                            MotionStatusChanged?.Invoke(this, new MotionStatusEventArgs(id, (MotionStatus)i, newValue));
                    }
                    motionStatus[id] = tmp;
                }
            }
        }


        [Obsolete("Use Save() instead")]
        public void SaveSettingToDefault()
        {
            Axes.Save();
            IOTable.Save();
        }
        [Obsolete("Use Load() instead")]
        public void LoadSettingFromDefault()
        {
            Axes.Load();
            IOTable.Load();
        }

        public void Load()
        {
            Axes.Load();
            IOTable.Load();
        }
        public void Save()
        {
            Axes.Save();
            IOTable.Save();
        }

        #endregion
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MotionStatus//目前針對204C
    {
        [Description("命令停止(但可能正在運行)")]
        CSTP = 0,
        [Description("處於最大速")]
        VM = 1,
        [Description("加速中")]
        ACC = 2,
        [Description("減速中")]
        DEC = 3,
        [Description("移動方向")]
        DIR = 4,
        [Description("動作完成")]
        MDN = 5,
        [Description("歸零中")]
        HMV = 6,
        [Description("等待狀態")]
        WAIT = 10,
        [Description("軸在點緩衝區中移動")]
        PTB = 11,
        [Description("處於點動中")]
        JOG = 15,
        [Description("異常停止")]
        ASTP = 16,
        [Description("混合運動中的軸")]
        BLD = 17,
        [Description("前距離事件")]
        PRED = 18,
        [Description("後距離事件")]
        POSTD = 19,
        [Description("在齒輪傳動中")]
        GER = 28,
        [Description("脈衝發生器啟用"), Model(DeviceName.PCIE_8332, DeviceName.PCIE_8334, DeviceName.PCIE_8338)]
        PSR = 29,
        [Description("空閒"), Model(DeviceName.AMP_20408C)]
        BACKLASH = 30,
        [Description("启用龙门模式时，该轴为主轴，其运动状态位 30（GRY）将打开。"), Model(DeviceName.PCIE_8332, DeviceName.PCIE_8334, DeviceName.PCIE_8338)]
        GRY = 30,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MotionIOStatus
    {
        [Description("伺服警報")]
        ALM = 0,
        [Description("正限位")]
        PEL = 1,
        [Description("負限位")]
        MEL = 2,
        [Description("原始位置傳感器")]
        ORG = 3,
        [Description("EMG傳感器")]
        EMG = 4,
        [Description("EZ通過")]
        EZ = 5,
        [Description("就位")]
        INP = 6,
        [Description("伺服器動")]
        SVON = 7,
        [Description("準備就緒"), Model(DeviceName.PCIE_8332, DeviceName.PCIE_8334, DeviceName.PCIE_8338)]
        RDY = 8,
        [Description("軟體循環限位")]
        SCL = 10,
        [Description("軟體正限位")]
        SPEL = 11,
        [Description("軟體負限位")]
        SMEL = 12,
        [Description("所有從站處於OP模式"), Model(DeviceName.PCIE_8332, DeviceName.PCIE_8334, DeviceName.PCIE_8338)]
        OP = 24,
        //10 ZSP 零速，零速输出范围设置，请参考伺服驱动器手册。
        //11 SPEL 软件正限位
        //12 SMEL 软件负限位
        //13 TLC 转矩受转矩极限值限制。 （当转矩控制打开时）
        //14 ABSL 绝对位置丢失。
    }

    public class MotionIOStatusEventArgs : EventArgs
    {
        public MotionIOStatusEventArgs(int axisID, MotionIOStatus status, bool value)
        {
            this.AxisID = axisID;
            this.Status = status;
            this.Value = value;
        }
        public int AxisID { get; }
        public MotionIOStatus Status { get; }
        public bool Value { get; }

        [Obsolete("Use AxisID instead.")]
        public int Channel => AxisID;
    }
    public class MotionStatusEventArgs : EventArgs
    {
        public MotionStatusEventArgs(int axisID, MotionStatus status, bool value)
        {
            this.AxisID = axisID;
            this.Status = status;
            this.Value = value;
        }
        public int AxisID { get; }
        public MotionStatus Status { get; }
        public bool Value { get; }

        [Obsolete("Use AxisID instead.")]
        public int Channel => AxisID;
    }

}