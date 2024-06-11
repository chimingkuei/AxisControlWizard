using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MessageBoxDemo.xaml
    /// </summary>
    public partial class CancelableMessageBoxDemo : Window
    {
        public CancelableMessageBoxDemo()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case nameof(btn_Show):

                    //The MessageBox under DeepWise namespace provides higher flexibility to users
                    var src = new CancellationTokenSource(3000);
                    MessageBox.ShowOK("This window will close in 3 seconds", "Caption", "OK", MessageBoxImage.Question, src.Token);
                    break;
            }
    
        }
    }
}