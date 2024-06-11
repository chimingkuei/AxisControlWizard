using DeepWise.Controls.Interactivity.ROI;
using DeepWise.Shapes;
using System;
using System.Diagnostics;

namespace DeepWise.Controls.Interactivity.BehaviorControllers
{
    public class ShapeCropper<T> : DisplayBehavior where T : IShape
    {
        public override bool ShowButtons => true;

        public T Region => inavROI != null ? (T)inavROI.GetRegion() : default(T);

        Point strPoint, endP = Point.NaN;
        InteractiveROI inavROI;
        bool dragging = false;
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            if (e.ChangedButton ==  System.Windows.Input.MouseButton.Left)
            {
                if(inavROI == null)
                {
                    strPoint = e.Location;
                    e.Display.CaptureMouse();
                    switch (typeof(T).Name)
                    {
                        case nameof(Rect): inavROI = new RectROI(strPoint.X, strPoint.Y, 0, 0); break;
                        default: throw new NotImplementedException();
                    }
                    Display.InteractiveObjects.Add(inavROI);
                    Display.SetFocus(inavROI);
                    Display.IsFocusLocked = true;
                    e.Cancel = true;
                    dragging = true;
                }
            }
            Debug.WriteLine("MouseDown");
        }

        public override void MouseMove(DisplayMouseEventArgs e)
        {
            if (dragging)
            {
                inavROI.SetRegion(strPoint, e.Location);
                endP = e.Location;
                e.Cancel = true;
            }
            else if (inavROI == null)
            {
                Display.Cursor = System.Windows.Input.Cursors.Cross;
                e.Cancel = true;
            }


        }
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && dragging)
            {
                dragging = false;
                BTN_Checked.IsEnabled = true;
                Display.Cursor = System.Windows.Input.Cursors.Arrow;
                e.Display.ReleaseMouseCapture();
            }
        }

        public override void Enter(Display display)
        {
            base.Enter(display);
            //display.InteractiveObjects.Clear();
        }

        public override void Exist()
        {
            base.Exist();
            Display.ClearFocus();
            Display.InteractiveObjects.Remove(inavROI);
            Display.IsFocusLocked = false;
        }
    }
}
