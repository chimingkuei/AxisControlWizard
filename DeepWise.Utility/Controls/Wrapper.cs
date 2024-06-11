using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepWise.Controls
{
    public class StringWrapper
    {
        public StringWrapper(string str)
        {
            Text = str;
        }
        public string Text { get; set; }
    }

    public class PointWrapper
    {
        public PointWrapper(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }
    public class Point3DWrapper
    {
        public Point3DWrapper(Point3D p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
