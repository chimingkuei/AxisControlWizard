using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace DeepWise.Expression
{
    /// <summary>
    /// Interaction logic for VairableWindow.xaml
    /// </summary>
    public partial class VariableView : UserControl
    {
        public VariableView()
        {
            InitializeComponent();
           dataGrid.ItemsSource = VariableCollection = VariableTable.Default;
        }

        public VariableTable VariableCollection { get; set; }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(btn_New):
                    {
                        var win = new CreateVariableDialog("", VariableCollection.Select(x => x.Name));
                        if (win.ShowDialog() == true)
                        {
                            VariableCollection.Add(win.Variable);
                        }
                    }
                    break;
                case nameof(btn_Copy):
                    {
                        
                        break;
                    }
                case nameof(btn_Delete):
                    {
                        break;
                    }
                case nameof(btn_Rename):
                    {
                        break;
                    }
            }
        }


        public static void Populate(object a,object b)
        {
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(b);
            Newtonsoft.Json.JsonConvert.PopulateObject(jsonString,a);
        }

    }


    public class TypeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type t)
            {
                var a = t.GetInterface("IList");
                return t.GetInterface("IList") == null;
            }
            else
                throw new NotImplementedException();
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type t)
            {
                if (parameter == null)
                    return GetName(t);
                else
                    throw new NotImplementedException();

            }
            else
                throw new NotImplementedException();
        }

        static string GetName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return $"List<{GetName(type.GetGenericArguments()[0])}>";
            }
            else
            {
                return type.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
