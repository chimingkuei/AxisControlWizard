using DeepWise.Controls;
using DeepWise.Data;
using DeepWise.Localization;
using DeepWise.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

    [Group("系統"),DisplayName("Config(存檔讀檔)")]
    [Description("此範例展示如何繼承以及使用Config類別，以簡化應用程式的物件存取功能。")]
    public partial class ConfigDemo : Window
    {
        //繼承至Config的物件位在實例化時會去自動讀取檔案。其中建構式引述代表其檔案路徑(相對或絕對)。
        public DemoCofig Config { get; } = new DemoCofig("demoConfig");
        
        public ConfigDemo()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = Config;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case "BTN_Save":
                    Config.SaveConfig();
                    break;
                case "BTN_Load":
                    Config.LoadConfig();
                    break;
            }
        }
    }

    public class DemoCofig : Config
    {
        public DemoCofig() : base(null) { }
        public DemoCofig(string path, bool relativePath = true, bool encrypt = false) : base(path, relativePath, encrypt)
        {

        }

        public DemoSubItem ReadonlyItem { get; } = new DemoSubItem();
        public DemoSubItem ReadWriteItem { get; set; } = new DemoSubItem();

        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _verticalAlignment = value;
                NotifyPropertyChanged();
            }
        }

        //[SmallButton(nameof(ShowGrid))]
        public Dictionary<string, Point3D> Point3Dic { get; } = new Dictionary<string, Point3D>()
            {
                {"Origin",new Point3D() },
                {"Top",new Point3D(10,10,80) },
            };

        void ShowGrid()
        {
            //var dlg = new Window();
            //var dataGrid = new DataGrid();
            //dlg.Content = new DataGrid() { ItemsSource = Point3DRefEditor.GetList(Point3Dic) };
            //dlg.ShowDialog();
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        VerticalAlignment _verticalAlignment = VerticalAlignment.Center;
        int _value = 125;
        string _name = "TestItem";
    }

    public class DemoSubItem
    {
        public string Text { get; set; }
        public List<Shapes.Point> Ps { get; } = new List<Shapes.Point>() { new Shapes.Point(3, 3) };
        public Dictionary<string, Point3D> Point3Dic { get; } = new Dictionary<string, Point3D>()
            {
                {"Origin",new Point3D() },
                {"Top",new Point3D(10,10,80) },
            };
        [JsonIgnore]
        public string Sec { get; } = DateTime.Now.Second.ToString();
    }
}
