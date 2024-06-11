using DeepWise.AccessControls;
using DeepWise.Controls;
using DeepWise.Data;
using DeepWise.Localization;
using DeepWise.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeepWise.Test
{
    [Group("系統"),DisplayName("AccessLevel(權限控制)")]
    [Description("此範例展示如何建立使用者，並在控制項上進行權限管理。")]
    public partial class AccessLevelDemo : Window
    {
        public AccessLevelDemo()
        {
            InitializeComponent();
            var ac = AccessController.Default;
            if (ac.Users.Count == 0)
            {
                ac.Users.Add(new User() { Name = "Engineer", Passwords = "0000",       AccessLevel = AccessLevel.Engineer });
                ac.Users.Add(new User() { Name = "Admin", Passwords = "0000",     AccessLevel = AccessLevel.Administrator });
                ac.Users.Add(new User() { Name = "Op", Passwords = "0000", AccessLevel = AccessLevel.Operator });
            }
            dataGrid.ItemsSource = ac.Users;
            propertyGrid.SelectedObject = new LogInDemoItem();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            AccessController.Default.SaveConfig();
        }

        private void TextBlock_MouseDown(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(btn_LogIn):
                    {
                        AccessController.Default.LogIn();
                        break;
                    }
                case nameof(btn_User):
                    {
                        var menu = new ContextMenu();
                        var item = new MenuItem() { Header = "登出" };
                        item.Click += (s2, e2) =>
                        {
                            if (MessageBox.Show("確定要登出嗎？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                DeepWise.AccessController.Default.CurrentUser = null;
                        };
                        menu.Items.Add(item);

                        item = new MenuItem() { Header = "管理" };

                        menu.Items.Add(item);

                        item = new MenuItem() { Header = "自動登出", StaysOpenOnClick = true, IsCheckable = true };

                        item.SetBinding(MenuItem.IsCheckedProperty, new Binding("AutoLogOut") { Source = AccessController.Default });
                        menu.Items.Add(item);

                        menu.IsOpen = true;
                    }
                    break;
            }
        }
    }

    public class NewItemConverter : IValueConverter
    {
        static SolidColorBrush ActiveColor => new SolidColorBrush(Color.FromArgb((byte)(255 * 0.3), 255, 165, 50));
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b =(bool)value;
            return b ? ActiveColor : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LogInDemoItem
    {
        public string Text { get; set; }
        [AccessLevel(AccessLevel.Operator)]
        public string NormalText { get; set; } 
        [AccessLevel(AccessLevel.Administrator)]
        public string AdministratorText { get; set; }
        [AccessLevel(AccessLevel.Engineer)]
        public string EngineerText { get; set; }
        [AccessLevel(AccessLevel.Engineer, DisableEffect.Hide)]
        public string EngineerText2 { get; set; }
    }

}
