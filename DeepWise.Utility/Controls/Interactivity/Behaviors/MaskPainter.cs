using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Point = DeepWise.Shapes.Point;
namespace DeepWise.Controls.Interactivity.BehaviorControllers
{
    public class MaskPainter : DisplayBehavior
    {
        public override bool ShowButtons => true;
        [Slider(1,100),DecimalPlaces(0)]
        public double PenSize
        {
            get => Circle.Radius;
            set
            {
                Circle.Radius = value;
                NotifyPropertyChanged();
            }
        }
        //public enum DrawMode { DrawMask,EraseMask,FillMask,}
        [Browsable(false)]
        public CircleMark Circle { get; } = new CircleMark(new Shapes.Circle(0, 0, 25));
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            switch(e.ChangedButton)
            {
                case MouseButton.Left:

                    if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
                    {
                        FloodFill(overlay.GDIBitmap, (int)(e.X + 0.5), (int)(e.Y + 0.5), OverlayColor);
                        overlay.InteropBitmap.Invalidate();
                    }
                    else
                    {
                        overlay.Draw(g =>
                        {
                            DisplayGDIPaintEventArgs args = new DisplayGDIPaintEventArgs(g, new Rectangle(0, 0, overlay.Width, overlay.Height), 0, 0, 1);
                            args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                            args.Color = OverlayColor;
                            args.Fill(new Circle(e.Location, PenSize));
                            this.BTN_Checked.IsEnabled = true;
                        });
                    }
                    break;
                case MouseButton.Right:
                    overlay.Draw(g =>
                    {
                        DisplayGDIPaintEventArgs args = new DisplayGDIPaintEventArgs(g, new Rectangle(0, 0, overlay.Width, overlay.Height), 0, 0, 1);
                        args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        args.Color = Color.FromArgb(0, 0, 0, 0);
                        args.Fill(new Circle(e.Location, PenSize));
                    });
                    break;
            }
            e.Cancel = true;
        }
        public override void MouseMove(DisplayMouseEventArgs e)
        {
            Circle.Center = e.Location;
            Circle.Visible = true; 
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                overlay.Draw(g =>
                {
                    DisplayGDIPaintEventArgs args = new DisplayGDIPaintEventArgs(g, new Rectangle(0, 0, overlay.Width, overlay.Height), 0, 0, 1);
                    args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    args.Color = OverlayColor;
                    args.Fill(new Circle(e.Location, PenSize));
                });
            }
            else if(e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                overlay.Draw(g =>
                {
                    DisplayGDIPaintEventArgs args = new DisplayGDIPaintEventArgs(g, new Rectangle(0, 0, overlay.Width, overlay.Height), 0, 0, 1);
                    args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    args.Color = Color.FromArgb(0, 0, 0, 0);
                    args.Fill(new Circle(e.Location, PenSize));
                });
            }
            crtLocation = e.Location;
            e.Cancel = true;
        }
        Point crtLocation = Point.NaN;
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            e.Cancel = true;
        }

        [Browsable(false)]
        public Color OverlayColor { get; set; } = Color.FromArgb(50, 0, 120, 215);

        [Button]
        public void Reset()
        {
            overlay.Draw(g =>
            {
                g.Clear(Color.Transparent);
            });
        }

        public MaskPainter(Bitmap mask)
        {
            oriMask = mask;
        }
        public MaskPainter()
        {

        }
        Bitmap oriMask;
        public override unsafe void Enter(Display display)
        {
            base.Enter(display);
            if (display.ImageWidth == 0 || display.ImageHeight == 0) throw new Exception();
            if (oriMask != null)
                overlay = new CoopBitmap(oriMask,OverlayColor);
            else
                overlay = new CoopBitmap(display.ImageWidth, display.ImageHeight);
            
            display.OverlayImage = overlay.InteropBitmap;
            display.InteractiveObjects.Add(Circle);
            display.MouseLeave += Display_MouseLeave;
            display.Cursor = Cursors.None;
        }

        private void Display_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Circle.Visible = false;
        }

        CoopBitmap overlay;
        public override void Exist()
        {
            base.Exist();
            //_mask = (Bitmap)overlay.GDIBitmap.Clone();
            Display.InteractiveObjects.Remove(Circle);
            Display.MouseLeave += Display_MouseLeave;
            Display.OverlayImage = null;
            Display.Cursor = Cursors.Arrow;
        }

        [Browsable(false)]
        public bool ClearWhenLeave { get; set; } = true;

        protected override void OnCheckedButtonClicked(object sender, EventArgs e)
        {
            base.OnCheckedButtonClicked(sender, e);
            _mask = GenerateMask();
        }

        unsafe Bitmap GenerateMask()
        {
            var bitmap = overlay.GDIBitmap.ConvertToMono8();
            var data = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int stride = bitmap.GetStride();
            var w = bitmap.Width;
            Parallel.For(0, bitmap.Height, j =>
            {
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0 + j * stride;
                    for (int i = 0; i < w; i++, ptr++)
                        if (*ptr != 0) *ptr = 255;
                }
            });
            bitmap.UnlockBits(data);
            return bitmap;
        }

        Bitmap _mask;
        [Browsable(false)]
        public Bitmap Mask
        {
            get
            {
                if (!Result) throw new Exception("繪製尚未完成");
                return _mask;
            }
        }

        void FloodFill(Bitmap bitmap, int x, int y, Color color)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<System.Drawing.Point> check = new LinkedList<System.Drawing.Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new System.Drawing.Point(x, y));
                while (check.Count > 0)
                {
                    System.Drawing.Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (System.Drawing.Point off in new System.Drawing.Point[] {
                new System.Drawing.Point(0, -1), new System.Drawing.Point(0, 1),
                new System.Drawing.Point(-1, 0), new System.Drawing.Point(1, 0)})
                    {
                        System.Drawing.Point next = new System.Drawing.Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }

            Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);
        }
    }
}
