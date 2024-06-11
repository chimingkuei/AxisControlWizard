using System;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;


namespace DeepWise.Test
{
    using DeepWise.Controls;
    using DeepWise.Controls.Interactivity;
    using DeepWise.Devices;
    using DeepWise.Metrology;
    using DeepWise.Windows;
    using OpenCvSharp.Extensions;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using RectRotatable = DeepWise.Shapes.RectRotatable;
    /// <summary>
    /// DisplayDemo.xaml 的互動邏輯
    /// </summary>
    [Group("互動")]
    [Description("此範例提供如何尋找邊緣線段，並求得其回歸線。")]
    public partial class EdgeDetectionDemo : Window
    {
        public Camera cam;
        public EdgeDetectionDemo()
        {
            InitializeComponent();
            display.InteractiveObjects.Add(ROI = new LineDetectionROI(new Shapes.RectRotatable(625,100,1300,200,0)));
#if MAT
            display.Image = bmp = Properties.Resources.detectionSample.ToMat().CvtColor(OpenCvSharp.ColorConversionCodes.RGB2GRAY);
#else
            display.Image = bmp = Properties.Resources.detectionSample;
#endif

            propertyGrid.SelectedObject = setting = new DetectionTest(this);
        }

        public LineDetectionROI ROI;
        public EdgeDetectionSetting setting;
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            cam?.Dispose();
        }

        public void Detect()
        {

            var binary = bmp.Threshold(40, 255, OpenCvSharp.ThresholdTypes.Binary);
            binary.ShowDialog();
            binary.FindContours(out var contours, out _, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxSimple);
            var contour = contours.OrderByDescending(x => x.Length).First();
            var roi = new OpenCvSharp.Rect(590, 100, 1200, 200);
            var linePs = contour.Where(x => roi.Contains(x));
            var segment = DeepWise.Shapes.Fitting.GetRegressedSegment(linePs.Select(x => (DeepWise.Shapes.Point)x));
      
            var toShow = bmp.CvtColor(OpenCvSharp.ColorConversionCodes.GRAY2RGB);
            toShow.Line((OpenCvSharp.Point)segment.P0, (OpenCvSharp.Point)segment.P1, new OpenCvSharp.Scalar(0, 255, 0));
            toShow.ShowDialog();
           //OpenCvSharp.Cv2.FitLine(linePs, OpenCvSharp.DistanceTypes.L2,)
            //===========================================
#if MAT
#else
            Bitmap mono = null;
            System.Drawing.Imaging.BitmapData data = null;
#endif
            try
            {
#if MAT
                var edge = bmp.GetShape<DeepWise.Shapes.Segment>(((Shapes.RectRotatable)ROI), setting);
#else
                mono = bmp.ConvertToMono8();
                data = mono.LockBits(new System.Drawing.Rectangle(0, 0, mono.Width, mono.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                //=============================================
                var edge = data.GetShape<DeepWise.Shapes.Segment>(((Shapes.RectRotatable)ROI), setting);
#endif
                display.InteractiveObjects.ClearMarks();
                if (!Shapes.Segment.IsNaN(edge))
                    display.InteractiveObjects.Add(new LineMark(edge));
                //=============================================
                //mono.UnlockBits(data);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);    
            }
            finally
            {
#if MAT
#else
                if (data!=null) mono.UnlockBits(data);
#endif
            }
        }

        public void FindEdgels()
        {
#if MAT
#else
            Bitmap mono = null;
            System.Drawing.Imaging.BitmapData data = null;
#endif
            try
            {
#if MAT
                var ps = bmp.GetShpaeEdgels<Shapes.Line>(((Shapes.RectRotatable)ROI), setting);
#else
                mono = bmp.ConvertToMono8();
                data = mono.LockBits(new System.Drawing.Rectangle(0, 0, mono.Width, mono.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                //=============================================
                var ps = data.GetShpaeEdgels<Shapes.Line>(((Shapes.RectRotatable)ROI), setting);
#endif

                display.InteractiveObjects.ClearMarks();
                foreach (var p in ps)
                {
                    display.InteractiveObjects.Add(new PointMark(p));
                }
                //=============================================
                //mono.UnlockBits(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
#if MAT
#else
                if (data != null) mono.UnlockBits(data);
#endif
            }
        }
#if MAT
        public OpenCvSharp.Mat bmp;
#else
        public System.Drawing.Bitmap bmp;
#endif

        internal void Load()
        {
            using(var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
#if MAT
                    bmp = new OpenCvSharp.Mat(dlg.FileName, OpenCvSharp.ImreadModes.Grayscale);
#else
                    bmp = System.Drawing.BitmapExtensions.UnlockLoad(dlg.FileName);
#endif
                    display.Image = bmp;
                }
            }

        }
    }

    public class DetectionTest : EdgeDetectionSetting
    {
        EdgeDetectionDemo window;
        public DetectionTest(EdgeDetectionDemo window)
        {
            this.window = window;
            SearchType = EdgelSearchOptions.First | EdgelSearchOptions.Crest;
            MinimumEdgeValue = 10;
        }

        [Button, DisplayName("讀取影像"),Display(GroupName ="測試")]
        public void Load()
        {
            window.Load();
        }

        [Button,DisplayName("線段偵測"), Display(GroupName = "測試")]
        public void FindLine()
        {
            window.Detect();
        }

        [Button,DisplayName("線段偵測(顯示點群)"), Display(GroupName = "測試")]
        public void FindEdgels()
        {
            window.FindEdgels();
        }

        [Button, DisplayName("顯示ROI數值"), Display(GroupName = "測試")]
        public void GetROI()
        {
            DeepWise.NotepadHelper.ShowInNotepad(Newtonsoft.Json.JsonConvert.SerializeObject(((RectRotatable)window.ROI)));
        }

    }
}
