using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DeepWise.Expression
{
    /// <summary>
    /// Interaction logic for NamingDialog.xaml
    /// </summary>
    public partial class CreateVariableDialog : Window
    {
        public CreateVariableDialog()
        {
            InitializeComponent();
        }

        public CreateVariableDialog(string defaultName, IEnumerable<string> parent) : this()
        {
            ResultName = defaultName;
            var b = tbx_Name.GetBindingExpression(TextBox.TextProperty).ParentBinding;
            b.ValidationRules.Add(validator = new NamingValidator(parent));

            this.cbx_Type.SelectedValuePath = "Value";
            this.cbx_Type.DisplayMemberPath = "Key";
            cbx_Type.ItemsSource = new Type[] 
            {
                typeof(double),
                typeof(bool),
                typeof(string),
                typeof(DeepWise.Shapes.Point),
                typeof(DeepWise.Shapes.Point3D),
                typeof(List<double>),
                typeof(List<bool>),
                typeof(List<string>),
                typeof(List<DeepWise.Shapes.Point>),
                typeof(List<DeepWise.Shapes.Point3D>),
            }.Select(x => new KeyValuePair<string, Type>(GetName(x), x));



            string GetName(Type type)
            {
                if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return $"List<{GetName(type.GetGenericArguments()[0])}>";
                }
                else
                {
                    return type.Name;
                }
            }
            cbx_Type.SelectedIndex = 0;
            //tbx_Name.GetBindingExpression(TextBox.TextProperty).BindingGroup.ValidationRules.Add(validator = new NamingValidator(parent));
        }

        NamingValidator validator;
        public string ResultName
        {
            get=> (string)GetValue(ResultNameProperty);
            set=> SetValue(ResultNameProperty, value);
        }

        public IVariable Variable { get; private set; }
        public static DependencyProperty ResultNameProperty = DependencyProperty.Register(nameof(ResultName), typeof(string), typeof(CreateVariableDialog));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = validator.Validate(tbx_Name.Text, System.Globalization.CultureInfo.CurrentCulture);
            if (result.IsValid) 
            {
                Variable = Activator.CreateInstance(typeof(Variable<>).MakeGenericType(cbx_Type.SelectedValue as Type), ResultName) as IVariable;
                DialogResult = true;
            }
            else
                MessageBox.Show(result.ErrorContent as string);
        }
    }
}
