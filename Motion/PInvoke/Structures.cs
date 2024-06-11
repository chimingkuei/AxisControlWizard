using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;

namespace ADLINK_DEVICE
{

    //ADLINK Structure++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [StructLayout(LayoutKind.Sequential)]
    public struct STR_SAMP_DATA_4CH
    {
        public int tick;
        public int data0; //Total channel = 4
        public int data1;
        public int data2;
        public int data3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOVE_PARA
    {
        public Int16 i16_accType;	//Axis parameter
        public Int16 i16_decType;	//Axis parameter
        public int i32_acc;		//Axis parameter
        public int i32_dec;		//Axis parameter
        public int i32_initSpeed;	//Axis parameter
        public int i32_maxSpeed;	//Axis parameter
        public int i32_endSpeed;    //Axis parameter
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT_DATA
    {
        public int i32_pos;     // Position data (relative or absolute) (pulse)
        public Int16 i16_accType;   // Acceleration pattern 0: T-curve,  1: S-curve
        public Int16 i16_decType;   // Deceleration pattern 0: T-curve,  1: S-curve
        public int i32_acc;     // Acceleration rate ( pulse / ss )
        public int i32_dec;     // Deceleration rate ( pulse / ss )
        public int i32_initSpeed;   // Start velocity	( pulse / s )
        public int i32_maxSpeed;    // Maximum velocity  ( pulse / s )
        public int i32_endSpeed;    // End velocity		( pulse / s )
        public int i32_angle;       // Arc move angle    ( degree, -360 ~ 360 )
        public int u32_dwell;       // Dwell times       ( unit: ms )
        public int i32_opt;    	// Option //0xABCD , D:0 absolute, 1:relative
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PNT_DATA
    {
        // Point table structure (One dimension)
        public UInt32 u32_opt;        // option, [0x00000000,0xFFFFFFFF]
        public int i32_x;          // x-axis component (pulse), [-2147483648,2147484647]
        public int i32_theta;      // x-y plane arc move angle (0.001 degree), [-360000,360000]
        public int i32_acc;        // acceleration rate (pulse/ss), [0,2147484647]
        public int i32_dec;        // deceleration rate (pulse/ss), [0,2147484647]
        public int i32_vi;         // initial velocity (pulse/s), [0,2147484647]
        public int i32_vm;         // maximum velocity (pulse/s), [0,2147484647]
        public int i32_ve;         // ending velocity (pulse/s), [0,2147484647]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PNT_DATA_2D
    {
        public UInt32 u32_opt;        // option, [0x00000000,0xFFFFFFFF]
        public int i32_x;          // x-axis component (pulse), [-2147483648,2147484647]
        public int i32_y;          // y-axis component (pulse), [-2147483648,2147484647]
        public int i32_theta;      // x-y plane arc move angle (0.000001 degree), [-360000,360000]
        public int i32_acc;        // acceleration rate (pulse/ss), [0,2147484647]
        public int i32_dec;        // deceleration rate (pulse/ss), [0,2147484647]
        public int i32_vi;         // initial velocity (pulse/s), [0,2147484647]
        public int i32_vm;         // maximum velocity (pulse/s), [0,2147484647]
        public int i32_ve;         // ending velocity (pulse/s), [0,2147484647]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PNT_DATA_2D_F64
    {
        public UInt32 u32_opt;        // option, [0x00000000,0xFFFFFFFF]
        public Double f64_x;          // x-axis component (pulse), [-2147483648,2147484647]
        public Double f64_y;          // y-axis component (pulse), [-2147483648,2147484647]
        public Double f64_theta;      // x-y plane arc move angle (0.000001 degree), [-360000,360000]
        public Double f64_acc;        // acceleration rate (pulse/ss), [0,2147484647]
        public Double f64_dec;        // deceleration rate (pulse/ss), [0,2147484647]
        public Double f64_vi;         // initial velocity (pulse/s), [0,2147484647]
        public Double f64_vm;         // maximum velocity (pulse/s), [0,2147484647]
        public Double f64_ve;         // ending velocity (pulse/s), [0,2147484647]
        public Double f64_sf;              // s-factor [0.0 ~ 1.0]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PNT_DATA_4DL
    {
        public UInt32 u32_opt;        // option, [0x00000000,0xFFFFFFFF]
        public int i32_x;          // x-axis component (pulse), [-2147483648,2147484647]
        public int i32_y;          // y-axis component (pulse), [-2147483648,2147484647]
        public int i32_z;          // z-axis component (pulse), [-2147483648,2147484647]
        public int i32_u;          // u-axis component (pulse), [-2147483648,2147484647]
        public int i32_acc;        // acceleration rate (pulse/ss), [0,2147484647]
        public int i32_dec;        // deceleration rate (pulse/ss), [0,2147484647]
        public int i32_vi;         // initial velocity (pulse/s), [0,2147484647]
        public int i32_vm;         // maximum velocity (pulse/s), [0,2147484647]
        public int i32_ve;         // ending velocity (pulse/s), [0,2147484647]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT_DATA_EX
    {
        public int i32_pos;           //(Center)Position data (could be relative or absolute value) 
        public Int16 i16_accType;       //Acceleration pattern 0: T curve, 1:S curve   
        public Int16 i16_decType;       // Deceleration pattern 0: T curve, 1:S curve 
        public int i32_acc;           //Acceleration rate ( pulse / sec 2 ) 
        public int i32_dec;           //Deceleration rate ( pulse / sec 2  ) 
        public int i32_initSpeed;     //Start velocity ( pulse / s ) 
        public int i32_maxSpeed;      //Maximum velocity    ( pulse / s ) 
        public int i32_endSpeed;      //End velocity  ( pulse / s )     
        public int i32_angle;         //Arc move angle ( degree, -360 ~ 360 ) 
        public UInt32 u32_dwell;        //dwell times ( unit: ms ) *Divided by system cycle time. 
        public int i32_opt;           //Point move option. (*) 
        public int i32_pitch;			// pitch for helical move
        public int i32_totalheight; // total hight
        public Int16 i16_cw;			// cw or ccw
        public Int16 i16_opt_ext;       // option extend
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct POINT_DATA2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] i32_pos;                   // Position data (relative or absolute) (pulse)

        public int i32_initSpeed;                 // Start velocity	( pulse / s ) 
        public int i32_maxSpeed;                  // Maximum velocity  ( pulse / s ) 
        public int i32_angle;                     // Arc move angle    ( degree, -360 ~ 360 ) 
        public UInt32 u32_dwell;                  // Dwell times       ( unit: ms ) 
        public int i32_opt;                   // Option //0xABCD , D:0 absolute, 1:relative
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct POINT_DATA3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] i32_pos;

        public int i32_maxSpeed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] i32_endPos;

        public int i32_dir;
        public int i32_opt;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VAO_DATA
    {
        //Param
        public int outputType;  //Output type, [0, 3]
        public int inputType;       //Input type, [0, 1]
        public int config;      //PWM configuration according to output type
        public int inputSrc;        //Input source by axis, [0, 0xf]

        //Mapping table
        public int minVel;                           //Minimum linear speed, [ positive ]
        public int velInterval;                      //Speed interval, [ positive ]
        public int totalPoints;                      //Total points, [1, 32]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] mappingDataArr;     //mapping data array
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PTSTS
    {
        public UInt16 BitSts;           //b0: Is PTB work? [1:working, 0:Stopped]
                                        //b1: Is point buffer full? [1:full, 0:not full]
                                        //b2: Is point buffer empty? [1:empty, 0:not empty]
                                        //b3, b4, b5: Reserved for future
                                        //b6~: Be always 0
        public UInt16 PntBufFreeSpace;
        public UInt16 PntBufUsageSpace;
        public UInt32 RunningCnt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LPSTS
    {
        public UInt32 MotionLoopLoading;
        public UInt32 HostLoopLoading;
        public UInt32 MotionLoopLoadingMax;
        public UInt32 HostLoopLoadingMax;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_DATA
    {
        public UInt16 ServoOffCondition;
        public Double DspCmdPos;
        public Double DspFeedbackPos;
        public Double FpgaCmdPos;
        public Double FpgaFeedbackPos;
        public Double FpgaOutputVoltage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_STATE
    {
        public UInt16 AxisState;
        public UInt16 GroupState;
        public UInt16 AxisSuperState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PTDWL
    {
        public Double DwTime; //Unit is ms
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PTLINE
    {
        public int Dim;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public Double[] Pos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PTA2CA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Byte[] Index;       //Index X,Y

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Double[] Center;  //Center Arr

        public Double Angle;                          //Angle
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PTA2CE
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Byte[] Index; //Index X,Y

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Double[] Center; //

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Double[] End; // 

        public Int16 Dir; //
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    [StructLayout(LayoutKind.Sequential)] // revised 20160801
    public struct PTA3CA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Byte[] Index;      //Index X,Y

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Center; //Center Arr

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Normal; //Normal Arr

        public Double Angle;                         //Angle
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    [StructLayout(LayoutKind.Sequential)] // revised 20160801
    public struct PTA3CE
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Byte[] Index;      //Index X,Y

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Center; //Center Arr

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] End;    //End Arr

        public Int16 Dir; //
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    [StructLayout(LayoutKind.Sequential)] // revised 20160801
    public struct PTHCA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Byte[] Index;      //Index X,Y

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Center; //Center Arr

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Normal; //Normal Arr

        public Double Angle;                         //Angle
        public Double DeltaH;
        public Double FinalR;
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    [StructLayout(LayoutKind.Sequential)] // revised 20160801
    public struct PTHCE
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Byte[] Index;      //Index X,Y

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Center; //Center Arr

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] Normal; //Normal Arr

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Double[] End;    //End Arr

        public Int16 Dir; //
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PTINFO
    {
        public int Dimension;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public int[] AxisArr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct STR_SAMP_DATA_8CH
    {
        public int tick;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] data; //Total channel = 8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SAMP_PARAM
    {
        public int rate;                            //Sampling rate
        public int edge;                            //Trigger edge
        public int level;                           //Trigger level
        public int trigCh;					    //Trigger channel
        public int[,] sourceByCh;

        public SAMP_PARAM(Int32 x, int y) : this()
        {
            sourceByCh = new int[x, y];
        }
        //Sampling source by channel, named sourceByCh[a][b], 
        //a: Channel
        //b: 0: Sampling source 1: Sampling axis
        //Sampling source: F64 data occupies two channel, I32 data occupies one channel.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOG_DATA
    {
        public Int16 i16_jogMode;	  // Jog mode. 0:Free running mode, 1:Step mode
        public Int16 i16_dir;		  // Jog direction. 0:positive, 1:negative direction
        public Int16 i16_accType;	  // Acceleration pattern 0: T-curve,  1: S-curve
        public int i32_acc;		  // Acceleration rate ( pulse / ss )
        public int i32_dec;		  // Deceleration rate ( pulse / ss )
        public int i32_maxSpeed;	  // Positive value, maximum velocity  ( pulse / s )
        public int i32_offset;	  // Positive value, a step (pulse)
        public int i32_delayTime;  // Delay time, ( range: 0 ~ 65535 millisecond, align by cycle time)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HOME_PARA
    {
        public ushort u8_homeMode;
        public ushort u8_homeDir;
        public ushort u8_curveType;
        public int i32_orgOffset;
        public int i32_acceleration;
        public int i32_startVelocity;
        public int i32_maxVelocity;
        public int i32_OrgVelocity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POS_DATA_2D
    {
        public UInt32 u32_opt;        // option, [0x00000000,0xFFFFFFFF]
        public int i32_x;          // x-axis component (pulse), [-2147483648,2147484647]
        public int i32_y;          // y-axis component (pulse), [-2147483648,2147484647]
        public int i32_theta;      // x-y plane arc move angle (0.000001 degree), [-360000,360000]
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ASYNCALL
    {
        public void* h_event;
        public int i32_ret;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_INFO
    {
        public UInt16 State;        // 
        public UInt16 RunTimeErr;     // 
        public UInt16 IP;
        public UInt16 SP;
        public UInt16 BP;
        public UInt16 MsgQueueSts;
    }

    //New ADCNC structure define
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    public struct POS_DATA_2D_F64
    {
        /* This structure extends original point data contents from "I32" to "F64" 
										   for internal computation. It's important to prevent data overflow. */
        public UInt32 u32_opt;        // option, [0x00000000, 0xFFFFFFFF]
        public Double f64_x;          // x-axis component (pulse), [-9223372036854775808, 9223372036854775807]
        public Double f64_y;          // y-axis component (pulse), [-9223372036854775808, 9223372036854775807]
        public Double f64_theta;      // x-y plane arc move angle (0.000001 degree), [-360000, 360000]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POS_DATA_2D_RPS
    {
        /* This structure adds another variable to record what point was be saved */
        public UInt32 u32_opt;        // option, [0x00000000, 0xFFFFFFFF]
        public int i32_x;          // x-axis component (pulse), [-2147483648, 2147483647]
        public int i32_y;          // y-axis component (pulse), [-2147483648, 2147483647]
        public int i32_theta;      // x-y plane arc move angle (0.000001 degree), [-360000, 360000]
        public UInt32 crpi;              // current reading point index
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POS_DATA_2D_F64_RPS
    {
        /* This structure adds another variable to record what point was be saved */
        public UInt32 u32_opt;        // option, [0x00000000, 0xFFFFFFFF]
        public Double f64_x;          // x-axis component (pulse), [-2147483648, 2147483647]
        public Double f64_y;          // y-axis component (pulse), [-2147483648, 2147483647]
        public Double f64_theta;      // x-y plane arc move angle (0.000001 degree), [-360000, 360000]
        public UInt32 crpi;               // current reading point index
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PNT_DATA_2D_EXT
    {
        public UInt32 u32_opt;        // option, [0x00000000,0xFFFFFFFF]
        public Double f64_x;          // x-axis component (pulse), [-2147483648,2147484647]
        public Double f64_y;          // y-axis component (pulse), [-2147483648,2147484647]
        public Double f64_theta;      // x-y plane arc move angle (0.000001 degree), [-360000,360000]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Double[] f64_acc; // acceleration rate (pulse/ss), [0,2147484647]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Double[] f64_dec; // deceleration rate (pulse/ss), [0,2147484647]		

        public int crossover;
        public int Iboundary;       // initial boundary

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Double[] f64_vi; // initial velocity (pulse/s), [0,2147484647]

        public UInt32 vi_cmpr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Double[] f64_vm; // maximum velocity (pulse/s), [0,2147484647]

        public UInt32 vm_cmpr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Double[] f64_ve; // ending velocity (pulse/s), [0,2147484647]

        public UInt32 ve_cmpr;
        public int Eboundary;       // end boundary		
        public Double f64_dist;     // point distance
        public Double f64_angle;        // path angle between previous & current point		
        public Double f64_radius;       // point radiua (used in arc move)
        public int i32_arcstate;
        public UInt32 spt;          // speed profile type

        // unit time measured by DSP sampling period
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Double[] t;

        // Horizontal & Vertical line flag
        public int HorizontalFlag;
        public int VerticalFlag;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DO_DATA_EX
    {
        public UInt32 Do_ValueL;        //bit[0~31]
        public UInt32 Do_ValueH;        //bit[32~63]
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DI_DATA_EX
    {
        public UInt32 Di_ValueL;        //bit[0~31]
        public UInt32 Di_ValueH;        //bit[32~63]
    }

    //**********************************************
    // New header functions; 20151102
    //**********************************************
    [StructLayout(LayoutKind.Sequential)]
    public struct MCMP_POINT
    {
        public Double axisX; // x axis data for multi-dimension comparator 0
        public Double axisY; // y axis data for multi-dimension comparator 1
        public Double axisZ; // z axis data for multi-dimension comparator 2
        public Double axisU; // u axis data for multi-dimension comparator 3
        public UInt32 chInBit; // pwm output channel in bit format; 20150609
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    public struct EC_MODULE_INFO
    {
        public int VendorID;
        public int ProductCode;
        public int RevisionNo;
        public int TotalAxisNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public int[] Axis_ID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public int[] Axis_ID_manual;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] All_ModuleType;

        public int DI_ModuleNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] DI_ModuleType;

        public int DO_ModuleNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] DO_ModuleType;

        public int AI_ModuleNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] AI_ModuleType;

        public int AO_ModuleNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] AO_ModuleType;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EC_Sub_MODULE_INFO
    {
        public int VendorID;
        public int ProductCode;
        public int RevisionNo;
        public int TotalSubModuleNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] SubModuleID;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EC_Sub_MODULE_OD_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public Byte[] DataName;

        public int BitLength;
        public int DataType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] DataTypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PDO_OFFSET
    {
        public UInt16 DataType;
        public UInt32 ByteSize;
        public UInt32 ByteOffset;
        public UInt32 Index;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public Byte[] NameArr;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OD_DESC_ENTRY
    {
        public UInt32 DataTypeNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] DataTypeName;

        public UInt32 BitLen;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] Description;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] Access;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] PdoMapInfo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] UnitType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] DefaultValue;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] MinValue;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] MaxValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Speed_profile
    {
        public int VS;      // start velocity ,range 1 ~ 4,000,000 (pulse)
        public int Vmax;        // Maximum  velocity ,range 1 ~ 4,000,000
        public int Acc;     // Acceleration ,range 1 ~ 500000000
        public int Dec;     // Deceleration ,range 1 ~ 500000000
        public Double s_factor; // range 0 ~ 10

    }
    //	For latch function, 2019.06.10
    [StructLayout(LayoutKind.Sequential)]
    public struct LATCH_POINT
    {
        public Double position; 		// Latched position
        public int ltcSrcInBit; 	// Latch source: bit 0~7: DI; bit 8~11: trigger channel
    }
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++			
    
}