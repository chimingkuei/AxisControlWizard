using DeepWise.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using DeepWise.Controls.Interactivity;
using System.ComponentModel;
using static System.Drawing.BitmapExtensions;
using Bitmap = System.Drawing.Bitmap;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Markup;

namespace DeepWise.Controls
{
    /// <summary>
    /// DisplayEx.xaml 的互動邏輯
    /// </summary>
    [ContentProperty]
    public partial class Display : UserControl , INotifyPropertyChanged
    {
        #region Variables
        private readonly MatrixTransform _transform = new MatrixTransform(1,0,0,1,0,0);
        private Point _initialMousePosition;
        public double OffsetX => _transform.Matrix.OffsetX / _transform.Matrix.M11;

        public bool IsFocusLocked { get; set; } = false;

        public double OffsetY => _transform.Matrix.OffsetY / _transform.Matrix.M22;
        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register("ZoomLevel", typeof(double), typeof(Display));
        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            private set => SetValue(ZoomLevelProperty, value);
        }
        public float Zoomfactor { get; set; } = 1.1f;
     
        #endregion

        public Display()
        {
            InitializeComponent();
            MouseDown += PanAndZoomCanvas_MouseDown;
            MouseUp += PanAndZoomCanvas_MouseUp;
            MouseMove += PanAndZoomCanvas_MouseMove;
            MouseWheel += PanAndZoomCanvas_MouseWheel;
            Loaded += DisplayEx_Loaded;
            
            LayoutUpdated += Display_LayoutUpdated;
            InteractiveObjects.CollectionChanged += InteractiveObjects_CollectionChanged;
            this.canvas.RenderTransform = _transform;
        }

        private void Display_LayoutUpdated(object sender, EventArgs e)
        {
            isUserVisible = this.IsUserVisible();
        }
        private bool isUserVisible;

        private void DisplayEx_Loaded(object sender, RoutedEventArgs e)
        {
            if(Camera!=null || Image!=null)
            {
                if (AutoStretched) Stretch();
                Loaded -= DisplayEx_Loaded;
            }
        }

        public readonly static DependencyProperty BehaviorProperty = DependencyProperty.Register(nameof(Behavior), typeof(IDisplayBehavior), typeof(Display),new PropertyMetadata(BehaviorChanged));

        static void BehaviorChanged(DependencyObject obj,DependencyPropertyChangedEventArgs e)
        {
            var _this = obj as Display;
            if (e.OldValue is IDisplayBehavior old)
                old.Exist();
            if (e.NewValue is IDisplayBehavior _new)
                _new.Enter(_this);
 
        }
        public IDisplayBehavior Behavior
        {
            get => (IDisplayBehavior)GetValue(BehaviorProperty);
            set => SetValue(BehaviorProperty, value);
        }

        public void CaptureSave()
        {
#if MAT
            OpenCvSharp.Mat bitmap = null;
#else
            System.Drawing.Bitmap bitmap = null;
#endif
            try
            {
                bitmap = Capture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (bitmap != null)
            {
                using (var dlg = new System.Windows.Forms.SaveFileDialog())
                using (bitmap)
                {
                    dlg.Filter = "點陣圖|*.bmp";
                    dlg.Title = "Save an Image File";
                    dlg.Title = "儲存圖片";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
#if MAT
                        bitmap.SaveImage(dlg.FileName);
#else
                        bitmap.Save(dlg.FileName);
#endif
                    }
                }
            }
            else
                MessageBox.Show("沒有可用的影像", "錯誤", MessageBoxButton.OK);

        }

#if MAT
        public OpenCvSharp.Mat Capture()
        {
            throw new NotImplementedException();
        }
#else
        public System.Drawing.Bitmap Capture()
        {
            if (Image != null)
            {
                return (System.Drawing.Bitmap)Image.Clone();
            }
            else if (Camera != null)
            {
                return (Camera as Camera).Capture();
            }
            else
                return null;
        }
#endif

        public void SaveWithContext(string path)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,(int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);

            rtb.Render(canvas);

            //var crop = new CroppedBitmap(rtb, rect);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = System.IO.File.OpenWrite(path))
            {
                pngEncoder.Save(fs);
            }
        }
        public int ImageWidth => Camera != null ? Camera.Size.Width : (Image != null ? Image.Width : 0);
        public int ImageHeight => Camera != null ? Camera.Size.Height : (Image != null ? Image.Height : 0);

        private void InteractiveObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (InteractiveObject obj in e.NewItems)
                    {
                        if(ZoomLevel!=0)
                            obj.UpdateLayout(ZoomLevel);
                        foreach (Shape shape in obj.ShapeElements)
                        {
                            if (ZoomLevel != 0)
                            {
                                if(shape.Tag == null || (shape.Tag is string str && str == "contour"))
                                shape.StrokeThickness = 1 / ZoomLevel;
                            }
                            canvas.Children.Add(shape);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (InteractiveObject obj in e.OldItems)
                    {
                        foreach (Shape shape in obj.ShapeElements)
                        {
                            canvas.Children.Remove(shape);
                        }
                        if (ActiveObject == obj) ActiveObject = null;

                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if(e.OldItems != null)
                    foreach (InteractiveObject obj in e.OldItems)
                    {
                        foreach (Shape shape in obj.ShapeElements)
                        {
                            canvas.Children.Remove(shape);
                        }
                    }
                    ActiveObject = null;
                    break;
            }
        }

        public InteractiveObjectCollcetion InteractiveObjects { get; } = new InteractiveObjectCollcetion();

        //TODO : merge  ActiveObject 、 SetFocus
        public InteractiveObject ActiveObject { get; set; }
        public bool DragginItem { get; set; }
        public void SetFocus(InteractiveObject obj)
        {
            if (ActiveObject == obj) return;
            if (ActiveObject != null) ActiveObject.Focused = false;
            ActiveObject = obj;
            foreach (var shape in ActiveObject.ShapeElements)
                canvas.Children.Remove(shape);
            //int c = canvas.Children.Count;
            foreach (var shape in obj.ShapeElements)
            {
                canvas.Children.Add(shape);
            }
            ActiveObject.Focused = true;
            InteractiveObjects.Move(InteractiveObjects.IndexOf(ActiveObject), 0);
            System.Diagnostics.Debug.WriteLine("SetFocus : " + obj);
        }

        public void ClearFocus()
        {
            if (ActiveObject != null) ActiveObject.Focused = false;
            ActiveObject = null;
            
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Stretch();
        }

        public Matrix ViewMatrix
        {
            get => _transform.Matrix;
            set
            {
                if (_transform.Matrix == value) return;
                _transform.Matrix = value;
                UpdateStrokeThickness();
            }
        }

        public void StretchNone()
        {
            _transform.Matrix = new Matrix(1, 0, 0, 1, 0,0);
            ZoomLevel = _transform.Matrix.M11;
            UpdateStrokeThickness();
        }

        public void ZoomIn()
        {
            float scaleFactor = Zoomfactor;
            Point mousePostion = new Point(ActualWidth / 2, ActualHeight / 2);
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;
            ZoomLevel = _transform.Matrix.M11;
            UpdateStrokeThickness();
        }
        public void ZoomOut()
        {
            float scaleFactor = 1/Zoomfactor;
            Point mousePostion = new Point(ActualWidth / 2, ActualHeight / 2);
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;
            ZoomLevel = _transform.Matrix.M11;
            UpdateStrokeThickness();
        }

        void UpdateStrokeThickness()
        {
            foreach (UIElement child in canvas.Children)
            {
                if (child is Shape shape)
                {
                    shape.StrokeThickness = 1 / ZoomLevel;
                }
            }

            foreach(var item in InteractiveObjects)
            {
                item.UpdateLayout(ZoomLevel);
            }
        }

        public static readonly DependencyProperty AutoStretchedProperty  = DependencyProperty.Register(nameof(AutoStretched), typeof(bool), typeof(Display),new PropertyMetadata(false, (s, e) => (s as Display)?.Stretch()));

        public bool AutoStretched
        {
            get => (bool)GetValue(AutoStretchedProperty);
            set=> SetValue(AutoStretchedProperty, value);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (!DesignerProperties.GetIsInDesignMode(this) && AutoStretched) Stretch();
        }

        public System.Drawing.Point? CursorPosition
        {
            get => (System.Drawing.Point?)GetValue(CursorPositionProperty);
            private set => SetValue(CursorPositionProperty, value);
        }
        static void OnCursorPositionChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)
        {
            var _this = sender as Display;
            if (e.NewValue != null)
            {
                var p = (System.Drawing.Point)e.NewValue;

                if (p.X >= 0 && p.X < _this.ImageWidth && p.Y >= 0 && p.Y < _this.ImageHeight)
                {
                    _this.CursorPositionPixelValue = _this.GetPixel(p);
                }
                else
                    _this.CursorPositionPixelValue = null;
            }
            else
                _this.CursorPositionPixelValue = null;

        }
        public static readonly DependencyProperty CursorPositionProperty = DependencyProperty.Register(nameof(CursorPosition), typeof(System.Drawing.Point?),typeof(Display),new PropertyMetadata(OnCursorPositionChanged));

        public int? CursorPositionPixelValue
        {
            get => (int?)GetValue(CursorPositionPixelValueProperty);
            private set => SetValue(CursorPositionPixelValueProperty, value);
        }
        public static readonly DependencyProperty CursorPositionPixelValueProperty = DependencyProperty.Register(nameof(CursorPositionPixelValue), typeof(int?),typeof(Display));

        public event PropertyChangedEventHandler PropertyChanged;

        private unsafe int? GetPixel(System.Drawing.Point p)
        {
            if(Camera != null)
            {
                var format = Camera.Format;
                var bpp = format.GetBytesPerPixel();
                int stride = format.GetStride( Camera.Size.Width );
                int pixelValue = 0;
                byte* ptr = (byte*)bufferedBitmap.Scan0;
                ptr += stride * p.Y + p.X * bpp;
                for (int i = 0; i < bpp; i++)
                {
                    pixelValue += ptr[i] << (8 * i);
                }
                return pixelValue;
            }
            else if(Image!=null)
            {
#if MAT
                var matType = Image.Type();
                if (matType == OpenCvSharp.MatType.CV_8UC1)
                    return Image.At<byte>(p.Y, p.X);
                else if (matType == OpenCvSharp.MatType.CV_8UC3)
                    return (Image.At<int>(p.Y, p.X) << 8 >> 8) & 0xFFFFFF;
                else if (matType == OpenCvSharp.MatType.CV_8UC4)
                    return Image.At<int>(p.Y, p.X);
                else
                    throw new NotImplementedException();
#else
                try
                {
                    var format = Image.PixelFormat;
                    var bpp = format.GetBytesPerPixel();
                    int stride = format.GetStride(Image.Width);
                    switch(format)
                    {
                        case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                            {
                                int pixelValue = 0;
                                byte* ptr = (byte*)imageBitmapData.Scan0;
                                ptr += stride * p.Y + p.X * bpp;
                                for (int i = 0; i < bpp; i++)
                                {
                                    pixelValue += ptr[i] << ((8 * i));
                                }
                                return pixelValue;
                            }
                        case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                            {
                                int pixelValue = 0;
                                byte* ptr = (byte*)imageBitmapData.Scan0;
                                ptr += stride * p.Y + p.X * bpp;
                                for (int i = 0; i < bpp; i++)
                                {
                                    pixelValue += ptr[i] << ((8 * i));
                                }
                                return pixelValue;
                            }
                        case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                            {
                                byte* ptr = (byte*)imageBitmapData.Scan0;
                                return ptr[stride * p.Y + p.X];
                            }
                            
                        default:
                            throw new NotImplementedException();
                    }
                    
                }
                catch { return null; }
#endif
            }
            else
                return null;
        }

        private void PanAndZoomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            }
            
            var args = new DisplayMouseEventArgs(_transform.Inverse.Transform(e.GetPosition(this)) - new Vector(0.5, 0.5),e.ChangedButton, e.LeftButton,e.RightButton, this);
            Behavior?.MouseDown(args);
            if (args.Cancel) goto Finish;
            switch(e.ChangedButton)
            {
                case MouseButton.Left:
                    {
                        if (ActiveObject != null)
                        {
                            if (ActiveObject.IsMouseOver(args))
                            {
                                ActiveObject.MouseDown(args);
                                System.Diagnostics.Debug.WriteLine("MouseDown : " + ActiveObject);
                                DragginItem = true;
                                return;
                            }
                            if (args.Cancel) goto Finish;
                        }
                        if (IsFocusLocked) goto Finish;
                        foreach (InteractiveObject roiObj in InteractiveObjects)
                        {
                            if (roiObj.IsMouseOver(args))
                            {
                                SetFocus(roiObj);
                                Cursor = ActiveObject.Cursor;

                                goto Finish;
                            }
                        }
                        ClearFocus();
                        ActiveObject = null;
                        DragginItem = false;
                    }
                    break;
                case MouseButton.Right:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        this.ContextMenu.IsOpen = true;
                    }
                    break;
            }
        Finish:
            return;
        }
        private void PanAndZoomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var args = new DisplayMouseEventArgs(_transform.Inverse.Transform(e.GetPosition(this)) - new Vector(0.5, 0.5), e.ChangedButton, e.LeftButton,e.RightButton, this);

            Behavior?.MouseUp(args);
            if (args.Cancel) return;

            if(DragginItem && e.ChangedButton == MouseButton.Left)
            {
                ActiveObject.MouseUp(args);
                DragginItem = false;
            }
        }
        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_transform == null || _transform.Inverse == null) return;
            if (e.MiddleButton == MouseButtonState.Pressed && !AutoStretched)
            {
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;
            }
            var args = new DisplayMouseEventArgs(_transform.Inverse.Transform(e.GetPosition(this)) - new Vector(0.5,0.5), e.LeftButton,e.RightButton, this);
            CursorPosition = args.PixelLocation;

            Behavior?.MouseMove(args);
            if (args.Cancel) return;

            if (DragginItem && e.LeftButton == MouseButtonState.Pressed)
            {
                ActiveObject.MouseMove(args);
                return;
            }

            foreach (InteractiveObject obj in InteractiveObjects)
            {
                //TODO : 增加Enable或Selectable機制
                if (obj.IsMouseOver(args))
                {
                    Cursor = obj == ActiveObject ? obj.Cursor : Cursors.Hand;
                    return;
                }
            }
            Cursor = Cursors.Arrow;
        }

        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (AutoStretched) return;
            float scaleFactor = Zoomfactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
            }

            Point mousePostion = e.GetPosition(this);
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;
            ZoomLevel = _transform.Matrix.M11;
            UpdateStrokeThickness();

            var p = _transform.Inverse.Transform(e.GetPosition(this));
            CursorPosition = new System.Drawing.Point((int)p.X, (int)p.Y);
        }

        private void OnContextMenuItemClicked(object sender, RoutedEventArgs e)
        {
          
            switch ((sender as FrameworkElement).Name)
            {
                case nameof(btn_SaveImage):
                    {
                        CaptureSave();
                        break;
                    }
            }
        }

        private void UserControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CursorPosition = null;
        }
    }
    
    public class InteractiveObjectCollcetion : ObservableCollection<InteractiveObject>
    {
        public void ClearMarks()
        {
            foreach (var item in this.Where(x => x is DeepWise.Controls.Interactivity.Mark).ToArray())
            {
                this.Remove(item);
            }
        }
        public void Clear(Type type)
        {
            foreach (var item in this.Items.Where(x=>type.IsAssignableFrom(x.GetType())).ToList())
                this.Remove(item);
            
        }
        public new void Clear()
        {
            foreach (var item in this.Items.ToArray())
                this.Remove(item);
        }
        protected override void ClearItems()
        {
            foreach (var item in this.Items.ToArray())
                this.Remove(item);
        }
    }
}
