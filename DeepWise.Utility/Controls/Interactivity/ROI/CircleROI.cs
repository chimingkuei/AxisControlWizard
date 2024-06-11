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

    public class CircleROI : InteractiveROI
    {
        public CircleROI(Circle p) : this()
        {
            _center = p.Center;
            _radius = p.Radius;
            UpdateLayout(1);
        }

        public CircleROI(double x, double y, double r) : this(new Circle(x, y, r)) { }
        public CircleROI(Point center, double r) : this(new Circle(center, r)) { }

  

        public override void UpdateLayout(double zoomLevel = 0)
        {
            Ellipse.Width = Ellipse.Height = _radius * 2;
            Ellipse.SetLocation(_center.X - _radius, _center.Y - _radius);
            base.UpdateLayout(zoomLevel);
        }

        public CircleROI()
        {
            ShapeElements.Add(Ellipse);
            foreach (var shape in ShapeElements)
            {
                shape.Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                shape.Fill = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
            }
        }
        Point _center;
        public Point Center
        {
            get => _center;
            set
            {
                _center = value;
                UpdateLayout();

            }
        }
        public double Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateLayout();
            }
        }
    
        #region UI


        System.Windows.Shapes.Ellipse Ellipse = new System.Windows.Shapes.Ellipse();
        private double _radius;
        #endregion UI

        public static explicit operator Circle(CircleROI mark) => new Circle(mark._center,mark._radius);


        #region interactive
        private enum Element
        {
            None, Face,Outfit
        }
        private Element selected;

        public override bool IsMouseOver(DisplayMouseEventArgs e)
        {
            var c = (Circle)this;
            if (Focused)
            {
                if (e.IsMouseOver(c))
                {
                    selected = Element.Outfit;
                    return true;
                }
               
            }

            if (c.Contains(e.Location))
            {
                selected = Element.Face;
                return true;
            }
            return false;
        }
        public override Cursor Cursor
        {
            get
            {
                switch (selected)
                {
                    case Element.Face: return Cursors.SizeAll;
                    case Element.Outfit:
                        return Cursors.Hand;
                    default:
                        return Cursors.Hand;
                }
            }
        }
        protected Point oriPos;
        protected Point oriCenter;
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            oriPos = e.Location;
            oriCenter = _center;
            base.MouseDown(e);
        }
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            base.MouseUp(e);
        }
        public override void MouseMove(DisplayMouseEventArgs e)
        {
            switch (selected)
            {
                case Element.Face:
                    {
                        _center = oriCenter + ((DeepWise.Shapes.Point)e.Location - oriPos);
                        break;
                    }
                case Element.Outfit:
                    {
                        var r = ((Point)e.Location - _center).Length;
                        _radius = r;
                        break;
                    }
            }
            UpdateLayout(e.ZoomLevel);
            base.MouseMove(e);
        }
        #endregion

        public override IShape GetRegion() => (Circle)this;

        public override void SetRegion(IShape shape)
        {
            if(shape is Circle c)
            {
                _center = c.Center;
                _radius = c.Radius;
                UpdateLayout();
            }
        }

        public override void SetRegion(System.Windows.Point from, System.Windows.Point to)
        {
            var v = (to - from) / 2;
            _center = from + v;
            _radius = v.Length;
            UpdateLayout();
            
        }

        

        public bool Visible
        {
            get => Ellipse.IsVisible;
            set => Ellipse.Visibility = value? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
    }
}
