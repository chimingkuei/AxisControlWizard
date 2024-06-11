using DeepWise.Data;
using DeepWise.Expression;
using DeepWise.Expression.Dynamic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DeepWise.Test
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            InitializeComponent();
            List<DemoWindowInfo> list = new List<DemoWindowInfo>();
            foreach (var item in this.GetType().Assembly.GetTypes().Where(x => x.Name.Contains("Demo") && typeof(Window).IsAssignableFrom(x)).OrderBy(x=>x.Name))
            {
                list.Add(new DemoWindowInfo(item));
            }
            list.Sort((x,y)=>string.CompareOrdinal(x.Name,y.Name) );
            ICollectionView view = CollectionViewSource.GetDefaultView(list);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            view.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            listBox.ItemsSource = view;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case nameof(btn_Demo):
                    (listBox.SelectedItem as DemoWindowInfo).ShowDemo();
                    break;
                case nameof(btn_CsCode):
                    {
                        var type = (listBox.SelectedItem as DemoWindowInfo).WindowType;
                        string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        path = System.IO.Path.GetDirectoryName(path);
                        path = System.IO.Path.GetDirectoryName(path);
                        path = System.IO.Path.GetDirectoryName(path);
                        path += @"\" + type.Name + ".xaml.cs";
                        System.Diagnostics.Process.Start(path);
                        break;
                    }
                case nameof(btn_Xaml):
                    {
                        var type = (listBox.SelectedItem as DemoWindowInfo).WindowType;
                        string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        path = System.IO.Path.GetDirectoryName(path);
                        path = System.IO.Path.GetDirectoryName(path);
                        path = System.IO.Path.GetDirectoryName(path);
                        path += @"\" + type.Name + ".xaml";
                        Process process = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo("devenv.exe", "/edit "+path);
                        process.StartInfo = startInfo;
                        process.Start();
                        break;
                    }
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(listBox.SelectedItem is DemoWindowInfo info)
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    var lbi = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    if (lbi == null) continue;
                    if (lbi.IsVisible && IsMouseOverTarget(lbi, e.GetPosition((IInputElement)lbi)))
                    {
                        info.ShowDemo();
                        return;
                    }
                }
            }

            bool IsMouseOverTarget(Visual target, Point point)
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(target);
                return bounds.Contains(point);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem is DemoWindowInfo info)
            {
                var atts = info.WindowType.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (atts.Length > 0)
                {
                    tbx_Description.Text = (atts[0] as DescriptionAttribute).Description;
                    //tbx_Description.Foreground = Brushes.Black;
                    tbx_Description.Foreground = new SolidColorBrush(Color.FromRgb(35,35,35));
                }
                else
                {
                    tbx_Description.Text = "no description";
                    tbx_Description.Foreground = Brushes.Gray;
                }
            }
            else
            {
                tbx_Description.Text = "no description";
                tbx_Description.Foreground = Brushes.Gray;
            }
        }
    }
    public class DemoWindowInfo
    {
        public Type WindowType { get; }
        public DemoWindowInfo(Type windowType)
        {
            WindowType = windowType;
            Name = WindowType.Name.ToString().Replace("Demo", "");
            if (windowType.TryGetCustomAttribute<GroupAttribute>(out var gAtt))
                Group = gAtt.GroupName;
            if (windowType.TryGetCustomAttribute<DisplayNameAttribute>(out var dnAtt))
                Name = dnAtt.DisplayName;
        }
        public string Name { get; set; }
        public string Group { get; set; } = "Default";
        public void ShowDemo()
        {
            var window = (Window)Activator.CreateInstance(WindowType);
            window.ShowDialog();
        }

        public override string ToString() => Name;
    }
    public class GroupAttribute : Attribute
    {
        public GroupAttribute(string name)
        {
            GroupName = name;
        }
        public string GroupName { get; }
    }
    public class StarAttribute : Attribute
    {

    }
}