using MotionControllers.Motion;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionControllers.UI
{
    /// <summary>
    /// Interaction logic for MotionRuleEditor.xaml
    /// </summary>
    public partial class MotionRuleEditor : Window
    {
        public MotionRuleEditor()
        {
            InitializeComponent();
        }

        public MotionRuleEditor(ADLINK_Motion controller, IOPortInfo port) : this()
        {

        }
    }
}
