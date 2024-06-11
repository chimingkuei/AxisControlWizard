using DeepWise.Shapes;
using System.Windows.Input;
using System.Windows.Media;

namespace DeepWise.Controls.Interactivity
{

    public class LineMark : Mark
    {
        public LineMark(Segment seg) : this()
        {
            Location = seg;
            LR.X1 = Location.X0 + 0.5;
            LR.Y1 = Location.Y0 + 0.5;
            LR.X2 = Location.X1 + 0.5;
            LR.Y2 = Location.Y1 + 0.5;
        }

        public LineMark(Point p0,Point p1) : this(new Segment(p0,p1))
        {
        }
        public LineMark(double x0,double y0, double x1,double y1) : this(new Segment(x0,y0,x1,y1))
        {
        }

        LineMark()
        {
            ShapeElements.Add(LR);
            foreach (var shape in ShapeElements)
            {
                shape.Stroke = DefaultBrush;
            }
        }

        Segment Location { get; set; }
        public void SetLocation(Segment seg)
        {
            Location = seg;
            LR.X1 = Location.X0 + 0.5;
            LR.Y1 = Location.Y0 + 0.5;
            LR.X2 = Location.X1 + 0.5;
            LR.Y2 = Location.Y1 + 0.5;
        }
        #region UI

        public override void UpdateLayout(double zoomLevel)
        {
          
        }

        System.Windows.Shapes.Line LR = new System.Windows.Shapes.Line();
        #endregion UI

        public static explicit operator Segment(LineMark mark) => mark.Location;

        public override bool IsMouseOver(DisplayMouseEventArgs e) => e.IsMouseOver(Location);

        public override Cursor Cursor=> Cursors.Hand;
        Point oriLocation;
    }
}
