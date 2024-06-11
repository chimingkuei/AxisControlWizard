using DeepWise.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Point = DeepWise.Shapes.Point;
namespace DeepWise.Controls.Interactivity.BehaviorControllers
{
    public class MaskDrawerGDI : DisplayControllerGDI
    {
        public override bool ShowButtons => true;
        public double PenSize { get; set; } = 25;
        //public enum DrawMode { DrawMask,EraseMask,FillMask,}

        public override void MouseDown(object sender, DisplayGDIMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if(System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
                {
                    var overlay = Display.CloneOverlayBitmap();
                    FloodFill(overlay, (int)(e.X + 0.5), (int)(e.Y + 0.5), OverlayColor);
                    Display.ReplaceOverlayBitmap(overlay);
                }
                else
                {
                    Display.DrawBitmapOverlay((s, args) =>
                    {
                        args.Color = OverlayColor;
                        args.Fill(new Circle(e.Location, PenSize));
                        this.BTN_Checked.Enabled = true;
                    });
                }
            }
            else if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Display.DrawBitmapOverlay((s, args) =>
                {
                    args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    args.Color = Color.FromArgb(0, 0, 0, 0);
                    args.Fill(new Circle(e.Location, PenSize));
                    this.BTN_Checked.Enabled = true;
                    args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                });
            }
        }
        public override void MouseMove(object sender, DisplayGDIMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Display.DrawBitmapOverlay((s, args) =>
                {
                    args.Color = OverlayColor;
                    args.Fill(new Circle(e.Location, PenSize));
                });
            }
            else if(e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Display.DrawBitmapOverlay((s, args) =>
                {
                    args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    args.Color = Color.FromArgb(0, 0, 0, 0);
                    args.Fill(new Circle(e.Location, PenSize));
                    this.BTN_Checked.Enabled = true;
                    args.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                });
            }
            crtLocation = e.Location;
        }
        Point crtLocation = Point.NaN;
        public override void MouseUp(object sender, DisplayGDIMouseEventArgs e)
        {
        }
        public override void DrawOverlay(DisplayGDIPaintEventArgs e)
        {
            e.pen.Color = Color.Magenta;
            e.Draw(new Circle(crtLocation, PenSize));
        }
        public Color OverlayColor { get; set; } = System.Drawing.SystemColors.Highlight;

        public override void Enter(DisplayGDI display)
        {
            base.Enter(display);
            Display.ClearBitmapOverlay();
        }

        public override void Exist()
        {
            base.Exist();
        }

        public bool ClearWhenLeave { get; set; } = true;

        unsafe Bitmap GenerateMask()
        {
            var bitmap = Display.CloneOverlayBitmap();
            var data = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int w = bitmap.Width;
            int strideRgb = data.Stride;
            var mask = new byte[data.Width * data.Height];
            Parallel.For(0, bitmap.Height, j =>
            {
                unsafe
                {
                    uint* ptr = (uint*)data.Scan0 + j * strideRgb/4;
                    string hex = (*ptr).ToString("X");
                    for (int i = 0; i < w; i++)
                    {
                        uint value = *ptr;
                        if (value!= 0xFF000000) mask[w * j + i] = 255;
                        ptr ++;                    
                    }
                }
            });
            bitmap.UnlockBits(data);
            fixed (byte* p = mask)
            {
                IntPtr ptr = (IntPtr)p;
                // do you stuff here
                var maskBmp = new Bitmap(bitmap.Width, bitmap.Height, bitmap.Width, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, ptr);
                maskBmp.Palette = BitmapExtensions.GetMono8Palette();
                return maskBmp;
            }

        }
        Bitmap _mask;
        public Bitmap Mask
        {
            get
            {
                if (!Result) throw new Exception("繪製尚未完成");
                if (_mask == null) _mask = GenerateMask();
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
