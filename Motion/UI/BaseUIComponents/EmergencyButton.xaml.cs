using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionControllers.UI
{
    /// <summary>
    /// StatusIndicator.xaml 的互動邏輯
    /// </summary>
    public partial class EmergencyButton : UserControl
    {
        public EmergencyButton()
        {
            InitializeComponent();
            Loaded += EmergencyButton_Loaded;
        }

        private void EmergencyButton_Loaded(object sender, RoutedEventArgs e)
        {
            //scaleT.ScaleX = ActualWidth / 150;
            //scaleT.ScaleY = ActualHeight / 150;
            ////mainGrid.RenderSize = new Size(RenderSize.Width * scaleT.ScaleX, RenderSize.Height*scaleT.ScaleX);
        }

        private async void EmergencyButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.IsEnabled = false;
                var value = true;
                double duration = 0.09;
                double s1 = 1;
                double s2 = 0.95;
                Color c1 = (Color)ColorConverter.ConvertFromString("#00404040");
                Color c2 = (Color)ColorConverter.ConvertFromString("#30000000");

                Storyboard storyboard = new Storyboard();
                storyboard.AutoReverse = true;
                

                var anim = new DoubleAnimation(value ? s1 : s2, value ? s2 : s1, new Duration(TimeSpan.FromSeconds(duration)));
                storyboard.Children.Add(anim);
                Storyboard.SetTarget(anim, btn_Grid);
                Storyboard.SetTargetProperty(anim, new PropertyPath("RenderTransform.(ScaleTransform.ScaleX)"));
                anim = new DoubleAnimation(value ? s1 : s2, value ? s2 : s1, new Duration(TimeSpan.FromSeconds(duration)));
                storyboard.Children.Add(anim);
                Storyboard.SetTarget(anim, btn_Grid);
                Storyboard.SetTargetProperty(anim, new PropertyPath("RenderTransform.(ScaleTransform.ScaleY)"));
                var clranim = new ColorAnimation(value ? c1 : c2, value ? c2 : c1, new Duration(TimeSpan.FromSeconds(duration)));

                storyboard.Children.Add(clranim);
                Storyboard.SetTarget(clranim, Overlay);
                Storyboard.SetTargetProperty(clranim, new PropertyPath("Fill.Color"));
                var task = storyboard.BeginAsync();
                Clicked?.Invoke(this, EventArgs.Empty);
                await task;
                this.IsEnabled = true;
            }
        }
        public event EventHandler Clicked;
       

        private void btn_Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                IsPressed = false;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            //if(e.Property.Name == "Width")
            //{
            //    scaleT.ScaleX = Width / 150;
            //}
            //else if (e.Property.Name == "Height")
            //{
            //    scaleT.ScaleY = Height / 150;
            //}
        }

        public Size ButtonSize { get; set; }
        bool _isPressed = false;
        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                _isPressed = value;
  

            }
        }

  
    }

    public static class StoryboardExtensions
    {
        public static Task BeginAsync(this Storyboard storyboard)
        {
            System.Threading.Tasks.TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin();
            }
            return tcs.Task;
        }
    }

}
