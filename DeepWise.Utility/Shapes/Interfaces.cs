using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Shapes
{
    /// <summary>
    /// 表示具有封閉區域的二維形狀。
    /// </summary>
    public interface IAreal : IShape
    {
        double Area { get; }
        double Perimeter { get; }
        bool Contains(Point p);
    }

    /// <summary>
    /// 表示可線掃描的二維面積區域。(作為邊緣偵測的掃描圖形)
    /// </summary>
    public interface IScannable
    {
        (Point from, Point to)[] GetLines();
    }

    /// <summary>
    /// 代表二維空間中的基本集合解構。
    /// (例 : <see cref="Point"/>、<see cref="Line"/>、<see cref="RectRotatable"/>等等...)
    /// </summary>
    public interface IShape
    {
        /// <summary>
        /// 判斷點是否在輪廓上。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool OnContour(Point p);
        /// <summary>
        /// 傳回位移特定向量後的新形狀。
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        IShape Move(Vector offset);
        /// <summary>
        /// 傳回相較於於特定基準點的新形狀。
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        IShape RelatvieTo(Point origin, double angle);
        /// <summary>
        /// 傳回附加於新基準點的新形狀。
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        IShape AppendOn(Point origin, double angle);
    }

}
