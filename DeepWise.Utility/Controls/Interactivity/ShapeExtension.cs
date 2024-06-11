using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DeepWise.Controls.Interactivity
{
    public static class ShapeExtension
    {
        public static void SetLocation(this Shape shape, Point p)
        {
            if (shape is Line) throw new Exception("Cant set location of a line");
            //aligment to center of a pixel
            Canvas.SetLeft(shape, p.X + 0.5);
            Canvas.SetTop(shape, p.Y + 0.5);
        }

        public static void SetLocation(this Shape shape, double x,double y)
        {
            if (shape is Line) throw new Exception("Cant set location of a line");
            //aligment to center of a pixel
            Canvas.SetLeft(shape, x + 0.5);
            Canvas.SetTop(shape, y + 0.5);
        }

        public static void SetLocation(this Line line,Point p0, Point p1)
        {
            line.X1 = p0.X + 0.5;
            line.Y1 = p0.Y + 0.5;

            line.X2 = p1.X + 0.5;
            line.Y2 = p1.Y + 0.5;
        }

        public static void SetLocation(this Line line, double x0, double y0, double x1, double y1)
        {
            line.X1 = x0 + 0.5;
            line.Y1 = y0 + 0.5;

            line.X2 = x1 + 0.5;
            line.Y2 = y1 + 0.5;
        }
    }
}
