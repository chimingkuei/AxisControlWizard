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
using DeepWise.Expression;
namespace DeepWise.Test
{

    [Group("系統"),DisplayName("Variables(變數表)")]
    [Description("此範例展示如何使用變數表，並透過動態的運算式來計算值。")]
    public partial class VariableDemo : Window
    {
        public VariableDemoCofig Config { get; } = new VariableDemoCofig("varDemoCfg");
        public VariableDemo()
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

        public class VariableDemoCofig : Config
        {
            public VariableDemoCofig(string path, bool relativePath = true, bool encrypt = false) : base(path, relativePath, encrypt)
            {
            }
     

            [DisplayName("DoubleExpression")]
            public IDynamicExpression Value1 { get; set; } = new DynamicExpression<double>();

            [Button("Calculate"), DisplayName("Test")]
            public void A()
            {
                try
                {
                    MessageBox.Show(Value1.Evaluate(Expression.VariableTable.Default).ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
