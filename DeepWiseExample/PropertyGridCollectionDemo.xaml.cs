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
    [DisplayName("PropertyGrid.Collection(集合)")]
    [Description("此範例提供如何使用PropertyGrid的控件，以及其提供的功能、擴充屬性。e.g.\r\n[SliderAttribute]\r\n[DisplayNameAttribute]\r\n[OrderAttribute]\r\n等等.......")]
    public partial class PropertyGridCollectionDemo : Window
    {
        public PropertyGridCollectionDemo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch( (sender as Button).Content as string )
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

    public class PropertyGridCollectionDemoObject 
    {
        [DisplayName("Dictionary<string, AxisSpeedSetting>")]
        public Dictionary<string, AxisSpeedSetting> Setting { get; } = new Dictionary<string, AxisSpeedSetting>()
        {
            {"X",new AxisSpeedSetting() },
            {"Y",new AxisSpeedSetting() },
            {"Z",new AxisSpeedSetting() },
        };

        public List<string> StringList { get; } = new List<string>() { "ASD","QWE" };

        [DisplayName("System.Drawing.PointF[]")]
        public System.Drawing.PointF[] TestArray { get; } = new System.Drawing.PointF[] { new System.Drawing.PointF(), new System.Drawing.PointF(3, 2) };

        [DisplayName("DemoObject[]")]
        public DemoObject[] ItemsArray { get; } = new DemoObject[] { new DemoObject(), new DemoObject() { Width = 1 } };

        [DisplayName("List<DemoObject>")]
        public List<DemoObject> ItemsList { get; } = new List<DemoObject> { new DemoObject(), new DemoObject() { Width = 1 } };

        [DisplayName("List<DemoObject>"), Editable(false), Mark(Mark.Exclamation), Description("with [Editable(false)] attribute")]
        public List<DemoObject> ItemsList2 { get; } = new List<DemoObject> { new DemoObject(), new DemoObject() { Width = 1 } };

        [DisplayName("Dictionary<string, Point3D>")]
        [Button(targetMethodPath: nameof(PointClicked))]
        public Dictionary<string, Point3D> PointTable { get; } = new Dictionary<string, Point3D>()
        {
            {"Origin",new Point3D() },
            {"Top",new Point3D(10,10,80) },
        };

        void PointClicked(object sender, DataGridButtonClickedEventArgs e)
        {
            MessageBox.Show(e.Key + ":" + e.Value);
        }
    }
}
