using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionControllers.Motion
{
    /// <summary>
    ///  从站信息的结构。
    /// </summary>
    public struct PEC_MODULE_INFO
    {
        /// <summary>
        /// 从站的供应商ID编号。
        /// </summary>
        public int VendorID { get; set; }
        /// <summary>
        /// 从站的产品编号。
        /// </summary>
        public int ProductCode { get; set; }
        /// <summary>
        /// 从站的修订号。
        /// </summary>
        public int RevisionNo { get; set; }
        /// <summary>
        /// 从站的总轴数。
        /// </summary>
        public int TotalAxisNum { get; set; }
        /// <summary>
        /// 自动从站ID模式的轴ID编号。
        /// </summary>
        public int[] Axis_ID { get; set; }// = new int[64];
        /// <summary>
        /// 手动从站ID模式的轴ID编号。
        /// </summary>
        public int[] Axis_ID_manual { get; set; }// = new int[64];
        /// <summary>
        /// 子模块ID按顺序排列。
        /// </summary>
        public int[] All_ModuleType { get; set; }// = new int[32];
        /// <summary>
        /// 从站中数字输入模块的数量。
        /// </summary>
        public int DI_ModuleNum { get; set; }
        /// <summary>
        /// 从站中数字输入模块的类型。
        /// </summary>
        public int[] DI_ModuleType { get; set; }//[32]
        /// <summary>
        /// 从站中数字输出模块的数量。
        /// </summary>
        public int DO_ModuleNum { get; set; }
        /// <summary>
        /// 从站中数字输出模块的类型。
        /// </summary>
        public int[] DO_ModuleType { get; set; }//[32]
        /// <summary>
        /// 从站中模拟输入模块的数量。
        /// </summary>
        public int AI_ModuleNum { get; set; }
        /// <summary>
        /// 从站中模拟输入模块的类型。
        /// </summary>
        public int[] AI_ModuleType { get; set; }//[32]
        /// <summary>
        /// 从站中模拟输出模块的数量。
        /// </summary>
        public int AO_ModuleNum { get; set; }
        /// <summary>
        /// 从站中模拟输出模块的类型。
        /// </summary>
        public int[] AO_ModuleType { get; set; }//[32]
        /// <summary>
        /// 已保留。
        /// </summary>
        public char[] Name { get; set; } //[128]
    }
}
