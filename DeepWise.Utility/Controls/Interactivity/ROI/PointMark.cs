using DeepWise.Shapes;
using System.Windows.Input;
using System.Windows.Media;

namespace DeepWise.Controls.Interactivity
{
    public abstract class Mark : InteractiveObject
    {
        public static Brush DefaultBrush => Brushes.Red;
        public static Brush HightlightBrush => Brushes.Magenta;

        private Brush _brush = DefaultBrush;
        public virtual Brush Stroke
        {
            get => _brush;
            set
            {
                _brush = value;
                if(!Focused)
                {
                    foreach (var shape in ShapeElements)
                    {
                        shape.Stroke = _brush;
                    }
                }
            }

        }
        public virtual Brush HightlightStroke { get; set; } = HightlightBrush;
        protected override void OnFocusedChanged()
        {
            foreach (var item in ShapeElements)
            {
                var clr = Focused ? HightlightStroke : Stroke;
                item.Stroke = clr;
                //clr.A = 80;
                //item.Fill = new SolidColorBrush(clr);
            }
        }
    }
    public class PointMark : Mark
    {
        public PointMark(Point p) : this()
        {
            Location = p;
            UpdateLayout(1);
        }

        public PointMark()
        {
            ShapeElements.Add(LR);
            ShapeElements.Add(RL);
            foreach (var shape in ShapeElements)
            {
                shape.Stroke = Stroke;
            }
        }
        public bool Static { get; set; } = true;
        Point _location;
        public Point Location
        {
            get => _location;
            set
            {
                if (value == _location) return;
                _location = value;
                UpdateLayout(10/(LR.X2 - LR.X1));
            }
        }
        #region UI

        public override void UpdateLayout(double zoomLevel)
        {
            double diff = 5 / zoomLevel;
            LR.X1 = Location.X + 0.5 - diff;
            LR.Y1 = Location.Y + 0.5 - diff;
            LR.X2 = Location.X + 0.5 + diff;
            LR.Y2 = Location.Y + 0.5 + diff;

            RL.X1 = Location.X + 0.5 + diff;
            RL.Y1 = Location.Y + 0.5 - diff;
            RL.X2 = Location.X + 0.5 - diff;
            RL.Y2 = Location.Y + 0.5 + diff;
        }

        System.Windows.Shapes.Line LR = new System.Windows.Shapes.Line();
        System.Windows.Shapes.Line RL = new System.Windows.Shapes.Line();
        #endregion UI

        public static explicit operator Point(PointMark mark) => mark.Location;

        public override bool IsMouseOver(DisplayMouseEventArgs e) => e.IsMouseOver(Location);

        public override Cursor Cursor=> Cursors.Hand;
        Point oriLocation;
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            oriLocation = Location;
            base.MouseDown(e);
        }
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            base.MouseUp(e);
        }
        public override void MouseMove(DisplayMouseEventArgs e)
        {
            if (!Static)
            {
                Point p = e.Location;
                Location = oriLocation + (e.Location - StartLocation);
                UpdateLayout(e.ZoomLevel);
            }
            base.MouseMove(e);
        }
    }
}
