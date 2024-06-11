using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using DeepWise.Shapes;
namespace DeepWise
{

    /// <summary>
    /// 用來轉換圖面座標(二維)與世界座標(三維)之間的點。
    /// </summary>
    [Serializable]
    public class CoordinateTransformator : ISerializable
    {
        public OrderType OrderMode { get; set; } = OrderType.L2;

        public Point3D T(Point p)
        {
            IEnumerable<PsPair> list;
            switch (OrderMode)
            {
                case OrderType.L1:
                    list = Pairs.OrderBy(pair => (pair.GraphicPoint - p).L1Distance);
                    break;
                case OrderType.L2:
                    list = Pairs.OrderBy(pair => (pair.GraphicPoint - p).LengthSquared);
                    break;
                default:
                    throw new NotImplementedException();
            }
            var closest = list.Take(2).ToArray();

            double dx = closest[1].GraphicPoint.X - closest[0].GraphicPoint.X, dy = closest[1].GraphicPoint.Y - closest[0].GraphicPoint.Y;
            var val = closest[0].GraphicPoint.X * closest[1].GraphicPoint.Y - closest[0].GraphicPoint.Y * closest[1].GraphicPoint.X;
            var d2 = dx * dx + dy * dy;
            var th = 1600 * d2;
            var pair2 = list.First(pair => 
            {
                var d = pair.GraphicPoint.X * dy - pair.GraphicPoint.Y * dx - val;
                return d * d > th;
            });

            return Interpolate(p, closest[0], closest[1], pair2);
        }
        public Point I(Point3D p)
        {
            throw new NotImplementedException();
        }

        [Obsolete("尚未測試")]
        public double GetAngle(Point p, double radian)
        {
            var list = Pairs.OrderBy(pair => (pair.GraphicPoint - p).LengthSquared).ToList();
            PsPair pair0 = list[0], pair1 = list[1];

            double dx = pair1.GraphicPoint.X - pair0.GraphicPoint.X, dy = pair1.GraphicPoint.Y - pair0.GraphicPoint.Y;
            var val = pair0.GraphicPoint.X * pair1.GraphicPoint.Y - pair0.GraphicPoint.Y * pair1.GraphicPoint.X;
            var d2 = dx * dx + dy * dy;
            var th = 1600 * d2;
            bool Predicate(PsPair pair)
            {
                var d = pair.GraphicPoint.X * dy - pair.GraphicPoint.Y * dx - val;
                return d * d > th;
            }
            var pair2 = list.First(Predicate);

            Point3D p0 = Interpolate(p, pair0, pair1, pair2);
            Point3D p1 = Interpolate(p + Vector.FormAngle(radian), pair0, pair1, pair2);
            return (new Vector(p1.X - p0.X, p1.Y - p0.Y)).Angle / Math.PI * 180;
        }

        static Point Interpolate(Point3D p, PsPair refP_0, PsPair refP_1, PsPair refP_2)
        {
            throw new NotImplementedException();
        }
        static Point3D Interpolate(Point p, PsPair refP_1, PsPair refP_2, PsPair refP_3)
        {
            double xp01 = Vector.CrossProduct(refP_1.GraphicPoint, refP_2.GraphicPoint);
            double xp12 = Vector.CrossProduct(refP_2.GraphicPoint, refP_3.GraphicPoint);
            double xp20 = Vector.CrossProduct(refP_3.GraphicPoint, refP_1.GraphicPoint);
            var ap = xp01 + xp12 + xp20;

            double x0 = Vector.CrossProduct(p, refP_1.GraphicPoint);
            double x1 = Vector.CrossProduct(p, refP_2.GraphicPoint);
            double x2 = Vector.CrossProduct(p, refP_3.GraphicPoint);
            double a12 = x1 + xp12 - x2;
            double a20 = x2 + xp20 - x0;
            double a01 = x0 + xp01 - x1;
            double x = (refP_1.WorldPoint.X * a12 + refP_2.WorldPoint.X * a20 + refP_3.WorldPoint.X * a01) / ap;
            double y = (refP_1.WorldPoint.Y * a12 + refP_2.WorldPoint.Y * a20 + refP_3.WorldPoint.Y * a01) / ap;
            double z = (refP_1.WorldPoint.Z * a12 + refP_2.WorldPoint.Z * a20 + refP_3.WorldPoint.Z * a01) / ap;
            return new Point3D(x, y, z);
        }

        [Obsolete("Use CoordinateTransformator(IEnumerable<PsPair> pairs) instead.")]
        public CoordinateTransformator(IEnumerable<PsPair> pairs, int column, int row)
        {
            var length = pairs.Count();
            if (pairs == null || length == 0) throw new Exception("陣列大小不可為空。");
            if (pairs == null || length < 3) throw new Exception("陣列大小必須大於3。");
            if (column * row != length) throw new Exception("陣列大小與行列數不符。");
            //this.Column = column;
            //this.Row = row;
            this.Pairs = pairs.ToList();//MakeCopy
        }
        public CoordinateTransformator(IEnumerable<PsPair> pairs)
        {
            var length = pairs.Count();
            if (pairs == null || length == 0) throw new Exception("陣列大小不可為空。");
            if (pairs == null || length < 3) throw new Exception("陣列大小必須大於3。");
            //this.Column = -1;
            //this.Row = -1;
            this.Pairs = pairs.ToList();//MakeCopy
        }

        public static CoordinateTransformator Load(string fileName) => JsonConvert.DeserializeObject<CoordinateTransformator>(File.ReadAllText(fileName));
        public void Save(string path) => File.WriteAllText(path, JsonConvert.SerializeObject(this));

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue(nameof(Column), Column);
            //info.AddValue(nameof(Row), Row);
            info.AddValue(nameof(Pairs), Pairs);
        }
        public CoordinateTransformator(SerializationInfo info, StreamingContext context)
        {
            //Column = (int)info.GetValue(nameof(Column), typeof(int));
            //Row = (int)info.GetValue(nameof(Row), typeof(int));
            Pairs = (List<PsPair>)info.GetValue(nameof(Pairs), typeof(List<PsPair>));
        }


        private List<PsPair> Pairs { get; } = new List<PsPair>();
    }

    /// <summary>
    /// 排序類型。
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Sumation of abs values
        /// </summary>
        L1,
        /// <summary>
        /// Sumation of sqrt roots
        /// </summary>
        L2
    }

    /// <summary>
    /// 座標轉換的點組合。
    /// </summary>
    [Serializable]
    public struct PsPair
    {
        public PsPair(Point graphicPoint, Point3D worldPoint)
        {
            GraphicPoint = graphicPoint;
            WorldPoint = worldPoint;
        }
        /// <summary>
        /// 圖面座標的點。
        /// </summary>
        public Point GraphicPoint;
        /// <summary>
        /// 世界座標的點。
        /// </summary>
        public Point3D WorldPoint;
    }

}
