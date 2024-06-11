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
    using DeepWise.Devices;
    using DeepWise.Metrology;
    using DeepWise.Windows;
    using OpenCvSharp.Extensions;
    using System.ComponentModel;

    /// <summary>
    /// DisplayDemo.xaml 的互動邏輯
    /// </summary>
    [Group("互動")]
    [Description("此範例展示如何透過IDisplayBehavior來控制Display控制項的互動邏輯。\n例如：繪製Mask、擷取ROI等等")]
    public partial class DisplayBehaviorDemo : Window
    {
        public DisplayBehaviorDemo()
        {
            InitializeComponent();
        }

        [Button]
        public async void CropRect()
        {
            var cropper = new ShapeCropper<DeepWise.Shapes.Rect>();
            propertyGrid.IsEnabled = false;
            display.Behavior = cropper;
            if (await cropper.WaitResult())
            {
                var src = display.Image;
                var tmp = cropper.Region;

                var rect = new System.Drawing.Rectangle((int)tmp.X, (int)tmp.Y, (int)tmp.Width, (int)tmp.Height);
                try
                {
#if MAT
                    var cropped = new OpenCvSharp.Mat(src, new OpenCvSharp.Rect(rect.X, rect.Y, rect.Width, rect.Height));
                    cropped.ShowDialog("cropped image");
#else
                    var cropped = (System.Drawing.Bitmap)src.Clone(rect, src.PixelFormat);
                    cropped.ShowDialog("cropped image");
#endif
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            propertyGrid.IsEnabled = true;
            display.Behavior = null;
        }

        [Button]
        public async void PaintMask()
        {
            var painter = new MaskPainter();
            propertyGrid.SelectedObject = painter;
            display.Behavior = painter;
            if (await painter.WaitResult())
            {
                propertyGrid.SelectedObject = this;
                display.Behavior = null;
#if MAT
                painter.Mask.ToMat().ShowDialog("mask");
#else
                painter.Mask.ShowDialog("mask");
#endif
            }
            else
            {
                propertyGrid.SelectedObject = this;
                display.Behavior = null;
            }

        }

        [Button]
        public async void PolygonBuilder()
        {
            var builder = new PolygonBuilder();
            propertyGrid.SelectedObject = builder;
            display.Behavior = builder;
            if (await builder.WaitResult())
            {
                MessageBox.Show($"Polygon[{builder.Polygon.Corners.Length}]") ;
                propertyGrid.SelectedObject = this;
                display.Behavior = null;
            }
            else
            {
                propertyGrid.SelectedObject = this;
                display.Behavior = null;
            }

        }
    }
   
}
