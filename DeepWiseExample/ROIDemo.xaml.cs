using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace DeepWise.Test
{
    using DeepWise.Controls;
    using DeepWise.Controls.Interactivity;
    using DeepWise.Controls.Interactivity.BehaviorControllers;
    using DeepWise.Data;
    using DeepWise.Devices;
    using DeepWise.Json;
    using DeepWise.Metrology;
    using DeepWise.Shapes;
    using DeepWise.Windows;
    using Newtonsoft.Json;
    using OpenCvSharp.Extensions;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using System.Runtime.Serialization;

    /// <summary>
    /// DisplayDemo.xaml 的互動邏輯
    /// </summary>
    [Group("互動")]
    public partial class ROIDemo : Window
    {
        // Camera cam = new CameraIDS();
        public ROIDemo()
        {
            InitializeComponent();
#if MAT
            display.Image = Properties.Resources.detectionSample.ToMat();
#else
            display.Image = Properties.Resources.detectionSample;
#endif

            //Ini
            propertyGrid.SelectedObject = Ctrl = new ROIDemoController(display, roiData);

            //Save when leave
            this.Closed += (s, e) =>
            {
                roiData.ROI = Ctrl.ROI;
                if (Ctrl.Mask != null)
                    roiData.Mask = new Bitmap(Ctrl.Mask.GDIBitmap, Ctrl.Mask.GDIBitmap.Width, Ctrl.Mask.GDIBitmap.Height);
                roiData.SaveConfig();
            };
        }

        ROIDemoController Ctrl;
        ROIData roiData { get; } = new ROIData("roiData");

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(btn_CropRect):
                    {
                        var cropper = new ShapeCropper<DeepWise.Shapes.Rect>();
                        (sender as Button).IsEnabled = false;
                        display.Behavior = cropper;
                        if (await cropper.WaitResult())
                        {
                            MessageBox.Show(cropper.Region.ToString());
                        }
                        (sender as Button).IsEnabled = true;
                        display.Behavior = null;
                        break;
                    }
                case nameof(btn_DrawMask):
                    {
                        var cropper = new MaskPainter();
                        (sender as Button).Visibility = Visibility.Hidden;
                        propertyGrid.SelectedObject = cropper;
                        display.Behavior = cropper;
                        if (await cropper.WaitResult())
                        {
#if MAT
                            cropper.Mask.ToMat().ShowDialog();
#else
                            cropper.Mask.ShowDialog();
#endif
                        }
                        (sender as Button).Visibility = Visibility.Visible;
                        propertyGrid.SelectedObject = null;
                        display.Behavior = null;
                        break;
                    }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ROIData : Config
    {
        public ROIData(string path, bool relativePath = true, bool encrypt = false) : base(path, relativePath, encrypt)
        {
        }

        [JsonConverter(typeof(BaseTypeConverter<IAreal>))]
        public IAreal ROI { get; set; }

        [JsonConverter(typeof(BitmapJsonConverter))]
        public Bitmap Mask { get; set; }

        public unsafe Bitmap GetCombinedMask()
        {
            if (ROI == null || Mask == null)
            {
                throw new Exception("ROIData 尚未初始化");
            }

            switch (Mask.PixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    break;
                default:
                    throw new Exception("PixelForat of Mask must be 'Format8bppIndexed' or 'Format32bppArgb'");
            }

            //Draw ROI
            var f_mask = new Bitmap(Mask.Width, Mask.Height);
            var g = Graphics.FromImage(f_mask);
            switch(ROI)
            {
                case Circle circle:
                    g.FillEllipse(Brushes.White, circle.GetBoundingBox());
                    break;
                case Ring ring:
                    g.FillEllipse(Brushes.White, ring.GetOuterCircle().GetBoundingBox());
                    g.FillEllipse(Brushes.Black, ring.GetInnerCircle().GetBoundingBox());
                    break;
                case RectRotatable rect:
                    var path = new DeepWise.Shapes.Point[] { rect.LeftTop, rect.RightTop, rect.RightBottom, rect.LeftBottom };
                    g.FillPolygon(Brushes.White,path.Select(p=>new PointF((float)p.X, (float)p.Y)).ToArray());
                    break;
                default:
                    throw new Exception();
            }
            g.Dispose();

            //Paint Mask
            f_mask = f_mask.ConvertToMono8();
            var srcData = this.Mask.LockBits(new Rectangle(0, 0, Mask.Width, Mask.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Mask.PixelFormat);
            var destData = f_mask.LockBits(new Rectangle(0, 0, f_mask.Width, f_mask.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            var w = Mask.Width;
            var stride = srcData.Stride;
            int bytesPerPixel = Mask.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed ? 1 : 4;
            Parallel.For(0, Mask.Height, y =>
            {
                int* src = (int*)srcData.Scan0;
                byte* dest = (byte*)destData.Scan0;
                src += srcData.Stride / bytesPerPixel * y;
                dest += destData.Stride  * y;
                for (int x = w - 1; x >= 0; x--)
                {
                    if (*src != 0) *dest = 0;
                    dest++;
                    src++;
                }
            });
            Mask.UnlockBits(srcData);
            f_mask.UnlockBits(destData);
            return f_mask;
        }
    }


    public class ROIDemoController : INotifyPropertyChanged
    {
        Display display;

        public event PropertyChangedEventHandler PropertyChanged;
        ROIData data;
        public ROIDemoController(Display display, ROIData data)
        {
            this.data = data;
            var roi = data.ROI;
            var mask = data.Mask;
            this.display = display;
            if(roi == null)
            {
                display.InteractiveObjects.Add(new CircleROI(new Circle(display.ImageWidth / 2, display.ImageHeight / 2, display.ImageHeight / 3)));
            }
            else
            {
                switch (roi)
                {
                    case DeepWise.Shapes.RectRotatable rect:
                        _ROIType = ROIDemoROIType.Rectangle;
                        display.InteractiveObjects.Add(new RotatableRectROI(rect));
                        break;
                    case DeepWise.Shapes.Ring ring:
                        _ROIType = ROIDemoROIType.Ring;
                        display.InteractiveObjects.Add(new RingROI(ring));
                        break;
                    case DeepWise.Shapes.Circle circle:
                        display.InteractiveObjects.Add(new CircleROI(circle));
                        _ROIType = ROIDemoROIType.Circle;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if(mask!=null)
            {
                this.Mask = new CoopBitmap(mask, Color.FromArgb(50, 0, 120, 215));
                display.OverlayImage = this.Mask.InteropBitmap;
            }
        }

        ROIDemoROIType _ROIType = ROIDemoROIType.Ring;

        [Browsable(false)]
        public IAreal ROI => (display.InteractiveObjects[0] as InteractiveROI).GetRegion() as IAreal;

        [DisplayName("ROI類型"), Order(1)]
        public ROIDemoROIType ROIType
        {
            get => _ROIType;
            set
            {
                display.InteractiveObjects.Clear();
                switch(value)
                {
                    case ROIDemoROIType.Circle:
                        display.InteractiveObjects.Clear();
                        display.InteractiveObjects.Add(new CircleROI(new Circle(display.ImageWidth / 2, display.ImageHeight / 2, display.ImageHeight / 3)));
                        break;
                    case ROIDemoROIType.Ring:
                        display.InteractiveObjects.Clear();
                        display.InteractiveObjects.Add(new RingROI(new Ring(display.ImageWidth / 2, display.ImageHeight / 2, display.ImageHeight / 3, display.ImageHeight / 5)));
                        break;
                    case ROIDemoROIType.Rectangle:
                        display.InteractiveObjects.Clear();
                        display.InteractiveObjects.Add(new RotatableRectROI(new RectRotatable(display.ImageWidth / 4, display.ImageHeight / 4, display.ImageWidth / 2, display.ImageHeight / 2, 0)));
                        break;
                }
                _ROIType = value;
            }
        }

        [DisplayName("編輯Mask"), Button, Order(2)]
        public async void PaintMask()
        {
            var ori = display.OverlayImage;
            var cropper = Mask != null ? new MaskPainter(Mask.GDIBitmap) : new MaskPainter();
            var propertyGrid = PropertyGrid.GetOwnerPropertyGrid(this);
            propertyGrid.SelectedObject = cropper;
            display.Behavior = cropper;

            if (await cropper.WaitResult())
            {
                var overlay = new CoopBitmap(cropper.Mask, cropper.OverlayColor);
                propertyGrid.SelectedObject = this;
                display.Behavior = null;
                display.OverlayImage = overlay.InteropBitmap;
                Mask = overlay;
            }
            else
            {
                propertyGrid.SelectedObject = this;
                display.Behavior = null;
                display.OverlayImage = ori;
            }
        }

        [DisplayName("預覽組合Mask"), Button, Order(3)]
        public void ShowCombinedMask()
        {
            data.ROI = ROI;
            if (Mask != null)
                data.Mask = new Bitmap(Mask.GDIBitmap, Mask.GDIBitmap.Width, Mask.GDIBitmap.Height);
            data.SaveConfig();
#if MAT
            data.GetCombinedMask().ToMat().ShowDialog();
#else
            data.GetCombinedMask().ShowDialog();
#endif
        }

        [Browsable(false)]
        public CoopBitmap Mask { get; set; }
    }

    public enum ROIDemoROIType
    {
        [Display(Name = "圓形")]
        Circle,
        [Display(Name = "環形")]
        Ring,
        [Display(Name ="矩形")]
        Rectangle,
    }

}
