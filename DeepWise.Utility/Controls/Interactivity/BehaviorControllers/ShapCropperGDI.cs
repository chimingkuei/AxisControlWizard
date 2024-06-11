using DeepWise.Controls.Interactivity.ROI;
using DeepWise.Shapes;
using System;
using System.Diagnostics;

namespace DeepWise.Controls.Interactivity.BehaviorControllers
{
    public class ShapeCropperGDI<T> : DisplayControllerGDI where T : IShape
    {
        public override bool ShowButtons => true;

        public T Region => interactiveROI != null ? (T)interactiveROI.GetShape() : default(T);

        Point strPoint , endP = Point.NaN;
        InteractiveROIGDI interactiveROI;
        bool dragging = false;
        public override void MouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && interactiveROI == null)
            {
                strPoint = e.Location;
                switch (typeof(T).Name)
                {
                    case nameof(RectRotatable): interactiveROI = new AnglurRectROIGDI(strPoint, DeepWise.Shapes.Size.Empty, 0); break;
                    case nameof(Rect): interactiveROI = new RectROIGDI(strPoint, Size.Empty); break;
                    case nameof(Ring): interactiveROI = new RingROIGDI(strPoint, 0, 0); break;
                    //case nameof(Circle): interactiveROI = new CircleROI(strPoint, 0); break;
                    default: throw new NotImplementedException();
                }
                Display.ActiveObject = interactiveROI;
                e.Cancel = true;
                dragging = true;
            }
            Debug.WriteLine("MouseDown");
        }

        public override void MouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            if (dragging)
            {
                interactiveROI.SetShape(strPoint, e.Location);
                endP = e.Location;
                Display.PushNewFrame();
                e.Cancel = true;
                Debug.WriteLine("Dragging");
            }
            else if (interactiveROI == null)
            {
                Display.Cursor = System.Windows.Forms.Cursors.Cross;
                e.Cancel = true;
            }
            

        }
        public override void MouseUp(object sender, DisplayGDIMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && dragging)
            {
                dragging = false;
                BTN_Checked.Enabled = true;
                Debug.WriteLine("Up");
            }
        }
        public override void DrawOverlay(DisplayGDIPaintEventArgs e)
        {
            if (!dragging) return;
            //if (interactiveROI is RingROI || interactiveROI is CircleROI)
            if (interactiveROI is RingROIGDI && !Point.IsNaN(endP))
            {
                e.pen.Color = ROIColors.Highlight;
                e.pen.DashPattern = new float[2] { 4, 2 };
                e.DrawLine(strPoint, endP);
                e.pen.DashPattern = new float[] {1};
                e.DrawArrow(endP, (endP - strPoint).Angle);
            }
        }
    }
}
