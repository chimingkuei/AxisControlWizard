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
using DeepWise.Threading;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using DeepWise.Json;

namespace MotionControllers.Motion
{
    public class MotionCommander
    {
        public List<MotionCommand> Commands { get; } = new List<MotionCommand>();

        public async Task<bool> Execute(ADLINK_Motion cntlr,SuspendableCancellationTokenSource handle)
        {
            foreach(var item in Commands)
            {
                try
                {
                    await item.Execute(cntlr, handle);
                }
                catch (Exception ex)
                {
                    return false;
                }
                if (await handle?.WaitWhileSuspended()) return false;
            }
            return true;
        }
    }

    [JsonConverter(typeof(BaseTypeConverter<MotionCommand>))]
    public abstract class MotionCommand 
    {
        public abstract Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle);
    }

    [DisplayName("軸移動")]
    public class MoveCommand : MotionCommand
    {
       
        public int AxisID { get; set; }
        public MotionType MotionType { get; set; } = MotionType.MoveRelative;
        public string Location { get; set; }
        public int Position { get; set; }
        public int Distance { get; set; }

        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            switch(MotionType)
            {
                case MotionType.MoveRelative:
                    cntlr.MoveRelative(AxisID, Distance);
                    break;
                case MotionType.MoveAbsolute:
                    cntlr.MoveToAbsolutePosition(AxisID, Position);
                    break;
                case MotionType.MoveAbsolute_MarkedPlace:
                    cntlr.MoveToAbsolutePosition(AxisID, Location);
                    break;
            }
        }
    }

    [DisplayName("設置數位訊號輸出(Digital Output)")]
    public class SetDigitalOutputSignalCommand : MotionCommand
    {
        public IOPortInfo PortInfo { get; set; }
        public bool Value { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            cntlr.SetOutputValue(PortInfo, Value);
        }
    }

    [DisplayName("設置類比訊號輸出(Analog Output)")]
    public class SetAnalogOutputSignalCommand : MotionCommand
    {
        public IOPortInfo PortInfo { get; set; }
        public double Value { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            cntlr.SetOutputValue(PortInfo, Value);
        }
    }

    [DisplayName("等待數位訊號(Digital I/O)")]
    public class WaitDigitalSignalCommand : MotionCommand
    {
        public IOPortInfo PortInfo { get; set; }
        public bool TargetValue { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            await cntlr.WaitDIO(PortInfo, TargetValue,handle.Token);
        }
    }

    [DisplayName("等待類比訊號(Analog I/O)")]
    public class WaitAnalogSignalCommand : MotionCommand
    {
        public IOPortInfo PortInfo { get; set; }
        public DeepWise.NumericalComparison<double> Comparison { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            await cntlr.WaitAIO(PortInfo, Comparison.Predicate, handle.Token);
        }
    }

    [DisplayName("等候時間")]
    public class DelayCommand : MotionCommand
    {
        public int MillisecondsDelay { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            await Task.Delay(MillisecondsDelay);
        }
    }

    [DisplayName("等候軸移動狀態")]
    public class WaitMotionStatus : MotionCommand
    {
        public MotionStatus Target { get; set; }
        public bool TargetValue { get; set; }
        public int AxisID { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            await cntlr.WaitMotionStatus(AxisID,Target,TargetValue);
        }
    }

    [DisplayName("等候軸移動I/O狀態")]
    public class WaitMotionIOStatus : MotionCommand
    {
        public MotionIOStatus Target { get; set; }
        public bool TargetValue { get; set; }
        public int AxisID { get; set; }
        public override async Task Execute(ADLINK_Motion cntlr, SuspendableCancellationTokenSource handle)
        {
            await cntlr.WaitMotionIOStatus(AxisID, Target, TargetValue);
        }
    }

    public enum MotionType
    {
        MoveRelative,
        MoveAbsolute,
        MoveAbsolute_MarkedPlace,
    }

}