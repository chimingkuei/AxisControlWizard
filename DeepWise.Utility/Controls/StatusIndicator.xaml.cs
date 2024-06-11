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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeepWise.Controls
{
    /// <summary>
    /// Interaction logic for StatusIndicator.xaml
    /// </summary>
    public partial class StatusIndicator : UserControl
    {
        public StatusIndicator()
        {
            InitializeComponent();
        }

        ColorAnimation Animation { get; } = new ColorAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.1)) };
        DoubleAnimation Animation2 { get; } = new DoubleAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.1)) };

        public static readonly DependencyProperty LightColorProperty = DependencyProperty.Register(nameof(LightColor), typeof(Color), typeof(StatusIndicator), new PropertyMetadata(Colors.Gray, OnLightColorChanged));

        private static void OnLightColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = sender as StatusIndicator;
            s.Animation.From = (s.Light.Fill as SolidColorBrush).Color;
            var color = (Color)e.NewValue;
            s.Animation.To = color;
            s.Light.Fill.BeginAnimation(SolidColorBrush.ColorProperty, s.Animation);
            s.Animation2.From = s.Blur.Radius;
            s.Animation2.To = color.GetBrightness() > 0.5f ? 10 : 1;
            s.Blur.BeginAnimation(BlurEffect.RadiusProperty, s.Animation2);
        }

        public Color LightColor
        {
            get => (Color)GetValue(LightColorProperty);
            set => SetValue(LightColorProperty, value);
        }
    }

    public class IndicatorColors
    {
        public static Color Red => (Color)ColorConverter.ConvertFromString("0xFFFF0000");
        public static Color RedDark => (Color)ColorConverter.ConvertFromString("0xFF600000");
        public static Color Green => (Color)ColorConverter.ConvertFromString("0xFF00FF00");
        public static Color GreenDark => (Color)ColorConverter.ConvertFromString("0xFF006000");
        public static Color Blue => (Color)ColorConverter.ConvertFromString("0xFF0000FF");
        public static Color BlueDark => (Color)ColorConverter.ConvertFromString("0xFF000060");
        public static Color Gray => (Color)ColorConverter.ConvertFromString("0xFF646464");
        public static Color White => (Color)ColorConverter.ConvertFromString("0xFFB4B4B4");
    }
}
