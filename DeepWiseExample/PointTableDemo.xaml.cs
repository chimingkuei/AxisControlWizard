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
    [DisplayName("DictionaryView(PointTable)")]
    public partial class PointTableDemo : Window
    {
        public Dictionary<string, Shapes.Point> dic = new Dictionary<string, DeepWise.Shapes.Point>()
        {
            {"A",new Shapes.Point(0,0) },
            {"B",new Shapes.Point(4,3) },
        };

        public List<Shapes.Point> list = new List<DeepWise.Shapes.Point>()
        {
            new Shapes.Point(0,0),
            new Shapes.Point(4,3),
        };

        public PointTableDemo()
        {
            InitializeComponent();
            valueTable.ItemsSource = list;
        }

        private void dictionaryView_SmallButtonClicked(object sender, DataGridButtonClickedEventArgs e)
        {
            MessageBox.Show($"{e.Key} = {e.Value}");
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            switch((e.Source as Button).Name)
            {
                case nameof(btn_UpdateSrc):
                    valueTable.UpdateSource();
                    break;
                case nameof(btn_UpdateTgt):
                    valueTable.UpdateTarget();
                    break;
                case nameof(btn_HideBtn):
                    valueTable.IsButtonVisible = !valueTable.IsButtonVisible;
                    //can also use "ButtonContent" property to set button's content
                    //and "ButtonClicked" event to handle click event
                    break;
                case nameof(btn_SwitchSrc):
                    if (valueTable.ItemsSource == list)
                        valueTable.ItemsSource = dic;
                    else
                        valueTable.ItemsSource = list;
                    break;
            }
            
        }

        private void numericTable_ButtonClicked(object sender, DataGridButtonClickedEventArgs e)
        {
            if (e.Key != null)
                MessageBox.Show($"{e.Key} = {e.Value}");
            else
                MessageBox.Show($"{e.Value}");
        }
    }
}
