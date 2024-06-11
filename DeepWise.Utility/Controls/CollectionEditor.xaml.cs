using DeepWise.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeepWise.Controls
{

    /// <summary>
    /// Interaction logic for CollectionEditor.xaml
    /// </summary>
    public partial class CollectionEditor : Window
    {
        public CollectionEditor()
        {
            InitializeComponent();
            this.DataContextChanged += CollectionEditor_DataContextChanged;
        }

        private void CollectionEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is IDictionary dic)
            {
                var gDicType = dic.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                if(gDicType.GetGenericArguments()[0] == typeof(string))
                {
                    listView.ItemsSource = dic;
                    Type elementType = gDicType.GetGenericArguments()[1];

                    if (elementType != typeof(object))
                    {
                        if (elementType.IsValueType)
                            cbx_ItemType.ItemsSource = new Type[] { elementType };
                        else
                            cbx_ItemType.ItemsSource = AppDomain.CurrentDomain.GetAssemblies().SelectMany(TryGetTypes).Where(x => IsValidType(x, elementType)).ToList();
                    }
                }
                else
                {
                    MessageBox.Show("僅支援Key為字串類型的字典");
                    listView.ItemsSource = null;
                    cbx_ItemType.ItemsSource = null;
                }
            }
            else if (e.NewValue is ICollection collection)
            {
                Type elementType = collection.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)).GetGenericArguments()[0];

                if (elementType != typeof(object))
                {
                    if (elementType.IsValueType)
                        cbx_ItemType.ItemsSource = new Type[] { elementType };
                    else
                        cbx_ItemType.ItemsSource = AppDomain.CurrentDomain.GetAssemblies().SelectMany(TryGetTypes).Where(x=>IsValidType(x,elementType)).ToList();
                }

                if(collection is Array)
                    CanUserEditList = false;
                else if (collection is IList || collection is IDictionary)
                    CanUserEditList = true;
                else
                    CanUserEditList = false;

                listView.ItemsSource = collection;
            }
            else
            {
                listView.ItemsSource = null;
                cbx_ItemType.ItemsSource = null;
            }

            if (cbx_ItemType.Items.Count > 0 && CanUserEditList) 
                cbx_ItemType.SelectedIndex = 0;
            else
                cbx_ItemType.SelectedIndex = -1;


            Type[] TryGetTypes(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return new Type[0];
                }
            }
            bool IsValidType(Type y, Type elementType)
            {
                try
                {
                    return y.IsPublic && !y.IsAbstract && !y.IsGenericType && elementType.IsAssignableFrom(y);
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool CanUserEditList
        {
            get => (bool)GetValue(CanUserEditListProperty);
            set => SetValue(CanUserEditListProperty, value);
        }

        static void OnCanUserEditListChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
                (sender as CollectionEditor).cbx_ItemType.SelectedIndex = -1;
        }
        public static readonly DependencyProperty CanUserEditListProperty = DependencyProperty.Register(nameof(CanUserEditList), typeof(bool), typeof(CollectionEditor),new PropertyMetadata(OnCanUserEditListChanged));

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if(e.Source is ListView listview)
            {
                try
                {
                    if (listview.SelectedItem != null)
                    {
                        var type = listview.SelectedItem.GetType();
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) && type.GetGenericArguments()[0] == typeof(string))
                        {
                            propertyGrid.SelectedObject = type.GetProperty("Value").GetValue(listview.SelectedItem);
                        }
                        else
                            propertyGrid.SelectedObject = listview.SelectedItem;
                    }
                    else
                        listview.SelectedItem = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    propertyGrid.SelectedObject = null;
                }

                if (listview.SelectedIndex == -1)
                {
                    btn_Delete.IsEnabled = false;
                    btn_MoveDown.IsEnabled = false;
                    btn_MoveUp.IsEnabled = false;
                }
                else
                {
                    btn_Delete.IsEnabled = true;
                    btn_MoveDown.IsEnabled = listview.SelectedIndex < listview.Items.Count - 1;
                    btn_MoveUp.IsEnabled = listview.SelectedIndex > 0;
                }
            }
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is IList list)
            {
                switch ((sender as Button).Name)
                {
                    case nameof(btn_Delete):
                        {
                            list.RemoveAt(listView.SelectedIndex);
                        }
                        break;
                    case nameof(btn_Add):
                        {
                            var type = cbx_ItemType.SelectedItem as Type;
                            list.Add(Activator.CreateInstance(type));
                            listView.SelectedIndex = listView.Items.Count - 1;
                            listView.Focus();
                            break;
                        }
                    case nameof(btn_MoveDown):
                        {
                            int index = listView.SelectedIndex;
                            if (index < list.Count - 1)
                            {
                                var item = listView.SelectedItem;
                                list.RemoveAt(index);
                                list.Insert(++index, item);
                                listView.SelectedIndex = index;
                                listView.Focus();
                            }
                        }
                        break;
                    case nameof(btn_MoveUp):
                        {
                            int index = listView.SelectedIndex;
                            if (index > 0)
                            {
                                var item = listView.SelectedItem;
                                list.RemoveAt(index);
                                list.Insert(--index, item);
                                listView.SelectedIndex = index;
                                listView.Focus();
                            }
                        }
                        break;
                }
                ICollectionView view = CollectionViewSource.GetDefaultView(list);
                view.Refresh();
            }
        }
    }

    public class CollectionViewItemNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;
            else if (value is string) return value as string;
            else if(value.GetType().IsGenericType)
            {
                var gType = value.GetType();
                if (gType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) && gType.GetGenericArguments()[0] == typeof(string))
                {
                    return gType.GetProperty("Key").GetValue(value) as string;
                }
                else
                    throw new TypeAccessException();
            }
            if (targetType == typeof(string))
            {
                var valueType = value.GetType();
                var nameProperty = valueType.GetProperty("Name");
                if (nameProperty != null && nameProperty.PropertyType == typeof(string))
                {
                    var name = nameProperty.GetValue(value) as string;
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
                return valueType.GetDisplayName();
            }
            else
                throw new TypeAccessException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
