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
    using DeepWise.Metrology;
    using OpenCvSharp.Extensions;
    using System.ComponentModel;

    /// <summary>
    /// DisplayDemo.xaml 的互動邏輯
    /// </summary>
    [DisplayName("SimpleWidthCalibration(長度校正)")]
    [Group("互動")]
    [Description("此範例展示利用量測功能進行簡易的長度校正。")]
    public partial class SimpleWidthCalibrationDemo : Window
    {
        public SimpleWidthCalibrationDemo()
        {
            InitializeComponent();
            swc = new SimpleWidthCalibration(this);
#if MAT
            display.Image = Properties.Resources.scale_ruler.ToMat();
#else
            display.Image = Properties.Resources.scale_ruler;
#endif
            
            display.InteractiveObjects.Add(ROI1);
            display.InteractiveObjects.Add(ROI2);
            propertyGrid.SelectedObject = swc;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            display.Stretch();
        }
        SimpleWidthCalibration swc;
        internal LineDetectionROI ROI1 = new LineDetectionROI(new Shapes.RectRotatable(2011,166.5,200,36.5, 0.0155058));
        internal LineDetectionROI ROI2 = new LineDetectionROI(new Shapes.RectRotatable(2001, 1873.5, 200, 12.5, -0.00657885));
    }

    public class SimpleWidthCalibration : EdgeDetectionSetting , INotifyPropertyChanged
    {
        SimpleWidthCalibrationDemo window;

        public event PropertyChangedEventHandler PropertyChanged;

        public SimpleWidthCalibration(SimpleWidthCalibrationDemo window)
        {
            this.window = window;
        }

        [Button]
        public void Calibrate()
        {
#if MAT
            var bmp = window.display.Capture();
            try
            {
                //=============================================
                var edge0 = bmp.GetShape<DeepWise.Shapes.Segment>((Shapes.RectRotatable)window.ROI1, this);
                var edge1 = bmp.GetShape<DeepWise.Shapes.Segment>((Shapes.RectRotatable)window.ROI2, this);
                window.display.InteractiveObjects.ClearMarks();
                if (!Shapes.Segment.IsNaN(edge0)) window.display.InteractiveObjects.Add(new LineMark(edge0));
                if (!Shapes.Segment.IsNaN(edge1)) window.display.InteractiveObjects.Add(new LineMark(edge1));
                if (!Shapes.Segment.IsNaN(edge0) && !Shapes.Segment.IsNaN(edge1))
                {
                    var vLine = new DeepWise.Shapes.Line(edge0.MidPoint, edge0.Angle + Math.PI / 2);
                    DeepWise.Shapes.Geometry.Intersect(vLine, (DeepWise.Shapes.Line)edge0, out DeepWise.Shapes.Point p0);
                    DeepWise.Shapes.Geometry.Intersect(vLine, (DeepWise.Shapes.Line)edge1, out DeepWise.Shapes.Point p1);
                    window.display.InteractiveObjects.Add(new LineMark(p0,p1));
                    this.MeasuredPixels = (p1 - p0).Length;

                    this.PixelResolution = Distance / MeasuredPixels;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasuredPixels)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PixelResolution)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
#else
            var bmp = window.display.Capture();
            if(bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                bmp = System.Drawing.BitmapExtensions.ConvertToMono8(bmp);

            System.Drawing.Imaging.BitmapData data = null;
            var mono = bmp;
            try
            {
                data = mono.LockBits(new System.Drawing.Rectangle(0, 0, mono.Width, mono.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                //=============================================
                var edge0 = data.GetShape<DeepWise.Shapes.Segment>((Shapes.RectRotatable)window.ROI1, this);
                var edge1 = data.GetShape<DeepWise.Shapes.Segment>((Shapes.RectRotatable)window.ROI2, this);
                window.display.InteractiveObjects.ClearMarks();
                if (!Shapes.Segment.IsNaN(edge0)) window.display.InteractiveObjects.Add(new LineMark(edge0));
                if (!Shapes.Segment.IsNaN(edge1)) window.display.InteractiveObjects.Add(new LineMark(edge1));
                if (!Shapes.Segment.IsNaN(edge0) && !Shapes.Segment.IsNaN(edge1))
                {
                    var vLine = new DeepWise.Shapes.Line(edge0.MidPoint, edge0.Angle + Math.PI / 2);
                    DeepWise.Shapes.Geometry.Intersect(vLine, (DeepWise.Shapes.Line)edge0, out DeepWise.Shapes.Point p0);
                    DeepWise.Shapes.Geometry.Intersect(vLine, (DeepWise.Shapes.Line)edge1, out DeepWise.Shapes.Point p1);
                    window.display.InteractiveObjects.Add(new LineMark(p0,p1));
                    this.MeasuredPixels = (p1 - p0).Length;

                    this.PixelResolution = Distance / MeasuredPixels;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasuredPixels)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PixelResolution)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (data != null) mono.UnlockBits(data);
            }
#endif
        }

        [Description("校正長度")]
        public double Distance { get; set; } = 3;
        [Description("校正長度")]
        public double MeasuredPixels { get; private set; }
        [DecimalPlaces(20)]
        public double PixelResolution { get; private set; }
    }


}
