using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeepWise.Controls
{
    public class DisplayMouseEventArgs : EventArgs
    {
        public DisplayMouseEventArgs(Point p, MouseButton changedButton, MouseButtonState lState, MouseButtonState rState, Display display)
        {

            Location = p;
            Display = display;
            LineHitRadius = DefaultLineHitRadius / display.ZoomLevel;
            DraggingPointRadius = DefaultDraggingPointRadius / display.ZoomLevel;
            ChangedButton = changedButton;
            LeftButton = lState;
            RightButton = rState;
        }

        public DisplayMouseEventArgs(Point p, MouseButtonState lState, MouseButtonState rState, Display display)
        {

            Location = p;
            Display = display;
            LineHitRadius = DefaultLineHitRadius / display.ZoomLevel;
            DraggingPointRadius = DefaultDraggingPointRadius / display.ZoomLevel;
            LeftButton = lState;
            RightButton = rState;
        }

        public System.Drawing.Point PixelLocation
        {
            get
            {
                int x = X >= 0 ? (int)(X + 0.5) : (int)(X - 0.5);
                int y = Y >= 0 ? (int)(Y + 0.5) : (int)(Y - 0.5);
                return new System.Drawing.Point(x, y);
            }
        }

        public MouseButton? ChangedButton { get; }
        public MouseButtonState LeftButton { get; }
        public MouseButtonState RightButton { get; }
        public double X => Location.X;
        public double Y => Location.Y;
        public Display Display { get; }
        public Point Location { get; }
        public bool Cancel { get; set; } = false;
        public double ZoomLevel => Display.ZoomLevel;
        public static double DefaultLineHitRadius = 4;
        public static double DefaultDraggingPointRadius = 5;
        public double LineHitRadius { get; }
        public double DraggingPointRadius { get; }

        public bool IsMouseOver(DeepWise.Shapes.Circle circle) => DeepWise.Shapes.Geometry.GetDistance(Location, circle, out _) < LineHitRadius;
        public bool IsMouseOver(DeepWise.Shapes.Point point) => (point - (DeepWise.Shapes.Point)Location).Length < DraggingPointRadius;
        public bool IsMouseOver(DeepWise.Shapes.Arc arc) => DeepWise.Shapes.Geometry.GetDistance(Location, arc, out _) < LineHitRadius;
        public bool IsMouseOver(DeepWise.Shapes.Segment segment) => DeepWise.Shapes.Geometry.GetDistance(Location, segment, out _) < LineHitRadius;
        public bool IsMouseOver(DeepWise.Shapes.Line line) => DeepWise.Shapes.Geometry.GetDistance(Location, line, out _) < LineHitRadius;
    }
}
