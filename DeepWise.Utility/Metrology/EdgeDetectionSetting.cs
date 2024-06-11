using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Metrology
{
    /// <summary>
    /// 邊緣偵測的次像素插值模式。
    /// </summary>
    public enum SubpixelsInterpolationMode : byte 
    { 
        /// <summary>
        /// 拋物線擬和。
        /// </summary>
        Parabola,
        /// <summary>
        /// 權重插值。
        /// </summary>
        Weight
    };

    public enum EdgeContrast
    {
        Small,
        Median,
        Larget,
    }

    [Flags,JsonConverter(typeof(StringEnumConverter))]
    /// <summary>
    /// 邊界點的搜尋模式。
    /// </summary>
    public enum EdgelSearchOptions : byte 
    {
        /// <summary>
        /// 峰值搜尋(僅搜尋由暗至亮的邊界)
        /// </summary>
        Crest = 0b01,
        /// <summary>
        /// 谷值搜尋(僅搜尋由亮至暗的邊界)
        /// </summary>
        Trough = 0b10,
        /// <summary>
        /// 僅搜尋找到的第一個峰谷值
        /// </summary>
        First = 0b100,

        /// <summary>
        /// 搜尋最大的峰或谷值
        /// </summary>
        Largest = 0b1000,
    };

    [Serializable]
    public class EdgeDetectionSetting
    {
        public EdgelSearchOptions SearchType { get; set; } = EdgelSearchOptions.Crest | EdgelSearchOptions.Largest;
        //public EdgelSearchMode SearchMode { get; set; } = EdgelSearchMode.Largest;
        public float MinimumEdgeValue { get; set; } = 10;
        public float MaximumEdgeValue { get; set; } = float.PositiveInfinity;

        public bool BreakIntoGroups { get; set; } = true;
        public double BreakDistance { get; set; } = 1;

        public bool ExcludeOutliers { get; set; } = true;
        public float ExcludeRadius { get; set; } = 2;

        public static EdgeDetectionSetting Default => _defaultSetting;
        private static EdgeDetectionSetting _defaultSetting = new EdgeDetectionSetting() { SearchType = EdgelSearchOptions.Crest| EdgelSearchOptions.First};
    }

}
