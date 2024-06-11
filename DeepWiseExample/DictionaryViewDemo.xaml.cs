using DeepWise.AccessControls;
using DeepWise.Controls;
using DeepWise.Localization;
using DeepWise.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// <summary>
    /// PropertyGridDemo.xaml 的互動邏輯
    /// </summary>
    [Group("UI控制項")]
    [DisplayName("DictionaryView")]
    public partial class DictionaryViewDemo : Window
    {
        public static Dictionary<string, Shapes.Point> dic = new Dictionary<string, DeepWise.Shapes.Point>()
            {
                {"A",new Shapes.Point(0,0) },
                {"B",new Shapes.Point(4,3) },
            };
        public DictionaryViewDemo()
        {
            InitializeComponent();

            dictionaryView.ItemSources = dic;
        }

        private void dictionaryView_SmallButtonClicked(object sender, DataGridButtonClickedEventArgs e)
        {
            MessageBox.Show($"{e.Key} = {e.Value}");
        }
    }
}
