using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;
using DeepWise.Localization;
using DeepWise.Properties;

namespace DeepWise.Shapes
{
    [Serializable]
    [DebuggerDisplay("Corners={Corners}")]
    [LocalizedDisplayName(nameof(Polygon), typeof(Resources))]
    public struct Polygon : IAreal, ISerializable
    {
        [LocalizedDisplayName(nameof(Polygon) + "." + nameof(Corners), typeof(Resources))]
        public Point[] Corners { get; set; }
        [LocalizedDisplayName(nameof(Perimeter), typeof(Resources))]
        public double Perimeter
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < Corners.Length - 1; i++) sum += (Corners[i] - Corners[i + 1]).Length;
                sum += (Corners[Corners.Length - 1] - Corners[0]).Length;
                return sum;
            }
        }
        [LocalizedDisplayName(nameof(Area), typeof(Resources))]
        public double Area => double.NaN;

        IShape IShape.Move(Vector offset) => Move(offset);
        IShape IShape.RelatvieTo(Point origin, double angle) => RelativeTo(origin, angle);
        IShape IShape.AppendOn(Point origin, double angle) => Append(origin, angle);
        //public Polygon Move(Vector offset) => new Polygon(Corners.Select(p => p + offset));
        public Polygon Move(Vector offset)
        {
            for (int i = 0; i < Corners.Length; i++)
                Corners[i] += offset;
            return new Polygon(Corners);
        }
        public Polygon RelativeTo(Point origin, double angle) => new Polygon(Corners.Select(p => p.RelativeTo(origin, angle)));
        public Polygon Append(Point origin, double angle) => new Polygon(Corners.Select(p => p.AppendOn(origin, angle)));
        //Constructor
        public Polygon(IEnumerable<Point> corners)
        {
            Corners = corners.ToArray();
        }
        public Polygon(SerializationInfo info, StreamingContext context)
        {
            Corners = (Point[])info.GetValue(nameof(Corners), typeof(Point[]));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Corners), Corners);
        }

        public static bool IsNaN(Rect rect) => double.IsNaN(rect.Width) | double.IsNaN(rect.Height) | double.IsNaN(rect.X) | double.IsNaN(rect.Y);

        public bool Contains(Point p)
        {
            if (Corners.Length < 3) return false;
            Point p1, p2;
            bool inside = false;

            var oldPoint = Corners[Corners.Length - 1];

            for (int i = 0; i < Corners.Length; i++)
            {
                Point newPoint = Corners[i];

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
        public static bool IsPointInPolygon(Point[] polygon, Point point)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
        public bool OnContour(Point p)
        {
            for (int i = 0; i < Corners.Length - 2; i++) if (new Segment(Corners[i], Corners[i + 1]).OnContour(p)) return true;
            if (new Segment(Corners[Corners.Length - 1], Corners[0]).OnContour(p)) return true;
            return false;
        }
    }
}
