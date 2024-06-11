using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MotionControllers.Motion
{
    public class APSException : Exception
    {
        public APSException(int errorCode) : base(GetAPSErrorMessage(errorCode)) 
        {
            ErrorCode = errorCode;
        }
        public int ErrorCode { get; }
        public static string GetAPSErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 0:
                    return "ERR_NoError 成功，没有错误";
                case -1:
                    return "ERR_OSVersion 操作系统版本错误。该函数不支持您使用的当前操作系统。";
                case -2:
                    return "ERR_OpenDriverFailed 打开驱动程序失败。 创建驱动程序接口失败。\n检查设备驱动程序是否正确安装。\n检查系统中是否正确安装了设备。";
                case -3:
                    return "ERR_InsufficientMemory 系统内存不足。\n您的系统中没有足够的内存。";
                case -4:
                    return "ERR_DeviceNotInitial 设备或板卡未初始化。\n检查卡号\n设备已关闭\n设备未初始化。";
                case -5:
                    return "ERR_NoDeviceFound 找不到装置\n检查设备驱动程序是否正确安装。\n检查系统中是否正确安装了设备。";
                case -6:
                    return "ERR_CardIdDuplicate 板卡号重复。\n检查卡ID设置（SW跳转）\n检查初始功能的参数是否正确。";
                case -7:
                    return "ERR_DeviceAlreadyIntialed 设备已经初始化。\n1.检查关闭板卡功能是否正常工作。";
                case -8:
                    return "ERR_InterruptNotEnable 中断事件未启用。\n1.启用硬件中断。\n2.检查中断因子设置是否正确。";
                case -9:
                    return "ERR_TimeOut 函数超时";
                case -10:
                    return "ERR_ParametersInvaild 参数的值不正确。\n检查参数的设置范围。 将参数的设置值与\n用户手册进行比较。";
                case -11:
                    return "ERR_SetEEPROM 硬件内存写入错误。";
                case -12:
                    return "ERR_GetEEPROM 硬件内存读取错误。";
                case -13:
                    return "ERR_FunctionNotAvailable 该函数在当前阶段不可用。\n设备不支持此函数。\n系统处于错误状态。\n1.检查函数库。\n900\n2.检查硬件连接（伺服驱动器连接）\n3.重新初始化（重新引导）系统。";
                case -14:
                    return "ERR_FirmwareError 固件处理错误。\n1.检查固件版本。";
                case -15:
                    return "ERR_CommandInProcess 上一条命令正在处理中。\nERR_OpenDriverFailed 打开驱动程序失败。\n 创建驱动程序接口失败。\n检查设备驱动程序是否正确安装。\n检查系统中是否正确安装了设备。";
                case -16:
                    return "ERR_AxisIdDuplicate 轴ID重复。";
                case -17:
                    return "ERR_ModuleNotFound 找不到从站模块。";
                case -18:
                    return "ERR_InsufficientModuleNo 系统模块编号不足";
                case -19:
                    return "ERR_HandShakeFailed 与DSP握手不合时宜。";
                case -20:
                    return "ERR_FILE_Format 配置文件格式错误。（无法解析）";
                case -21:
                    return "ERR_ParametersReadOnly 函数参数为只读。";
                case -22:
                    return "ERR_DistantNotEnough 距离不足以完成运动";
                case -26:
                    return "ERR_TrimDAC_Channel";
                case -27:
                    return "ERR_Satellite_Type";
                case -28:
                    return "ERR_Over_Voltage_Spec";
                case -29:
                    return "ERR_Over_Current_Spec";
                case -30:
                    return "ERR_SlaveIsNotAI";
                case -31:
                    return "ERR_Over_AO_Channel_Scope";
                case -32:
                    return "ERR_DllFuncFailed";
                case -33:
                    return "ERR_FeederAbnormalStop";
                case -34:
                    return "ERR_AreadyClose";
                case -35:
                    return "ERR_NullObject";
                case -36:
                    return "ERR_PreMoveErr";
                case -37:
                    return "ERR_PreMoveNotDone";
                case -38:
                    return "ERR_MismatchState";
                case -39:
                    return "ERR_Read_ModuleType_Dismatch";
                case -40:
                    return "ERR_DoubleOverflow";
                case -41:
                    return "ERR_SlaveNumberErr";
                case -42:
                    return "ERR_SlaveStatusErr";
                case -43:
                    return "ERR_MapPDOOffset_TimeOut";
                case -44:
                    return "ERR_Fifo_Access_Fail";
                case -45:
                    return "ERR_KernelVerifyError";
                case -46:
                    return "ERR_LatchFlowErr";
                case -47:
                    return "ERR_NoSystemAuthority";
                case -50:
                    return "ERR_KernelUpdateError";
                case -51:
                    return "ERR_KernelGeneralFunc";
                case -1000:
                    return "ERR_Win32Error 没有此类事件编号或WIN32_API错误，请与凌华科技FAE工作人员联系。";
                case -1001:
                    return "ERR_NoENIFile 生成ADLINK_Config2.xml文件（ENI文件）失败。";
                case -1002:
                    return "ERR_TimeOut_SetVoltageEnable 在伺服开启过程中设置电压启用超时。";
                case -1003:
                    return "ERR_TimeOut_SetReadyToSwitch 设置就绪以在伺服开启过程中切换超时。";
                case -1004:
                    return "ERR_TimeOut_SetShutdown 通过伺服开启过程设置关闭时间。";
                case -1005:
                    return "ERR_TimeOut_SetSwitchOn 设置伺服开启过程中的开启超时。";
                case -1006:
                    return "ERR_TimeOut_SetOperationEnable 通过伺服开启过程设置操作启用超时。";
                case -1007:
                    return "ERR_RegistryPath 系统注册表路径失败或没有注册表。";
                case -1008:
                    return "ERR_MasterNotOPState 主站未处于OP状态。";
                case -1009:
                    return "ERR_SlaveNotOPState 从站不处于OP状态。";
                case -1010:
                    return "ERR_SlaveTotalAxisNumber 扫描的EtherCAT从站轴数超过了PCIe-8334/8可以支持的最大轴数。";
                case -1011:
                    return "ERR_MissESIFileOrMissENIPath 缺少ESI文件或ENI路径。";
                case -1012:
                    return "ERR_MissConfig_1_Xml 缺少Config_1 xml文件。";
                case -1013:
                    return "ERR_CopyConfig_1_Xml_fail 复制Config_1 xml文件失败。";
                case -1014:
                    return "ERR_MissConfig_2_Xml 缺少Config_2 xml文件。";
                case -1015:
                    return "ERR_CopyConfig_2_Xml_fail 复制Config_2 xml文件失败。";
                case -2001:
                    return "MKERR_AXIS_INDEX 轴范围误差";
                case -2002:
                    return "MKERR_CHANNEL_INDEX 通道范围错误";
                case -2003:
                    return "MKERR_PARA_UNDEFINE 参数编号未定义";
                case -2004:
                    return "MKERR_PARA_FAULT 参数数据错误";
                case -2005:
                    return "MKERR_STATE_UNAVAILABLE 此状态无法执行此操作。（仅当轴处于空闲状态时才能设置命令位置）";
                case -2006:
                    return "MKERR_CHECKSUM 校验和错误（内部错误），（校验码错误）";
                case -2007:
                    return "MKERR_TIME_OUT 超时错误";
                case -2008:
                    return "MKERR_MEM_TEST 内存测试错误";
                case -2009:
                    return "MKERR_CTRL_CMD 未知命令或此状态不能接受此命令";
                case -2010:
                    return "MKERR_AXES_DIMENSION 轴的维度无效。";
                case -2011:
                    return "MKERR_MBUF_FULL 运动缓冲区已满";
                case -2012:
                    return "MKERR_NO_AVAILABLE_SPG 轴处于混合状态时，此函数不能接受（所有 spg都在忙）";
                case -2013:
                    return "MKERR_BLEND_PERCENT 转换参数“percent”超出范围。";
                case -2014:
                    return "MKERR_TRANSITION_MODE 转换模式未定义或不支持。";
                case -2015:
                    return "MKERR_COORD_TRANS_INDEX 转换矩阵列索引> MAX_AXES（无效）";
                case -2016:
                    return "MKERR_SINP_WIDTH Soft-inp 宽度无效（> = 0）";
                case -2017:
                    return "MKERR_SINP_STABLE_TIME Soft-inp 稳定时间无效（<65535）";
                case -2018:
                    return "MKERR_GANTRY_MASTER 龙门主轴无效";
                case -2019:
                    return "MKERR_FCMP_SIZE 比较日期的大小超出范围。";
                case -2020:
                    return "MKERR_VS_INVALID 开始速度无效，(vs >= 0)";
                case -2021:
                    return "MKERR_VE_INVALID 终止速度无效， (ve >= 0)";
                case -2022:
                    return "MKERR_VM_INVALID 最大速度无效，(vm > 0)";
                case -2023:
                    return "MKERR_ACC_INVALID 加速无效。(acc > 0)";
                case -2024:
                    return "MKERR_DEC_INVALID 减速无效。(dec > 0)";
                case -2025:
                    return "MKERR_S_INVALID S 无效。 ( 0 <= S <= 1 )";
                case -2026:
                    return "MKERR_SD_DEC_INVALID SD Dec 无效。(>0)";
                case -2027:
                    return "MKERR_AXES_OVERLAPPING 轴数不能重叠。";
                case -2028:
                    return "MKERR_BLEND_DISTANCE 转换参数“ ResidueDistance”不能＜0.0。";
                case -2029:
                    return "MKERR_ERROR_POS_LEVEL 错误位置检查级别必须> = 0.0";
                case -2030:
                    return "MKERR_WAIT_MOVE 轴运动时不接受等待模式";
                case -2031:
                    return "MKERR_AX_DISABLE 轴已禁用（伺服关闭）";
                case -2032:
                    return "MKERR_AX_ERROR 轴处于错误状态（AX_ERR_STOPPING/AX_ERR_STOPPED）。您应该重置错误状态。";
                case -2033:
                    return "MKERR_AX_MOVING 轴在运动中，命令无法接受（无法覆盖），轴必须具有相同的尺寸，相同的轴";
                case -2034:
                    return "MKERR_PRE_EVENT_DIST 事件发生前距离必须> = 0";
                case -2035:
                    return "MKERR_POST_EVENT_DIST 事件发生后距离必须> = 0";
                case -2040:
                    return "MKERR_ARC_PARA 该函数不能接受半圆弧或全圆弧参数";
                case -2041:
                    return "MKERR_ARC_FINAL_R finalR 不能为负值。";
                case -2042:
                    return "MKERR_ARC_NORMAL 法线向量无效。 或弧参数无效。";
                case -2050:
                    return "MKERR_GANTRY_DEV_PROTECT 龙门偏差保护值必须> = 0";
                case -2051:
                    return "MKERR_INVAILD_IN_GANTRY 龙门模式下不允许使用此命令。";
                case -2052:
                    return "MKERR_INVAILD_GEAR_MASTER 未定义齿轮主站";
                case -2053:
                    return "MKERR_ENGAGE_RATE ";
                case -2054:
                    return "MKERR_GEAR_RATIO ";
                case -2055:
                    return "MKERR_GEAR_ENABLE_MODE ";
                case -2056:
                    return "MKERR_GEAR_LOOP 不能是齿轮环。";
                case -2057:
                    return "MKERR_JOG_OFFSET 点动偏移量参数错误。 必须> 0";
                case -2062:
                    return "MKERR_DI_GROUP DI 组号错误。";
                case -2063:
                    return "MKERR_DI_CH DI 通道号错误。";
                case -2070:
                    return "MKERR_FILTER_COEFFICIENT 滤波系数错误";
                case -2071:
                    return "MKERR_FBK_VEL_COEFFICIENT ";
                case -2080:
                    return "MKERR_PTBUFF_AXIS_NOT_IDLE 轴现在处于运动状态，无法开始点缓冲";
                case -2081:
                    return "MKERR_PTBUFF_DIMENSION 无效的轴尺寸设置";
                case -2082:
                    return "MKERR_PTBUFF_AXIS_IN_USE 轴编号已被另一个 PTB 使用";
                case -2083:
                    return "MKERR_PTBUFF_NOT_ENABLE PTBUFF 未启用";
                case -2084:
                    return "MKERR_PTBUFF_FULL PTBUFF 已满";
                case -2085:
                    return "MKERR_PTBUFF_CURVE_TYPE 无效的运动曲线类型";
                case -2086:
                    return "MKERR_PTBUFF_DIMENSION_MISS 马赫线运动尺寸和 PTBUFF 尺寸";
                case -2087:
                    return "MKERR_PTBUFF_CURVE_DIMENSION 曲线类型及其尺寸不匹配";
                case -2088:
                    return "MKERR_PTBUFF_ABNORMAL_STOP 轴异常停止，请检查轴停止代码。";
                case -2089:
                    return "MKERR_PTBUFF_M_QUEUE_EXCEPTION 轴运动队列异常";
                case -2090:
                    return "MKERR_PTBUFF_AXIS_INDEX_INVAILD 目标轴索引超过 PTBUFF 尺寸";
                case -2091:
                    return "MKERR_PTBUFF_ID 无效的 PTBUFF ID。 检查输入参数。";
                case -2092:
                    return "MKERR_PTBUFF_CTRL_COMMAND 无效的 PTBUF 控制命令。检查输入参数。";
                case -2093:
                    return "MKERR_PTBUFF_AXES_IN_MOTION 启动 PTBUFF 时，检测到的 PTBUFF 轴在运动。";
                case -2094:
                    return "MKERR_PTBUFF_EXT_COMMANDS_NUM PTBUFF 附加命令必须<= 7";
                case -2095:
                    return "MKERR_PTBUFF_EXT_COMMAND_EMPTY 执行额外的命令，但命令队列为空。";
                case -2096:
                    return "MKERR_PTBUFF_EXT_COMMAND_FULL 推送额外的命令，但命令队列已满。";
                case -2097:
                    return "MKERR_PTBUFF_EXT_COMMNAD 无效的额外命令代码。";
                case -2098:
                    return "MKERR_PTBUFF_NOT_STOPPED 点缓冲区运行时命令无效。";
                case -2099:
                    return "MKERR_PTBUFF_DWELL_TIME 停留时间必须> = 0";
                case -2100:
                    return "MKERR_HS_UNKNOWN_CMD 握手数据错误。命令未知。";
                case -2101:
                    return "MKERR_HS_SIZE 握手数据错误。大小或尺寸无效。";
                case -2102:
                    return "MKERR_HS_SUB_ERRORS 子函数有错误。请检查子返回码";
                case -2201:
                    return "MKERR_DSP_SYSTEM_CONFIG DSP 初始化错误。 （请致电凌华研发人员）";
                case -2208:
                    return "MKERR_LOAD_CALIBRATION_DATA 负载校准数据失败。";
                case -2209:
                    return "MKERR_DPRAM_TEST_FAILED DPRAM 硬件测试失败。 （请致电凌华）";
                case -2210:
                    return "MKERR_FPGA_VERSION FPGA 版本已过时。 （请致电凌华）";
                case -2211:
                    return "MKERR_PSC_TIME_OUT ";
                case -2212:
                    return "MKERR_PLL_TIME_OUT";
                case -2213:
                    return "MKERR_PSC_PARAM_ERR";
                case -2300:
                    return "MKERR_MOTION_LOOP_TIMING 运动循环的时间超出范围。";
                case -2401:
                    return "MKERR_WRITE_ROM";
                case -2402:
                    return "MKERR_READ_ROM";
                case -2403:
                    return "MKERR_REF_5V ";
                case -2404:
                    return "MKERR_SET_AI_OFFSET ";
                case -2405:
                    return "MKERR_SET_AI_GAIN ";
                case -2406:
                    return "MKERR_SET_AO_OFFSET ";
                case -2407:
                    return "MKERR_SET_AO_GAIN ";
                case -2408:
                    return "MKERR_NO_CALIB EEPROM 中没有校准数据。 （并非 AO/AI 的所有增益/偏移都已校准）";
                case -2409:
                    return "MKERR_DATA_SIZE ";
                case -2410:
                    return "MKERR_BACKDOOR_PWD 后门密码错误。";
                case -2411:
                    return "MKERR_ROM_PROG_SIZE （闪存）数据字节数过多或偏移量超出范围。";
                case -2500:
                    return "MKERR_OVER_QUEUE_SIZE 队列大小小于输入数组";
                case -2600:
                    return "MKERR_FRP_STATE 频率响应过程的状态无效";
                case -2601:
                    return "MKERR_RCP_STATE 继电器控制过程的状态无效";
                case -2700:
                    return "MKERR_TASK_NUM 任务编号错误";
                case -2701:
                    return "MKERR_UNKNOWN_PROGRAM_REGISTER 未知程序寄存器。";
                case -2702:
                    return "MKERR_CANNOT_SET_REG_WHEN_PG_NOT_STOP 程序正在运行，您不能发出此命令";
                case -2703:
                    return "MKERR_OVER_STACK_SIZE 超出堆栈大小范围";
                case -2800:
                    return "MKERR_ACCESS_UNDEFINE 访问类型未定义";
                case -2801:
                    return "MKERR_ACCESS_DENIED 由于参数访问处于保存/加载过程中，因此被拒绝";
                case -2802:
                    return "MKERR_ERASE_SECTION 由于超时，擦除部分失败";
                case -2803:
                    return "MKERR_LAST_MARK_INVALID 加载数据错误； ; 闪存中的最后一个标记是错误的";
                case -2804:
                    return "MKERR_CHK_PARITY 加载数据错误； 奇偶校验位错误";
                case -2805:
                    return "MKERR_OVER_DATA_TYPE 加载数据错误； 超过最大数据类型定义";
                case -2806:
                    return "MKERR_OVER_PA_TYPE 加载数据错误； 超过最大参数类型定义";
                case -2807:
                    return "MKERR_OVER_MAX_BOARD 加载数据错误； 超过最大板参数定义";
                case -2808:
                    return "MKERR_OVER_MAX_AXIS 加载数据错误； 超过最大轴参数定义";
                case -2900:
                    return "MKERR_WDT1_STATE 看门狗定时器 1 已启用";
                case -2901:
                    return "MKERR_WDT1_PERIOD 看门狗定时器 1 的最大复位周期超出范围";
                case -4001:
                    return "EC_INIT_MASTER_ERR 初始化的 EtherCAT 主站错误";
                case -4011:
                    return "EC_GET_SLV_NUM_ERR 获取总从站编号错误";
                case -4012:
                    return "EC_CONFIG_MASTER_ERR 配置 EtherCAT 主站错误";
                case -4013:
                    return "EC_BUSCONFIG_MISMATCH 拓扑信息与当前数据不匹配";
                case -4014:
                    return "EC_CONFIGDATA_READ_ERR 读取配置数据错误";
                case -4015:
                    return "EC_ENI_NO_SAFEOP_OP_SUPPORT 不支持 safeop 和 op 状态";
                case -4021:
                    return "EC_CONFIG_DC_ERR 配置直流参数错误";
                case -4022:
                    return "EC_DCM_MODE_NO_SUPPORT 不支持 dcm 模式";
                case -4023:
                    return "EC_CONFIG_DCM_FEATURE_DISABLED dcm 功能已禁用";
                case -4024:
                    return "EC_CONFIG_DCM_ERR 配置 DCM 参数错误";
                case -4031:
                    return "EC_REG_CLIENT_ERR 注册客户端错误";
                case -4041:
                    return "EC_SET_INIT_STATE_ERR 将 EtherCAT 主站设置为初始状态错误";
                case -4042:
                    return "EC_SET_PREOP_STATE_ERR 将 EtherCAT 主站设置为操作前状态错误";
                case -4043:
                    return "EC_SET_SAFEOP_STATE_ERR 将 EtherCAT 主站设置为 safeop 状态错误";
                case -4044:
                    return "EC_SET_OP_STATE_ERR 将 EtherCAT 主站设置为操作状态错误";
                case -4051:
                    return "EC_DE_INIT_MASTER_ERR 初始化的 EtherCAT 主站错误";
                case -4061:
                    return "EC_ENI_FOPEN_ERR 无法打开 ENI 信息";
                case -4062:
                    return "EC_ENI_FREAD_ERR 无法读取 ENI 信息";
                case -4063:
                    return "EC_GEN_EBI_BUSSCAN_ERR 扫描 EtherCAT 总线错误";
                case -4081:
                    return "EC_WRONG_PORT_NO 输入错误的 EtherCAT 主站端口号";
                case -4091:
                    return "EC_GET_SLAVE_INFO_ERR 获取从站信息错误";
                case -4101:
                    return "EC_COE_SDO_UPLOAD_ERR 执行 sdo 上传输入数据错误";
                case -4102:
                    return "EC_COE_SDO_HOME_MODE_ERR 输入 ECAT 伺服归零模式错误或数据错误。";
                case -4103:
                    return "EC_COE_SDO_HOME_ACCDEC_ERR 输入 ECAT 伺服归零 ACC/DEC 错误或数据错误。";
                case -4104:
                    return "EC_COE_SDO_HOME_VM_SWITCH_ERR 输入 ECAT 伺服归零限位开关错误或数据错误。";
                case -4105:
                    return "EC_COE_SDO_HOME_VM_ZERO_ERR 输入 ECAT 伺服归零错误或错误数据。";
                case -4106:
                    return "EC_COE_SDO_HOME_OFFSET_ERR 输入 ECAT 伺服归零偏移错误或数据错误。";
                case -4107:
                    return "EC_CONTROL_WORD_HOME_ERR 输入 ECAT 伺服归零控制字错误或数据错误。";
                case -4108:
                    return "EC_COE_SDO_STOP_ERR 输入 ECAT 伺服归零停止错误或数据错误。";
                case -4109:
                    return "EC_CONTROL_WORD_STOP_ERR 输入 ECAT 伺服归零限位开关错误或数据错误。";
                case -4110:
                    return "EC_SET_OP_MODE_HOME_ERR 使用 ECAT 主进程设置 OP 模式错误。";
                case -4201:
                    return "EC_WRONG_SLAVE_NO 输入从站号超出限制";
                case -4202:
                    return "EC_WRONG_MODULE_NO 输入模块数量超出限制";
                case -4203:
                    return "EC_WRONG_AI_CHANNEL_NO 输入的 AI 通道数超出限制";
                case -4204:
                    return "EC_WRONG_AO_CHANNEL_NO 输入的 AO 通道数超出限制";
                case -4205:
                    return "EC_COE_SDO_DOWNLOAD_ERR 执行 sdo 下载输入数据错误";
                case -4301:
                    return "EC_COE_OD_INIT_ERR 初始化对象字典时创建内存错误";
                case -4302:
                    return "EC_COE_GET_OD_NUM_ERR 获取对象字典编号错误";
                case -4303:
                    return "EC_COE_GET_OD_NUM_LAST 输入对象字典号是最后一个";
                case -4304:
                    return "EC_COE_GET_OD_DESC_ERR 获取对象字典描述错误";
                case -4305:
                    return "EC_COE_GET_OD_DESC_ENTRY_ERR 获取对象字典描述输入错误";
                case -4306:
                    return "EC_COE_GET_OD_STATUS_PEND 获取对象字典待处理";
                case -4503:
                    return "EC_DUPLICATE_SLAVE_ID_ERR 从站 ID 号重复出现";
                case -4504:
                    return "EC_GET_SLAVE_REGISTER_ERR 获取从属寄存器错误";
                case -4505:
                    return "EC_SET_SLAVE_REGISTER_ERR 设置从寄存器错误";
                default:
                    return "未知的錯誤代碼 : " + errorCode;
            }
        }
    }
}