using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DeepWise.Controls.Interactivity
{

    public class CircleMark : Mark
    {
        public CircleMark(Circle p) : this()
        {
            _center = p.Center;
            _radius = p.Radius;
            UpdateLocation();
        }

        public CircleMark(double x, double y, double r) : this(new Circle(x, y, r)) { }
        public CircleMark(Point center, double r) : this(new Circle(center, r)) { }

        void UpdateLocation()
        {
            Ellipse.Width = Ellipse.Height = Radius * 2;
            Ellipse.SetLocation(Center.X - Radius, Center.Y - Radius);
        }

        public CircleMark()
        {
            ShapeElements.Add(Ellipse);
            foreach (var shape in ShapeElements)
            {
                shape.Stroke = DefaultBrush;
            }
        }
        Point _center;
        public Point Center
        {
            get => _center;
            set
            {
                _center = value;
                UpdateLocation();

            }
        }
        public double Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateLocation();
            }
        }
    
        #region UI


        System.Windows.Shapes.Ellipse Ellipse = new System.Windows.Shapes.Ellipse();
        private double _radius;
        #endregion UI

        public static explicit operator Circle(CircleMark mark) => new Circle(mark._center,mark._radius);

        public override bool IsMouseOver(DisplayMouseEventArgs e) => false;// e.IsMouseOver(Circle);

        public override Cursor Cursor=> Cursors.Hand;

        public bool Visible
        {
            get => Ellipse.IsVisible;
            set => Ellipse.Visibility = value? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
    }
}
