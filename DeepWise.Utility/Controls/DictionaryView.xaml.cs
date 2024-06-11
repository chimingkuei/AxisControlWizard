using DeepWise.Shapes;
using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DeepWise;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using DeepWise.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeepWise.Localization;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;

namespace DeepWise.Controls
{
    /// <summary>
    /// DictionaryView.xaml 的互動邏輯
    /// </summary>
    [ContentProperty("ItemSources")]
    public partial class DictionaryView : UserControl
    {
        public DictionaryView()
        {
            InitializeComponent();
            DataContextChanged += DictionaryView_DataContextChanged;    
        }

        private void DictionaryView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IDictionary collection)
                ItemSources = collection;
            else
                ItemSources = null;
        }

        [Obsolete("\nUse \"ItemSource\" property to set the content\n\n Also if you want to enable the small button,\n Set \"IsSmallButtonVisiable\" to True and add handler to \"SmallButtonClicked\" event")]
        public DictionaryView(IDictionary dic, DataGridButtonClickedEventHandller moveButtonClicked = null) : this()
        {
            DataContext = dic;
            if(moveButtonClicked != null)
            {
                columnButton.Visibility = Visibility.Visible;
                this.SmallButtonClicked += moveButtonClicked;
            }
        }

        public void Update()
        {
            if(InnerSources!=null)
            {
                foreach(var item in InnerSources)
                {
                    item.Update();
                }
            }
        }

        ObservableCollection<DictionaryPointItem> InnerSources;
        IDictionary _itemSources;
        public IDictionary ItemSources
        {
            get => _itemSources;
            set
            {
                if (_itemSources != value)
                {
                    _itemSources = value;
                }

                if (value == null) return;
                var type = value.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                if (type != null)
                {
                    if (type.GetGenericArguments()[0] == typeof(string))
                    {
                        PointType = type.GetGenericArguments()[1];
                        var propertyNames = PointType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToArray();
                        var all = new string[] { "X", "Y", "Z", "A", "B", "C", "Width", "Height" };
                        var intersect = propertyNames.Intersect(all);
                        var exp = all.Where(x => !intersect.Contains(x));
                        foreach (var item in exp)
                        {
                            dataGrid.Columns.Remove(dataGrid.Columns.FirstOrDefault(x => x.Header is string str && str == item));
                        }
                        dataGrid.ItemsSource = InnerSources =  DeepWise.Data.DictionaryPointItem.GetList(value);
                    }
                    else
                        throw new Exception("SelectedDictionary's TKey must be type of string");
                }
                else
                    throw new Exception("SelectedDictionary's must be type of IDictionary<TKey,TString>");
            }
        }
        private Type PointType;

        public Func<object, string, bool> CreateObject { get; set; }
        public Func<object, string, bool> CopyObject { get; set; }
        public Func<object, string, bool> TryDeleteObject { get; set; } = (obj, name) => true;
        public Func<object, string, bool> TryRenameObject { get; set; } = (obj, newName) => true;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(btn_New):

                    var name = ItemSources.GetNewName("p");
                    ItemSources.Add(name, Activator.CreateInstance(PointType));
                    InnerSources.Add(new DictionaryPointItem(ItemSources, name));
                    break;
                case nameof(btn_Copy):
                    {
                        var item = dataGrid.SelectedItem as DictionaryPointItem;
                        var dlg = new NamingDialog(item.Name+"_copy", (_itemSources as IDictionary).Keys as ICollection<string>);
                        if (dlg.ShowDialog() == true)
                        {
                            var value = item.GetValue();
                    
                            ItemSources.Add(dlg.ResultName, value);
                            var copy = new DictionaryPointItem(ItemSources, dlg.ResultName);
                            InnerSources.Add(copy);
                            dataGrid.SelectedItem = copy;
                            dataGrid.ScrollIntoView(copy);
                        }
                        break;
                    }
                case nameof(btn_Delete):
                    {
                        var item = dataGrid.SelectedItem as DictionaryPointItem;
                        InnerSources.Remove(item);
                        break;
                    }
                case nameof(btn_Rename):
                    {
                        var item = dataGrid.SelectedItem as DictionaryPointItem;
                        var dlg = new NamingDialog(item.Name, (_itemSources as IDictionary).Keys as ICollection<string>);
                        if (dlg.ShowDialog() == true)
                        {
                            var value = item.GetValue();
                            var index = InnerSources.IndexOf(item);
                            InnerSources.Remove(item);
                            ItemSources.Add(dlg.ResultName, value);
                            InnerSources.Insert(index, new DictionaryPointItem(ItemSources, dlg.ResultName));
                        }
                        break;
                    }
            }
        }
        private void TextBox_KeyDown(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox tbx)
            {
                if (e is KeyEventArgs ke)
                {
                    if (ke.Key == Key.Enter)
                        tbx.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                }
                else
                    tbx.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            }
        }
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DeepWise.Data.DictionaryPointItem pItem)
            {
                SmallButtonClicked?.Invoke(sender, new DataGridButtonClickedEventArgs(pItem.Name, (ItemSources as IDictionary)[pItem.Name],pItem.ItemType));
            }
        }

        public bool IsSmallButtonVisiable
        {
            get => columnButton.Visibility == Visibility.Visible;
            set
            {
                columnButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public event DataGridButtonClickedEventHandller SmallButtonClicked;

    }

    public delegate void DataGridButtonClickedEventHandller(object sender, DataGridButtonClickedEventArgs e);
    public class DataGridButtonClickedEventArgs : EventArgs
    {
        public DataGridButtonClickedEventArgs(string key,object value,Type type)
        {
            Key = key;
            Value = value;
            ValueType = type;
        }
        public string Key { get; }
        public object Value { get; }
        public Type ValueType { get; }
    }

    public class DataGridDictionaryItem : Dictionary<string, object>
    {
        public DataGridDictionaryItem(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public static class DataGridDictionaryHelper
    {
        public static IEnumerable<DataGridDictionaryItem> GetDataGridItems(IDictionary source)
        {
            var valueType = source.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments()[1];
            foreach (var key in source.Keys.Cast<string>())
            {
                var dic = new DataGridDictionaryItem(key);
                var item = source[key];
                foreach (var property in item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var a = Activator.CreateInstance(typeof(DictionaryValueProperty<>).MakeGenericType(property.PropertyType), source, key, property.Name, valueType);
                    dic.Add(property.Name, a);
                }
                yield return dic;
            }
        }

        public static IEnumerable<DataGridColumn> GetDataGridCoulmns(Type type,bool includeName = true)
        {
            if(includeName)
            {
                yield return new DataGridTextColumn()
                {
                    Header = "Name",
                    Binding = new Binding("Name") { Mode = BindingMode.OneWay },
                };
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var column = new DataGridTextColumn()
                {
                    Header = property.GetDisplayName(),
                    Binding = new Binding($"[{property.Name}].Value") { Mode = property.CanWrite ? BindingMode.Default : BindingMode.OneWay},
                    
                };
                yield return column;
            }
        }
    }

    internal class DictionaryValueProperty<T>
    {
        IDictionary _dic;
        string _key;
        string propertyName;
        public DictionaryValueProperty(IDictionary dic,string key,string propertyName,Type parentType)
        {
            _dic = dic;
            _key = key;
            this.propertyName = propertyName;
            ParentType = parentType;
        }
        public Type ParentType { get; }
        public T Value
        {
            get
            {
                return (T)ParentType.GetProperty(propertyName).GetValue(_dic[_key]);
            }
            set
            {
                object newValue = _dic[_key];
                ParentType.GetProperty(propertyName).SetValue(newValue, value);
                _dic[_key] = newValue;
            }
        }

    }
}
