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
    using OpenCvSharp.Extensions;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    /// <summary>
    /// DisplayDemo.xaml 的互動邏輯
    /// </summary>
    [Group("UI控制項")]
    [DisplayName("Display(影像、相機顯示)")]
    [Description("此範例提展示Display控制像，以用來檢視相機影像或圖片。")]
    public partial class DisplayDemo : Window
    {
       
        //==================
        Camera cam;
        public DisplayDemo()
        {
            InitializeComponent();

            var pnl = new StackPanel();
            var btn = new Button() { Content = "IDS", Width = 200, Height = 48, Margin = new Thickness(10, 10, 10, 10) };
            btn.Click += (s, e2) => cam = new CameraIDS();
            try
            {
                new CameraIDS();
            }
            catch (Exception ex)
            {
                btn.IsEnabled = false;
            }
            pnl.Children.Add(btn);
            //===========================================
            btn = new Button() { Content = "IDS_U3", Width = 200, Height = 48, Margin = new Thickness(10, 0, 10, 10) };
            try
            {
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + @"C:\Program Files\IDS\ids_peak\generic_sdk\api\binding\dotnet\x86_64");
                Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + @"C:\Program Files\IDS\ids_peak\generic_sdk\api\lib\x86_64");
                new CameraIDS_U3();
            }
            catch (Exception ex)
            {
                btn.IsEnabled = false;
            }
            btn.Click += (s, e2) => cam = new CameraIDS_U3();
            pnl.Children.Add(btn);
            //===========================================
            btn = new Button() { Content = "Basler", Width = 200, Height = 48, Margin = new Thickness(10, 0, 10, 10) };
            try
            {
                new CameraBasler();
            }
            catch (Exception ex)
            {
                btn.IsEnabled = false;
            }
            btn.Click += (s, e2) => cam = new CameraBasler();
            pnl.Children.Add(btn);
            //===================================================
            btn = new Button() { Content = "Image", Width = 200, Height = 48, Margin = new Thickness(10, 0, 10, 10) };
            pnl.Children.Add(btn);
            foreach (Button _btn in pnl.Children)
                _btn.Click += (s, e2) => Window.GetWindow(s as Button).DialogResult = true;
            var win = new Window() { Content = pnl ,SizeToContent = SizeToContent.WidthAndHeight ,WindowStartupLocation = WindowStartupLocation.CenterScreen};
            if (win.ShowDialog() is bool b && b)
            {

                if (cam != null)
                {
                    try
                    {

                        if (cam.Initialize())
                        {
                            cam.Start();
                            display.Camera = cam;
                        }
                        else
                            MessageBox.Show("相機開啟失敗");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
#if MAT
                    display.Image = Properties.Resources.detectionSample.ToMat();
#else
                    display.Image = Properties.Resources.detectionSample;
#endif
                }
            }
            else
            {
                this.Loaded += (s, e) =>
                {
                    //Close secretly
                    Opacity = 0;
                    this.WindowStyle = WindowStyle.None;
                    Close();
                };
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            cam?.Dispose();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as Button).Name)
                {
                    case nameof(btn_Capture):
                        cam?.Capture();
                        break;
                    case nameof(btn_Start):
                        cam?.Start();
                        break;
                    case nameof(btn_Stop):
                        cam?.Stop();
                        break;
                    case nameof(btn_Setting):
                        if (cam != null)
                            PropertyGrid.Show(cam);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_Setting_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
