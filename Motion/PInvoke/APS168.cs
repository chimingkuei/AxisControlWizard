#define x64
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace ADLINK_DEVICE
{
    public static class APS168
    {
#if x64
        const string _dll = "APS168x64.dll";
#else
        const string _dll = "APS168.dll";
#endif
        /// <summary>
        /// Start torque operation
        /// </summary>
        [DllImport(_dll)] public static extern int APS_torque_move(int AxisID, short TorqueValue, ulong Slope, ushort Option, ref ASYNCALL Wait);
        /// <summary>
        /// Get command torque value back
        /// </summary>
        [DllImport(_dll)] public static extern int APS_get_torque_command(int AxisID, out int cmd);
        /// <summary>
        /// Set the command control mode of axis
        /// </summary>
        [DllImport(_dll)] public static extern int APS_set_command_control_mode(int AxisID, byte mode);
        /// <summary>
        /// Get the command control mode of axis
        /// </summary>
        [DllImport(_dll)] public static extern int APS_get_command_control_mode(int AxisID, out byte mode);

        // System & Initialization
        [DllImport(_dll)] public static extern int APS_initial(ref int BoardID_InBits, int Mode);
        [DllImport(_dll)] public static extern int APS_close();
        [DllImport(_dll)] public static extern int APS_version();
        [DllImport(_dll)] public static extern int APS_device_driver_version(int Board_ID);
        [DllImport(_dll)] public static extern int APS_get_axis_info(int Axis_ID, ref int Board_ID, ref int Axis_No, ref int Port_ID, ref int Module_ID);
        [DllImport(_dll)] public static extern int APS_set_board_param(int Board_ID, int BOD_Param_No, int BOD_Param);
        [DllImport(_dll)] public static extern int APS_get_board_param(int Board_ID, int BOD_Param_No, ref int BOD_Param);
        [DllImport(_dll)] public static extern int APS_set_axis_param(int Axis_ID, int AXS_Param_No, int AXS_Param);
        [DllImport(_dll)] public static extern int APS_get_axis_param(int Axis_ID, int AXS_Param_No, ref int AXS_Param);
        [DllImport(_dll)] public static extern int APS_get_device_info(int Board_ID, int Info_No, ref int Info);
        [DllImport(_dll)] public static extern int APS_get_card_name(int Board_ID, out DeviceName CardName);
        [DllImport(_dll)] public static extern int APS_disable_device(int DeviceName);
        [DllImport(_dll)] public static extern int APS_load_param_from_file(string pXMLFile);
        [DllImport(_dll)] public static extern int APS_get_first_axisId(int Board_ID, ref int StartAxisID, ref int TotalAxisNum);
        [DllImport(_dll)] public static extern int APS_get_system_timer(int Board_ID, ref int Timer);
        [DllImport(_dll)] public static extern int APS_get_system_loading(int Board_ID, ref double Loading1, ref double Loading2, ref double Loading3, ref double Loading4);
        [DllImport(_dll)] public static extern int APS_set_security_key(int Board_ID, int OldPassword, int NewPassword);
        [DllImport(_dll)] public static extern int APS_check_security_key(int Board_ID, int Password);
        [DllImport(_dll)] public static extern int APS_reset_security_key(int Board_ID);

        //Control driver mode [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_get_curr_sys_ctrl_mode(int Axis_ID, ref int Mode);

        //Virtual board settings [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_register_virtual_board(int VirCardIndex, int Count);
        [DllImport(_dll)] public static extern int APS_get_virtual_board_info(int VirCardIndex, ref int Count);

        //Parameters setting by float [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_set_axis_param_f(int Axis_ID, int AXS_Param_No, double AXS_Param);
        [DllImport(_dll)] public static extern int APS_get_axis_param_f(int Axis_ID, int AXS_Param_No, ref double AXS_Param);

        //[For PCI-7856, MNET series]
        [DllImport(_dll)] public static extern int APS_save_param_to_file(int Board_ID, string pXMLFile);

        //Motion queue status [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_get_mq_free_space(int Axis_ID, ref int Space);
        [DllImport(_dll)] public static extern int APS_get_mq_usage(int Axis_ID, ref int Usage);

        //Motion stop code [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_get_stop_code(int Axis_ID, ref int Code);

        //Helical interpolation [For PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_absolute_helix_move(int Dimension, ref int Axis_ID_Array, ref int Center_Pos_Array, int Max_Arc_Speed, int Pitch, int TotalHeight, int CwOrCcw);
        [DllImport(_dll)] public static extern int APS_relative_helix_move(int Dimension, ref int Axis_ID_Array, ref int Center_PosOffset_Array, int Max_Arc_Speed, int Pitch, int TotalHeight, int CwOrCcw);

        //Helical interpolation [For PCI(e)-8154/58]
        [DllImport(_dll)] public static extern int APS_absolute_helical_move(int[] Axis_ID_Array, int[] Center_Pos_Array, int[] End_Pos_Array, int Pitch, int Dir, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_relative_helical_move(int[] Axis_ID_Array, int[] Center_Offset_Array, int[] End_Offset_Array, int Pitch, int Dir, int Max_Speed);

        //Circular interpolation( Support 2D and 3D ) [For PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_absolute_arc_move_3pe(int Dimension, ref int Axis_ID_Array, ref int Pass_Pos_Array, ref int End_Pos_Array, int Max_Arc_Speed);
        [DllImport(_dll)] public static extern int APS_relative_arc_move_3pe(int Dimension, ref int Axis_ID_Array, ref int Pass_PosOffset_Array, ref int End_PosOffset_Array, int Max_Arc_Speed);

        //Field bus motion interrupt [For PCI-7856, MNET series]
        [DllImport(_dll)] public static extern int APS_set_field_bus_int_factor_motion(int Axis_ID, int Factor_No, int Enable);
        [DllImport(_dll)] public static extern int APS_get_field_bus_int_factor_motion(int Axis_ID, int Factor_No, ref int Enable);
        [DllImport(_dll)] public static extern int APS_set_field_bus_int_factor_error(int Axis_ID, int Factor_No, int Enable);
        [DllImport(_dll)] public static extern int APS_get_field_bus_int_factor_error(int Axis_ID, int Factor_No, ref int Enable);
        [DllImport(_dll)] public static extern int APS_reset_field_bus_int_motion(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_wait_field_bus_error_int_motion(int Axis_ID, int Time_Out);

        [DllImport(_dll)] public static extern int APS_set_field_bus_int_factor_di(int Board_ID, int BUS_No, int MOD_No, int bitsOfCheck);
        [DllImport(_dll)] public static extern int APS_get_field_bus_int_factor_di(int Board_ID, int BUS_No, int MOD_No, ref int bitsOfCheck);

        //Flash functions [For PCI-8253/56, PCI-8392(H)]
        [DllImport(_dll)] public static extern int APS_save_parameter_to_flash(int Board_ID);
        [DllImport(_dll)] public static extern int APS_load_parameter_from_flash(int Board_ID);
        [DllImport(_dll)] public static extern int APS_load_parameter_from_default(int Board_ID);

        //SSCNET-3 functions [For PCI-8392(H)] 
        [DllImport(_dll)] public static extern int APS_start_sscnet(int Board_ID, ref int AxisFound_InBits);
        [DllImport(_dll)] public static extern int APS_stop_sscnet(int Board_ID);
        [DllImport(_dll)] public static extern int APS_get_sscnet_servo_param(int Axis_ID, int Para_No1, ref int Para_Dat1, int Para_No2, ref int Para_Dat2);
        [DllImport(_dll)] public static extern int APS_set_sscnet_servo_param(int Axis_ID, int Para_No1, int Para_Dat1, int Para_No2, int Para_Dat2);
        [DllImport(_dll)] public static extern int APS_get_sscnet_servo_alarm(int Axis_ID, ref int Alarm_No, ref int Alarm_Detail);
        [DllImport(_dll)] public static extern int APS_reset_sscnet_servo_alarm(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_save_sscnet_servo_param(int Board_ID);
        [DllImport(_dll)] public static extern int APS_get_sscnet_servo_abs_position(int Axis_ID, ref int Cyc_Cnt, ref int Res_Cnt);
        [DllImport(_dll)] public static extern int APS_save_sscnet_servo_abs_position(int Board_ID);
        [DllImport(_dll)] public static extern int APS_load_sscnet_servo_abs_position(int Axis_ID, int Abs_Option, ref int Cyc_Cnt, ref int Res_Cnt);
        [DllImport(_dll)] public static extern int APS_get_sscnet_link_status(int Board_ID, ref int Link_Status);
        [DllImport(_dll)] public static extern int APS_set_sscnet_servo_monitor_src(int Axis_ID, int Mon_No, int Mon_Src);
        [DllImport(_dll)] public static extern int APS_get_sscnet_servo_monitor_src(int Axis_ID, int Mon_No, ref int Mon_Src);
        [DllImport(_dll)] public static extern int APS_get_sscnet_servo_monitor_data(int Axis_ID, int Arr_Size, ref int Data_Arr);
        [DllImport(_dll)] public static extern int APS_set_sscnet_control_mode(int Axis_ID, int Mode);
        [DllImport(_dll)] public static extern int APS_set_sscnet_abs_enable(int Board_ID, int Option);
        [DllImport(_dll)] public static extern int APS_set_sscnet_abs_enable_by_axis(int Axis_ID, int Option);

        //Motion IO & motion status functions
        [DllImport(_dll)] public static extern int APS_motion_status(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_motion_io_status(int Axis_ID);

        //Monitor functions
        [DllImport(_dll)] public static extern int APS_get_command(int Axis_ID, out int Command);
        [DllImport(_dll)] public static extern int APS_set_command(int Axis_ID, int Command);
        [DllImport(_dll)] public static extern int APS_set_servo_on(int Axis_ID, int Servo_On);
        //[DllImport(_dll)] public static extern int APS_get_position(int Axis_ID, ref int Position);
        [DllImport(_dll)] public static extern int APS_get_position(int Axis_ID, out int Position);
        [DllImport(_dll)] public static extern int APS_set_position(int Axis_ID, int Position);
        [DllImport(_dll)] public static extern int APS_get_command_velocity(int Axis_ID, out int Velocity);
        [DllImport(_dll)] public static extern int APS_get_feedback_velocity(int Axis_ID, out int Velocity);
        [DllImport(_dll)] public static extern int APS_get_error_position(int Axis_ID, out int Err_Pos);
        [DllImport(_dll)] public static extern int APS_get_target_position(int Axis_ID, out int Targ_Pos);
        [DllImport(_dll)] public static extern int APS_get_command_f(int Axis_ID, out double Command);
        [DllImport(_dll)] public static extern int APS_set_command_f(int Axis_ID, double Command);
        [DllImport(_dll)] public static extern int APS_get_position_f(int Axis_ID, out double Position);
        [DllImport(_dll)] public static extern int APS_set_position_f(int Axis_ID, double Position);
        [DllImport(_dll)] public static extern int APS_get_command_velocity_f(int Axis_ID, out double Velocity);
        [DllImport(_dll)] public static extern int APS_get_target_position_f(int Axis_ID, out double Targ_Pos);
        [DllImport(_dll)] public static extern int APS_get_error_position_f(int Axis_ID, out double Err_Pos);
        [DllImport(_dll)] public static extern int APS_get_feedback_velocity_f(int Axis_ID, out double Velocity);

        // Single axis motion
        [DllImport(_dll)] public static extern int APS_relative_move(int Axis_ID, int Distance, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_absolute_move(int Axis_ID, int Position, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_velocity_move(int Axis_ID, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_home_move(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_stop_move(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_emg_stop(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_relative_move2(int Axis_ID, int Distance, int Start_Speed, int Max_Speed, int End_Speed, int Acc_Rate, int Dec_Rate);
        [DllImport(_dll)] public static extern int APS_absolute_move2(int Axis_ID, int Position, int Start_Speed, int Max_Speed, int End_Speed, int Acc_Rate, int Dec_Rate);
        [DllImport(_dll)] public static extern int APS_home_move2(int Axis_ID, int Dir, int Acc, int Start_Speed, int Max_Speed, int ORG_Speed);
        [DllImport(_dll)] public static extern int APS_home_escape(int Axis_ID);

        //JOG functions [For PCI-8392(H), PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_set_jog_param(int Axis_ID, ref JOG_DATA pStr_Jog, int Mask);
        [DllImport(_dll)] public static extern int APS_get_jog_param(int Axis_ID, ref JOG_DATA pStr_Jog);
        [DllImport(_dll)] public static extern int APS_jog_mode_switch(int Axis_ID, int Turn_No);
        [DllImport(_dll)] public static extern int APS_jog_start(int Axis_ID, int STA_On);

        // Interpolation
        [DllImport(_dll)] public static extern int APS_absolute_linear_move(int Dimension, int[] Axis_ID_Array, int[] Position_Array, int Max_Linear_Speed);
        [DllImport(_dll)] public static extern int APS_relative_linear_move(int Dimension, int[] Axis_ID_Array, int[] Distance_Array, int Max_Linear_Speed);
        [DllImport(_dll)] public static extern int APS_absolute_arc_move(int Dimension, int[] Axis_ID_Array, int[] Center_Pos_Array, int Max_Arc_Speed, int Angle);
        [DllImport(_dll)] public static extern int APS_relative_arc_move(int Dimension, int[] Axis_ID_Array, int[] Center_Offset_Array, int Max_Arc_Speed, int Angle);
        [DllImport(_dll)] public static extern int APS_absolute_arc_move_f(int Dimension, int[] Axis_ID_Array, int[] Center_Pos_Array, int Max_Arc_Speed, double Angle);
        [DllImport(_dll)] public static extern int APS_relative_arc_move_f(int Dimension, int[] Axis_ID_Array, int[] Center_Offset_Array, int Max_Arc_Speed, double Angle);

        // Interrupt functions
        [DllImport(_dll)] public static extern int APS_int_enable(int Board_ID, int Enable);
        [DllImport(_dll)] public static extern int APS_set_int_factor(int Board_ID, int Item_No, int Factor_No, int Enable);
        [DllImport(_dll)] public static extern int APS_get_int_factor(int Board_ID, int Item_No, int Factor_No, ref int Enable);

        [DllImport(_dll)] public static extern int APS_set_int_factorH(int Board_ID, int Item_No, int Factor_No, int Enable);
        [DllImport(_dll)] public static extern int APS_int_no_to_handle(int Int_No);

        [DllImport(_dll)] public static extern int APS_wait_single_int(int Int_No, int Time_Out);
        [DllImport(_dll)] public static extern int APS_wait_multiple_int(int Int_Count, int[] Int_No_Array, int Wait_All, int Time_Out);
        [DllImport(_dll)] public static extern int APS_reset_int(int Int_No);
        [DllImport(_dll)] public static extern int APS_set_int(int Int_No);
        [DllImport(_dll)] public static extern int APS_wait_error_int(int Board_ID, int Item_No, int Time_Out);


        //Sampling functions [For PCI-8392(H), PCI-8253/56, PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_set_sampling_param(int Board_ID, int ParaNum, int ParaDat);
        [DllImport(_dll)] public static extern int APS_get_sampling_param(int Board_ID, int ParaNum, ref int ParaDat);
        [DllImport(_dll)] public static extern int APS_wait_trigger_sampling(int Board_ID, int Length, int PreTrgLen, int TimeOutMs, ref STR_SAMP_DATA_4CH DataArr);
        [DllImport(_dll)] public static extern int APS_wait_trigger_sampling_async(int Board_ID, int Length, int PreTrgLen, int TimeOutMs, ref STR_SAMP_DATA_4CH DataArr);
        [DllImport(_dll)] public static extern int APS_get_sampling_count(int Board_ID, ref int SampCnt);
        [DllImport(_dll)] public static extern int APS_stop_wait_sampling(int Board_ID);
        [DllImport(_dll)] public static extern int APS_auto_sampling(int Board_ID, int StartStop);
        [DllImport(_dll)] public static extern int APS_get_sampling_data(int Board_ID, ref int Length, ref STR_SAMP_DATA_4CH DataArr, ref int Status);

        //Sampling functions extension [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_set_sampling_param_ex(int Board_ID, ref SAMP_PARAM Param);
        [DllImport(_dll)] public static extern int APS_get_sampling_param_ex(int Board_ID, ref SAMP_PARAM Param);
        [DllImport(_dll)] public static extern int APS_wait_trigger_sampling_ex(int Board_ID, int Length, int PreTrgLen, int TimeOutMs, ref STR_SAMP_DATA_8CH DataArr);
        [DllImport(_dll)] public static extern int APS_wait_trigger_sampling_async_ex(int Board_ID, int Length, int PreTrgLen, int TimeOutMs, ref STR_SAMP_DATA_8CH DataArr);
        [DllImport(_dll)] public static extern int APS_get_sampling_data_ex(int Board_ID, ref int Length, ref STR_SAMP_DATA_8CH DataArr, ref int Status);

        //DIO & AIO functions
        [DllImport(_dll)] public static extern int APS_write_d_output(int Board_ID, int DO_Group, int DO_Data);
        [DllImport(_dll)] public static extern int APS_read_d_output(int Board_ID, int DO_Group, ref int DO_Data);
        [DllImport(_dll)] public static extern int APS_read_d_input(int Board_ID, int DI_Group, out int DI_Data);

        [DllImport(_dll)] public static extern int APS_write_d_channel_output(int Board_ID, int DO_Group, int Ch_No, int DO_Data);
        [DllImport(_dll)] public static extern int APS_read_d_channel_output(int Board_ID, int DO_Group, int Ch_No, ref int DO_Data);
        [DllImport(_dll)] public static extern int APS_read_d_channel_input(int Board_ID, int DI_Group, int Ch_No, out int DI_Data);

        [DllImport(_dll)] public static extern int APS_read_a_input_value(int Board_ID, int Channel_No, ref double Convert_Data);
        [DllImport(_dll)] public static extern int APS_read_a_input_data(int Board_ID, int Channel_No, ref int Raw_Data);
        [DllImport(_dll)] public static extern int APS_write_a_output_value(int Board_ID, int Channel_No, double Convert_Data);
        [DllImport(_dll)] public static extern int APS_write_a_output_data(int Board_ID, int Channel_No, int Raw_Data);
        //AIO [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_read_a_output_value(int Board_ID, int Channel_No, ref double Convert_Data);

        //Point table move functions [For PCI-8253/56, PCI-8392(H)]
        [DllImport(_dll)] public static extern int APS_set_point_table(int Axis_ID, int Index, ref POINT_DATA Point);
        [DllImport(_dll)] public static extern int APS_get_point_table(int Axis_ID, int Index, ref POINT_DATA Point);
        [DllImport(_dll)] public static extern int APS_get_running_point_index(int Axis_ID, ref int Index);
        [DllImport(_dll)] public static extern int APS_get_start_point_index(int Axis_ID, ref int Index);
        [DllImport(_dll)] public static extern int APS_get_end_point_index(int Axis_ID, ref int Index);
        [DllImport(_dll)] public static extern int APS_set_table_move_pause(int Axis_ID, int Pause_en);
        [DllImport(_dll)] public static extern int APS_set_table_move_repeat(int Axis_ID, int Repeat_en);
        [DllImport(_dll)] public static extern int APS_get_table_move_repeat_count(int Axis_ID, ref int RepeatCnt);
        [DllImport(_dll)] public static extern int APS_point_table_move(int Dimension, ref int Axis_ID_Array, int StartIndex, int EndIndex);
        [DllImport(_dll)] public static extern int APS_set_point_tableEx(int Axis_ID, int Index, ref PNT_DATA Point);
        [DllImport(_dll)] public static extern int APS_set_point_tableEx_2D(int Axis_ID, int Axis_ID_2, int Index, ref PNT_DATA_2D Point);
        [DllImport(_dll)] public static extern int APS_set_point_table_4DL(ref int Axis_ID_Array, int Index, ref PNT_DATA_4DL Point);

        //Point table + IO - Pause / Resume [For PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_set_table_move_ex_pause(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_set_table_move_ex_rollback(int Axis_ID, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_set_table_move_ex_resume(int Axis_ID);

        //Point table with extend option [For PCI-8392(H)]
        [DllImport(_dll)] public static extern int APS_set_point_table_ex(int Axis_ID, int Index, ref POINT_DATA_EX Point);
        [DllImport(_dll)] public static extern int APS_get_point_table_ex(int Axis_ID, int Index, ref POINT_DATA_EX Point);

        //Point table Feeder [For PCI-8253/56, PCI-8392(H)]
        [DllImport(_dll)] public static extern int APS_set_feeder_group(int GroupId, int Dimension, ref int Axis_ID_Array);
        [DllImport(_dll)] public static extern int APS_get_feeder_group(int GroupId, ref int Dimension, ref int Axis_ID_Array);
        [DllImport(_dll)] public static extern int APS_free_feeder_group(int GroupId);
        [DllImport(_dll)] public static extern int APS_reset_feeder_buffer(int GroupId);
        [DllImport(_dll)] public static extern int APS_set_feeder_point_2D(int GroupId, ref PNT_DATA_2D PtArray, int Size, int LastFlag);
        [DllImport(_dll)] public static extern int APS_set_feeder_point_2D_ex(int GroupId, IntPtr PtArray, int Size, int LastFlag);
        [DllImport(_dll)] public static extern int APS_start_feeder_move(int GroupId);
        [DllImport(_dll)] public static extern int APS_get_feeder_status(int GroupId, ref int State, ref int ErrCode);
        [DllImport(_dll)] public static extern int APS_get_feeder_running_index(int GroupId, ref int Index);
        [DllImport(_dll)] public static extern int APS_get_feeder_feed_index(int GroupId, ref int Index);
        [DllImport(_dll)] public static extern int APS_set_feeder_ex_pause(int GroupId);
        [DllImport(_dll)] public static extern int APS_set_feeder_ex_rollback(int GroupId, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_set_feeder_ex_resume(int GroupId);
        [DllImport(_dll)] public static extern int APS_set_feeder_cfg_acc_type(int GroupId, int Type);

        //Point table functions [For MNET-4XMO-C]
        [DllImport(_dll)] public static extern int APS_set_point_table_mode2(int Axis_ID, int Mode);
        [DllImport(_dll)] public static extern int APS_set_point_table2(int Dimension, ref int Axis_ID_Array, int Index, ref POINT_DATA2 Point);
        [DllImport(_dll)] public static extern int APS_point_table_continuous_move2(int Dimension, ref int Axis_ID_Array);
        [DllImport(_dll)] public static extern int APS_point_table_single_move2(int Axis_ID, int Index);
        [DllImport(_dll)] public static extern int APS_get_running_point_index2(int Axis_ID, ref int Index);
        [DllImport(_dll)] public static extern int APS_point_table_status2(int Axis_ID, ref int Status);
        [DllImport(_dll)] public static extern int APS_point_table_setting_continuous_move2(int Dimension, ref int Axis_ID_Array, int TotalPoints, ref POINT_DATA2 Point);
        [DllImport(_dll)] public static extern int APS_set_point_table2_maximum_speed_check(int Dimension, ref int Axis_ID_Array, int Index, ref POINT_DATA2 Point);

        //Point table functions [For HSL-4XMO]
        [DllImport(_dll)] public static extern int APS_set_point_table3(int Dimension, ref int Axis_ID_Array, int Index, ref POINT_DATA3 Point);
        [DllImport(_dll)] public static extern int APS_point_table_move3(int Dimension, ref int Axis_ID_Array, int StartIndex, int EndIndex);
        [DllImport(_dll)] public static extern int APS_set_point_table_param3(int FirstAxid, int ParaNum, int ParaDat);

        //Digital filter functions [For PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_set_filter_param(int Axis_ID, int Filter_paramNo, int param_val);
        [DllImport(_dll)] public static extern int APS_get_filter_param(int Axis_ID, int Filter_paramNo, ref int param_val);
        [DllImport(_dll)] public static extern int APS_get_field_bus_device_info(int Board_ID, int BUS_No, int MOD_No, int Info_No, ref int Info);
        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_first_axisno(int Board_ID, int BUS_No, int MOD_No, ref int AxisNo, ref int TotalAxes);

        //Field bus DIO slave functions [For PCI-8392(H)]
        [DllImport(_dll)] public static extern int APS_set_field_bus_d_channel_output(int Board_ID, int BUS_No, int MOD_No, int Ch_No, int DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_channel_output(int Board_ID, int BUS_No, int MOD_No, int Ch_No, ref int DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_channel_input(int Board_ID, int BUS_No, int MOD_No, int Ch_No, out int DI_Value);

        //Field bus AIO slave function
        [DllImport(_dll)] public static extern int APS_set_field_bus_a_output_plc(int Board_ID, int BUS_No, int MOD_No, int Ch_No, double AO_Value, System.Int16 RunStep);
        [DllImport(_dll)] public static extern int APS_get_field_bus_a_input_plc(int Board_ID, int BUS_No, int MOD_No, int Ch_No, ref double AI_Value, System.Int16 RunStep);

        //Field bus comparing trigger functions
        [DllImport(_dll)] public static extern int APS_set_field_bus_trigger_param(int Board_ID, int BUS_No, int MOD_No, int Param_No, int Param_Val);
        [DllImport(_dll)] public static extern int APS_get_field_bus_trigger_param(int Board_ID, int BUS_No, int MOD_No, int Param_No, ref int Param_Val);
        [DllImport(_dll)] public static extern int APS_set_field_bus_trigger_linear(int Board_ID, int BUS_No, int MOD_No, int LCmpCh, int StartPoint, int RepeatTimes, int Interval);
        [DllImport(_dll)] public static extern int APS_set_field_bus_trigger_table(int Board_ID, int BUS_No, int MOD_No, int TCmpCh, int[] DataArr, int ArraySize);
        [DllImport(_dll)] public static extern int APS_set_field_bus_trigger_manual(int Board_ID, int BUS_No, int MOD_No, int TrgCh);
        [DllImport(_dll)] public static extern int APS_set_field_bus_trigger_manual_s(int Board_ID, int BUS_No, int MOD_No, int TrgChInBit);
        [DllImport(_dll)] public static extern int APS_get_field_bus_trigger_table_cmp(int Board_ID, int BUS_No, int MOD_No, int TCmpCh, ref int CmpVal);
        [DllImport(_dll)] public static extern int APS_get_field_bus_trigger_linear_cmp(int Board_ID, int BUS_No, int MOD_No, int LCmpCh, ref int CmpVal);
        [DllImport(_dll)] public static extern int APS_get_field_bus_trigger_count(int Board_ID, int BUS_No, int MOD_No, int TrgCh, ref int TrgCnt);
        [DllImport(_dll)] public static extern int APS_reset_field_bus_trigger_count(int Board_ID, int BUS_No, int MOD_No, int TrgCh);
        [DllImport(_dll)] public static extern int APS_get_field_bus_linear_cmp_remain_count(int Board_ID, int BUS_No, int MOD_No, int LCmpCh, ref int Cnt);
        [DllImport(_dll)] public static extern int APS_get_field_bus_table_cmp_remain_count(int Board_ID, int BUS_No, int MOD_No, int TCmpCh, ref int Cnt);
        [DllImport(_dll)] public static extern int APS_get_field_bus_encoder(int Board_ID, int BUS_No, int MOD_No, int EncCh, ref int EncCnt);
        [DllImport(_dll)] public static extern int APS_set_field_bus_encoder(int Board_ID, int BUS_No, int MOD_No, int EncCh, int EncCnt);
        [DllImport(_dll)] public static extern int APS_get_field_bus_timer_counter(int Board_ID, int BUS_No, int MOD_No, int TmrCh, ref int Cnt);
        [DllImport(_dll)] public static extern int APS_set_field_bus_timer_counter(int Board_ID, int BUS_No, int MOD_No, int TmrCh, int Cnt);

        //Field bus latch functions
        [DllImport(_dll)] public static extern int APS_enable_field_bus_ltc_fifo(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, int Enable);
        [DllImport(_dll)] public static extern int APS_get_field_bus_ltc_fifo_point(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, ref int ArraySize, ref LATCH_POINT LatchPoint);
        [DllImport(_dll)] public static extern int APS_set_field_bus_ltc_fifo_param(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, int Param_No, int Param_Val);
        [DllImport(_dll)] public static extern int APS_get_field_bus_ltc_fifo_param(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, int Param_No, ref int Param_Val);
        [DllImport(_dll)] public static extern int APS_reset_field_bus_ltc_fifo(int Board_ID, int BUS_No, int MOD_No, int FLtcCh);
        [DllImport(_dll)] public static extern int APS_get_field_bus_ltc_fifo_usage(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, ref int Usage);
        [DllImport(_dll)] public static extern int APS_get_field_bus_ltc_fifo_free_space(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, ref int FreeSpace);
        [DllImport(_dll)] public static extern int APS_get_field_bus_ltc_fifo_status(int Board_ID, int BUS_No, int MOD_No, int FLtcCh, ref int Status);


        // Comparing trigger functions
        [DllImport(_dll)] public static extern int APS_reset_trigger_count(int Board_ID, int TrgCh);
        [DllImport(_dll)] public static extern int APS_enable_trigger_fifo_cmp(int Board_ID, int FCmpCh, int Enable);
        [DllImport(_dll)] public static extern int APS_get_trigger_fifo_cmp(int Board_ID, int FCmpCh, ref int CmpVal);
        [DllImport(_dll)] public static extern int APS_get_trigger_fifo_status(int Board_ID, int FCmpCh, ref int FifoSts);
        [DllImport(_dll)] public static extern int APS_set_trigger_fifo_data(int Board_ID, int FCmpCh, ref int DataArr, int ArraySize, int ShiftFlag);
        [DllImport(_dll)] public static extern int APS_set_trigger_encoder_counter(int Board_ID, int TrgCh, int TrgCnt);
        [DllImport(_dll)] public static extern int APS_get_trigger_encoder_counter(int Board_ID, int TrgCh, ref int TrgCnt);

        [DllImport(_dll)] public static extern int APS_start_timer(int Board_ID, int TrgCh, int Start);
        [DllImport(_dll)] public static extern int APS_get_timer_counter(int Board_ID, int TmrCh, ref int Cnt);
        [DllImport(_dll)] public static extern int APS_set_timer_counter(int Board_ID, int TmrCh, int Cnt);
        [DllImport(_dll)] public static extern int APS_start_trigger_timer(int Board_ID, int TrgCh, int Start);
        [DllImport(_dll)] public static extern int APS_get_trigger_timer_counter(int Board_ID, int TmrCh, ref int TmrCnt);


        //VAO functions [For PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_set_vao_param(int Board_ID, int Param_No, int Param_Val);
        [DllImport(_dll)] public static extern int APS_get_vao_param(int Board_ID, int Param_No, ref int Param_Val);
        [DllImport(_dll)] public static extern int APS_set_vao_table(int Board_ID, int Table_No, int MinVelocity, int VelInterval, int TotalPoints, int[] MappingDataArray);
        [DllImport(_dll)] public static extern int APS_switch_vao_table(int Board_ID, int Table_No);
        [DllImport(_dll)] public static extern int APS_start_vao(int Board_ID, int Output_Ch, int Enable);
        [DllImport(_dll)] public static extern int APS_get_vao_status(int Board_ID, ref int Status);
        [DllImport(_dll)] public static extern int APS_check_vao_param(int Board_ID, int Table_No, ref int Status);
        [DllImport(_dll)] public static extern int APS_set_vao_param_ex(int Board_ID, int Table_No, ref VAO_DATA VaoData);
        [DllImport(_dll)] public static extern int APS_get_vao_param_ex(int Board_ID, int Table_No, ref VAO_DATA VaoData);

        //Simultaneous move
        [DllImport(_dll)] public static extern int APS_set_relative_simultaneous_move(int Dimension, ref int Axis_ID_Array, ref int Distance_Array, ref int Max_Speed_Array);
        [DllImport(_dll)] public static extern int APS_set_absolute_simultaneous_move(int Dimension, ref int Axis_ID_Array, ref int Position_Array, ref int Max_Speed_Array);
        [DllImport(_dll)] public static extern int APS_start_simultaneous_move(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_stop_simultaneous_move(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_set_velocity_simultaneous_move(int Dimension, ref int Axis_ID_Array, ref int Max_Speed_Array);
        [DllImport(_dll)] public static extern int APS_Release_simultaneous_move(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_release_simultaneous_move(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_emg_stop_simultaneous_move(int Axis_ID);

        //Override functions
        [DllImport(_dll)] public static extern int APS_speed_override(int Axis_ID, int MaxSpeed);
        [DllImport(_dll)] public static extern int APS_relative_move_ovrd(int Axis_ID, int Distance, int Max_Speed);
        [DllImport(_dll)] public static extern int APS_absolute_move_ovrd(int Axis_ID, int Position, int Max_Speed);

        //Point table functions [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_pt_dwell(int Board_ID, int PtbId, ref PTDWL Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_line(int Board_ID, int PtbId, ref PTLINE Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_arc2_ca(int Board_ID, int PtbId, ref PTA2CA Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_arc2_ce(int Board_ID, int PtbId, ref PTA2CE Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_arc3_ca(int Board_ID, int PtbId, ref PTA3CA Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_arc3_ce(int Board_ID, int PtbId, ref PTA3CE Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_spiral_ca(int Board_ID, int PtbId, ref PTHCA Prof, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_pt_spiral_ce(int Board_ID, int PtbId, ref PTHCE Prof, ref PTSTS Status);

        [DllImport(_dll)] public static extern int APS_pt_enable(int Board_ID, int PtbId, int Dimension, int[] AxisArr);
        [DllImport(_dll)] public static extern int APS_pt_disable(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_get_pt_info(int Board_ID, int PtbId, ref PTINFO Info);
        [DllImport(_dll)] public static extern int APS_pt_set_vs(int Board_ID, int PtbId, double Vs);
        [DllImport(_dll)] public static extern int APS_pt_get_vs(int Board_ID, int PtbId, ref double Vs);
        [DllImport(_dll)] public static extern int APS_pt_start(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_pt_stop(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_get_pt_status(int Board_ID, int PtbId, ref PTSTS Status);
        [DllImport(_dll)] public static extern int APS_reset_pt_buffer(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_pt_roll_back(int Board_ID, int PtbId, double Max_Speed);
        [DllImport(_dll)] public static extern int APS_pt_get_error(int Board_ID, int PtbId, ref int ErrCode);

        //Cmd buffer setting
        [DllImport(_dll)] public static extern int APS_pt_ext_set_do_ch(int Board_ID, int PtbId, int Channel, int OnOff);
        [DllImport(_dll)] public static extern int APS_pt_ext_set_table_no(int Board_ID, int PtbId, int CtrlNo, int TableNo);

        //Profile buffer setting
        [DllImport(_dll)] public static extern int APS_pt_set_absolute(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_pt_set_relative(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_pt_set_trans_buffered(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_pt_set_trans_inp(int Board_ID, int PtbId);
        [DllImport(_dll)] public static extern int APS_pt_set_trans_blend_dec(int Board_ID, int PtbId, double Bp);
        [DllImport(_dll)] public static extern int APS_pt_set_trans_blend_dist(int Board_ID, int PtbId, double Bp);
        [DllImport(_dll)] public static extern int APS_pt_set_trans_blend_pcnt(int Board_ID, int PtbId, double Bp);
        [DllImport(_dll)] public static extern int APS_pt_set_acc(int Board_ID, int PtbId, double Acc);
        [DllImport(_dll)] public static extern int APS_pt_set_dec(int Board_ID, int PtbId, double Dec);
        [DllImport(_dll)] public static extern int APS_pt_set_acc_dec(int Board_ID, int PtbId, double AccDec);
        [DllImport(_dll)] public static extern int APS_pt_set_s(int Board_ID, int PtbId, double Sf);
        [DllImport(_dll)] public static extern int APS_pt_set_vm(int Board_ID, int PtbId, double Vm);
        [DllImport(_dll)] public static extern int APS_pt_set_ve(int Board_ID, int PtbId, double Ve);

        //Program download functions
        [DllImport(_dll)] public static extern int APS_load_vmc_program(int Board_ID, int TaskNum, string pFile, int Password);
        [DllImport(_dll)] public static extern int APS_save_vmc_program(int Board_ID, int TaskNum, string pFile, int Password);
        [DllImport(_dll)] public static extern int APS_load_amc_program(int Board_ID, int TaskNum, string pFile, int Password);
        [DllImport(_dll)] public static extern int APS_save_amc_program(int Board_ID, int TaskNum, string pFile, int Password);

        [DllImport(_dll)] public static extern int APS_set_task_mode(int Board_ID, int TaskNum, byte Mode, ushort LastIP);
        [DllImport(_dll)] public static extern int APS_get_task_mode(int Board_ID, int TaskNum, ref byte Mode, ref ushort LastIP);
        [DllImport(_dll)] public static extern int APS_start_task(int Board_ID, int TaskNum, int CtrlCmd);
        [DllImport(_dll)] public static extern int APS_get_task_info(int Board_ID, int TaskNum, ref TSK_INFO Info);
        [DllImport(_dll)] public static extern int APS_get_task_msg(int Board_ID, ushort QueueSts, ref ushort ActualSize, ref byte CharArr);

        //Latch functions
        [DllImport(_dll)] public static extern int APS_get_encoder(int Axis_ID, ref int Encoder);
        [DllImport(_dll)] public static extern int APS_get_latch_counter(int Axis_ID, int Src, ref int Counter);
        [DllImport(_dll)] public static extern int APS_get_latch_event(int Axis_ID, int Src, ref int Event);

        //Raw command counter [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_get_command_counter(int Axis_ID, ref int Counter);

        //Reset raw command counter [For PCIe-8338]
        [DllImport(_dll)] public static extern int APS_reset_command_counter(int Axis_ID);

        //Watch dog timer 
        [DllImport(_dll)] public static extern int APS_wdt_start(int Board_ID, int TimerNo, int TimeOut);
        [DllImport(_dll)] public static extern int APS_wdt_get_timeout_period(int Board_ID, int TimerNo, ref int TimeOut);
        [DllImport(_dll)] public static extern int APS_wdt_reset_counter(int Board_ID, int TimerNo);
        [DllImport(_dll)] public static extern int APS_wdt_get_counter(int Board_ID, int TimerNo, ref int Counter);
        [DllImport(_dll)] public static extern int APS_wdt_set_action_event(int Board_ID, int TimerNo, int EventByBit);
        [DllImport(_dll)] public static extern int APS_wdt_get_action_event(int Board_ID, int TimerNo, ref int EventByBit);

        //Multi-axes simultaneuos move start/stop [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_move_trigger(int Dimension, int[] Axis_ID_Array);
        [DllImport(_dll)] public static extern int APS_stop_move_multi(int Dimension, ref int Axis_ID_Array);
        [DllImport(_dll)] public static extern int APS_emg_stop_multi(int Dimension, ref int Axis_ID_Array);

        //Gear/Gantry functions [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_start_gear(int Axis_ID, int Mode);
        [DllImport(_dll)] public static extern int APS_get_gear_status(int Axis_ID, ref int Status);

        //Multi-latch functions
        [DllImport(_dll)] public static extern int APS_set_ltc_counter(int Board_ID, int CntNum, int CntValue);
        [DllImport(_dll)] public static extern int APS_get_ltc_counter(int Board_ID, int CntNum, ref int CntValue);
        [DllImport(_dll)] public static extern int APS_set_ltc_fifo_param(int Board_ID, int FLtcCh, int Param_No, int Param_Val);
        [DllImport(_dll)] public static extern int APS_get_ltc_fifo_param(int Board_ID, int FLtcCh, int Param_No, ref int Param_Val);
        [DllImport(_dll)] public static extern int APS_manual_latch(int Board_ID, int LatchSignalInBits);
        [DllImport(_dll)] public static extern int APS_enable_ltc_fifo(int Board_ID, int FLtcCh, int Enable);
        [DllImport(_dll)] public static extern int APS_reset_ltc_fifo(int Board_ID, int FLtcCh);
        [DllImport(_dll)] public static extern int APS_get_ltc_fifo_data(int Board_ID, int FLtcCh, ref int Data);
        [DllImport(_dll)] public static extern int APS_get_ltc_fifo_usage(int Board_ID, int FLtcCh, ref int Usage);
        [DllImport(_dll)] public static extern int APS_get_ltc_fifo_free_space(int Board_ID, int FLtcCh, ref int FreeSpace);
        [DllImport(_dll)] public static extern int APS_get_ltc_fifo_status(int Board_ID, int FLtcCh, ref int Status);
        [DllImport(_dll)] public static extern int APS_get_ltc_fifo_point(int Board_ID, int FLtcCh, ref int ArraySize, ref LATCH_POINT LatchPoint);

        //Single latch functions 
        [DllImport(_dll)] public static extern int APS_manual_latch2(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_get_latch_data2(int Axis_ID, int LatchNum, ref int LatchData);
        [DllImport(_dll)] public static extern int APS_set_backlash_en(int Axis_ID, int Enable);
        [DllImport(_dll)] public static extern int APS_get_backlash_en(int Axis_ID, ref int Enable);

        //ODM functions for Mechatrolink
        [DllImport(_dll)] public static extern int APS_start_mlink(int Board_ID, ref int AxisFound_InBits);
        [DllImport(_dll)] public static extern int APS_stop_mlink(int Board_ID);
        [DllImport(_dll)] public static extern int APS_set_mlink_servo_param(int Axis_ID, int Para_No, int Para_Dat);
        [DllImport(_dll)] public static extern int APS_get_mlink_servo_param(int Axis_ID, int Para_No, ref int Para_Dat);
        [DllImport(_dll)] public static extern int APS_config_mlink(int Board_ID, int TotalAxes, ref int AxesArray);
        [DllImport(_dll)] public static extern int APS_get_mlink_rv_ptr(int Axis_ID, out IntPtr rptr);
        [DllImport(_dll)] public static extern int APS_get_mlink_sd_ptr(int Axis_ID, out IntPtr sptr);
        [DllImport(_dll)] public static extern int APS_get_mlink_servo_alarm(int Axis_ID, int Alarm_No, ref int Alarm_Detail);
        [DllImport(_dll)] public static extern int APS_reset_mlink_servo_alarm(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_set_mlink_pulse_per_rev(int Axis_ID, int PPR);

        //Apply smooth servo off [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_smooth_servo_off(int Axis_ID, double Decay_Rate);
        [DllImport(_dll)] public static extern int APS_set_smooth_servo_off(int Board_ID, int Axis_ID, int cnt_Max, ref int cnt_Err);

        //ODM functions
        [DllImport(_dll)] public static extern int APS_relative_move_wait(int Axis_ID, int Distance, int Max_Speed, int Time_Out, int Delay_Time, ref int MotionSts);
        [DllImport(_dll)] public static extern int APS_absolute_move_wait(int Axis_ID, int Position, int Max_Speed, int Time_Out, int Delay_Time, ref int MotionSts);
        [DllImport(_dll)] public static extern int APS_relative_linear_move_wait(int Dimension, ref int Axis_ID_Array, ref int Distance_Array, int Max_Linear_Speed, int Time_Out, int Delay_Time, ref int MotionSts);
        [DllImport(_dll)] public static extern int APS_absolute_linear_move_wait(int Dimension, ref int Axis_ID_Array, ref int Position_Array, int Max_Linear_Speed, int Time_Out, int Delay_Time, ref int MotionSts);
        [DllImport(_dll)] public static extern int APS_relative_move_non_wait(int Axis_ID, int Distance, int Max_Speed, int Time_Out, int Delay_Time);
        [DllImport(_dll)] public static extern int APS_absolute_move_non_wait(int Axis_ID, int Position, int Max_Speed, int Time_Out, int Delay_Time);
        [DllImport(_dll)] public static extern int APS_relative_linear_move_non_wait(int Dimension, ref int Axis_ID_Array, ref int Distance_Array, int Max_Linear_Speed, int Time_Out, int Delay_Time);
        [DllImport(_dll)] public static extern int APS_absolute_linear_move_non_wait(int Dimension, ref int Axis_ID_Array, ref int Position_Array, int Max_Linear_Speed, int Time_Out, int Delay_Time);
        [DllImport(_dll)] public static extern int APS_wait_move_done(int Axis_ID, ref int MotionSts);

        //ODM functions [For MNET-4XMO-C]
        [DllImport(_dll)] public static extern int APS_absolute_arc_move_ex(int Dimension, ref int Axis_ID_Array, ref int Center_Pos_Array, ref int End_Pos_Array, int CwOrCcw, int Max_Arc_Speed);
        [DllImport(_dll)] public static extern int APS_motion_status_ex(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_motion_io_status_ex(int Axis_ID);

        //Gantry functions [For PCI-8392(H), PCI-8253/56]
        [DllImport(_dll)] public static extern int APS_set_gantry_param(int Board_ID, int GroupNum, int ParaNum, int ParaDat);
        [DllImport(_dll)] public static extern int APS_get_gantry_param(int Board_ID, int GroupNum, int ParaNum, ref int ParaDat);
        [DllImport(_dll)] public static extern int APS_set_gantry_axis(int Board_ID, int GroupNum, int Master_Axis_ID, int Slave_Axis_ID);
        [DllImport(_dll)] public static extern int APS_get_gantry_axis(int Board_ID, int GroupNum, ref int Master_Axis_ID, ref int Slave_Axis_ID);
        [DllImport(_dll)] public static extern int APS_get_gantry_error(int Board_ID, int GroupNum, ref int GentryError);

        //Field bus master functions
        [DllImport(_dll)] public static extern int APS_set_field_bus_param(int Board_ID, int BUS_No, int BUS_Param_No, int BUS_Param);
        [DllImport(_dll)] public static extern int APS_get_field_bus_param(int Board_ID, int BUS_No, int BUS_Param_No, ref int BUS_Param);
        [DllImport(_dll)] public static extern int APS_start_field_bus(int Board_ID, int BUS_No, int Start_Axis_ID);
        [DllImport(_dll)] public static extern int APS_scan_field_bus(int Board_ID, int BUS_No);
        [DllImport(_dll)] public static extern int APS_stop_field_bus(int Board_ID, int BUS_No);
        [DllImport(_dll)] public static extern int APS_get_field_bus_master_status(int Board_ID, int BUS_No, ref uint Status);

        [DllImport(_dll)] public static extern int APS_get_field_bus_last_scan_info(int Board_ID, int BUS_No, ref int Info_Array, int Array_Size, ref int Info_Count);
        [DllImport(_dll)] public static extern int APS_get_field_bus_master_type(int Board_ID, int BUS_No, ref int BUS_Type);
        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_type(int Board_ID, int BUS_No, int MOD_No, ref int MOD_Type);
        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_name(int Board_ID, int BUS_No, int MOD_No, ref int MOD_Name);
        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_serialID(int Board_ID, int BUS_No, int MOD_No, ref System.Int16 Serial_ID);

        //Field bus slave functions
        [DllImport(_dll)] public static extern int APS_set_field_bus_slave_param(int Board_ID, int BUS_No, int MOD_No, int Ch_No, int ParaNum, int ParaDat);
        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_param(int Board_ID, int BUS_No, int MOD_No, int Ch_No, int ParaNum, ref int ParaDat);
        [DllImport(_dll)] public static extern int APS_get_slave_connect_quality(int Board_ID, int BUS_No, int MOD_No, ref int Sts_data);
        [DllImport(_dll)] public static extern int APS_get_slave_online_status(int Board_ID, int BUS_No, int MOD_No, ref int Live);
        [DllImport(_dll)] public static extern int APS_set_field_bus_slave_recovery(int Board_ID, int BUS_No, int MOD_No);


        [DllImport(_dll)] public static extern int APS_get_field_bus_ESC_register(int Board_ID, int BUS_No, int MOD_No, int RegOffset, int DataSize, ref int DataValue);
        [DllImport(_dll)] public static extern int APS_set_field_bus_ESC_register(int Board_ID, int BUS_No, int MOD_No, int RegOffset, int DataSize, ref int DataValue);

        //Field bus DIO slave functions [For PCI-8392(H)]
        [DllImport(_dll)] public static extern int APS_set_field_bus_d_output(int Board_ID, int BUS_No, int MOD_No, int DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_output(int Board_ID, int BUS_No, int MOD_No, ref int DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_input(int Board_ID, int BUS_No, int MOD_No, ref int DI_Value);

        //Modules be 64 bits gpio
        [DllImport(_dll)] public static extern int APS_set_field_bus_d_output_ex(int Board_ID, int BUS_No, int MOD_No, DO_DATA_EX DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_output_ex(int Board_ID, int BUS_No, int MOD_No, ref DO_DATA_EX DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_input_ex(int Board_ID, int BUS_No, int MOD_No, ref DI_DATA_EX DI_Value);

        //Field bus AIO slave functions
        [DllImport(_dll)] public static extern int APS_set_field_bus_a_output(int Board_ID, int BUS_No, int MOD_No, int Ch_No, double AO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_a_output(int Board_ID, int BUS_No, int MOD_No, int Ch_No, ref double AO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_a_input(int Board_ID, int BUS_No, int MOD_No, int Ch_No, out double AI_Value);

        //ODM functions
        [DllImport(_dll)] public static extern int APS_start_vao_by_mode(int Board_ID, int ChannelInBit, int Mode, int Enable);
        [DllImport(_dll)] public static extern int APS_set_vao_pwm_burst_count(int Board_ID, int Table_No, int Count);

        //PWM functions
        [DllImport(_dll)] public static extern int APS_set_pwm_width(int Board_ID, int PWM_Ch, int Width);
        [DllImport(_dll)] public static extern int APS_get_pwm_width(int Board_ID, int PWM_Ch, ref int Width);
        [DllImport(_dll)] public static extern int APS_set_pwm_frequency(int Board_ID, int PWM_Ch, int Frequency);
        [DllImport(_dll)] public static extern int APS_get_pwm_frequency(int Board_ID, int PWM_Ch, ref int Frequency);
        [DllImport(_dll)] public static extern int APS_set_pwm_on(int Board_ID, int PWM_Ch, int PWM_On);

        // Comparing trigger functions
        [DllImport(_dll)] public static extern int APS_set_trigger_param(int Board_ID, int Param_No, int Param_Val);
        [DllImport(_dll)] public static extern int APS_get_trigger_param(int Board_ID, int Param_No, ref int Param_Val);
        [DllImport(_dll)] public static extern int APS_set_trigger_linear(int Board_ID, int LCmpCh, int StartPoint, int RepeatTimes, int Interval);
        [DllImport(_dll)] public static extern int APS_set_trigger_table(int Board_ID, int TCmpCh, int[] DataArr, int ArraySize);
        [DllImport(_dll)] public static extern int APS_set_trigger_manual(int Board_ID, int TrgCh);
        [DllImport(_dll)] public static extern int APS_set_trigger_manual_s(int Board_ID, int TrgChInBit);
        [DllImport(_dll)] public static extern int APS_get_trigger_table_cmp(int Board_ID, int TCmpCh, ref int CmpVal);
        [DllImport(_dll)] public static extern int APS_get_trigger_linear_cmp(int Board_ID, int LCmpCh, ref int CmpVal);
        [DllImport(_dll)] public static extern int APS_get_trigger_count(int Board_ID, int TrgCh, ref int TrgCnt);

        //Pulser counter functions
        [DllImport(_dll)] public static extern int APS_get_pulser_counter(int Board_ID, ref int Counter);
        [DllImport(_dll)] public static extern int APS_set_pulser_counter(int Board_ID, int Counter);

        //Reserved functions [Legacy functions]
        [DllImport(_dll)] public static extern int APS_field_bus_slave_set_param(int Board_ID, int BUS_No, int MOD_No, int Ch_No, int ParaNum, int ParaDat);
        [DllImport(_dll)] public static extern int APS_field_bus_slave_get_param(int Board_ID, int BUS_No, int MOD_No, int Ch_No, int ParaNum, ref int ParaDat);

        [DllImport(_dll)] public static extern int APS_field_bus_d_set_output(int Board_ID, int BUS_No, int MOD_No, int DO_Value);
        [DllImport(_dll)] public static extern int APS_field_bus_d_get_output(int Board_ID, int BUS_No, int MOD_No, ref int DO_Value);
        [DllImport(_dll)] public static extern int APS_field_bus_d_get_input(int Board_ID, int BUS_No, int MOD_No, ref int DI_Value);

        [DllImport(_dll)] public static extern int APS_field_bus_A_set_output(int Board_ID, int BUS_No, int MOD_No, int Ch_No, double AO_Value);
        [DllImport(_dll)] public static extern int APS_field_bus_A_get_output(int Board_ID, int BUS_No, int MOD_No, int Ch_No, ref double AO_Value);
        [DllImport(_dll)] public static extern int APS_field_bus_A_get_input(int Board_ID, int BUS_No, int MOD_No, int Ch_No, ref double AI_Value);

        [DllImport(_dll)] public static extern int APS_field_bus_A_set_output_plc(int Board_ID, int BUS_No, int MOD_No, int Ch_No, double AO_Value, System.Int16 RunStep);
        [DllImport(_dll)] public static extern int APS_field_bus_A_get_input_plc(int Board_ID, int BUS_No, int MOD_No, int Ch_No, ref double AI_Value, System.Int16 RunStep);

        [DllImport(_dll)] public static extern int APS_get_eep_curr_drv_ctrl_mode(int Board_ID, ref int ModeInBit);

        //DPAC functions
        [DllImport(_dll)] public static extern int APS_rescan_CF(int Board_ID);
        [DllImport(_dll)] public static extern int APS_get_battery_status(int Board_ID, ref int Battery_status);

        //DPAC display & Display button
        [DllImport(_dll)] public static extern int APS_get_display_data(int Board_ID, int displayDigit, ref int displayIndex);
        [DllImport(_dll)] public static extern int APS_set_display_data(int Board_ID, int displayDigit, int displayIndex);
        [DllImport(_dll)] public static extern int APS_get_button_status(int Board_ID, ref int buttonstatus);

        //NV RAM functions
        [DllImport(_dll)] public static extern int APS_set_nv_ram(int Board_ID, int RamNo, int DataWidth, int Offset, int Data);
        [DllImport(_dll)] public static extern int APS_get_nv_ram(int Board_ID, int RamNo, int DataWidth, int Offset, ref int Data);
        [DllImport(_dll)] public static extern int APS_clear_nv_ram(int Board_ID, int RamNo);

        //Advanced single move & interpolation [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_ptp(int Axis_ID, int Option, double Position, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_ptp_v(int Axis_ID, int Option, double Position, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_ptp_all(int Axis_ID, int Option, double Position, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_vel(int Axis_ID, int Option, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_vel_all(int Axis_ID, int Option, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_line(int Dimension, int[] Axis_ID_Array, int Option, double[] PositionArray, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_line_v(int Dimension, int[] Axis_ID_Array, int Option, double[] PositionArray, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_line_all(int Dimension, int[] Axis_ID_Array, int Option, double[] PositionArray, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc2_ca(int[] Axis_ID_Array, int Option, double[] CenterArray, double Angle, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc2_ca_v(int[] Axis_ID_Array, int Option, double[] CenterArray, double Angle, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc2_ca_all(int[] Axis_ID_Array, int Option, double[] CenterArray, double Angle, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc2_ce(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] EndArray, short Dir, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc2_ce_v(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc2_ce_all(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc3_ca(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double Angle, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc3_ca_v(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double Angle, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc3_ca_all(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double Angle, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc3_ce(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] EndArray, System.Int16 Dir, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc3_ce_v(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_arc3_ce_all(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_spiral_ca(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double Angle, double DeltaH, double FinalR, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_spiral_ca_v(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double Angle, double DeltaH, double FinalR, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_spiral_ca_all(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double Angle, double DeltaH, double FinalR, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_spiral_ce(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double[] EndArray, System.Int16 Dir, ref double TransPara, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_spiral_ce_v(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vm, ref ASYNCALL Wait);
        [DllImport(_dll)] public static extern int APS_spiral_ce_all(int[] Axis_ID_Array, int Option, double[] CenterArray, double[] NormalArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);

        //Ring counter functions [For PCI-8154/8]
        [DllImport(_dll)] public static extern int APS_set_ring_counter(int Axis_ID, int RingVal);
        [DllImport(_dll)] public static extern int APS_get_ring_counter(int Axis_ID, ref int RingVal);



        //**********************************************
        // New header functions; 20151102
        //**********************************************

        //Pitch error compensation [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_set_pitch_table(int Axis_ID, int Comp_Type, int Total_Points, int MinPosition, uint Interval, int[] Comp_Data);
        [DllImport(_dll)] public static extern int APS_get_pitch_table(int Axis_ID, ref int Comp_Type, ref int Total_Points, ref int MinPosition, ref uint Interval, ref int Comp_Data);
        [DllImport(_dll)] public static extern int APS_start_pitch_comp(int Axis_ID, int Enable);

        //2D compensation [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_set_2d_compensation_table(ref int AxisIdArray, uint CompType, ref uint TotalPointArray, ref double StartPosArray, ref double IntervalArray, ref double CompDataArrayX, ref double CompDataArrayY);
        [DllImport(_dll)] public static extern int APS_get_2d_compensation_table(ref int AxisIdArray, ref uint CompType, ref uint TotalPointArray, ref double StartPosArray, ref double IntervalArray, ref double CompDataArrayX, ref double CompDataArrayY);
        [DllImport(_dll)] public static extern int APS_start_2d_compensation(int Axis_ID, int Enable);
        [DllImport(_dll)] public static extern int APS_absolute_linear_move_2d_compensation(ref int Axis_ID_Array, ref double Position_Array, double Max_Linear_Speed);
        [DllImport(_dll)] public static extern int APS_get_2d_compensation_command_position(int Axis_ID, ref double CommandX, ref double CommandY, ref double PositionX, ref double PositionY);
        //20200120
        [DllImport(_dll)] public static extern int APS_set_trigger_table_data(int Board_ID, int TCmpCh, int[] DataArr, int ArraySize);
        [DllImport(_dll)] public static extern int APS_get_trigger_table_status(int Board_ID, int TCmpCh, ref int FreeSpace, ref int FifoSts);
        [DllImport(_dll)] public static extern int APS_get_trigger_cmp_value(int Board_ID, int TCmpCh, ref int CmpVal);
        [DllImport(_dll)] public static extern int APS_enable_trigger_table(int Board_ID, int TCmpCh, int Enable);
        [DllImport(_dll)] public static extern int APS_reset_trigger_table(int Board_ID, int TCmpCh);


        //Multi-dimension comparator functions [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_set_multi_trigger_table(int Board_ID, int Dimension, MCMP_POINT[] DataArr, int ArraySize, int Window);
        [DllImport(_dll)] public static extern int APS_get_multi_trigger_table_cmp(int Board_ID, int Dimension, ref MCMP_POINT CmpVal);

        //Pulser functions
        [DllImport(_dll)] public static extern int APS_manual_pulser_start(int Board_ID, int Enable);
        [DllImport(_dll)] public static extern int APS_manual_pulser_velocity_move(int Axis_ID, double SpeedLimit);
        [DllImport(_dll)] public static extern int APS_manual_pulser_relative_move(int Axis_ID, double Distance, double SpeedLimit);
        [DllImport(_dll)] public static extern int APS_manual_pulser_home_move(int Axis_ID);

        // [Wei-Li suggests to remove]
        //**********************************************
        // 2D arc-interpolation for 3-point
        [DllImport(_dll)] public static extern int APS_arc2_ct_all(int[] Axis_ID_Array, int APS_Option, double[] AnyArray, double[] EndArray, System.Int16 Dir, ref double TransPara, double Vs, double Vm, double Ve, double Acc, double Dec, double SFac, ref ASYNCALL Wait);
        //**********************************************

        // [Reserved for unknown usage]
        //**********************************************
        [DllImport(_dll)] public static extern int APS_get_watch_timer(int Board_ID, ref int Timer);
        [DllImport(_dll)] public static extern int APS_reset_wdt(int Board_ID, int WDT_No);
        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_mapto_AxisID(int Board_ID, int BUS_No, int MOD_No, ref int AxisID);
        //**********************************************

        //for 8338 
        [DllImport(_dll)] public static extern int APS_get_field_bus_module_info(int Board_ID, int BUS_No, int MOD_No, [In, Out] EC_MODULE_INFO[] Module_info);
        [DllImport(_dll)] public static extern int APS_reset_field_bus_alarm(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_get_field_bus_alarm(int Axis_ID, ref uint AlarmCode);
        [DllImport(_dll)] public static extern int APS_get_field_bus_pdo_offset(int Board_ID, int BUS_No, int MOD_No, out IntPtr PPTx, ref uint NumOfTx, out IntPtr PPRx, ref uint NumOfRx);
        [DllImport(_dll)] public static extern int APS_get_field_bus_pdo(int Board_ID, int BUS_No, ushort ByteOffset, ushort Size, ref uint Value);
        [DllImport(_dll)] public static extern int APS_set_field_bus_pdo(int Board_ID, int BUS_No, ushort ByteOffset, ushort Size, uint Value);
        [DllImport(_dll)]
        public static extern int APS_get_field_bus_sdo(
                                                         int Board_ID,
                                                         int BUS_No,
                                                         int MOD_No,
                                                         ushort ODIndex,
                                                         ushort ODSubIndex,
                                                         byte[] Data,
                                                         uint DataLen,
                                                         ref uint OutDatalen,
                                                         uint Timeout,
                                                         uint Flags
                                                        );

        [DllImport(_dll)]
        public static extern int APS_set_field_bus_sdo(
                                                         int Board_ID,
                                                         int BUS_No,
                                                         int MOD_No,
                                                         ushort ODIndex,
                                                         ushort ODSubIndex,
                                                         byte[] Data,
                                                         uint DataLen,
                                                         uint Timeout,
                                                         uint Flags
                                                        );

        [DllImport(_dll)] public static extern int APS_get_field_bus_od_num(int Board_ID, int BUS_No, int MOD_No, ref ushort Num, out IntPtr ODList);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_desc(int Board_ID, int BUS_No, int MOD_No, ushort ODIndex, ref ushort MaxNumSubIndex, byte[] Description, uint Size);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_desc_entry(int Board_ID, int BUS_No, int MOD_No, ushort ODIndex, ushort ODSubIndex, ref OD_DESC_ENTRY pOD_DESC_ENTRY);

        [DllImport(_dll)] public static extern int APS_get_actual_torque(int Axis_ID, ref int Torque);
        [DllImport(_dll)] public static extern int APS_set_field_bus_d_port_output(int Board_ID, int BUS_No, int MOD_No, int Port_No, uint DO_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_port_input(int Board_ID, int BUS_No, int MOD_No, int Port_No, ref uint DI_Value);
        [DllImport(_dll)] public static extern int APS_get_field_bus_d_port_output(int Board_ID, int BUS_No, int MOD_No, int Port_No, ref uint DO_Value);

        [DllImport(_dll)] public static extern int APS_set_circular_limit(int Axis_A, int Axis_B, double Center_A, double Center_B, double Radius, int Stop_Mode, int Enable);
        [DllImport(_dll)] public static extern int APS_get_circular_limit(int Axis_A, int Axis_B, ref double Center_A, ref double Center_B, ref double Radius, ref int Stop_Mode, ref int Enable);

        [DllImport(_dll)] public static extern int APS_get_field_bus_loss_package(int Board_ID, int BUS_No, ref int Loss_Count);

        [DllImport(_dll)] public static extern int APS_set_field_bus_od_data(int Board_ID, int BUS_No, int MOD_No, int SubMOD_No, int ODIndex, uint RawData);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_data(int Board_ID, int BUS_No, int MOD_No, int SubMOD_No, int ODIndex, ref uint RawData);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_module_info(int Board_ID, int BUS_No, int MOD_No, ref EC_Sub_MODULE_INFO Sub_Module_info);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_number(int Board_ID, int BUS_No, int MOD_No, int SubMOD_No, ref int TxODNum, ref int RxODNum);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_tx(int Board_ID, int BUS_No, int MOD_No, int SubMOD_No, int TxODIndex, ref EC_Sub_MODULE_OD_INFO Sub_MODULE_OD_INFO);
        [DllImport(_dll)] public static extern int APS_get_field_bus_od_rx(int Board_ID, int BUS_No, int MOD_No, int SubMOD_No, int RxODIndex, ref EC_Sub_MODULE_OD_INFO Sub_MODULE_OD_INFO);

        // PVT function;		
        [DllImport(_dll)] public static extern int APS_pvt_add_point(int Axis_ID, int ArraySize, ref double PositionArray, ref double VelocityArray, ref double TimeArray);
        [DllImport(_dll)] public static extern int APS_pvt_get_status(int Axis_ID, ref int FreeSize, ref int PointCount, ref int State);
        [DllImport(_dll)] public static extern int APS_pvt_start(int Dimension, ref int Axis_ID_Array, int Enable);
        [DllImport(_dll)] public static extern int APS_pvt_reset(int Axis_ID);

        // PT functions;
        [DllImport(_dll)] public static extern int APS_pt_motion_add_point(int Axis_ID, int ArraySize, ref double PositionArray, ref double TimeArray);
        [DllImport(_dll)] public static extern int APS_pt_motion_get_status(int Axis_ID, ref int FreeSize, ref int PointCount, ref int State);
        [DllImport(_dll)] public static extern int APS_pt_motion_start(int Dimension, ref int Axis_ID_Array, int Enable);
        [DllImport(_dll)] public static extern int APS_pt_motion_reset(int Axis_ID);

        //Get speed profile calculation
        [DllImport(_dll)] public static extern int APS_relative_move_profile(int Axis_ID, int Distance, int Max_Speed, ref int StrVel, ref int MaxVel, ref double Tacc, ref double Tdec, ref double Tconst);
        [DllImport(_dll)] public static extern int APS_absolute_move_profile(int Axis_ID, int Position, int Max_Speed, ref int StrVel, ref int MaxVel, ref double Tacc, ref double Tdec, ref double Tconst);

        //ASYNC mode
        [DllImport(_dll)] public static extern int APS_get_error_code(int Axis_ID, uint Index, ref int ErrorCode);
        [DllImport(_dll)] public static extern int APS_get_cmd_fifo_usage(int Axis_ID, ref uint Number);

        //Get fpga latch value [For PCI-8254/58]
        [DllImport(_dll)] public static extern int APS_get_axis_latch_data(int Axis_ID, int latch_channel, ref int latch_data);

        [DllImport(_dll)] public static extern int APS_register_emx(int emx_online, int option);
        [DllImport(_dll)] public static extern int APS_get_deviceIP(int Board_ID, ref string option);
        [DllImport(_dll)] public static extern int APS_reset_emx_alarm(int Axis_ID);
        [DllImport(_dll)] public static extern int APS_check_motion_profile_emx(int Axis_ID, ref Speed_profile profile_input, ref Speed_profile profile_output, ref int MinDis);

        [DllImport(_dll)] public static extern int APS_get_field_bus_module_map(int Board_ID, int BUS_No, ref uint MOD_No_Arr, uint Size);
        [DllImport(_dll)] public static extern int APS_set_field_bus_module_map(int Board_ID, int BUS_No, ref uint MOD_No_Arr, uint Size);
        [DllImport(_dll)] public static extern int APS_get_field_bus_analysis_topology(int Board_ID, int BUS_No, ref int Error_Slave_No, [In, Out] EC_MODULE_INFO[] Current_slave_info, ref int Current_slave_num, [In, Out] EC_MODULE_INFO[] Past_slave_info, ref int Past_slave_num);

        [DllImport(_dll)] public static extern int APS_get_gantry_number(int Axis_ID, ref int SlaveAxisIDSize);
        [DllImport(_dll)] public static extern int APS_get_gantry_info(int Axis_ID, int SlaveAxisIDSize, ref int SlaveAxisIDArray);
        [DllImport(_dll)] public static extern int APS_get_gantry_deviation(int Axis_ID, int SlaveAxisIDSize, ref int SlaveAxisIDArray, ref double DeviationArray);

        [DllImport(_dll)] public static extern int APS_get_field_bus_slave_state(int Board_ID, int BUS_No, int MOD_No, ref int State);
        [DllImport(_dll)] public static extern int APS_set_field_bus_slave_state(int Board_ID, int BUS_No, int MOD_No, int State);
        // Coordinate transform 20190624
        [DllImport(_dll)] public static extern int APS_set_coordTransform2D_config(int Board_ID, int AxisID_X, int AxisID_Y, double XYAngle, int Enable);
        [DllImport(_dll)] public static extern int APS_get_coordTransform2D_config(int Board_ID, ref int AxisID_X, ref int AxisID_Y, ref double XYAngle, ref int Enable);
        [DllImport(_dll)] public static extern int APS_get_coordTransform2D_position(int Board_ID, ref double Cmd_transform_X, ref int Cmd_transform_Y, ref double Fbk_transform_X, ref double Fbk_transform_Y);

    }
}