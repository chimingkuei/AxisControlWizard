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
    [DisplayName("PropertyGrid.CustomEditor(使用者擴充)")]
    [Description("此範例展示如何利用CustomEditorAttribute來使用自訂義的視窗或控制項")]
    public partial class PropertyGridCustomEditorDemo : Window
    {
        public PropertyGridCustomEditorDemo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Content as string)
            {
                case "Value1 = 50":
                    //(propertyGrid.SelectedObject as PropertyGridDemoObject).Value1 = 50;
                    //You can either implement INotifyPropertyChanged interface for the target classes
                    //or call 'Update' method each time you manual changed the property value behind code
                    break;
                case "Update Value1":
                    propertyGrid.Update("Value1");
                    //propertyGrid.Update() ->use method with no parameters for updates all properties
                    break;
                case "Change Layout":
                    propertyGrid.LabelWidth = new GridLength(3, GridUnitType.Star);
                    propertyGrid.FieldWidth = new GridLength(1, GridUnitType.Star);
                    //Equals to <UI:PropertyGrid  LabelWidth="3*" FieldWidth="1*"/> in Xaml
                    propertyGrid.ItemMargin = new Thickness(10);
                    break;
            }
        }
    }

    public class PropertyGridCustomEditorDemoObject : INotifyPropertyChanged
    {

        [CustomEditor(typeof(DataGridTest))]
        public List<string> StringList { get; } = new List<string>() { "ABC", "DEF" };

        [CustomEditor(typeof(ListBox),true, contenProperty: "ItemsSource")]
        public List<int> NumberList { get; } = new List<int>() { 1, 2, 3, 4 };

        [CustomEditor(typeof(CounterButton))]
        public int Counter
        {
            get => _counter;
            set
            {
                //call PropertyChanged event in order to set binding successfully
                if (_counter == value) return;
                _counter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Counter)));
            }
        }

        int _counter;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class CounterButton : Button
    {
        public CounterButton()
        {
            this.DataContextChanged += CounterButton_DataContextChanged;
        }

        private void CounterButton_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                var tbk = new TextBlock();
                this.Content = tbk;
                tbk.SetBinding(TextBlock.TextProperty, new Binding(Name)
                {
                    Source = DataContext,
                    Mode = BindingMode.OneWay,
                });
            }
        }

        protected override void OnClick()
        {
            base.OnClick();
            var pInfo = DataContext.GetType().GetProperty(Name);
            var crt = (int)pInfo.GetValue(DataContext);
            pInfo.SetValue(DataContext, crt + 1);
            //this.Content = crt + 1;
        }
    }

    

    public class DataGridTest : Window
    {
        public DataGridTest(List<string> item)
        {
            this.Content = new ListBox() { ItemsSource = item };
        }
    }
    
}
