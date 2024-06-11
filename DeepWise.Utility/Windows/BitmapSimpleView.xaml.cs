using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DeepWise.Windows
{
    /// <summary>
    /// BitmapSimpleView.xaml 的互動邏輯
    /// </summary>
    public partial class BitmapSimpleView : System.Windows.Window
    {
        internal BitmapSimpleView()
        {
            InitializeComponent();
        }
#if MAT
        public BitmapSimpleView(OpenCvSharp.Mat image) : this()
        {
            display.Image = image;
        }
#else
        public BitmapSimpleView(System.Drawing.Bitmap image) : this()
        {
            display.Image = image;
        }
#endif
        bool flag = false;
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if(!flag)
            {
                display.Stretch();
                flag = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            display?.Stretch();
        }
    }

    public static class BitmapViewWindowExtensions
    {
#if MAT
        public static void ShowWindow(this Mat bmp, string title = "",System.Windows.Window parent=null)
        {
            new BitmapSimpleView(bmp) { Title = title ,Owner = parent}.Show();
        }

        [DebuggerStepThrough]
        public static void ShowDialog(this Mat bmp, string title = "")
        {
            new BitmapSimpleView(bmp) { Title = title }.ShowDialog();
        }
#else
        public static void ShowWindow(this System.Drawing.Bitmap bmp)
        {
            new BitmapSimpleView(bmp).Show();
        }

        public static void ShowDialog(this System.Drawing.Bitmap bmp)
        {
            new BitmapSimpleView(bmp).ShowDialog();
        }

        public static void ShowDialog(this System.Drawing.Bitmap bmp,string title)
        {
            new BitmapSimpleView(bmp) { Title = title}.ShowDialog();
        }
#endif
    }
}
