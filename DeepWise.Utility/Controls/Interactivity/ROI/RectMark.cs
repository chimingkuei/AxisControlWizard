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
using Size = DeepWise.Shapes.Size;
namespace DeepWise.Controls.Interactivity
{

    public class RectMark : Mark
    {
        public RectMark(Rect p,string toolTip =null) : this()
        {
            _location = p.Location;
            _size = p.Size;
            Rect.ToolTip = toolTip;
            UpdateLocation();
        }
        public RectMark()
        {
            ShapeElements.Add(Rect);
            foreach (var shape in ShapeElements)
            {
                shape.Stroke = Stroke;
            }
        }
        public RectMark(double x, double y, double w, double h) : this(new Rect(x, y, w, h)) { }
        public RectMark(Point center, Size size) : this(new Shapes.Rect(center, size)) { }

        void UpdateLocation()
        {
            Rect.Width = _size.Width;
            Rect.Height = _size.Height;
            Rect.SetLocation(Location.X, Location.Y);
        }

       
        Point _location;
        public Point Location
        {
            get => _location;
            set
            {
                _location = value;
                UpdateLocation();

            }
        }
        public Size Size
        {
            get => _size;
            set
            {
                _size = value;
                UpdateLocation();
            }
        }
    
        #region UI


        System.Windows.Shapes.Rectangle Rect = new System.Windows.Shapes.Rectangle() { Fill = Brushes.Transparent};
        private Size _size;
        #endregion UI

        public static explicit operator Rect(RectMark mark) => new Rect(mark._location,mark._size);

        public override bool IsMouseOver(DisplayMouseEventArgs e) => false;// e.IsMouseOver(Circle);

        public override Cursor Cursor=> Cursors.Hand;

        public bool Visible
        {
            get => Rect.IsVisible;
            set => Rect.Visibility = value? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
    }
}
