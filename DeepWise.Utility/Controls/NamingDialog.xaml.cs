using System;
using System.Collections.Generic;
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

namespace DeepWise.Controls
{
    /// <summary>
    /// Interaction logic for NamingDialog.xaml
    /// </summary>
    public partial class NamingDialog : Window
    {
        public NamingDialog()
        {
            InitializeComponent();
        }

        public NamingDialog(string defaultName, IEnumerable<string> parent) : this()
        {
            ResultName = defaultName;
            var b = tbx_Name.GetBindingExpression(TextBox.TextProperty).ParentBinding;
            b.ValidationRules.Add(validator = new NamingValidator(parent));
            //tbx_Name.GetBindingExpression(TextBox.TextProperty).BindingGroup.ValidationRules.Add(validator = new NamingValidator(parent));
        }

        NamingValidator validator;
        public string ResultName
        {
            get=> (string)GetValue(ResultNameProperty);
            set=> SetValue(ResultNameProperty, value);
        }
        public static DependencyProperty ResultNameProperty = DependencyProperty.Register(nameof(ResultName), typeof(string), typeof(NamingDialog));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = validator.Validate(tbx_Name.Text, System.Globalization.CultureInfo.CurrentCulture);
            if (result.IsValid)
            {
                DialogResult = true;
            }
            else
                MessageBox.Show(result.ErrorContent as string);
        }
    }
}
