using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using DeepWise.Shapes;
using Point = DeepWise.Shapes.Point;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static WindowsAPI.GDI32;
using WindowsAPI;
using DeepWise.Devices;
using System.Threading;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace DeepWise.Controls.Interactivity
{
    public partial class DisplayGDI : Control, INotifyPropertyChanged 
    {
        //===================================================================================================
        //TODO : 實作 IMessageFilter 確保滾輪有效
        //const int WM_MOUSEWHEEL = 0x020a;
        //public bool PreFilterMessage(ref Message m)
        //{
        //    if (m.Msg == WM_MOUSEWHEEL && !mFiltering && this.FindForm() == Form.ActiveForm && ClientRectangle.Contains(PointToClient(Control.MousePosition)))
        //    {
        //        mFiltering = true;
        //        SendMessage(this.Handle, m.Msg, m.WParam, m.LParam);
        //        m.Result = IntPtr.Zero;
        //        mFiltering = false;
        //        return true;
        //    }
        //    return false;
        //}
        //// P/Invoke declarations
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        //===================================================================================================

        #region InteractiveController
        public IInteractiveGDI Controller
        {
            get => _controller;
            set
            {
                if (_controller == value) return;
                _controller?.Exist();
                this.Controls.Clear();
                this.InteractiveObjects.Clear();
                //this.SelectedObject = null;
                value?.Enter(this);
                _controller = value;
                PushNewFrame();
            }
        }IInteractiveGDI _controller;
        #endregion

        #region Pixel Access
        public bool ReadPixelByCursor { get; set; } = true;
        public IProgress<PixelInfo?> PixelInfoProgress { get; set; }
        private void GetPixelAtCursor(DisplayGDIMouseEventArgs args)
        {
            var x = (int)(args.Location.X+0.5);
            var y = (int)(args.Location.Y+0.5);
            var format = Camera != null ? Camera.Format : System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            var pixelValue = GetPixelFromHBitmap(hbmp_displayImg, x, y);
            if(format == System.Drawing.Imaging.PixelFormat.Format8bppIndexed && pixelValue>0)
            {
                pixelValue = (pixelValue>>16) & 255 ;
            }
            PixelInfoProgress?.Report(new PixelInfo()
            {
                Position = new System.Drawing.Point(x, y),
                PixelFormat = Camera != null ? Camera.Format : System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                PixelValue = pixelValue,
            }) ;
        }
        #endregion

        int GetPixelFromHBitmap(IntPtr hBmp, int x, int y)
        {
            var hdc = CreateCompatibleDC(IntPtr.Zero);
            SelectObject(hdc, hBmp);
            var color = WindowsAPI.GDI32.GetPixel(hdc, x, y);
            DeleteDC(hdc);
            //Debug.WriteLine($"{x.ToString().PadRight(4)},{y.ToString().PadRight(4)}={color.ToString("X")}");
            return color;
        }

        BufferedGraphicsContext bufferedGraphicsContext;
        private int backColor;
        public DisplayGDI()
        {
            //DoubleBuffered = true;
            bufferedGraphicsContext = new BufferedGraphicsContext();
            InteractiveObjects.CollectionChanged += InvalidateOverlay;
            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = 15;
            Timer.Tick += UpdateVideo;
            Timer.Start();
        }
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
        }

        public ToolTip tp = new ToolTip();

        protected override void Dispose(bool disposing)
        {
            //if (buffer != null) buffer.Dispose();
            if (_camera != null) _camera.ReceivedImage -= OnRecievedCameraImage;
            if (hbmp_displayImg != IntPtr.Zero) DeleteObject(hbmp_displayImg);
            if (hbmp_bitmapOverlay != IntPtr.Zero) DeleteObject(hbmp_bitmapOverlay);
            if (hbmp_contextOverlay != IntPtr.Zero) DeleteObject(hbmp_contextOverlay);
            bufferedGraphicsContext.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            backColor = BackColor.R + (BackColor.G << 8) + (BackColor.B << 16);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (DesignMode) return;
            backColor = BackColor.R + (BackColor.G << 8) + (BackColor.B << 16);
            IntPtr dc = User32.GetDC(Handle);
            hbmp_contextOverlay = CreateCompatibleBitmap(dc, Width, Height);
            User32.ReleaseDC(Handle, dc);
            InvalidateOverlay();
        }
  
        protected override void OnSizeChanged(EventArgs e)
        {
            if (!Size.IsEmpty)
            {
                if (hbmp_contextOverlay != IntPtr.Zero) DeleteObject(hbmp_contextOverlay);
                IntPtr wndDC = User32.GetDC(Handle);
                hbmp_contextOverlay = CreateCompatibleBitmap(wndDC, Width, Height);
                User32.ReleaseDC(Handle, wndDC);
                InvalidateOverlay();
                PushNewFrame();
            }
            base.OnSizeChanged(e);
        }
        #region Main Methods

        /// <summary>
        /// 表示Display要顯示的影像。
        /// 屬性<see cref="Camera"/>與<see cref="Image"/>只能同時存在一個，若指派新值到其中一個屬性，另一個屬性則被指派為null。
        /// </summary>
        public Bitmap Image
        {
            get => _image;
            set
            {
                if (_camera != null) _camera = null;
                if (value != null)
                    SetDisplayImage(_image = value);
            }
        }Bitmap _image;

        void SetDisplayImage(Bitmap image)
        {
            if (image == null) throw new ArgumentNullException("輸入影像不可為空");
            if (_camera != null) _camera.ReceivedImage -= OnRecievedCameraImage; _camera = null;
            if (hbmp_displayImg != IntPtr.Zero) DeleteObject(hbmp_displayImg); hbmp_displayImg = IntPtr.Zero;
            //if (buffer != null) buffer.Dispose();
            ImageSize = new System.Drawing.Size(image.Width, image.Height);
            var oldOverlay = hbmp_displayImg;
            var oldImg = hbmp_displayImg;
            hbmp_displayImg = image.GetHbitmap();
            hbmp_bitmapOverlay = image.GetHbitmap();
            var dc = CreateCompatibleDC(IntPtr.Zero);
            SetStretchBltMode(dc, StretchMode.STRETCH_DELETESCANS);
            var oldbmp_bdc = SelectObject(dc, hbmp_bitmapOverlay);
            User32.RECT rect = new User32.RECT(0, 0, ImageSize.Width, ImageSize.Height);
            IntPtr brush = CreateSolidBrush((int)OverlayTransparentColor);
            User32.FillRect(dc, ref rect, brush);
            DeleteObject(brush);
            DeleteObject(oldOverlay);
            DeleteObject(oldImg);
            SelectObject(dc, oldbmp_bdc);
            DeleteDC(dc);
            PushNewFrame();
        }
        object installing =new object();
        internal unsafe void InstallPixels(IntPtr lpBits, int width, int height, int channel = 1)
        {
            //lock(installing)
            {
                if (ModifierKeys.HasFlag(Keys.Control)) return;
                int size = width * height;
                if (lpBits == IntPtr.Zero || size == 0)
                {
                    installing = false;
                    throw new Exception();
                }

                if (channel == 4)
                {
                    SetBitmapBits(hbmp_displayImg, (uint)size * 4, lpBits);
                }
                else if (channel == 3)
                {
                    byte[] tmp = new byte[size * 4];
                    byte* src = (byte*)lpBits;
                    int srcStride = width * 3;
                    if (srcStride % 4 != 0) srcStride += (4 - srcStride % 4);
                    Parallel.For(0, height, j =>
                    {
                        int countDest = j * width * 4;
                        int countSrc = j * srcStride;
                        //int countSrc = j * width * 3;
                        for (int i = 0; i < width; i++)
                        {
                            tmp[countDest++] = src[countSrc++];
                            tmp[countDest++] = src[countSrc++];
                            tmp[countDest++] = src[countSrc++];
                            countDest++;
                        }
                    });
                    fixed (byte* p = tmp)
                    {
                        SetBitmapBits(hbmp_displayImg, (uint)size * 4, (IntPtr)p);
                    }
                }
                else if (channel == 1)
                {
                    uint[] b32 = new uint[size];
                    byte* src = (byte*)lpBits;
                    Parallel.For(0, height, j =>
                    {
                        int index = j * width;
                        for (int i = 0; i < width; i++)
                        {
                            uint tmp = src[index];
                            b32[index] = (tmp << 16) | (tmp << 8) | tmp;
                            index++;
                        }
                    });
                    fixed (uint* p = b32)
                    {
                        SetBitmapBits(hbmp_displayImg, (uint)size * 4, (IntPtr)p);
                    }
                }
                else
                {
                    installing = false;
                    throw new Exception("影像頻道數不支援");
                }
            }
        }
        /// <summary>
        /// 表示Display要顯示的影像。
        /// 屬性<see cref="Camera"/>與<see cref="Image"/>只能同時存在一個，若指派新值到其中一個屬性，另一個屬性則被指派為null。
        /// </summary>
        public IStreamVideo Camera
        {
            get => _camera;
            set
            {
                if (_camera == value) return;
                if (_camera != null) _camera.ReceivedImage -= OnRecievedCameraImage;
                if (value != null)
                {
                    var bmp = new Bitmap((int)value.Size.Width, (int)value.Size.Height);
                    SetDisplayImage(bmp);
                    value.ReceivedImage += OnRecievedCameraImage;
                    _camera = value;
                }
            }
        }IStreamVideo _camera;

        private void OnRecievedCameraImage(object sender, ImageRecievedEventArgs e)
        {
            try
            {
                InstallPixels(e.Scan0, e.Size.Width, e.Size.Height, e.Format.GetBytesPerPixel());
                buffers.Enqueue(GetNewFrame());
            }
            catch(Exception ex)
            {

            }
        }

      
        #endregion

        System.Windows.Forms.Timer Timer;
        void UpdateVideo(object sender, EventArgs e)
        {
            while (buffers.Count > 0)
            {
                if (buffers.TryDequeue(out var buffer) && buffer != IntPtr.Zero)
                {
                    IntPtr wndDC = User32.GetDC(Handle);
                    var bufferDC = CreateCompatibleDC(wndDC);
                    SetStretchBltMode(bufferDC, StretchMode.STRETCH_DELETESCANS);
                    SelectObject(bufferDC, buffer);
                    BitBlt(wndDC, 0, 0, Width, Height, bufferDC, 0, 0, TernaryRasterOperations.SRCCOPY);
                    GDI32.DeleteObject(buffer);
                    DeleteDC(bufferDC);
                    DeleteDC(wndDC);
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPaint(PaintEventArgs e)
        {
            //Refresh();
            if (DesignMode) base.OnPaint(e);
        }
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (DesignMode || (Camera == null && Image == null)) base.OnPaintBackground(pevent);
        }

        #region Interaction
        int offsetX, offsetY;
        float zoomLevel = 1;
        public System.Drawing.Size PanOffset
        {
            get
            {
                return new System.Drawing.Size(offsetX, offsetY);
            }
            set
            {
                offsetX = value.Width;
                offsetY = value.Height;
            }
        }
        public void ZoomIn(object sender = null,EventArgs e = null)
        {
            zoomLevel *= 1.125f;
            if (zoomLevel > MaxZoomLevel) zoomLevel = MaxZoomLevel;
            NotifyPropertyChanged(nameof(ZoomLevel));
        }
        public void ZoomOut(object sender = null, EventArgs e = null)
        {
            zoomLevel /= 1.125f;
            if (zoomLevel < MinZoomLevel) zoomLevel = MinZoomLevel;
            NotifyPropertyChanged(nameof(ZoomLevel));
        }
        public void ResetLoaction(object sender = null, EventArgs e = null)
        {
            zoomLevel = 1;
            offsetX = 0;
            offsetY = 0;
            NotifyPropertyChanged(nameof(ZoomLevel));
            NotifyPropertyChanged(nameof(PanOffset));
            PushNewFrame();
        }
        public void AutoSizeImage(object sender = null,EventArgs e = null)
        {
            var wr = (float)Width/ ImageSize.Width ;
            var hr = (float)Height/ ImageSize.Height;
            zoomLevel = wr < hr ? wr : hr;
            var nw = (int)(ImageSize.Width * zoomLevel);
            var nh = (int)(ImageSize.Height * zoomLevel);
            offsetX = (Width - nw) / 2;
            offsetY = (Height - nh) / 2;
            NotifyPropertyChanged(nameof(ZoomLevel));
            NotifyPropertyChanged(nameof(PanOffset));
            PushNewFrame();
        }

        public float ZoomLevel
        {
            get => zoomLevel;
            set
            {
                if (value == zoomLevel) return;
                if (value <= 0.125) zoomLevel = 0.125f;
                else if (value >= 20) zoomLevel = 20;
                else zoomLevel = (float)value;
                NotifyPropertyChanged();
            }

        }
        public float MaxZoomLevel { get; set; } = 20;
        public float MinZoomLevel { get; set; } = 0.0625f;
        public new event EventHandler<DisplayGDIMouseEventArgs> MouseDown;
        public new event EventHandler<DisplayGDIMouseEventArgs> MouseMove;
        public event EventHandler<DisplayGDIMouseEventArgs> PreviewMouseDown;
        public event EventHandler<DisplayGDIMouseEventArgs> PreviewMouseMove;
        public new event EventHandler<DisplayGDIMouseEventArgs> MouseUp;
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (panning) return;
            Point crtPoint = new Point((e.Location.X - offsetX) / ZoomLevel, (e.Location.Y - offsetY) / ZoomLevel);
            if (e.Delta > 0)
                ZoomIn();
            else
                ZoomOut();
            Point newPoint = new Point((e.Location.X - offsetX) / ZoomLevel, (e.Location.Y - offsetY) / ZoomLevel);
            offsetX -= (int)((crtPoint.X - newPoint.X) * zoomLevel);
            offsetY -= (int)((crtPoint.Y - newPoint.Y) * zoomLevel);
            //Point finalPoint = new Point((e.Location.X - offsetX) / ZoomLevel, (e.Location.Y - offsetY) / ZoomLevel);
            InvalidateOverlay();
            PushNewFrame();
            //if (panning)
            //{
            //    DZ.Shapes.Point relative = new DZ.Shapes.Point((e.Location.X - offsetOrigin.X) / ZoomLevel, (e.Location.Y - offsetOrigin.Y) / ZoomLevel);

            //    if (panning)
            //    {
            //        var t = (relative - panOrigin) * ZoomLevel + offsetOrigin;
            //        PanOffset = new Size((int)t.X, (int)t.Y);
            //    }
            //}
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            DisplayGDIMouseEventArgs args = new DisplayGDIMouseEventArgs(e, offsetX, offsetY, zoomLevel);
            if (e.Button == MouseButtons.Middle)
            {
                panOrigin = new DeepWise.Shapes.Point(e.Location.X, e.Location.Y);
                offsetOrigin = new DeepWise.Shapes.Vector(offsetX, offsetY);
                panning = true;
            }

            PreviewMouseDown?.Invoke(this, args);
            if (args.Cancel) goto Finish;

            Controller?.MouseDown(this, args);
            if (args.Cancel) goto Finish;

            if (e.Button == MouseButtons.Left)
            {
                if (ActiveObject != null)
                {
                    ActiveObject.OnMouseDown(this, args);
                    if (args.Cancel) goto Finish;
                }

                foreach (InteractiveObjectGDI roiObj in InteractiveObjects)
                {
                    if (roiObj.IsMouseOver(args))
                    {
                        roiObj.Focus(this);
                        goto Finish;
                    }
                }

                ActiveObject = null;
                DragginItem = false;
            }
            else if(e.Button == MouseButtons.Right)
            {
                var hits = InteractiveObjects.Where(x => x.IsMouseOver(args));
                if (hits.Count() > 0)
                {
                    var menu = new ContextMenu();
                    
                    foreach(var item in hits)
                    {
                        if(item == ActiveObject)
                        {
                            foreach (var text in item.MenuItems)
                            {
                                var tmp = new MenuItem(text, item.OnMenuItemsClicked);
                                tmp.Click += RefreshAll;
                                tmp.Tag = item;
                                menu.MenuItems.Add(tmp);
                            }
                        }
                        else
                        {
                            var tmp = new MenuItem("選擇\""+item.Name+"\"",SelectROI);
                            tmp.Click += RefreshAll;
                            tmp.Tag = item;
                            menu.MenuItems.Add(tmp);
                        }
                    }
                    menu.Show(this, e.Location);
                    return;
                    
                }
                
              
            }

            MouseDown?.Invoke(this, args);

            Finish: 
            InvalidateOverlay();
            PushNewFrame();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (panning)
            {
                DeepWise.Shapes.Point current = new DeepWise.Shapes.Point(e.Location.X, e.Location.Y);
                DeepWise.Shapes.Vector v = (current - panOrigin);
                var t = offsetOrigin + v;
                PanOffset = new System.Drawing.Size((int)t.X, (int)t.Y);
                InvalidateOverlay();
                PushNewFrame();
            }
            var args = new DisplayGDIMouseEventArgs(e, offsetX, offsetY, zoomLevel);
            GetPixelAtCursor(args);
            if(DragginItem)//Top most
            {
                ActiveObject.OnMouseMove(this, args);
                InvalidateOverlay();
                PushNewFrame();
                return;
            }

            PreviewMouseMove?.Invoke(this, args);
            if (args.Cancel) goto Finish;



            bool setCursorFlag = false;
            //Looking for interactive objects
            foreach (InteractiveObjectGDI obj in InteractiveObjects)
            {
                //TODO : 增加Enable或Selectable機制
                if (obj.IsMouseOver(args)) 
                {
                    Cursor = obj == ActiveObject ? obj.Cursor : Cursors.Hand;
                    setCursorFlag = true;
                    break;
                }
            }

            Controller?.MouseMove(this, args);
            if (args.Cancel) goto Finish;

            MouseMove?.Invoke(this, args);
            if (args.Cancel) return;

            if(!setCursorFlag) Cursor = DefaultCursor;


            Finish:
            InvalidateOverlay();
            PushNewFrame();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Middle)
                panning = false;

            var args = new DisplayGDIMouseEventArgs(e, offsetX, offsetY, zoomLevel);

            Controller?.MouseUp(this, args);
            if(args.Cancel) goto Finish;

            if (DragginItem)
            {
                ActiveObject.OnMouseUp(this, args);
                goto Finish;
            }

            MouseUp?.Invoke(this, args);

        Finish:
            InvalidateOverlay();
            PushNewFrame();
        }

        public void RefreshAll(object sender = null,EventArgs e = null)
        {
            InvalidateOverlay();
            PushNewFrame();
        }

        void SelectROI(object sender,EventArgs e)
        {
            var obj = ((dynamic)sender).Tag as InteractiveObjectGDI;
            obj.Focus(this);
            InvalidateOverlay();
            PushNewFrame();
        }

        private bool panning = false;
        private DeepWise.Shapes.Point panOrigin;
        private DeepWise.Shapes.Vector offsetOrigin;
        #endregion

        #region ROI
        public void PaintROI(DisplayGDIPaintEventArgs eventArgs)
        {
            for (int i = InteractiveObjects.Count - 1; i >= 0; i--)
                InteractiveObjects[i].Paint(eventArgs);
        }

        public ObservableCollection<InteractiveObjectGDI> InteractiveObjects { get; } = new ObservableCollection<InteractiveObjectGDI>();
        public InteractiveObjectGDI ActiveObject
        {
            get => _activeObj;
            set
            {
                if (_activeObj == value) return;
                if (_activeObj != null) _activeObj.Focused = false;
                _activeObj = value;
                if (value != null)
                {
                    value.Focused = true;
                    InteractiveObjects.Remove(value);
                    InteractiveObjects.Insert(0, value);
                }
                NotifyPropertyChanged();
            }
        }
        private InteractiveObjectGDI _activeObj;

        public bool DragginItem { get; internal set; }
        internal void InteractiveObjectsMouseDown(DisplayGDIMouseEventArgs e)
        {
            if (ActiveObject != null)
            {
                ActiveObject.OnMouseDown(this, e);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                foreach (InteractiveObjectGDI roiObj in InteractiveObjects)
                {
                    if (roiObj.IsMouseOver(e))
                    {
                        roiObj.OnMouseDown(this, e);
                        ActiveObject = roiObj;
                        return ;
                    }
                }

                ActiveObject = null;
                return;
            }
  
            //==============================================
            //if (e.Button == MouseButtons.Left)
            //{
            //    foreach (InteractiveObject roiObj in ROIObjects)
            //    {
            //        if (roiObj.IsMouseOver(e))
            //        {
            //            roiObj.OnMouseDown(this, e);
            //            ActiveObject = roiObj;
            //            return true;
            //        }
            //    }

            //    ActiveObject = null;
            //    return false;
            //}
            //else
            //    return false;
        }
        #endregion

        #region Paint

        public System.Drawing.Size ImageSize { get; private set; }

        public void InvalidateOverlay(object sender = null,EventArgs e = null)
        {
            updateOverlayFlag = true;
        }
        bool updateOverlayFlag = false;
        object updateOverlayLock = new object();
 
        public event DisplayGDIPaintEventHandler PaintContextOverlay;
        public void PushNewFrame()
        {
            //TODO : 如果沒有標籤則不更新 (沒overlay、沒新frame、沒panning)
            if (!IsHandleCreated) return;
            try
            {
                buffers.Enqueue(GetNewFrame());
            }
            catch(InvalidAsynchronousStateException ex)
            {

            }
            catch(ObjectDisposedException ex)
            {

            }
        }

        ConcurrentQueue<IntPtr> buffers = new ConcurrentQueue<IntPtr>();

        object newFrameLock = new object();
         /// <summary>
        /// 混合背景以及覆蓋層
        /// </summary>
        /// <returns></returns>
        private IntPtr GetNewFrame()
        {
            if (Monitor.TryEnter(newFrameLock))
            {
                IntPtr bDC = IntPtr.Zero;
                IntPtr pDC = IntPtr.Zero;
                IntPtr oldbmp_pdc = IntPtr.Zero;
                IntPtr oldbmp_bdc = IntPtr.Zero;
                IntPtr buffer = IntPtr.Zero;
                try
                {
                    if (ImageSize.IsEmpty) return IntPtr.Zero;
                    buffer = CreateBitmap(Width, Height, 4, 8, IntPtr.Zero);
                    bDC = CreateCompatibleDC(IntPtr.Zero);
                    SetStretchBltMode(bDC, StretchMode.STRETCH_DELETESCANS);
                    oldbmp_bdc = SelectObject(bDC, buffer);
                    User32.RECT rect = new User32.RECT(0, 0, Width, Height);
                    IntPtr brush = CreateSolidBrush(backColor);
                    User32.FillRect(bDC, ref rect, brush);
                    DeleteObject(brush);

                    int mapW = (int)(ImageSize.Width * zoomLevel);
                    int mapH = (int)(ImageSize.Height * zoomLevel);
                    Rectangle imgRect = new Rectangle(0, 0, ImageSize.Width, ImageSize.Height);
                    Rectangle mapRect = new Rectangle(offsetX, offsetY, mapW, mapH);
                    if (mapRect.Left < 0)
                    {
                        int offset = -(int)(offsetX / ZoomLevel);
                        mapRect.X += (int)(offset * ZoomLevel);
                        mapRect.Width -= (int)(offset * ZoomLevel);
                        imgRect.X += offset;
                        imgRect.Width -= offset;
                    }
                    if (mapRect.Top < 0)
                    {
                        int offset = -(int)(offsetY / ZoomLevel);
                        mapRect.Y += (int)(offset * ZoomLevel);
                        mapRect.Height -= (int)(offset * ZoomLevel);
                        imgRect.Y += offset;
                        imgRect.Height -= offset;
                    }
                    if (mapRect.Right > Right)
                    {
                        int offset = (int)((mapRect.Right - Right) / ZoomLevel);
                        mapRect.Width -= (int)(offset * ZoomLevel);
                        imgRect.Width -= offset;
                    }
                    if (mapRect.Bottom > Bottom)
                    {
                        int offset = (int)((mapRect.Bottom - Bottom) / ZoomLevel);
                        mapRect.Height -= (int)(offset * ZoomLevel);
                        imgRect.Height -= offset;
                    }

                    pDC = CreateCompatibleDC(IntPtr.Zero);
                    if (pDC != IntPtr.Zero)
                    {
                        oldbmp_pdc = SelectObject(pDC, hbmp_displayImg);
                        SetStretchBltMode(pDC, StretchMode.STRETCH_DELETESCANS);
                        StretchBlt(bDC, mapRect.X, mapRect.Y, mapRect.Width, mapRect.Height, pDC, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height, TernaryRasterOperations.SRCCOPY);
                        oldbmp_pdc = SelectObject(pDC, hbmp_bitmapOverlay);
                        TransparentBlt(bDC, mapRect.X, mapRect.Y, mapRect.Width, mapRect.Height, pDC, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height, OverlayTransparentColor);

                        //Draw Overlay
                        if (updateOverlayFlag)
                        {
                            updateOverlayFlag = false;
                            UpdateContextOverlay(pDC);
                        }
                        SelectObject(pDC, hbmp_contextOverlay);
                        AlphaBlend(bDC, 0, 0, Width, Height, pDC, 0, 0, Width, Height, new BLENDFUNCTION(BLENDFUNCTION.AC_SRC_OVER, 0, 0xff, BLENDFUNCTION.AC_SRC_ALPHA));
                    }
                    
                    return buffer;
                }
                catch (Exception ex)
                {
     
                    if (buffer != IntPtr.Zero) DeleteObject(buffer);
                    return IntPtr.Zero;
                }
                finally
                {
                    if (pDC != IntPtr.Zero)
                    {
                        SelectObject(pDC, oldbmp_pdc);
                        DeleteDC(pDC);
                    }
                    if (bDC != IntPtr.Zero)
                    {
                        SelectObject(bDC, oldbmp_bdc);
                        DeleteDC(bDC);
                    }
                    Monitor.Exit(newFrameLock);
                }
            }
            else
                return IntPtr.Zero;
        }

        void UpdateContextOverlay(IntPtr dc)//應該只會在內部被叫一次
        {
            SelectObject(dc, hbmp_contextOverlay);
            using (Graphics g = Graphics.FromHdc(dc))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(TransparentColor);
                DisplayGDIPaintEventArgs args = new DisplayGDIPaintEventArgs(g, new Rectangle(0, 0, Width, Height), offsetX, offsetY, zoomLevel);
                PaintROI(args);
                Controller?.DrawOverlay(args);
                PaintContextOverlay?.Invoke(this, args);
            }
        }

        #region BitmapOverlay
        public void ClearBitmapOverlay()
        {
            var dc = CreateCompatibleDC(IntPtr.Zero);
            SetStretchBltMode(dc, StretchMode.STRETCH_DELETESCANS);
            var oldbmp_bdc = SelectObject(dc, hbmp_bitmapOverlay);
            User32.RECT rect = new User32.RECT(0, 0, ImageSize.Width, ImageSize.Height);
            IntPtr brush = CreateSolidBrush((int)OverlayTransparentColor);
            User32.FillRect(dc, ref rect, brush);
            DeleteObject(brush);
            SelectObject(dc, oldbmp_bdc);
            DeleteDC(dc);
        }
        object DrawOverlayLock = new object();
        public void DrawBitmapOverlay(EventHandler<DisplayGDIPaintEventArgs> action)
        {
            //TODO : Add Lock to prevent flicker
            lock(DrawOverlayLock)
            {
                var dc = CreateCompatibleDC(IntPtr.Zero);
                var oldbmp_bdc = SelectObject(dc, hbmp_bitmapOverlay);
                using (var g = Graphics.FromHdc(dc))
                {
                    DisplayGDIPaintEventArgs args = new DisplayGDIPaintEventArgs(g, new Rectangle(0, 0, ImageSize.Width, ImageSize.Height), 0, 0, 1);
                    action.Invoke(this, args);
                }
                SelectObject(dc, oldbmp_bdc);
                DeleteDC(dc);
            }
        }
        private uint OverlayTransparentColor = 0xFFFFFF00;
        private IntPtr hbmp_bitmapOverlay = IntPtr.Zero;
        public Bitmap CloneOverlayBitmap() => Bitmap.FromHbitmap(hbmp_bitmapOverlay);
        public void ReplaceOverlayBitmap(Bitmap bmp)
        {
            var old = hbmp_bitmapOverlay;
            hbmp_bitmapOverlay = bmp.GetHbitmap();
            DeleteObject(old);
        }
        #endregion

        #endregion
        public static Color TransparentColor = Color.Black;
        private IntPtr hbmp_displayImg = IntPtr.Zero;
        private IntPtr hbmp_contextOverlay = IntPtr.Zero;
    }

    public class DisplayGDIPaintEventArgs : EventArgs
    {
        public int OffsetX { get; }
        public int OffsetY { get; }
        public float ZoomLevel { get; }
        public float DraggingPointRadius { get; }
        public Graphics Graphics { get; }
        public System.Drawing.Rectangle ClipRectangle { get; }
        public Rect ViewRect { get; set; } = new Rect(0, 0, 4000, 3000);
        public DisplayGDIPaintEventArgs(Graphics g, System.Drawing.Rectangle rect, int offsetX, int offsetY, float zoomLevel)
        {
            this.Graphics = g;
            ClipRectangle = rect;
            OffsetX = offsetX;
            OffsetY = offsetY;
            ZoomLevel = zoomLevel;
            DraggingPointRadius = (float)DisplayGDIMouseEventArgs.DefaultDraggingPointRadius / ZoomLevel;
        }
        public System.Drawing.Point CoordinatePoints(Point p)
        {
            PointF pf = new PointF((float)(ZoomLevel * (p.X + 0.5)), (float)(ZoomLevel * (p.Y + 0.5)));
            return System.Drawing.Point.Round(pf);
        }
        const float arrowLength = 10;

        //public void Draw(IShape shape)
        //{
        //    switch (shape.GetType().Name)
        //    {
        //        case nameof(Arc): DrawArc(pen, (Arc)shape); break;
        //        case nameof(Line): DrawLine(pen, (Line)shape); break;
        //        case nameof(Segment): DrawLine(pen, (Segment)shape); break;
        //        case nameof(Rect): DrawRect(pen, (Rect)shape); break;
        //        case nameof(RectAngle): DrawRect(pen, (RectAngle)shape); break;
        //    }
        //}

        public void DrawDistanceMark(Point p1, Point p2)
        {
            var pv = (p2 - p1).Rotate(Math.PI / 2);
            pv.Normalize();
            pv *= 10 / ZoomLevel;
            DrawLine(p1, p2);
            DrawLine(p1 - pv, p1 + pv);
            DrawLine(p2 - pv, p2 + pv);
        }

        public void DrawStringAng(string str, Font font, Point p1, Point p2)
        {
            var state = Graphics.Save();
            // Get the segment's angle.
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var angle = (float)(180 * Math.Atan2(dy, dx) / Math.PI);

            // Find the center point.
            float cx = (float)((p1.X + p2.X) / 2 + 0.5) * ZoomLevel + OffsetX;
            float cy = (float)((p1.Y + p2.Y) / 2 + 0.5) * ZoomLevel + OffsetY;
            // Translate and rotate the origin
            // to the center of the segment.
            Graphics.RotateTransform(angle, System.Drawing.Drawing2D.MatrixOrder.Append);
            Graphics.TranslateTransform((float)cx, (float)cy, System.Drawing.Drawing2D.MatrixOrder.Append);
            Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            // Get the string's dimensions.
            SizeF size = Graphics.MeasureString(str, font);
            // Make a rectangle to contain the text.
            float y = 0;
            RectangleF rect = new RectangleF(-size.Width / 2, 0, size.Width, size.Height);

            // Draw the text centered in the rectangle.
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                Graphics.DrawString(str, font, brush, rect, sf);
            }
            Graphics.Restore(state);
        }
        public void Draw(Line line)
        {
            if (Geometry.Intersect(ViewRect, line, out Point[] hits) && hits.Length >= 2)
                DrawLine(hits[0], hits[1]);
        }
        public void Draw(Segment segment) => DrawLine(segment.P0, segment.P1);
        public void Draw(Circle c)
        {
            int r = (int)(c.Radius * 2 * ZoomLevel);
            if (r > 0)
                Graphics.DrawEllipse(pen, new RectangleF(F(c.Center - new Vector(c.Radius, c.Radius)), new System.Drawing.Size(r, r)));
        }
        public void Draw(Arc c)
        {
            int r = (int)(c.Radius * 2 * ZoomLevel);
            if (r > 0)
                Graphics.DrawArc(pen, new RectangleF(F(c.Center - new Vector(c.Radius, c.Radius)), new System.Drawing.Size(r, r)), (float)(c.StartAngle / Math.PI * 180), (float)(Geometry.GetSweepAngle(c.StartAngle, c.EndAngle) / Math.PI * 180));
        }
        public void Draw(Rect rect) => Graphics.DrawRectangle(pen, (float)(rect.X + 0.5f) * ZoomLevel + OffsetX, (float)(rect.Y + 0.5f) * ZoomLevel + OffsetY, (float)rect.Width * ZoomLevel, (float)rect.Height * ZoomLevel);
        public void Draw(RectRotatable rect) => Graphics.DrawLines(pen, new System.Drawing.PointF[] { F(rect.LeftTop), F(rect.RightTop), F(rect.RightBottom), F(rect.LeftBottom), F(rect.LeftTop) });
        public void DrawLine(Point p1, Point p2)
        {
            if (ZoomLevel == 1)
                Graphics.DrawLine(pen, (float)p1.X + OffsetX, (float)p1.Y + OffsetY, (float)p2.X + OffsetX, (float)p2.Y + OffsetY);
            else
                Graphics.DrawLine(pen, (float)(p1.X + 0.5f) * ZoomLevel + OffsetX, (float)(p1.Y + 0.5f) * ZoomLevel + OffsetY, (float)(p2.X + 0.5f) * ZoomLevel + OffsetX, (float)(p2.Y + 0.5f) * ZoomLevel + OffsetY);
        }
        public void DrawArrow(Point p, double direction)
        {
            direction += Math.PI;
            System.Drawing.PointF np = F(p);
            Graphics.DrawLine(pen, np, np + (System.Drawing.SizeF)(arrowLength * Vector.FormAngle(direction + 0.643501109)));
            Graphics.DrawLine(pen, np, np + (System.Drawing.SizeF)(arrowLength * Vector.FormAngle(direction - 0.643501109)));
        }
        public void DrawCross(Point result)
        {
            Vector v = 2 * Vector.FormAngle(Math.PI / 4) * DisplayGDIMouseEventArgs.DefaultLineHitRadius / ZoomLevel;
            DrawLine(result - v, result + v);
            v = 2 * Vector.FormAngle(-Math.PI / 4) * DisplayGDIMouseEventArgs.DefaultLineHitRadius / ZoomLevel;
            DrawLine(result - v, result + v);
        }
        public void Fill(RectRotatable rect) => Graphics.FillPolygon(new SolidBrush(Color), new System.Drawing.PointF[] { F(rect.Location), F(rect.RightTop), F(rect.RightBottom), F(rect.LeftBottom) });
        public void Fill(Circle circle) => Graphics.FillEllipse(new SolidBrush(Color), (float)(circle.Center.X - circle.Radius/* + 0.5*/) * ZoomLevel + OffsetX, (float)(circle.Center.Y - circle.Radius /*+ 0.5*/) * ZoomLevel + OffsetY, 2 * (float)circle.Radius * ZoomLevel, 2 * (float)circle.Radius * ZoomLevel);
        public void Fill(Rect shape)
        {
            var p = F(shape.Location);
            Graphics.FillRectangle(new SolidBrush(Color), p.X, p.Y, (float)(shape.Width * ZoomLevel), (float)(shape.Height * ZoomLevel));
        }
        public Color Color
        {
            get => pen.Color;
            set => pen.Color = value;
        }
        public Pen pen = new Pen(Color.Red);
        public Brush brush = new SolidBrush(Color.Red);

        //internal System.Drawing.Point F(Point p) => new System.Drawing.Point((int)(ZoomLevel * (p.X + 0.5f)) + OffsetX, (int)(ZoomLevel * (p.Y + 0.5f)) + OffsetY);

        internal PointF F(Point p)
        {
            if (ZoomLevel == 1)
            {
                return new PointF((float)p.X + OffsetX, (float)p.Y + OffsetY);
            }
            else //if(ZoomLevel>1)
                return new PointF(ZoomLevel * ((float)p.X + 0.5f) + OffsetX, ZoomLevel * ((float)p.Y + 0.5f) + OffsetY);
        }


    }
    public delegate void DisplayGDIPaintEventHandler(object sender, DisplayGDIPaintEventArgs e);
    public class SimpleViewForm : Form,IProgress<PixelInfo?>
    {
        
        public SimpleViewForm(Bitmap img)
        {
            Size = new System.Drawing.Size(600, 400);
            StartPosition= FormStartPosition.CenterScreen;
            var dp = new DisplayGDI() { Dock = DockStyle.Fill, Image = img, PixelInfoProgress = this };
            this.Controls.Add(dp);
            dp.AutoSizeImage();
            ForeColor = Color.Black;
        }

        public void Report(PixelInfo? value)
        {
            this.Invoke((Action)(()=>Text = value.ToString()));
        }
    }

    public static class DisplayHelper
    {
        public static void ShowForm(this Bitmap bmp) => new SimpleViewForm(bmp).Show();
    }

    public class DisplayGDIMouseEventArgs : MouseEventArgs
    {
        public DisplayGDIMouseEventArgs(MouseEventArgs e, int offsetX, int offsetY, double zoomLevel) : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            this.ZoomLevel = zoomLevel;
            X = (double)(e.Location.X- offsetX ) / zoomLevel-0.5;
            Y = (double)(e.Location.Y- offsetY ) / zoomLevel-0.5;
            LineHitRadius = DefaultLineHitRadius / zoomLevel;
            DraggingPointRadius = DefaultDraggingPointRadius / zoomLevel;
        }

        public bool IsMouseOver(Circle circle) => Geometry.GetDistance(Location, circle, out _) < LineHitRadius;
        public bool IsMouseOver(Point point) => (point - Location).Length < DraggingPointRadius;
        public bool IsMouseOver(Arc arc) => Geometry.GetDistance(Location, arc, out _) < LineHitRadius;
        public bool IsMouseOver(Segment segment) => Geometry.GetDistance(Location, segment, out _) < LineHitRadius;
        public bool IsMouseOver(Line line) => Geometry.GetDistance(Location, line, out _) < LineHitRadius;

        public new double X { get; }
        public new double Y { get; }
        public Vector Offset => new Vector(X, Y);
        public int BaseX => base.X;
        public int BaseY => base.Y;
        public double ZoomLevel { get; }
        public System.Drawing.Point BaseLocation => base.Location;
        public new Point Location => new Point(X, Y);
        public bool Cancel { get; set; } = false;

        public static double DefaultLineHitRadius = 3.5;
        public static double DefaultDraggingPointRadius = 5;
        public double LineHitRadius { get; }
        public double DraggingPointRadius { get; }
    }

    public struct PixelInfo
    {
        public System.Drawing.Point Position;
        public int PixelValue;
        public System.Drawing.Imaging.PixelFormat PixelFormat;

        public override string ToString()
        {
            switch (PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return $"{Position.X.ToString().PadLeft(4)},{Position.Y.ToString().PadLeft(4)} = {PixelValue.ToString("X6")}";
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return $"{Position.X.ToString().PadLeft(4)},{Position.Y.ToString().PadLeft(4)} = {PixelValue.ToString("X6")}";
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return $"{Position.X.ToString().PadLeft(4)},{Position.Y.ToString().PadLeft(4)} = {PixelValue}";
                default:
                    throw new NotImplementedException();
            }
            //Debug.WriteLine();
        }
    }
}
