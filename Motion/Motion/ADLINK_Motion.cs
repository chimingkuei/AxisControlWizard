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
using System.ComponentModel.DataAnnotations;

namespace MotionControllers.Motion
{
    /// <summary>
    /// 請先避免使用含有底線的方法。
    /// </summary>
    public partial class ADLINK_Motion : MotionController
    {
        public AxisMotionController this[int axisID] => Axes[axisID];
        public AxisMotionController this[string name] => Axes[name];

        public static bool EnableThrowException { get; set; } = false;
        
        #region Trigger
        public void SetTriggerParameter(TriggerParameter_PCI_and_AMP param, int value)
        {
            int result = APS_set_trigger_param(this.BoardID, (int)param, value);
            if (result != 0)
                throw new APSException(result);
        }

        public int GetTriggerParameter(TriggerParameter_PCI_and_AMP param)
        {
            int value = 0;
            int result = APS_get_trigger_param(this.BoardID, (int)param, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }

        /// <summary>
        /// 该函数用于设置线性比较函数。线性触发操作完成后，总的比较点将为：总的比较点数=重复时间。（以 StartPoint 作为第一个触发点）
        /// </summary>
        /// <param name="lCmpCh">线性比较设置通道。从零开始。对于 PCI-8254/58 / AMP-204/8C，I32 LCmpCh：线性比较集通道。从 0 开始。 范围是 0 到 3。</param>
        /// <param name="startPoint">启动线性触发点。</param>
        /// <param name="repeatTimes">触发重复次数。</param>
        /// <param name="interval"> 触发间隔。
        /// <para>对于 PCI-8253/56，时间间隔：24 位无符号值。</para>
        /// <para>对于 PCI-8254/58 / AMP-204/8C，I32 间隔：触发间隔。（-16777215〜16777215，单位是脉冲）</para>
        /// </param>
        /// <exception cref="APSException"></exception>
        public void SetTriggerLinear(int lCmpCh, int startPoint, int repeatTimes, int interval)
        {
            int result = APS_set_trigger_linear(BoardID, lCmpCh, startPoint, repeatTimes, interval);
            if (result != 0) throw new APSException(result);
        }
        #endregion

        #region Trigger EtherCAT
        //TODO :　add　24. Field bus compare trigger

        public void SetTriggerParameterEtherCAT(int busNo, int modNo, TriggerParameter_MNET_4XMO_C Param_No, int value)
        {
            int result = APS_set_field_bus_trigger_param(BoardID, busNo, modNo, (int)Param_No, value);
            if (result != 0) throw new APSException(result);
        }

        public void SetTriggerParameterEtherCAT(int busNo, int modNo, TriggerParameter_ECAT_4XMO Param_No, int value)
        {
            int result = APS_set_field_bus_trigger_param(BoardID, busNo, modNo, (int)Param_No, value);
            if (result != 0) throw new APSException(result);
        }

        public int GetTriggerParameterEtherCAT(int busNo, int modNo, TriggerParameter_ECAT_4XMO param)
        {
            int value = 0;
            int result = APS_get_field_bus_trigger_param(BoardID, busNo, modNo, (int)param, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }

        public int GetTriggerParameterEtherCAT(int busNo, int modNo, TriggerParameter_MNET_4XMO_C param)
        {
            int value = 0;
            int result = APS_get_field_bus_trigger_param(BoardID, busNo, modNo, (int)param, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        /// <summary>
        /// 此函数用于从指定的表比较器中获取当前的比较值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="trgCh">指定的触发输出计数器通道编号。</param>
        /// <exception cref="APSException"></exception>
        public int GetTriggerCountEtherCAT(int busNo, int modNo, int trgCh)
        {
            int value = 0;
            int result = APS_get_field_bus_trigger_count(BoardID, busNo, modNo, trgCh, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        /// <summary>
        /// 置触发计数。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="trgCh">指定的触发输出计数器通道编号。</param>
        /// <exception cref="APSException"></exception>
        public void ResetTriggerCountEtherCAT(int busNo, int modNo, int trgCh)
        {
            int result = APS_reset_field_bus_trigger_count(BoardID, busNo, modNo, trgCh);
            if (result != 0)
                throw new APSException(result);
        }
        /// <summary>
        /// 此函数用于获取编码器计数值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="encCh">指定编码器通道编号。从零开始。对于 MNET-4XMO-C，TCmpCh 的范围是 0 到 4。（LCmpCh 0〜3 用于通用比较器。LCmpCh 4 用于高速比较器。）</param>
        /// <exception cref="APSException"></exception>
        public int GetTriggerEncoderEtherCAT(int busNo, int modNo, int encCh)
        {
            int value = 0;
            int result = APS_get_field_bus_encoder(BoardID, busNo, modNo, encCh, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        /// <summary>
        /// 此函数用于获取编码器计数值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="encCh">指定编码器通道编号。从零开始。对于 MNET-4XMO-C，TCmpCh 的范围是 0 到 4。（LCmpCh 0〜3 用于通用比较器。LCmpCh 4 用于高速比较器。）</param>
        /// <param name="encCnt">编码器计数</param>
        /// <exception cref="APSException"></exception>
        public void SetTriggerEncoderEtherCAT(int busNo, int modNo, int encCh, int encCnt)
        {
            int result = APS_set_field_bus_encoder(BoardID, busNo, modNo, encCh, encCnt);
            if (result != 0)
                throw new APSException(result);
        }

        public void SetTriggerLinearEtherCAT(int busNo, int modNo, int lCmpCh, int startPoint, int repeatTimes, int interval)
        {
            int result = APS_set_field_bus_trigger_linear(busNo, busNo, modNo, lCmpCh, startPoint, repeatTimes, interval);
            if (result != 0) throw new APSException(result);
        }
        /// <summary>
        /// 此函数用于从指定的表比较器中获取当前的比较值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="tcmpCh">此函数用于从指定的线性比较器中获取当前的比较值。</param>
        /// <exception cref="APSException"></exception>
        public int GetTriggerLinearCompareValueEtherCAT(int busNo, int modNo, int tcmpCh)
        {
            int value = 0;
            int result = APS_get_field_bus_trigger_linear_cmp(BoardID, busNo, modNo, tcmpCh, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        /// <summary>
        /// 该函数用于获取线性比较的剩余计数值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="LCmpCh">指定线性比较器的通道编号。从零开始。对于 MNET-4XMO-C，TCmpCh 的范围是 0 到 4。（LCmpCh 0〜3 用于通用比较器。LCmpCh 4 用于高速比较器。）</param>
        /// <exception cref="APSException"></exception>
        public int GetTriggerLinearCompareRemainCountEtherCAT(int busNo, int modNo, int LCmpCh)
        {
            int value = 0;
            int result = APS_get_field_bus_linear_cmp_remain_count(BoardID, busNo, modNo, LCmpCh, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }

        /// <summary>
        /// 此函数用于配置指定的比较表。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="tcmpCh">对于 MNET-4XMO-C，TCmpCh 的范围是 0 到 3。（TCmpCh 0〜3 用于通用比较器。）</param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        /// <exception cref="APSException"></exception>
        public void SetTriggerTable(int busNo, int modNo, int tcmpCh, int[] data, int dataSize)
        {

            int result = APS_set_field_bus_trigger_table(BoardID, busNo, modNo, tcmpCh, data, dataSize);
            if (result != 0) throw new APSException(result);
        }
        /// <summary>
        /// 此函数用于从指定的表比较器中获取当前的比较值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="tcmpCh">指定表比较器的通道编号。</param>
        /// <exception cref="APSException"></exception>
        public int GetTriggerTableCompareValueEtherCAT(int busNo, int modNo, int tcmpCh)
        {
            int value = 0;
            int result = APS_get_field_bus_trigger_table_cmp(BoardID, busNo, modNo, tcmpCh, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }
        /// <summary>
        /// 该函数用于获取表比较的剩余计数值。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="LCmpCh">指定编码器通道编号。从零开始。对于 MNET-4XMO-C，TCmpCh 的范围是 0 到 4。（LCmpCh 0〜3 用于通用比较器。LCmpCh 4 用于高速比较器。）</param>
        /// <exception cref="APSException"></exception>
        public int GetTriggerTableCompareRemainCountEtherCAT(int busNo, int modNo, int LCmpCh)
        {
            int value = 0;
            int result = APS_get_field_bus_table_cmp_remain_count(BoardID, busNo, modNo, LCmpCh, ref value);
            if (result == 0)
                return value;
            else
                throw new APSException(result);
        }

        /// <summary>
        /// 此函数用于在指定的触发输出通道上强制输出触发。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="trgCh">对于 MNET-4XMO-C, TrgCh 的范围是 0 到 3。</param>
        /// <exception cref="APSException"></exception>
        public void SetTriggerManual(int busNo, int modNo, int trgCh)
        {
            int result = APS_set_field_bus_trigger_manual(BoardID, busNo, modNo, trgCh);
            if (result != 0) throw new APSException(result);
        }
        /// <summary>
        /// 此函数用于强制输出触发。通过此函数，所有输出通道都可以同步触发输出。
        /// </summary>
        /// <param name="busNo"></param>
        /// <param name="modNo"></param>
        /// <param name="trgChInBit"> 1: 输出触发，0：不输出触发 对于 MNET-4XMO-C : Bit0: TRG0, Bit1: TRG1, Bit2: TRG2, Bit3: TRG3</param>
        /// <exception cref="APSException"></exception>
        public void SetTriggerManualSEtherCAT(int busNo, int modNo, int trgChInBit)
        {
            int result = APS_set_field_bus_trigger_manual_s(BoardID, busNo, modNo, trgChInBit);
            if (result != 0) throw new APSException(result);
        }
        #endregion

        #region Motion


        /// <summary>
        /// Dec_Mode: When a different direction occurs or the deceleration distance is insufficient, the curve stops. <br/>
        /// P_bufferMode: 20XC p.104 <br/>
        /// </summary>
        /// <param name="P_boardAxisID"></param>
        /// <param name="P_command"></param>
        /// <param name="P_isRelativeMove"></param>
        /// <param name="P_isWaitTrigger"></param>
        /// <param name="P_startSpeed"></param>
        /// <param name="P_maxSpeed"></param>
        /// <param name="P_endSpeed"></param>
        /// <param name="P_accSpeed"></param>
        /// <param name="P_decSpeed"></param>
        /// <param name="P_SpeedFactor"></param>
        /// <param name="P_waitEnd"></param>
        /// <param name="P_timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public bool MoveAdvanced(int P_boardAxisID, double P_command,
            bool P_isRelativeMove = false, bool P_isWaitTrigger = false,
            Motion_Deceleration_Mode P_Dec_Mode = Motion_Deceleration_Mode.Smooth,
            Motion_Buffer_Mode P_bufferMode = Motion_Buffer_Mode.Aborting,
            double P_startSpeed = -1, double P_maxSpeed = -1, double P_endSpeed = -1, double P_accSpeed = -1, double P_decSpeed = -1, double P_SpeedFactor = -1,
            bool P_waitEnd = false, int P_timeout = -1, bool False_Stop = false)
        {
            //Check Parameter
            if (P_waitEnd == true)
            //if (P_waitEnd == true && (this.MonitorAxisStateFlag == 0))
            {
                WriteMessage("Method fail : MoveAdvanced", "Monitor is not enabled", DeepWise.MessageLevel.Error);
                if (False_Stop == true)
                {
                    this.StopMoveEmergency(P_boardAxisID);
                }
                return false;
            }

            //BoardID to axisID
            int _L_errorCode;
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, P_boardAxisID);

            //Var setting
            double StartSpeed = 0;
            if (P_startSpeed == -1)
            {
                double _StartSpeed = 0;
                this.Get_axis_param(P_boardAxisID, (int)APS_Define.PRA_VS, ref _StartSpeed);
                StartSpeed = (double)_StartSpeed;
            }
            else
            {
                StartSpeed = P_startSpeed;
            }
            double Max_Speed = 0;
            if (P_maxSpeed == -1)
            {
                double _Max_Speed = 0;
                this.Get_axis_param(P_boardAxisID, (int)APS_Define.PRA_VM, ref _Max_Speed);
                Max_Speed = (double)_Max_Speed;
            }
            else
            {
                Max_Speed = P_maxSpeed;
            }
            double End_Speed = 0;
            if (P_endSpeed == -1)
            {
                double _End_Speed = 0;
                this.Get_axis_param(P_boardAxisID, (int)APS_Define.PRA_VE, ref _End_Speed);
                End_Speed = (double)_End_Speed;
            }
            else
            {
                End_Speed = P_endSpeed;
            }
            double Acc_Speed = 0;
            if (P_accSpeed == -1)
            {
                double _Acc_Speed = 0;
                this.Get_axis_param(P_boardAxisID, (int)APS_Define.PRA_ACC, ref _Acc_Speed);
                Acc_Speed = (double)_Acc_Speed;
            }
            else
            {
                Acc_Speed = P_accSpeed;
            }
            double Dec_Speed = 0;
            if (P_decSpeed == -1)
            {
                double _Dec_Speed = 0;
                this.Get_axis_param(P_boardAxisID, (int)APS_Define.PRA_DEC, ref _Dec_Speed);
                Dec_Speed = (double)_Dec_Speed;
            }
            else
            {
                Dec_Speed = P_decSpeed;
            }
            double SpeedFactor = 0;
            if (P_SpeedFactor == -1)
            {
                double _SpeedFactor = 0;
                this.Get_axis_param(P_boardAxisID, (int)APS_Define.PRA_SF, ref _SpeedFactor);
                SpeedFactor = (double)_SpeedFactor;
            }
            else
            {
                SpeedFactor = P_SpeedFactor;
            }

            int Optial = 0;
            Optial.Variable_Bit_Set(0, Convert.ToInt32(P_isRelativeMove));
            Optial.Variable_Bit_Set(8, Convert.ToInt32(P_isWaitTrigger));
            Optial.Variable_Bit_Set(9, (int)P_Dec_Mode);
            Optial.Variable_Multi_Bit_Set(12, 4, (int)P_bufferMode);

            //Variable_Bit_Set(ref Optial, 0, Convert.ToInt32(P_isRelativeMove));
            //Variable_Bit_Set(ref Optial, 8, Convert.ToInt32(P_isWaitTrigger));
            //Variable_Bit_Set(ref Optial, 9, (int)P_Dec_Mode);
            //Variable_Multi_Bit_Set(ref Optial, 12, 4, (int)P_bufferMode);
            var asyncall = new ADLINK_DEVICE.ASYNCALL();

            if (L_result >= 0)
            {
                _L_errorCode = APS_ptp_all(L_axisID, Optial, P_command, StartSpeed, Max_Speed, End_Speed, Acc_Speed, Dec_Speed, SpeedFactor, ref asyncall);
                if (_L_errorCode != 0)
                {
                    WriteMessage(new APSException(_L_errorCode));
                    if (False_Stop == true)
                    {
                        this.StopMoveEmergency(P_boardAxisID);
                    }
                    return false;
                }
                if (P_waitEnd == true)
                {
                    bool Waitfunc()
                    {
                        Thread.Sleep(this.Monitor.UpdateRate);
                        //this.Update_Axis_Info();
                        bool result = true;
                        bool condition_1 = GetMotionStatus(P_boardAxisID, MotionStatus.MDN);
                        //bool condition_1 = (this.G_axisInfo.motion_status_MDN[P_boardAxisID] == true);
                        if (condition_1 == false)
                        {
                            result = false;
                        }
                        if (this.Wait_Func_Position_Validation == true)
                        {
                            bool condition_2 = (Math.Abs(GetErrorPosition(P_boardAxisID)) <= (double)this.Wait_Func_Position_Validation_Error_Range);
#if AxisInfo
                            bool condition_2 = (Math.Abs(this.G_axisInfo.error_position[P_boardAxisID]) <= (double)this.Wait_Func_Position_Validation_Error_Range);
#endif
                            if (condition_2 == false)
                            {
                                result = false;
                            }
                        }
                        return result;
                    }
                    if (SpinWait.SpinUntil(Waitfunc, P_timeout) == true)
                    {
                        return true;
                    }
                    else
                    {
                        WriteMessage("Method fail : MoveAdvanced", "Timeout", DeepWise.MessageLevel.Error);
                        if (False_Stop == true)
                        {
                            this.StopMoveEmergency(P_boardAxisID);
                        }
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                WriteMessage("Method fail : MoveAdvanced", "DeviceNotInitial", DeepWise.MessageLevel.Error);
                if (False_Stop == true)
                {
                    this.StopMoveEmergency(P_boardAxisID);
                }
                return false;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="intervals"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="moveCount"></param>
        /// <returns></returns>
        [Obsolete]
        public void Move_Interval(int[] axisID, int[] intervals, int[] maxSpeed, int moveCount, Action<IntervalMoveEventArgs> method, bool stepByStep = true)
        {
            int[] L_boardAxisID_List = new int[axisID.Length];
            if (intervals.Any(x => x == 0)) throw new ArgumentException("Relative distance can not be \'0\' .");
            if (axisID.Distinct().Count() != axisID.Length) throw new ArgumentException();
            //Get AxisID and Precheck 
            for (int i = 0; i < moveCount; i++)
            {
                int result = this.BoardAxisID_To_AxisID(ref L_boardAxisID_List[i], axisID[i]);
                if (result < 0) throw new APSException(result);
            }
            // Move
            for (int N_cycle = 0; N_cycle < moveCount; N_cycle++)
            {
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < moveCount; i++)
                {
                    if (stepByStep)
                    {
                        MoveRelative(L_boardAxisID_List[i], intervals[i], maxSpeed[i]);
                        Thread.Sleep(500);
                        WaitMotionStatus(L_boardAxisID_List[i], MotionStatus.MDN, true).Wait();
                    }
                    else
                        tasks.Add(Task.Run(() => MoveRelative(L_boardAxisID_List[i], intervals[i], maxSpeed[i])));
                }
                Task.WaitAll(tasks.ToArray());
                method?.Invoke(new IntervalMoveEventArgs(N_cycle));
            }
        }

        [Obsolete]
        public void TriggerMove(int AxisCount, int[] boardAxisID_Array)
        {
            int[] AxisID_Array = BoardAxisID_To_AxisID(boardAxisID_Array);
            int _L_errorCode = APS_move_trigger(AxisCount, AxisID_Array);
            if (_L_errorCode < 0) throw new APSException(_L_errorCode);
        }

        /// <summary>
        /// Dec_Mode: When a different direction occurs or the deceleration distance is insufficient, the curve stops. <br/>
        /// P_bufferMode: 20XC p.104 <br/>
        /// </summary>
        /// <param name="P_boardAxisID"></param>
        /// <param name="P_command"></param>
        /// <param name="P_isRelativeMove"></param>
        /// <param name="P_waitTrigger"></param>
        /// <param name="P_startSpeed"></param>
        /// <param name="P_maxSpeed"></param>
        /// <param name="P_endSpeed"></param>
        /// <param name="P_accSpeed"></param>
        /// <param name="P_decSpeed"></param>
        /// <param name="P_SpeedFactor"></param>
        /// <param name="P_waitEnd"></param>
        /// <param name="P_timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public bool Advanced_move_Line_v(int AxisCount, int[] boardAxisID_Array, double[] PositionArray,
            double P_startSpeed, double P_maxSpeed, double P_endSpeed, double P_accSpeed, double P_decSpeed, double P_SpeedFactor,
            bool P_isRelativeMove = false, bool P_isWaitTrigger = false,
            Motion_Deceleration_Mode P_Dec_Mode = Motion_Deceleration_Mode.Smooth,
            Motion_Buffer_Mode_Line P_bufferMode = Motion_Buffer_Mode_Line.Aborting_Stop_And_Blend)
        {
            //BoardID to axisID
            Int32 _L_errorCode;
            int[] AxisID_Array = BoardAxisID_To_AxisID(boardAxisID_Array);
            //Var setting
            int Optial = 0;
            Optial.Variable_Bit_Set(0, Convert.ToInt32(P_isRelativeMove));
            Optial.Variable_Bit_Set(8, Convert.ToInt32(P_isWaitTrigger));
            Optial.Variable_Bit_Set(9, (int)P_Dec_Mode);
            Optial.Variable_Multi_Bit_Set(12, 4, (int)P_bufferMode);
            ASYNCALL asyncall = new ASYNCALL();

            //Run
            if (AxisID_Array != null)
            {
                double TransPara = 0;
                _L_errorCode = APS_line_all(AxisCount, AxisID_Array, Optial, PositionArray,
                    ref TransPara, P_startSpeed, P_maxSpeed, P_endSpeed, P_accSpeed, P_decSpeed, P_SpeedFactor,
                    ref asyncall);
                if (_L_errorCode != 0)
                {
                    
                    WriteMessage(new APSException(_L_errorCode));
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                WriteMessage("Method fail : MoveAdvanced", "DeviceNotInitial", DeepWise.MessageLevel.Error);
                return false;
            }

        }

        /// <summary>
        /// Dec_Mode: When a different direction occurs or the deceleration distance is insufficient, the curve stops. <br/>
        /// P_bufferMode: 20XC p.104 <br/>
        /// </summary>
        /// <param name="P_boardAxisID"></param>
        /// <param name="P_command"></param>
        /// <param name="P_isRelativeMove"></param>
        /// <param name="P_waitTrigger"></param>
        /// <param name="P_startSpeed"></param>
        /// <param name="P_maxSpeed"></param>
        /// <param name="P_endSpeed"></param>
        /// <param name="P_accSpeed"></param>
        /// <param name="P_decSpeed"></param>
        /// <param name="P_SpeedFactor"></param>
        /// <param name="P_waitEnd"></param>
        /// <param name="P_timeout"></param>
        /// <returns></returns>
        [Obsolete]
        public bool Advanced_move_Line(int AxisCount, int[] boardAxisID_Array, double[] PositionArray,
            bool P_isRelativeMove = false, bool P_isWaitTrigger = false,
            Motion_Deceleration_Mode P_Dec_Mode = Motion_Deceleration_Mode.Smooth,
            Motion_Buffer_Mode_Line P_bufferMode = Motion_Buffer_Mode_Line.Aborting_Stop_And_Blend)
        {
            //BoardID to axisID
            int _L_errorCode;
            int[] AxisID_Array = BoardAxisID_To_AxisID(boardAxisID_Array);

            //Var setting
            int Optial = 0;
            Optial.Variable_Bit_Set(0, Convert.ToInt32(P_isRelativeMove));
            Optial.Variable_Bit_Set(8, Convert.ToInt32(P_isWaitTrigger));
            Optial.Variable_Bit_Set(9, (int)P_Dec_Mode);
            Optial.Variable_Multi_Bit_Set(12, 4, (int)P_bufferMode);
            var asyncall = new ADLINK_DEVICE.ASYNCALL();

            //Run            
            if (AxisID_Array != null)
            {
                double TransPara = 0;
                _L_errorCode = APS_line(AxisCount, AxisID_Array, Optial, PositionArray, ref TransPara, ref asyncall);
                if (_L_errorCode != 0)
                {
                    WriteMessage(new APSException(_L_errorCode));
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                WriteMessage("Method Fail : Advanced_move_Line", "DeviceNotInitial", DeepWise.MessageLevel.Error);
                return false;
            }

        }

        /// <summary>
        /// This function is used to get current free space ofmotion queue. <br/>
        /// Each axis has own motion queue (FIFO) to buffer the motion commands. <br/>
        /// </summary>
        /// <param name="P_boardAxisID"></param>
        /// <param name="P_gettingValue"></param>
        /// <returns></returns>
        public int Get_Axis_Buffer_SpaceCount(int P_boardAxisID)
        {
            //BoardID to axisID
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, P_boardAxisID);

            int rtn = -1;
            if (L_result <= 0)
            {
                APS_get_mq_free_space(L_axisID, ref rtn);
            }
            return rtn;
        }

        /// <summary>
        /// This function is used to get current usage from motion queue.
        /// Each axis has own motion queue (FIFO) to buffer the motion commands.
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="P_gettingValue"></param>
        /// <returns></returns>
        public int Get_Axis_Buffer_UsageCount(int axisID, ref int P_gettingValue)
        {
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, axisID);
            if (L_result >= 0)
            {
                var _L_errorCode = APS_get_mq_usage(L_axisID, ref P_gettingValue);
                return _L_errorCode;
            }
            else { return (int)APS_Define.ERR_DeviceNotInitial; }
        }

        #endregion

        #region Stop
        /// <summary>
        /// 
        /// </summary>
        /// <param name="P_boardAxisID"></param>
        /// <param name="P_distance"></param>
        /// <param name="P_maxSpeed"></param>
        /// <returns></returns>
        public void StopMove(int P_boardAxisID)
        {
            int L_axisID = -1;
            int result = this.BoardAxisID_To_AxisID(ref L_axisID, P_boardAxisID);
            if (result >= 0)
            {
                result = APS_stop_move(L_axisID);

                if (result != 0) throw new APSException(result);
            }
            else
                throw new APSException(result);
        }
        public int StopMoveAll()
        {
            var exs = new List<Exception>();
            for (int i = 0; i < CurrentAxisCount; i++)
            {
                try
                {
                    this.StopMove(BoardStartAxisID + i);
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                }
            }

            if (exs.Count == 1)
                throw exs[0];
            else
                throw new AggregateException(exs);
        }

        public void StopMoveEmergency(int P_boardAxisID)
        {
            int result;
            int L_axisID = 0;
            result = this.BoardAxisID_To_AxisID(ref L_axisID, P_boardAxisID);
            if (result >= 0)
            {
                result = APS_emg_stop(L_axisID);
                if (result != 0)
                    throw new APSException(result);
            }
            else
                throw new APSException(result);
        }

        public void StopMoveAllEmergency()
        {
            var exs = new List<Exception>();
            for (int i = 0; i < CurrentAxisCount; i++)
            {
                try
                {
                    StopMoveEmergency(BoardStartAxisID + i);
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                }

            }
            if (exs.Count == 1)
                throw exs[0];
            else
                throw new AggregateException(exs);
        }

        public void StopEmergency()
        {
            this.SetServoAll(false);
            this.StopMoveAllEmergency();
        }
        #endregion

        //==========================================================

        #region Board Info
        //public string Name = "unnamed";
        DeviceName g_boardCardName = 0;
        // Board Axis info

        private object Axis_Info_Update_Lock = new object();
        bool EMG_Flag = false;
        #endregion

        #region Get set
        public DeviceName Model => g_boardCardName;
        public int BoardID { get; private set; } = -1;
        public int BoardChannel { get; private set; } = 0;
        public int CurrentAxisCount { get; private set; } = 0;
        public int BoardStartAxisID { get; private set; } = -1;
        public int BoardTotalAxis { get; private set; } = 0;

        public bool IsInitialized { get; private set; } = false;
        #endregion

        #region Wait_Func_Position_Validation
        public bool Wait_Func_Position_Validation = false;
        public int Wait_Func_Position_Validation_Error_Range = 2;
        #endregion

        #region Initial
        public ADLINK_Motion(string P_name) : base(P_name)
        {
            Monitor = new MotionControllerMonitor(this, WriteMessage);
        }

        public bool Initial() => Initial(0, null);

        /// <summary>
        /// initial
        /// </summary>
        /// <param name="P_mode">
        /// Bit 0:    Enable the On board dip switch (SW1) to decide the Card ID. [0:By system assigned, 1:By dip switch] <br/>
        /// Bit 1:    Parallel type axis indexing mode [0: auto mode (default),  1: fixed mode] <br/>
        /// Bit 2:    Serial type axis indexing modeForPCIe-833x:[Note 1] [0: auto mode (default), 1: fixed mode] <br/>
        /// Bit 4/5:  Option of load system and axes parametersmethod. <br/>
        /// 0: Do nothing, parameters keep current value.(01B), 1: Load from default(02B) 2: Load from flash(*1) <br/>
        /// Bit 9:    Option to select behavior of MDN bit of motion status.(PCI-8254/58 / AMP-204/8Cand PCIe-833x) only <br/>
        /// 0: MDN turns on when CSTP or ASTP occurs. (default) 1: MDN turns on when CSTP occurs] <br/>
        /// </param>
        /// <returns>
        /// APS Error Code
        /// </returns>
        public int Initial(uint BoardID, Int32 P_mode = 0b0000_0000_0000_0001, string LoadPath = null)
        {
            int _L_errorCode = 0;
            Int32 L_startAxisID = 0;
            Int32 L_totalAxisNum = 0;

            // Check initial
            if (IsInitialized)
            {
                return (int)APS_Define.ERR_DeviceAlreadyInitialed;
            }

            // Card(Board) initial
            if (MotionController.BoardID_Status == -1)
            {
                _L_errorCode = APS_initial(ref MotionController.BoardID_Status, P_mode);
            }
            // current not supchannel multi-card
            // Initial whether is succeed?
            if (_L_errorCode == 0)
            {
                if (_L_errorCode != 0)
                {
                    IsInitialized = false;
                    return _L_errorCode;
                }
                // Check initial device state and record device state.
                int L_temp_CurrentBitState = 0;
                int L_tempFlag_HaveTheDevice = 0;

                // Each bit is used as a the board ID states, Board id by board system assign or Hardware_DIP_SW Setting.
                L_temp_CurrentBitState = (MotionController.BoardID_Status >> (int)BoardID) & 1;
                if (L_temp_CurrentBitState == 1)
                {
                    // Get card name through the board ID states bit.
                    _L_errorCode = APS_get_card_name((int)BoardID, out g_boardCardName);

                    _L_errorCode = APS_get_first_axisId((int)BoardID, ref L_startAxisID, ref L_totalAxisNum);

                    this.BoardID = (int)BoardID;
                    BoardTotalAxis = CurrentAxisCount = L_totalAxisNum;
                    BoardStartAxisID = L_startAxisID;

                    if (this.BoardTotalAxis == 4) BoardChannel = 2;
                    else if (this.BoardTotalAxis == 8) BoardChannel = 4;

                    L_tempFlag_HaveTheDevice = 1;
                }

                Thread.Sleep(2500);
                // check whether Have 204C series Device.
                if (L_tempFlag_HaveTheDevice == 1)
                {
                    IsInitialized = true;
                    if (LoadPath != null)
                    {
                        LoadParamFromFile(LoadPath);
                    }
#if AxisInfo
                    this.Axis_Info_Inital();
                    this.Update_Axis_Info();
#endif
                    //this.Check_Available_Axis();
                    return 0;
                }
                else
                {
                    IsInitialized = false;
                    return (int)APS_Define.ERR_NoDeviceFound;
                }
            }
            else
            {
                IsInitialized = false;
                return (int)APS_Define.ERR_NoDeviceFound;
            }
        }

        /// <summary>
        /// initial
        /// </summary>
        /// <param name="mode">
        /// Bit 0:    Enable the On board dip switch (SW1) to decide the Card ID. [0:By system assigned, 1:By dip switch] <br/>
        /// Bit 1:    Parallel type axis indexing mode [0: auto mode (default),  1: fixed mode] <br/>
        /// Bit 2:    Serial type axis indexing modeForPCIe-833x:[Note 1] [0: auto mode (default), 1: fixed mode] <br/>
        /// Bit 4/5:  Option of load system and axes parametersmethod. <br/>
        /// 0: Do nothing, parameters keep current value.(01B), 1: Load from default(02B) 2: Load from flash(*1) <br/>
        /// Bit 9:    Option to select behavior of MDN bit of motion status.(PCI-8254/58 / AMP-204/8Cand PCIe-833x) only <br/>
        /// 0: MDN turns on when CSTP or ASTP occurs. (default) 1: MDN turns on when CSTP occurs] <br/>
        /// </param>
        /// <returns>
        /// APS Error Code
        /// </returns>
        public bool Initial(int mode, string LoadPath)
        {
            int error = 0;
            try
            {
                error = APS_initial(ref MotionController.BoardID_Status, mode);
            }
            catch (Exception ex)
            {
                WriteMessage(ex);
                if (EnableThrowException) throw ex;
                return false;
            }

            if (error != 0)
            {
                var ex = new APSException(error);
                if (EnableThrowException) throw ex;
                WriteMessage(ex, Environment.StackTrace);
                return false;
            }

            if (MotionController.BoardID_Status != 1) throw new Exception("初始化失敗：尚未支援兩張板卡");

            Int32 L_startAxisID = 0;
            Int32 L_totalAxisNum = 0;
            error = APS_get_card_name(0, out g_boardCardName);
            if (error != 0) ThrowException();
            error = APS_get_first_axisId(0, ref L_startAxisID, ref L_totalAxisNum);
            if (error != 0) ThrowException();

            this.BoardID = 0;
            this.BoardTotalAxis = CurrentAxisCount = L_totalAxisNum;
            this.BoardStartAxisID = L_startAxisID;

            if (this.BoardTotalAxis == 4) BoardChannel = 2;
            else if (this.BoardTotalAxis == 8) BoardChannel = 4;

            Thread.Sleep(2500);
            IDictionary<int, EC_MODULE_INFO> dic = null;
            if (IsEtherCATSupported)
            {
                try
                {
                    error = APS_start_field_bus(this.BoardID, 0, this.BoardStartAxisID);
                    if (error != 0) throw new APSException(error);
                }
                catch (APSException ex)
                {
                    //(-1011) ERR_MissESIFileOrMissENIPath 缺少ESI 文件或 ENI 路径。
                    if (ex.ErrorCode == -1011 || ex.ErrorCode == -4013)
                    {
                        error = APS_scan_field_bus(this.BoardID, 0);
                        if (error != 0) throw new APSException(error);
                        
                        error = APS_start_field_bus(this.BoardID, 0, this.BoardStartAxisID);
                        if (error != 0) throw new APSException(error);
                    }
                    else
                    {
                        APS_close();
                        throw ex;
                    }
                }
                dic = InitializeEtherCAT();
            }

            InitializeIOList(dic);

            motionStatus = new int[CurrentAxisCount];
            motionIOStatus = new int[CurrentAxisCount];

            InitializeAxes();

            //Initialize other setting
            if (LoadPath != null) LoadParamFromFile(LoadPath);
            //this.Check_Available_Axis();

            Monitor.Update += UpdateMotioStatus;
            Monitor.Update += UpdateGeneralInput;
            Monitor.Start();
            if (IsEtherCATSupported) Monitor.Update += UpdateEtherCATInput;

            IsInitialized = true;
            return true;
            void ThrowException()
            {
                APS_close();
                IsInitialized = false;
                if (error != 0) throw new APSException(error);
            }
        }

        public MotionControllerMonitor Monitor { get; }

        //public ReadOnlyDictionary<int, EC_MODULE_INFO> SlaveIOInfo { get; private set; } = new ReadOnlyDictionary<int, EC_MODULE_INFO>(new Dictionary<int, EC_MODULE_INFO>());
        Dictionary<int, bool[]> diDataEther = new Dictionary<int, bool[]>();
        Dictionary<int, double[]> aiDataEther = new Dictionary<int, double[]>();
        Dictionary<int, EC_MODULE_INFO> InitializeEtherCAT()
        {
            int[] ary = new int[1];
            int count = 0;
            APS_get_field_bus_last_scan_info(0, 0, ref ary[0], 1, ref count);
            int slaveCount = ary[0];
            int axisCount = 0;

            var slaveInfo = new Dictionary<int, EC_MODULE_INFO>();
            for (int slaveID = 0; slaveID < slaveCount; slaveID++)
            {
                EC_MODULE_INFO[] info = new EC_MODULE_INFO[1];
                APS_get_field_bus_module_info(0, 0, slaveID, info);
                axisCount += info[0].TotalAxisNum;
                if (new int[] { info[0].DI_ModuleNum, info[0].DO_ModuleNum, info[0].AI_ModuleNum, info[0].AO_ModuleNum }.Any(x => x > 0))
                {
                    if (info[0].DI_ModuleNum > 0)
                    {
                        //TODO : 判斷Input模型Channel數量
                        int inputChannelsCountPerMod = 8;
                        diDataEther.Add(slaveID, new bool[info[0].DI_ModuleNum * inputChannelsCountPerMod]);
                        for (int i = 0; i < info[0].DI_ModuleNum * inputChannelsCountPerMod; i++)
                        {
                            if(APS_get_field_bus_d_channel_input(this.BoardID, 0, slaveID, i, out var value)==0)
                                diDataEther[slaveID][i] = value != 0;
                        }
                    }
                    if (info[0].AI_ModuleNum > 0)
                    {
                        //TODO : 判斷Input模型Channel數量
                        int inputChannelsCountPerMod = 4;
                        aiDataEther.Add(slaveID, new double[info[0].AI_ModuleNum * inputChannelsCountPerMod]);
                        for (int i = 0; i < info[0].AI_ModuleNum * inputChannelsCountPerMod; i++)
                        {
                            APS_get_field_bus_a_input(this.BoardID, 0, slaveID, i, out aiDataEther[slaveID][i]);
                        }
                    }

                    slaveInfo.Add(slaveID, info[0]);
                }
            }
            
            Debug.WriteLine($"InitializeEtherCAT()\r\nAxisCount : {axisCount}\r\nSlaveIOInfo : {slaveInfo.Count}");
            CurrentAxisCount = axisCount;
            return slaveInfo;
        }
        #endregion

        #region Servo
        public bool SetServoAll(bool state)
        {
            bool Error_Flag = false;
            if (IsInitialized)
            {
                for (int i = 0; i < this.CurrentAxisCount; i++)
                {
                    var axis = this.BoardStartAxisID + i;
                    var _L_errorCode = APS_set_servo_on(axis, state ? 1 : 0);
                    if (_L_errorCode != 0)
                    {
                        //DebugInfo.Info_Descript = string.Format("204C_ErrorCode,{0}", _L_errorCode);
                        var ex = new APSException(_L_errorCode);
                        this.WriteMessage(new APSException(_L_errorCode), Environment.StackTrace);
                        if (EnableThrowException) throw ex;
                        Error_Flag = true;
                    }
                }
            }
            if (Error_Flag == true)
            {
                return false;
            }
            else
            {
                Thread.Sleep(2500);
                if (state && this.EMG_Flag == true)
                {
                    this.EMG_Flag = false;
                }
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool SetServo(int axisID, bool state)
        {
            int _L_errorCode = 0;
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, axisID);
            if (L_result >= 0)
            {
                _L_errorCode = APS_set_servo_on(L_axisID, state ? 1 : 0);
                if (_L_errorCode != 0)
                {
                    var ex = new APSException(_L_errorCode);
                    WriteMessage(ex, Environment.StackTrace);
                    if (EnableThrowException) throw ex;
                    return false;
                }
                Thread.Sleep(50);//2500
                if (this.EMG_Flag == true)
                {
                    this.EMG_Flag = false;
                }
                return true;
            }
            else
            {
                var ex = new APSException((int)APS_Define.ERR_DeviceNotInitial);
                WriteMessage(ex, Environment.StackTrace);
                if (EnableThrowException) throw ex;
                return false;
            }
        }

        /// <summary>
        /// Warning! This function will close all about motion control from ADLINK_DEVICE. <br/>
        /// Suggest use Servo_OFF_AllBoardAxis.<br/>
        /// </summary>
        /// <returns>APS Error Code</returns>
        public int Close()
        {
            try
            {
                Monitor.Stop();
            }
            catch { }
            //monitorThread?.Abort();
            this.SetServoAll(false);
            var _L_errorCode = APS_close();
            IsInitialized = false;
            return _L_errorCode;
        }
        #endregion

  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="P_boardAxisID"></param>
        /// <param name="P_paramID"></param>
        /// <param name="P_paramValue"></param>
        /// <returns>APS Error Code</returns>
        [Obsolete]
        public int Get_axis_param(int P_boardAxisID, int P_paramID, ref double P_paramValue)
        {
            if (!Is_Float_axis_Param_ID(P_paramID)) return (int)APS_Define.ERR_ParametersInvalid;
            int L_axisID = -1;
            int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, P_boardAxisID);
            if (L_result >= 0)
            {
                return APS_get_axis_param_f(L_axisID, P_paramID, ref P_paramValue);
            }
            else { return (int)APS_Define.ERR_DeviceNotInitial; }
        }

        #region Save and Load
        public void SaveParameterToFlash()
        {
            var result = APS_save_parameter_to_flash(this.BoardID);
            if (result != 0) throw new APSException(result);
        }
        public void LoadParameterFromFlash()
        {
            var result = APS_load_parameter_from_flash(this.BoardID);
            if (result != 0) throw new APSException(result);
        }
        public void LoadParameterFromDefault()
        {
            var result = APS_load_parameter_from_default(this.BoardID);
            if (result != 0) throw new APSException(result);
        }
        public void SaveParameterToFile(string path)
        {
            var result = APS_save_param_to_file(this.BoardID, path);
            if (result != 0) throw new APSException(result);
        }

        /// <summary>
        /// Load xml to has been saved boardID.
        /// </summary>
        /// <param name="Save_path"></param>
        /// <returns>APS Error Code</returns>
        public void LoadParamFromFile(string P_savePath)
        {
            int _L_errorCode = APS_load_param_from_file(P_savePath);
            if (_L_errorCode != 0) throw new APSException(_L_errorCode);
        }
        #endregion

        //TODO : implement APSParameterTypeAttribute
        private bool Is_Float_axis_Param_ID(int Param_ID)
        {
            switch (Param_ID)
            {
                case (int)APS_Define.PRA_PSR_JERK:
                case (int)APS_Define.PRA_PSR_ACC:
                case (int)APS_Define.PRA_PSR_RATIO_VALUE:
                case (int)APS_Define.PRA_BIQUAD1_DIV:
                case (int)APS_Define.PRA_BIQUAD1_B2:
                case (int)APS_Define.PRA_BIQUAD1_B1:
                case (int)APS_Define.PRA_BIQUAD1_B0:
                case (int)APS_Define.PRA_BIQUAD1_A2:
                case (int)APS_Define.PRA_BIQUAD1_A1:
                case (int)APS_Define.PRA_BIQUAD0_DIV:
                case (int)APS_Define.PRA_BIQUAD0_B2:
                case (int)APS_Define.PRA_BIQUAD0_B1:
                case (int)APS_Define.PRA_BIQUAD0_B0:
                case (int)APS_Define.PRA_BIQUAD0_A2:
                case (int)APS_Define.PRA_BIQUAD0_A1:
                case (int)APS_Define.PRA_BKL_CNSP:
                case (int)APS_Define.PRA_BKL_DIST:
                case (int)APS_Define.PRA_ERR_POS_LEVEL:
                case (int)APS_Define.PRA_SERVO_V_LIMIT:
                case (int)APS_Define.PRA_SERVO_V_BIAS:
                case (int)APS_Define.PRA_MOVE_RATIO:
                case (int)APS_Define.PRA_POS_UNIT_FACTOR:
                case (int)APS_Define.PRA_GANTRY_PROTECT_2:
                case (int)APS_Define.PRA_GANTRY_PROTECT_1:
                case (int)APS_Define.PRA_GEAR_RATIO:
                case (int)APS_Define.PRA_GEAR_ENGAGE_RATE:
                case (int)APS_Define.PRA_JG_OFFSET:
                case (int)APS_Define.PRA_JG_VM:
                case (int)APS_Define.PRA_JG_DEC:
                case (int)APS_Define.PRA_JG_ACC:
                case (int)APS_Define.PRA_JG_SF:
                case (int)APS_Define.PRA_POST_EVENT_DIST:
                case (int)APS_Define.PRA_PRE_EVENT_DIST:
                case (int)APS_Define.PRA_VE:
                case (int)APS_Define.PRA_VM:
                case (int)APS_Define.PRA_VS:
                case (int)APS_Define.PRA_DEC:
                case (int)APS_Define.PRA_ACC:
                case (int)APS_Define.PRA_SF:
                case (int)APS_Define.PRA_HOME_POS:
                case (int)APS_Define.PRA_HOME_VO:
                case (int)APS_Define.PRA_HOME_SHIFT:
                case (int)APS_Define.PRA_HOME_VM:
                case (int)APS_Define.PRA_HOME_VS:
                case (int)APS_Define.PRA_HOME_ACC:
                case (int)APS_Define.PRA_HOME_CURVE:
                case (int)APS_Define.PRA_SMEL_POS:
                case (int)APS_Define.PRA_SPEL_POS:
                case (int)APS_Define.PRA_SD_DEC:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// this.BoardAxisID_To_APSAxisID
        /// </summary>
        /// <param name="P_boardAxisIDID"></param>
        /// <returns>
        /// CommonStateCode
        /// </returns>
        public int BoardAxisID_To_AxisID(ref int APSAxisID, int P_boardAxisID)
        {
            if (BoardStartAxisID == -1 || !IsInitialized)
            {
                return (int)APS_Define.ERR_DeviceNotInitial;
            }
            else
            {
                if (P_boardAxisID < BoardTotalAxis)
                {
                    APSAxisID = BoardStartAxisID + P_boardAxisID;
                    return (int)APS_Define.ERR_NoError;
                }
                else
                {
                    return (int)APS_Define.ERR_ParametersInvalid;
                }
            }
        }

        private int[] BoardAxisID_To_AxisID(int[] boardAxisID_Array)
        {
            #region BoardID to axisID
            int L_axisID = -1;
            int[] AxisID_Array = new int[boardAxisID_Array.Length];
            for (int i = 0; i < boardAxisID_Array.Length; i++)
            {
                int L_result = this.BoardAxisID_To_AxisID(ref L_axisID, boardAxisID_Array[i]);
                AxisID_Array[i] = L_axisID;
                if (L_result != 0)
                {
                    return null;
                }
            }
            return AxisID_Array;
            #endregion
        }

    }

    public enum Motion_Buffer_Mode
    {
        Aborting = (0),
        Buffered = (1),
        Blending_Low = (2),
        Blending_Previous = (3),
        Blending_Next = (4),
        Blending_High = (5),
    }

    public enum Motion_Buffer_Mode_Line
    {
        Aborting_Stop_And_Blend = (0),
        Aborting_Force_Abort = (1),
        Aborting_Stop_Then_Go = (2),
        Buffered = (3),
        Blending_Deceleration_Event = (4),
        Blending_Residue_Distance = (5),
        Blending_Residue_Distance_Percentage = (6),
    }

    public enum Motion_Deceleration_Mode
    {
        Smooth = (0),
        Immediately = (1),
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerParameter_ECAT_4XMO
    {
        /// <summary>
        /// <para>Linear compare 0 (LCMP0) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_LCMP0_SRC = 0x00,
        /// <summary>
        /// <para>Linear compare 1 (LCMP1) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_LCMP1_SRC = 0x01,
        /// <summary>
        /// <para>Table compare 0 (TCMP0) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_TCMP0_SRC = 0x02,
        /// <summary>
        /// <para>Table compare 1 (TCMP1) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_TCMP1_SRC = 0x03,
        /// <summary>
        /// <para>Table compare 0 (TCMP0) direction </para> 
        /// <para>0: Negative direction </para> 
        /// <para>1: Positive direction </para> 
        /// <para>2: Bi-direction(No direction) </para> 
        /// </summary>
        [DefaultValue(1)]
        TGR_TCMP0_DIR = 0x04,
        /// <summary>
        /// <para>Table compare 1 (TCMP1) direction </para> 
        /// <para>0: Negative direction </para> 
        /// <para>1: Positive direction </para> 
        /// <para>2: Bi-direction(No direction) </para> 
        /// </summary>
        [DefaultValue(1)]
        TGR_TCMP1_DIR = 0x05,
        /// <summary>
        /// <para>TRG 0 ~ 3 enable by bit </para> 
        /// <para>Bit x: ( 0: disable, 1: enable) </para> 
        /// <para>Bit 0:TRG0 enable, PWM pulse out0 enable. </para> 
        /// <para>Bit 1:TRG1 enable, PWM pulse out1 enable. </para> 
        /// <para>Bit 2:TRG2 enable, PWM pulse out2 enable. </para> 
        /// <para>Bit 3:TRG3 enable, PWM pulse out3 enable. </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG_EN = 0x06,
        /// <summary>
        /// <para>Table compare 0 (TCMP0) compare data reuse function enable. </para> 
        /// <para>0: Disable reuse function. </para> 
        /// <para>1: Enable reuse function. </para> 
        /// <para>NOTE: Must make sure comparison is not running before using. </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TCMP0_REUSE = 0x07,
        /// <summary>
        /// <para>Table compare 1(TCMP1) compare data reuse function enable. </para> 
        /// <para>0: Disable reuse function. </para> 
        /// <para>1: Enable reuse function. </para> 
        /// <para>NOTE: Must make sure comparison is not running before using. </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TCMP1_REUSE = 0x08,
        /// <summary>
        /// <para>Table compare 0(TCMP0) data transfer done status. </para> 
        /// <para>-1: Not in using. </para> 
        /// <para>0: Transfer not finish. </para> 
        /// <para>1: Transfer finish. </para> 
        /// <para>Be attention: Transfer finish status is read clear status. Status will be “-1”  </para> 
        /// <para>-> “0”... data transfer…  </para> 
        /// <para>-> ”1” (By user program read) -> “-1”. </para> 
        /// </summary>
        [DefaultValue(-1)]
        TGR_TCMP0_TRANSFER_DONE = 0x09,
        /// <summary>
        /// <para>Table compare 1(TCMP1) data  </para> 
        /// <para>transfer done status. </para> 
        /// <para>-1: Not in using. </para> 
        /// <para>0: Transfer not finish. </para> 
        /// <para>1: Transfer finish. </para> 
        /// <para>Be attention: Transfer finish status is read clear status. Status will be “-1”  </para> 
        /// <para>-> “0”... data transfer… -1 -> ”1” (By user program read) -> “-1”.</para> 
        /// </summary>
        [DefaultValue(-1)]
        TGR_TCMP1_TRANSFER_DONE = 0x0A,
        /// <summary>
        /// <para>Trigger output 0  </para> 
        /// <para>(TRG0) source </para> 
        /// <para>Note: OR multi-sources, then output to TRG0 ) </para> 
        /// <para>Bit x:( 1: On, 0: Off ) </para> 
        /// <para>Bit 0:Manual0 </para> 
        /// <para>Bit 1:Reserved </para> 
        /// <para>Bit 2:TCMP0 </para> 
        /// <para>Bit 3:TCMP1 </para> 
        /// <para>Bit 4:LCMP0 </para> 
        /// <para>Bit 5:LCMP1 </para> 
        /// <para>Bit 6: Reserved </para> 
        /// <para>Bit 7:TCMP2 </para> 
        /// <para>Bit 8:TCMP3 </para> 
        /// <para>Bit 9:LCMP2 </para> 
        /// <para>Bit 10:LCMP3 </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG0_SRC = 0x10,
        /// <summary>
        /// <para>Trigger output 1 (TRG1) source </para> 
        /// <para>Note: OR multi-sources, then output to TRG0 ) </para> 
        /// <para>Bit x:( 1: On, 0: Off ) </para> 
        /// <para>Bit 0: Manual0 </para> 
        /// <para>Bit 1:Reserved </para> 
        /// <para>Bit 2:TCMP0 </para> 
        /// <para>Bit 3:TCMP1 </para> 
        /// <para>Bit 4:LCMP0 </para> 
        /// <para>Bit 5:LCMP1 </para> 
        /// <para>Bit 6: Reserved </para> 
        /// <para>Bit 7:TCMP2 </para> 
        /// <para>Bit 8:TCMP3 </para> 
        /// <para>Bit 9:LCMP2 </para> 
        /// <para>Bit 10:LCMP3 </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG1_SRC = 0x11,
        /// <summary>
        /// <para>Trigger output 2 (TRG2) source </para> 
        /// <para>Note: OR multi-sources, then output to TRG0 ) </para> 
        /// <para>Bit x:( 1: On, 0: Off ) </para> 
        /// <para>Bit 0: Manual0 </para> 
        /// <para>Bit 1:Reserved </para> 
        /// <para>Bit 2:TCMP0 </para> 
        /// <para>Bit 3:TCMP1 </para> 
        /// <para>Bit 4:LCMP0 </para> 
        /// <para>Bit 5:LCMP1 </para> 
        /// <para>Bit 6: Reserved </para> 
        /// <para>Bit 7:TCMP2 </para> 
        /// <para>Bit 8:TCMP3 </para> 
        /// <para>Bit 9:LCMP2 </para> 
        /// <para>Bit 10:LCMP3 </para> 
        /// <para>0 </para> 
        /// </summary>
        [DefaultValue(1076)]
        TGR_TRG2_SRC = 0x12,
        /// <summary>
        /// <para>Trigger output 3 (TRG3) source </para> 
        /// <para>Note: OR multi-sources, then output to TRG0 ) </para> 
        /// <para>Bit x:( 1: On, 0: Off ) </para> 
        /// <para>Bit 0:Manual0 </para> 
        /// <para>Bit 1:Reserved </para> 
        /// <para>Bit 2:TCMP0 </para> 
        /// <para>Bit 3:TCMP1 </para> 
        /// <para>Bit 4:LCMP0 </para> 
        /// <para>Bit 5:LCMP1 </para> 
        /// <para>Bit 6: Reserved </para> 
        /// <para>Bit 7:TCMP2 </para> 
        /// <para>Bit 8:TCMP3 </para> 
        /// <para>Bit 9:LCMP2 </para> 
        /// <para>Bit 10:LCMP3 </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG3_SRC = 0x13,
        /// <summary>
        /// <para>TRG0 pulse width  </para> 
        /// <para>Pulse Width = (N-1)*20ns </para> 
        /// <para>N = 2 ~ 0xffffff </para> 
        /// </summary>
        [DefaultValue(11)]
        TGR_TRG0_PWD = 0x14,
        /// <summary>
        /// <para>TRG1 pulse width  </para> 
        /// <para>Pulse Width = (N-1)*20ns </para> 
        /// <para>N = 2 ~ 0xffffff </para> 
        /// </summary>
        [DefaultValue(11)]
        TGR_TRG1_PWD = 0x15,
        /// <summary>
        /// <para>TRG2 pulse width  </para> 
        /// <para>Pulse Width = (N-1)*20ns </para> 
        /// <para>N = 2 ~ 0xffffff </para> 
        /// </summary>
        [DefaultValue(11)]
        TGR_TRG2_PWD = 0x16,
        /// <summary>
        /// <para>TRG3 pulse width  </para> 
        /// <para>Pulse Width = (N-1)*20ns </para> 
        /// <para>N = 2 ~ 0xffffff </para> 
        /// </summary>
        [DefaultValue(11)]
        TGR_TRG3_PWD = 0x17,
        /// <summary>
        /// <para>TRG 0 logic  </para> 
        /// <para>0: Not inverse </para> 
        /// <para>1: Inverse </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG0_LOGIC = 0x18,
        /// <summary>
        /// <para>TRG 1 logic  </para> 
        /// <para>0: Not inverse </para> 
        /// <para>1: Inverse </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG1_LOGIC = 0x19,
        /// <summary>
        /// <para>TRG 2 logic  </para> 
        /// <para>0: Not inverse </para> 
        /// <para>1: Inverse </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG2_LOGIC = 0x1A,
        /// <summary>
        /// <para>TRG 3 logic  </para> 
        /// <para>0: Not inverse </para> 
        /// <para>1: Inverse </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG3_LOGIC = 0x1B,
        /// <summary>
        /// <para>TRG 0 toggle mode  </para> 
        /// <para>0: Pulse out </para> 
        /// <para>1: Toggle out </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG0_TGL = 0x1C,
        /// <summary>
        /// <para>TRG 1 toggle mode  </para> 
        /// <para>0: Pulse out </para> 
        /// <para>1: Toggle out </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG1_TGL = 0x1D,
        /// <summary>
        /// <para>TRG 2 toggle mode  </para> 
        /// <para>0: Pulse out </para> 
        /// <para>1: Toggle out </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG2_TGL = 0x1E,
        /// <summary>
        /// <para>TRG 3 toggle mode  </para> 
        /// <para>0: Pulse out </para> 
        /// <para>1: Toggle out </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TRG3_TGL = 0x1F,
        /// <summary>
        /// <para>Timer Interval Timer Interval = N * 40 ns  </para> 
        /// <para>N = 1~ 107374182 </para> 
        /// </summary>
        [DefaultValue(1)]
        TIMR_ITV = 0x20,
        /// <summary>
        /// <para>Timer direction  </para> 
        /// <para>0: Positive count </para> 
        /// <para>1: Negative count </para> 
        /// </summary>
        [DefaultValue(0)]
        TIMR_DIR = 0x21,
        /// <summary>
        /// <para>Enable timer counter to be as Ring counter </para> 
        /// <para>0: Disable </para> 
        /// <para>(0x7fffffff+10x80000000)  </para> 
        /// <para>(0x80000001-10x80000000) </para> 
        /// <para>1: Enable </para> 
        /// <para>(0x7fffffff+10x00000000) </para> 
        /// <para>(0x00000000-10x7fffffff) </para> 
        /// </summary>
        [DefaultValue(0)]
        TIMR_RING_EN = 0x22,
        /// <summary>
        /// <para>Timer enable 0: Disable, 1:Enable  </para> 
        /// </summary>
        [DefaultValue(0)]
        TIMR_EN = 0x23,
        /// <summary>
        /// <para>Table compare 2(TCMP2) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_TCMP2_SRC = 0x40,
        /// <summary>
        /// <para>Table compare 3(TCMP3) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_TCMP3_SRC = 0x41,
        /// <summary>
        /// <para>Table compare 2(TCMP2) direction </para> 
        /// <para>0: Negative direction </para> 
        /// <para>1: Positive direction </para> 
        /// <para>2: Bi-direction(No direction) </para> 
        /// </summary>
        [DefaultValue(1)]
        TGR_TCMP2_DIR = 0x42,
        /// <summary>
        /// <para>Table compare 3 (TCMP3) direction </para> 
        /// <para>0: Negative direction </para> 
        /// <para>1: Positive direction </para> 
        /// <para>2: Bi-direction(No direction) </para> 
        /// </summary>
        [DefaultValue(1)]
        TGR_TCMP3_DIR = 0x43,
        /// <summary>
        /// <para>Linear compare 2(LCMP2) source </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_LCMP2_SRC = 0x44,
        /// <summary>
        /// <para>Linear compare 3  </para> 
        /// <para>0 ~ 3: Encoder counter  </para> 
        /// <para>5 </para> 
        /// <para>(LCMP3) source 0~3 </para> 
        /// <para>4: Timer 0 counter </para> 
        /// <para>5: Disable </para> 
        /// </summary>
        [DefaultValue(5)]
        TGR_LCMP3_SRC = 0x45,
        /// <summary>
        /// <para>Table compare 2(TCMP2) compare data reuse function enable. </para> 
        /// <para>0: Disable reuse function. </para> 
        /// <para>1: Enable reuse function. </para> 
        /// <para>NOTE: Must make sure comparison is not running before using. </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TCMP2_REUSE = 0x46,
        /// <summary>
        /// <para>Table compare 3(TCMP3) compare data reuse function enable. </para> 
        /// <para>0: Disable reuse function. </para> 
        /// <para>1: Enable reuse function. </para> 
        /// <para>NOTE: Must make sure comparison is not running before using. </para> 
        /// </summary>
        [DefaultValue(0)]
        TGR_TCMP3_REUSE = 0x47,
        /// <summary>
        /// <para>Table compare  </para> 
        /// <para>2(TCMP2) data  </para> 
        /// <para>transfer done status. </para> 
        /// <para>-1: Not in using. </para> 
        /// <para>0: Transfer not finish. </para> 
        /// <para>1: Transfer finish. </para> 
        /// <para>Be attention: Transfer finish status is read clear status. Status will be “-1”  </para> 
        /// <para>-> “0”... data transfer…  </para> 
        /// <para>-> ”1” (By user program read) -> “-1”. </para> 
        /// </summary>
        [DefaultValue(-1)]
        TGR_TCMP2_TRANSFER_DONE = 0x48,
        /// <summary>
        /// <para>Table compare 2(TCMP3) data transfer done status. </para> 
        /// <para>-1: Not in using. </para> 
        /// <para>0: Transfer not finish. </para> 
        /// <para>1: Transfer finish. </para> 
        /// <para>Be attention: Transfer  </para> 
        /// <para>finish status is read clear  </para> 
        /// <para>status. Status will be “-1”  </para> 
        /// <para>-> “0”... data transfer…  </para> 
        /// <para>-> ”1” (By user program read) -> “-1”. </para> 
        /// </summary>
        [DefaultValue(-1)]
        TGR_TCMP3_TRANSFER_DONE = 0x49,


    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerParameter_MNET_4XMO_C
    {
        /// <summary>
        /// 比較源0。
        /// <para>0:指定計數器</para> 
        /// <para>1:位置計數器</para> 
        /// </summary>
        TG_CMP0_SRC = 0x00,
        /// <summary>
        /// 比較源1。
        /// <para>0:指定計數器</para> 
        /// <para>1:位置計數器</para> 
        /// </summary>
        TG_CMP1_SRC = 0x01,
        /// <summary>
        /// 比較源2。
        /// <para>0:指定計數器</para> 
        /// <para>1:位置計數器</para> 
        /// </summary>
        TG_CMP2_SRC = 0x02,
        /// <summary>
        /// 比較源3。
        /// <para>0:指定計數器</para> 
        /// <para>1:位置計數器</para> 
        /// </summary>
        TG_CMP3_SRC = 0x03,
        /// <summary>
        /// 启动比较0 。
        /// <para>0:禁用</para> 
        /// <para>1:数据 = cmp计数器（与计数方向无关）</para> 
        /// <para>2:数据 = cmp计数器（上计数）</para> 
        /// <para>3:数据 = cmp计数器（下计数）</para> 
        /// <para>4:数据 &gt; cmp计数器</para> 
        /// <para>5:数据 &lt; cmp计数器</para> 
        /// </summary>
        TG_CMP0_EN = 0x04,
        /// <summary>
        /// 启动比较1 。
        /// <para>0:禁用</para> 
        /// <para>1:数据 = cmp计数器（与计数方向无关）</para> 
        /// <para>2:数据 = cmp计数器（上计数）</para> 
        /// <para>3:数据 = cmp计数器（下计数）</para> 
        /// <para>4:数据 &gt; cmp计数器</para> 
        /// <para>5:数据 &lt; cmp计数器</para> 
        /// </summary>
        TG_CMP1_EN = 0x05,
        /// <summary>
        /// 启动比较2 。
        /// <para>0:禁用</para> 
        /// <para>1:数据 = cmp计数器（与计数方向无关）</para> 
        /// <para>2:数据 = cmp计数器（上计数）</para> 
        /// <para>3:数据 = cmp计数器（下计数）</para> 
        /// <para>4:数据 &gt; cmp计数器</para> 
        /// <para>5:数据 &lt; cmp计数器</para> 
        /// </summary>
        TG_CMP2_EN = 0x06,
        /// <summary>
        /// 启动比较3 。
        /// <para>0:禁用</para> 
        /// <para>1:数据 = cmp计数器（与计数方向无关）</para> 
        /// <para>2:数据 = cmp计数器（上计数）</para> 
        /// <para>3:数据 = cmp计数器（下计数）</para> 
        /// <para>4:数据 &gt; cmp计数器</para> 
        /// <para>5:数据 &lt; cmp计数器</para> 
        /// </summary>
        TG_CMP3_EN = 0x07,
        /// <summary>
        /// 比较0类型。
        /// <para>0: 表比较</para> 
        /// <para>1: 线性比较</para> 
        /// </summary>
        TG_CMP0_TYPE = 0x08,
        /// <summary>
        /// 比较1类型。
        /// <para>0: 表比较</para> 
        /// <para>1: 线性比较</para> 
        /// </summary>
        TG_CMP1_TYPE = 0x09,
        /// <summary>
        /// 比较2类型。
        /// <para>0: 表比较</para> 
        /// <para>1: 线性比较</para> 
        /// </summary>
        TG_CMP2_TYPE = 0x0A,
        /// <summary>
        /// 比较3类型。
        /// <para>0: 表比较</para> 
        /// <para>1: 线性比较</para> 
        /// </summary>
        TG_CMP3_TYPE = 0x0B,
        /// <summary>
        /// 启用比较H 。
        /// <para>0: 禁用</para> 
        /// <para>1: 启用</para> 
        /// </summary>
        TG_CMPH_EN = 0x0C,
        /// <summary>
        /// 比较H方向启用。
        /// <para>0: 禁用</para> 
        /// <para>1: 启用</para> 
        /// </summary>
        TG_CMPH_DIR_EN = 0x0D,
        /// <summary>
        /// 比较H方向。
        /// <para>0: 正方向</para> 
        /// <para>1: 反方向</para> 
        /// </summary>
        TG_CMPH_DIR = 0x0E,
        /// <summary>
        /// 触发输出0（TRG0）源。
        /// <para>Bit 0:CMP 0</para> 
        /// <para>Bit 1:CMP 1</para> 
        /// <para>Bit 2:CMP 2</para> 
        /// <para>Bit 3:CMP 3</para> 
        /// <para>Bit 4:CMP H</para> 
        /// <para>值: 0x00 ~ 0x1f</para> 
        /// </summary>
        TG_TRG0_SRC = 0x10,
        /// <summary>
        /// 触发输出1（TRG1）源。
        /// <para>Bit 0:CMP 0</para> 
        /// <para>Bit 1:CMP 1</para> 
        /// <para>Bit 2:CMP 2</para> 
        /// <para>Bit 3:CMP 3</para> 
        /// <para>Bit 4:CMP H</para> 
        /// <para>值: 0x00 ~ 0x1f</para> 
        /// </summary>
        TG_TRG1_SRC = 0x11,
        /// <summary>
        /// 触发输出2（TRG2）源。
        /// <para>Bit 0:CMP 0</para> 
        /// <para>Bit 1:CMP 1</para> 
        /// <para>Bit 2:CMP 2</para> 
        /// <para>Bit 3:CMP 3</para> 
        /// <para>Bit 4:CMP H</para> 
        /// <para>值: 0x00 ~ 0x1f</para> 
        /// </summary>
        TG_TRG2_SRC = 0x12,
        /// <summary>
        /// 触发输出3（TRG3）源。
        /// <para>Bit 0:CMP 0</para> 
        /// <para>Bit 1:CMP 1</para> 
        /// <para>Bit 2:CMP 2</para> 
        /// <para>Bit 3:CMP 3</para> 
        /// <para>Bit 4:CMP H</para> 
        /// <para>值: 0x00 ~ 0x1f</para> 
        /// </summary>
        TG_TRG3_SRC = 0x13,
        /// <summary>
        /// TRG0 脉冲宽度 。
        /// <para>脉冲宽度 = ( N+ 5) * 10 ns</para> 
        /// <para>值: 0x05 ~ 0x7fffffff</para> 
        /// <para>小于0x05的值视为0x05。</para> 
        /// </summary>
        TG_TRG0_PWD = 0x14,
        /// <summary>
        /// TRG1 脉冲宽度 。
        /// <para>脉冲宽度 = ( N+ 5) * 10 ns</para> 
        /// <para>值: 0x05 ~ 0x7fffffff</para> 
        /// <para>小于0x05的值视为0x05。</para> 
        /// </summary>
        TG_TRG1_PWD = 0x15,
        /// <summary>
        /// TRG2 脉冲宽度 。
        /// <para>脉冲宽度 = ( N+ 5) * 10 ns</para> 
        /// <para>值: 0x05 ~ 0x7fffffff</para> 
        /// <para>小于0x05的值视为0x05。</para> 
        /// </summary>
        TG_TRG2_PWD = 0x16,
        /// <summary>
        /// TRG3 脉冲宽度 。
        /// <para>脉冲宽度 = ( N+ 5) * 10 ns</para> 
        /// <para>值: 0x05 ~ 0x7fffffff</para> 
        /// <para>小于0x05的值视为0x05。</para> 
        /// </summary>
        TG_TRG3_PWD = 0x17,
        /// <summary>
        /// TRG 0 配置。
        /// <para>Bit 0: 脉冲逻辑逆。不逆(0) / 逆 (1) </para> 
        /// <para>Bit 1~2: 脉冲 (0) / 切换(toggle)(1) / ByPass (2) / 禁用(3)</para> 
        /// <para>Bit 3~31: 预留 （设为0）</para>
        /// </summary>
        TG_TRG0_CFG = 0x18,
        /// <summary>
        /// TRG 1 配置。
        /// <para>Bit 0: 脉冲逻辑逆。不逆(0) / 逆 (1) </para> 
        /// <para>Bit 1~2: 脉冲 (0) / 切换(toggle)(1) / ByPass (2) / 禁用(3)</para> 
        /// <para>Bit 3~31: 预留 （设为0）</para>
        /// </summary>
        TG_TRG1_CFG = 0x19,
        /// <summary>
        /// TRG 2 配置。
        /// <para>Bit 0: 脉冲逻辑逆。不逆(0) / 逆 (1) </para> 
        /// <para>Bit 1~2: 脉冲 (0) / 切换(toggle)(1) / ByPass (2) / 禁用(3)</para> 
        /// <para>Bit 3~31: 预留 （设为0）</para>
        /// </summary>
        TG_TRG2_CFG = 0x1A,
        /// <summary>
        /// TRG 3 配置。
        /// <para>Bit 0: 脉冲逻辑逆。不逆(0) / 逆 (1) </para> 
        /// <para>Bit 1~2: 脉冲 (0) / 切换(toggle)(1) / ByPass (2) / 禁用(3)</para> 
        /// <para>Bit 3~31: 预留 （设为0）</para>
        /// </summary>
        TG_TRG3_CFG = 0x1B,
        /// <summary>
        /// Encoder H 配置。
        /// <para>Bit 0: 启用过滤器. 1:启用, 0: 禁用.</para> 
        /// <para>Bit 1: 计数器方向逆。0：不逆，1：逆</para> 
        /// <para>Bit 2~4: 解码器模式。</para> 
        /// <para>0x00: OUT/DIR</para> 
        /// <para>0x01: CW/CCW</para> 
        /// <para>0x02: 1XAB</para> 
        /// <para>0x03: 2XAB</para> 
        /// <para>0x04: 4XAB</para> 
        /// </summary>
        TG_ENCH_CFG = 0x20,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerParameter_PCI_and_AMP
    {
        /// <summary>
        ///  线性比较 0（LCMP0）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9：禁用</para>
        /// </summary>
        TGR_LCMP0_SRC = 0x00,
        /// <summary>
        ///  线性比较 1（LCMP1）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9：禁用</para>
        /// </summary>
        TGR_LCMP1_SRC = 0x01,
        /// <summary>
        /// 表比较 0（TCMP0）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9：禁用</para>
        /// </summary>
        TGR_TCMP0_SRC = 0x02,
        /// <summary>
        /// 表比较 1（TCMP1）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9：禁用</para>
        /// </summary>
        TGR_TCMP1_SRC = 0x03,
        /// <summary>
        /// 表比较 0（TCMP0）方向
        /// <para>0：反方向</para>
        /// <para>1：正方向</para>
        /// <para>双向（无方向）</para>
        /// </summary>
        TGR_TCMP0_DIR = 0x04,
        /// <summary>
        /// 表比较 1（TCMP1）方向
        /// <para>0：反方向</para>
        /// <para>1：正方向</para>
        /// <para>双向（无方向）</para>
        /// </summary>
        TGR_TCMP1_DIR = 0x05,
        /// <summary>
        /// TRG 0〜3 按位启用注意：此参数也由板载参数“ PWMx mapDO”和“ VAOtable”函数控制。
        /// <para>Bit x: ( 0: 禁用, 1: 启用)</para>
        /// <para>Bit 0:TRG0 启用</para>
        /// <para>Bit 1:TRG1 启用</para>
        /// <para>Bit 2:TRG2 启用</para>
        /// <para>Bit 3:TRG3 启用</para>
        /// </summary>
        TGR_TRG_EN = 0x06,
        /// <summary>
        /// 触发输出 0（TRG0）源注意：“或”多源，然后输出到 TRG0）
        /// <para>Bit x:( 1: 开, 0: 关 )</para>
        /// <para>Bit 0:Manual0</para>
        /// <para>Bit 1:预留</para>
        /// <para>Bit 2:FCMP0</para>
        /// <para>Bit 3:FCMP1</para>
        /// <para>Bit 4:LCMP0</para>
        /// <para>Bit 5:LCMP1</para>
        /// <para>Bit 6:MCMP</para>
        /// <para>Bit 7:FCMP2</para>
        /// <para>Bit 8:FCMP3</para>
        /// <para>Bit 9 LCMP2</para>
        /// <para>Bit 10 LCMP3</para>
        /// </summary>
        TGR_TRG0_SRC = 0x10,
        /// <summary>
        /// 触发输出 1（TRG1）源注意：“或”多源，然后输出到 TRG0）
        /// <para>Bit x:( 1: 开, 0: 关 )</para>
        /// <para>Bit 0:Manual0</para>
        /// <para>Bit 1:预留</para>
        /// <para>Bit 2:FCMP0</para>
        /// <para>Bit 3:FCMP1</para>
        /// <para>Bit 4:LCMP0</para>
        /// <para>Bit 5:LCMP1</para>
        /// <para>Bit 6:MCMP</para>
        /// <para>Bit 7:FCMP2</para>
        /// <para>Bit 8:FCMP3</para>
        /// <para>Bit 9 LCMP2</para>
        /// <para>Bit 10 LCMP3</para>
        /// </summary>
        TGR_TRG1_SRC = 0x11,
        /// <summary>
        /// 触发输出 2（TRG2）源注意：“或”多源，然后输出到 TRG0）
        /// <para>Bit x:( 1: 开, 0: 关 )</para>
        /// <para>Bit 0:Manual0</para>
        /// <para>Bit 1:预留</para>
        /// <para>Bit 2:FCMP0</para>
        /// <para>Bit 3:FCMP1</para>
        /// <para>Bit 4:LCMP0</para>
        /// <para>Bit 5:LCMP1</para>
        /// <para>Bit 6:MCMP</para>
        /// <para>Bit 7:FCMP2</para>
        /// <para>Bit 8:FCMP3</para>
        /// <para>Bit 9 LCMP2</para>
        /// <para>Bit 10 LCMP3</para>
        /// </summary>
        TGR_TRG2_SRC = 0x12,
        /// <summary>
        /// 触发输出 3（TRG3）源注意：“或”多源，然后输出到 TRG0）
        /// <para>Bit x:( 1: 开, 0: 关 )</para>
        /// <para>Bit 0:Manual0</para>
        /// <para>Bit 1:预留</para>
        /// <para>Bit 2:FCMP0</para>
        /// <para>Bit 3:FCMP1</para>
        /// <para>Bit 4:LCMP0</para>
        /// <para>Bit 5:LCMP1</para>
        /// <para>Bit 6:MCMP</para>
        /// <para>Bit 7:FCMP2</para>
        /// <para>Bit 8:FCMP3</para>
        /// <para>Bit 9 LCMP2</para>
        /// <para>Bit 10 LCMP3</para>
        /// </summary>
        TGR_TRG3_SRC = 0x13,
        /// <summary>
        /// TRG0 脉冲宽度
        /// <para>脉冲宽度 = (N-1)*20ns</para>
        /// <para>N = 2 ~ 0xffffff</para>
        /// </summary>
        TGR_TRG0_PWD = 0x14,
        /// <summary>
        /// TRG1 脉冲宽度
        /// <para>脉冲宽度 = (N-1)*20ns</para>
        /// <para>N = 2 ~ 0xffffff</para>
        /// </summary>
        TGR_TRG1_PWD = 0x15,
        /// <summary>
        /// TRG2 脉冲宽度
        /// <para>脉冲宽度 = (N-1)*20ns</para>
        /// <para>N = 2 ~ 0xffffff</para>
        /// </summary>
        TGR_TRG2_PWD = 0x16,
        /// <summary>
        /// TRG3 脉冲宽度
        /// <para>脉冲宽度 = (N-1)*20ns</para>
        /// <para>N = 2 ~ 0xffffff</para>
        /// </summary>
        TGR_TRG3_PWD = 0x17,
        /// <summary>
        /// TRG 0逻辑
        /// <para> 0:不逆</para>
        /// <para> 1:逆</para>
        /// </summary>
        TGR_TRG0_LOGIC = 0x18,
        /// <summary>
        /// TRG 1逻辑
        /// <para> 0:不逆</para>
        /// <para> 1:逆</para>
        /// </summary>
        TGR_TRG1_LOGIC = 0x19,
        /// <summary>
        /// TRG 2逻辑
        /// <para> 0:不逆</para>
        /// <para> 1:逆</para>
        /// </summary>
        TGR_TRG2_LOGIC = 0x1A,
        /// <summary>
        /// TRG 3逻辑
        /// <para> 0:不逆</para>
        /// <para> 1:逆</para>
        /// </summary>
        TGR_TRG3_LOGIC = 0x1B,
        /// <summary>
        ///  TRG 0 toggle 模式
        /// <para>0：脉冲输出</para>
        /// <para>1：Toggle 输出</para>
        /// </summary>
        TGR_TRG0_TGL = 0x1C,
        /// <summary>
        ///  TRG 1 toggle 模式
        /// <para>0：脉冲输出</para>
        /// <para>1：Toggle 输出</para>
        /// </summary>
        TGR_TRG1_TGL = 0x1D,
        /// <summary>
        ///  TRG 2 toggle 模式
        /// <para>0：脉冲输出</para>
        /// <para>1：Toggle 输出</para>
        /// </summary>
        TGR_TRG2_TGL = 0x1E,
        /// <summary>
        ///  TRG 3 toggle 模式
        /// <para>0：脉冲输出</para>
        /// <para>1：Toggle 输出</para>
        /// </summary>
        TGR_TRG3_TGL = 0x1F,
        /// <summary>
        /// 计时器间隔
        /// <para>计时器间隔 = N * 100ns</para>
        /// <para>N = 1~ 536870911(100 ns) </para>
        /// </summary>
        TIMR_ITV = 0x20,
        /// <summary>
        /// 计时器方向
        /// <para>0：正方向数</para>
        /// <para>1：反方向计数</para>
        /// </summary>
        TIMR_DIR = 0x21,
        /// <summary>
        /// 使定时器计数器成为环计数器
        /// <para>0: 禁用 (0x7fffffff+1 -> 0x80000000) (0x80000001-1 -> 0x80000000)</para>
        /// <para>1: 启用 (0x7fffffff+1 -> 0x00000000)(0x00000000-1 -> 0x7fffffff)</para>
        /// </summary>
        TIMR_RING_EN = 0x22,
        /// <summary>
        /// 启用定时器
        /// <para>0：禁用</para>
        /// <para>1：启用</para>
        /// </summary>
        TIMR_EN = 0x23,
        /// <summary>
        /// 多轴比较器 0（MCMP0）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_MCMP0_SRC = 0x30,
        /// <summary>
        /// 多轴比较器 1（MCMP1）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_MCMP1_SRC = 0x31,
        /// <summary>
        /// 多轴比较器 2（MCMP2）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_MCMP2_SRC = 0x32,
        /// <summary>
        /// 多轴比较器 3（MCMP3）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_MCMP3_SRC = 0x33,
        /// <summary>
        /// 决定多轴比较器何时可以触发 PWM 的模式
        /// <para>0：原始模式：当电机到达窗口定义的边界时</para>
        /// <para>1：精确模式：当电动机处于窗口定义的边界内并且相对于比较点的位置偏差最短时</para>
        /// </summary>
        TGR_MCMP_MODE = 0x34,
        /// <summary>
        /// 启用此模式可允许用户设置触发输出引脚的状态。 注意：此模式将影响其他 FPGA 模块，例如“比较”触发器。
        /// <para>0: 禁用</para>
        /// <para>1: 启用</para>
        /// </summary>
        TGR_TRG0_TOGGLE_MODE = 0x35,
        /// <summary>
        /// 启用此模式可允许用户设置触发输出引脚的状态。 注意：此模式将影响其他 FPGA 模块，例如“比较”触发器。
        /// <para>0: 禁用</para>
        /// <para>1: 启用</para>
        /// </summary>
        TGR_TRG1_TOGGLE_MODE = 0x36,
        /// <summary>
        /// 启用此模式可允许用户设置触发输出引脚的状态。 注意：此模式将影响其他 FPGA 模块，例如“比较”触发器。
        /// <para>0: 禁用</para>
        /// <para>1: 启用</para>
        /// </summary>
        TGR_TRG2_TOGGLE_MODE = 0x37,
        /// <summary>
        /// 启用此模式可允许用户设置触发输出引脚的状态。 注意：此模式将影响其他 FPGA 模块，例如“比较”触发器。
        /// <para>0: 禁用</para>
        /// <para>1: 启用</para>
        /// </summary>
        TGR_TRG3_TOGGLE_MODE = 0x38,
        /// <summary>
        /// 写入和读取切换输出状态
        /// <para>0：输出低电平</para>
        /// <para>1：输出高电平</para>
        /// </summary>
        TGR_TRG0_TOGGLE_STATUS = 0x39,
        /// <summary>
        /// 写入和读取切换输出状态
        /// <para>0：输出低电平</para>
        /// <para>1：输出高电平</para>
        /// </summary>
        TGR_TRG1_TOGGLE_STATUS = 0x3A,
        /// <summary>
        /// 写入和读取切换输出状态
        /// <para>0：输出低电平</para>
        /// <para>1：输出高电平</para>
        /// </summary>
        TGR_TRG2_TOGGLE_STATUS = 0x3B,
        /// <summary>
        /// 写入和读取切换输出状态
        /// <para>0：输出低电平</para>
        /// <para>1：输出高电平</para>
        /// </summary>
        TGR_TRG3_TOGGLE_STATUS = 0x3C,
        /// <summary>
        /// 表比较 2（TCMP2）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_TCMP2_SRC = 0x40,
        /// <summary>
        /// 表比较 3（TCMP3）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_TCMP3_SRC = 0x41,
        /// <summary>
        /// 表比较 2（TCMP2）方向
        /// <para>0：反方向</para>
        /// <para>1：正方向</para>
        /// <para>2：双向（无方向）</para>
        /// </summary>
        TGR_TCMP2_DIR = 0x42,
        /// <summary>
        /// 表比较 3（TCMP3）方向
        /// <para>0：反方向</para>
        /// <para>1：正方向</para>
        /// <para>2：双向（无方向）</para>
        /// </summary>
        TGR_TCMP3_DIR = 0x43,
        /// <summary>
        /// 线性比较 2（LCMP2）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_LCMP2_SRC = 0x44,
        /// <summary>
        /// 线性比较 3（LCMP3）源
        /// <para>0〜7：编码器计数器</para>
        /// <para>8：定时器 0 计数器</para>
        /// <para>9: 禁用</para>
        /// </summary>
        TGR_LCMP3_SRC = 0x45,
    }

    //==============================
    public class IntervalMoveEventArgs : EventArgs
    {
        public IntervalMoveEventArgs(int currentStep)
        {
            CurrentStep = currentStep;
        }
        public int CurrentStep { get; }
    }


    public class ModelAttribute : Attribute
    {
        public ADLINK_DEVICE.DeviceName[] Models { get; }
        public ModelAttribute(params ADLINK_DEVICE.DeviceName[] models)
        {
            this.Models = models;
        }
    }

}