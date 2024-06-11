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
    [Description("此範例提展示如何在Display控件上利用GDI+繪製覆蓋的點陣圖。")]
    public partial class BitmapOverlayDemo : Window
    {
        // Camera cam = new CameraIDS();
        public BitmapOverlayDemo()
        {
            InitializeComponent();
#if MAT
            display.Image = Properties.Resources.detectionSample.ToMat();
#else
            display.Image = Properties.Resources.detectionSample;
#endif

            //初始化CoopBitmap作為Overlay的點陣圖
#if MAT
            Overlay = new CoopBitmap(new System.Drawing.Size(display.Image.Width, display.Image.Height));
#else
            Overlay = new CoopBitmap(display.Image.Size);
#endif
            display.OverlayImage = Overlay.InteropBitmap;
        }

        CoopBitmap Overlay;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(sender == btn_Clear)
            {
                Overlay.Clear();
            }
            else if(sender == btn_Paint)
            {
                //透過CoopBitmap.Draw方法繪製Overlay圖層
                Overlay.Draw(g =>
                {
                    System.Drawing.Drawing2D.LinearGradientBrush linGrBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
      new System.Drawing.Point(100, 100),
      new System.Drawing.Point(1500, 1500),
      System.Drawing.Color.FromArgb(50, 255, 0, 0),   // Opaque red
      System.Drawing.Color.FromArgb(50, 0, 0, 255));
                    
                    g.FillEllipse(linGrBrush, new RectangleF(500, 500, 500, 500));
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    g.FillEllipse(Brushes.Transparent, new RectangleF(550, 550, 600, 600));
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    for (int i = 0; i < 3; i++)
                        g.FillEllipse(linGrBrush, new RectangleF(700 + i * 125, 700 + i * 125, 100, 100));
                });


                Grid grid = new Grid();

                //==========================================
                grid.ColumnDefinitions.Clear();
                grid.RowDefinitions.Clear();

                //初始化行列數
                int column = 6, row = 6;
                for (int i = 1; i < column; i++)
                    grid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int i = 1; i < row; i++)
                    grid.RowDefinitions.Add(new RowDefinition());


                //將img 放到位置3,3
                var img = new  System.Windows.Controls.Image();
                Grid.SetColumn(img, 3);
                Grid.SetRow(img, 3);
                grid.Children.Add(img);
                //==========================================
            }
        }
    }
}
