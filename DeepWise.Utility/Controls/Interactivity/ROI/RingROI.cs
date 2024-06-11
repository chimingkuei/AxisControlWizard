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

    public class RingROI : InteractiveROI
    {
        public RingROI(Ring p) : this()
        {
            _center = p.Center;
            _radius = p.StartRadius;
            _radius2 = p.EndRadius;
            UpdateLayout(1);
        }

        public RingROI(double x, double y, double r,double r2) : this(new Ring(x, y, r,r2)) 
        { 
        }
        public RingROI(Point center, double r, double r2) : this(new Ring(center, r,r2)) 
        {
        }

        public override void UpdateLayout(double zoomLevel = 0)
        {
            var d1 = R1.Width = R1.Height = _radius * 2;
            R1.SetLocation(_center.X - _radius, _center.Y - _radius);

            var d2 = R2.Width = R2.Height = _radius2 * 2;
            R2.SetLocation(_center.X - _radius2, _center.Y - _radius2);

            var max = _radius2 > _radius ? _radius2 : _radius;
            fill.Width = fill.Height = max * 2;
            fill.SetLocation(_center.X - max, _center.Y - max);
            fill.StrokeThickness = max - (_radius > _radius2 ? _radius2 : _radius);

            base.UpdateLayout(zoomLevel);
        }

        public RingROI()
        {
            ShapeElements.Add(R1);
            ShapeElements.Add(R2);
            ShapeElements.Add(fill);
            InitializeDefaultColor();
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
        public double Radius2
        {
            get => _radius2;
            set
            {
                _radius2 = value;
                UpdateLayout();
            }
        }

        #region UI
        System.Windows.Shapes.Ellipse R1 = new System.Windows.Shapes.Ellipse() { Tag = "contour"};
        System.Windows.Shapes.Ellipse R2 = new System.Windows.Shapes.Ellipse() {  Tag = "contour" };
        System.Windows.Shapes.Ellipse fill = new System.Windows.Shapes.Ellipse() { Tag = "fill(contour)" };
        private double _radius, _radius2;
        #endregion UI

        public static explicit operator Ring(RingROI mark) => new Ring(mark._center, mark._radius, mark._radius2);

        #region interactive
        private enum Element
        {
            None, Face,R2,R1
        }
        private Element selected;

        public override bool IsMouseOver(DisplayMouseEventArgs e)
        {
            var c = (Ring)this;
            if (Focused)
            {
                if (e.IsMouseOver(new Circle(Center, _radius2)))
                {
                    selected = Element.R2;
                    return true;
                }
                else if (e.IsMouseOver(new Circle(Center, _radius)))
                {
                    selected = Element.R1;
;
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
                    case Element.R2:
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
                case Element.R2:
                    {
                        var r = ((Point)e.Location - _center).Length;
                        _radius2 = r;
                        break;
                    }
                case Element.R1:
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

        public override IShape GetRegion() => (Ring)this;

        public override void SetRegion(IShape shape)
        {
            if(shape is Ring c)
            {
                _center = c.Center;
                _radius = c.StartRadius;
                _radius2 = c.EndRadius;
                UpdateLayout();
            }
        }

        public override void SetRegion(System.Windows.Point from, System.Windows.Point to)
        {
            var v = (to - from) / 2;
            _center = from + v;
            _radius = v.Length;
            _radius2 = _radius / 2;
            UpdateLayout();
        }

        public bool Visible
        {
            get => R1.IsVisible;
            set => R1.Visibility = value? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
    }
}
