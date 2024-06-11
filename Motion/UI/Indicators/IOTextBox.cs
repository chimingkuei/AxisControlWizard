using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MotionControllers.UI
{
    public class IOTextBox : TextBox
    {
        public IOTextBox() : base()
        {
            DataContextChanged += IOTextBox_DataContextChanged;
        }

        private void IOTextBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ADLINK_Motion cntlr)
            {
                Binding binding = new Binding("Value")
                {
                    Source = PortInfo.Type.HasFlag(IOTypes.Analog) ? (object)new IOPort<double>(cntlr, PortInfo) : (object)new IOPort<bool>(cntlr, PortInfo),
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                };

                SetBinding(TextBox.TextProperty, binding);
                KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        BindingExpression binding = BindingOperations.GetBindingExpression((s as TextBox), TextBox.TextProperty);
                        if (binding != null) { binding.UpdateSource(); }
                    }
                };
                this.IsReadOnly = PortInfo.Type.HasFlag(IOTypes.Input);
            }
        }

        IOPortInfo _portInfo;
        public IOPortInfo PortInfo
        {
            get => _portInfo;
            set
            {
                if (_portInfo == value) return;
                _portInfo = value;
            }
        }
    }
}
