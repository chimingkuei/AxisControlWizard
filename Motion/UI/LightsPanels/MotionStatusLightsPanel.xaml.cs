using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace MotionControllers.UI
{
    /// <summary>
    /// MotionStatusPanel.xaml 的互動邏輯
    /// </summary>
    public partial class MotionStatusLightsPanel : UserControl
    {
        public MotionStatusLightsPanel()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty AxisIDProperty = DependencyProperty.Register(nameof(AxisID), typeof(int), typeof(MotionStatusLightsPanel),new PropertyMetadata(-1));
        public int AxisID
        {
            get => (int)GetValue(AxisIDProperty);
            set => SetValue(AxisIDProperty, value);
        }

    }
}
