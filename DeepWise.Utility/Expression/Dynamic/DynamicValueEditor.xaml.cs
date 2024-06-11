using System;
using System.Collections.Generic;
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

namespace DeepWise.Expression.Dynamic
{
    /// <summary>
    /// Interaction logic for DynamicValueEditor.xaml
    /// </summary>
    public partial class DynamicValueEditor : Window
    {
        public DynamicValueEditor()
        {
            InitializeComponent();
        }

        public IDynamicExpression DynamicValue { get;  }
        public IEvaluationContext EvaluationContext { get;  }
        public DynamicValueEditor(IDynamicExpression dyExp, IEvaluationContext context = null) : this()
        {
            //dyExp = DynamicValue = Newtonsoft.Json.JsonConvert.DeserializeObject<IDynamicExpression>(Newtonsoft.Json.JsonConvert.SerializeObject(dyExp));
            DynamicValue = dyExp;
            EvaluationContext = context;
            var list = Enum.GetValues(typeof(Constructor)).Cast<Constructor>().Where(Match).ToList();
            list.Insert(0, Constructor.Expression);
            list.Insert(0, Constructor.Conditional);
            list.Insert(0, Constructor.Variable);
            if (dyExp.ValueType == typeof(bool))
                list.Insert(0, Constructor.BooleanValue);
            else if (dyExp.ValueType == typeof(double))
                list.Insert(0, Constructor.RealNumber);
            else if (dyExp.ValueType == typeof(string))
                list.Insert(0, Constructor.String);
            cbx_Constructor.ItemsSource = list;

            if (dyExp.Constructor == Constructor.None)
            {
                if (dyExp.ValueType == typeof(bool))
                    cbx_Constructor.SelectedIndex = 0;
                else
                    cbx_Constructor.SelectedIndex = 0;
            }
            else
            {
                cbx_Constructor.SelectedItem = dyExp.Constructor;
            }

            bool Match(Constructor c)
            {
                var enumInfo = typeof(Constructor).GetMember(c.ToString())[0];
                var att = enumInfo.GetCustomAttribute<ConstructorInfoAttribute>();
                return att!=null && att.ResultType == dyExp.ValueType;
            }
        }

        private void cbx_Constructor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbx_Constructor.SelectedItem is Constructor c)
            {
                this.DynamicValue.Constructor = c;
                switch(c)
                {
               
                    case Constructor.Variable:
                        {
                            grid_Content.Children.Clear();
                            var cbx = new ComboBox()
                            {
                                ItemsSource = VariableTable.Default.Where(x=>x.ValueType == DynamicValue.ValueType).Select(x=>x.Name),
                                VerticalAlignment = VerticalAlignment.Top,
                                HorizontalAlignment = HorizontalAlignment.Left,
                            };
                            
                            grid_Content.Children.Add(cbx);
                            cbx.SelectionChanged += (s2, e2) => DynamicValue.Arguments[0] = cbx.SelectedItem as string;
                            if (bool.TryParse(DynamicValue.Arguments[0], out var ori))
                                cbx.SelectedItem = ori;
                            else
                                cbx.SelectedIndex = 0;

                            break;
                        }
                    case Constructor.BooleanValue:
                        {
                            grid_Content.Children.Clear();
                            var cbx = new ComboBox()
                            {
                                ItemsSource = new List<bool>() { true, false },
                                VerticalAlignment = VerticalAlignment.Top,
                                HorizontalAlignment = HorizontalAlignment.Left,
                            };
                       
                            grid_Content.Children.Add(cbx);
                            cbx.SelectionChanged += (s2, e2) => DynamicValue.Arguments[0] = cbx.SelectedItem.ToString();
                            if (bool.TryParse(DynamicValue.Arguments[0], out var ori))
                                cbx.SelectedItem = ori;
                            else
                                cbx.SelectedIndex = 0;

                            break;
                        }
                    case Constructor.RealNumber:
                        {
                            grid_Content.Children.Clear();

                            var value = new Temp<double>();
                            if (double.TryParse(DynamicValue.Arguments[0], out var ori))
                            {
                                value.Value = ori;
                            }
                            var tbx = new TextBox()
                            {
                                VerticalAlignment = VerticalAlignment.Top,
                            };
                            tbx.TextChanged += (s2, e2) => DynamicValue.Arguments[0] = tbx.Text.ToString();
                            tbx.SetBinding(TextBox.TextProperty, new Binding("Value") { Source = value });
                            grid_Content.Children.Add(tbx);
                            break;
                        }
                    case Constructor.Expression:
                    case Constructor.String:
                        {
                            grid_Content.Children.Clear();
                            var tbx = new TextBox()
                            {
                                Margin=new Thickness(10),
                            };
                            tbx.SetBinding(TextBox.TextProperty, new Binding("Arguments[0]") { Source = DynamicValue });
                            grid_Content.Children.Add(tbx);
                            tbx.SelectionChanged += (s2, e2) => DynamicValue.Arguments[0] = tbx.Text.ToString();
                            break;
                        }
                    default:
                        {
                            Type[] parameters;
                            string text;
                            switch (c)
                            {
                                case Constructor.Conditional:
                                    parameters = new Type[] { typeof(bool), DynamicValue.ValueType, DynamicValue.ValueType };
                                    text = "若條件{0}成立則{1}否則{2}";
                                    break;
                                default:
                                    var att = typeof(Constructor).GetMember(c.ToString())[0].GetCustomAttribute<ConstructorInfoAttribute>();
                                    parameters = att.Parameters;
                                    text = att.Description;
                                    break;
                            }
                            grid_Content.Children.Clear();
                            grid_Content.Children.Add(tbx_Content);
                            tbx_Content.Inlines.Clear();
                            
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                var index = text.IndexOf($"{{{i}}}");
                                string preceding = text.Substring(0, index);
                                tbx_Content.Inlines.Add(new Run(preceding));
                                Hyperlink hyperl = new Hyperlink(new Run($"({DynamicValue.References[i] ?? (object)parameters[i]})"));
                                hyperl.Tag = new Tuple<int,Type>(i,parameters[i]);
                                hyperl.Click += Hyperl_Click;
                                tbx_Content.Inlines.Add(hyperl);
                                text = text.Substring($"{{{i}}}".Length + index);
                            }

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                tbx_Content.Inlines.Add(new Run(text));
                            }
                            break;
                        }
                }
            }
        }

        private void Hyperl_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is Tuple<int,Type> info)
            {
                var index = info.Item1;
                var targetType = info.Item2;
                IDynamicExpression dyExp;
                if (DynamicValue.References[index] is null)
                {
                    dyExp = Activator.CreateInstance(typeof(DynamicExpression<>).MakeGenericType(targetType)) as IDynamicExpression;
                }
                else
                {
                    //Clone
                    try
                    {
                        dyExp = Newtonsoft.Json.JsonConvert.DeserializeObject<IDynamicExpression>(Newtonsoft.Json.JsonConvert.SerializeObject(DynamicValue.References[index]));
                    }
                    catch
                    {
                        MessageBox.Show("物件解析失敗");
                        dyExp = Activator.CreateInstance(typeof(DynamicExpression<>).MakeGenericType(targetType)) as IDynamicExpression;
                    }


                }

                if (new DynamicValueEditor(dyExp).ShowDialog() == true)
                {
                    DynamicValue.References[index] = dyExp;
                    hyperlink.Inlines.Clear();
                    hyperlink.Inlines.Add(dyExp.ToString());

                }
            }
        }

        private void btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(sender == btn_Confirm)
            {
                //Do some verification
                DialogResult = true;
            }
            else if(sender == btn_Cancel)
            {
                DialogResult = false;
            }
        }
    }

    public class Temp<T>
    {
        public T Value { get; set; }
    }
}
