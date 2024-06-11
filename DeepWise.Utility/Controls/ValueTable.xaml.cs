using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DeepWise.Controls
{
    /// <summary>
    /// Interaction logic for ValueTable.xaml
    /// </summary>
    public partial class ValueTable : UserControl
    {
        public ValueTable()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ValueTable),typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: null,
                //flags: FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: ItemsSouceChanged),
                validateValueCallback: IsSourceValid);

        static void ItemsSouceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = e.NewValue;
            Type elementType;
            var grid = (d as ValueTable).dataGrid;
            if (value is IList list)
            {
                elementType = value.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)).GetGenericArguments()[0];
                var wrapperType = GetWrapperType(elementType);
                IList newList = Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(wrapperType)) as IList;
                foreach (var item in list)
                    newList.Add(Activator.CreateInstance(wrapperType, item));
                grid.ItemsSource = newList;

            }
            else if (value is IDictionary dic)
            {
                elementType = value.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments()[1];
                var wrapperType = GetWrapperType(elementType);
                IList newList = Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(wrapperType)) as IList;
                foreach (dynamic item in dic)
                    newList.Add(Activator.CreateInstance(wrapperType, item.Key, item.Value));
                grid.ItemsSource = newList;
            }
            else
                grid.ItemsSource = null;
        }

        public void UpdateSource()
        {
            if (ItemsSource is IList list)
            {
                var elementType = ItemsSource.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)).GetGenericArguments()[0];
                if (ItemsSource is Array array)
                {
                    var newValues = dataGrid.ItemsSource.Cast<IWrapper>().ToArray();
                    if (array.Length != newValues.Length) throw new Exception("size is different to original array");
                    for (int i = 0; i < array.Length; i++)
                        array.SetValue(newValues[i].Cast(elementType), i);
                }
                else
                {
                    list.Clear();
                    foreach (IWrapper item in dataGrid.ItemsSource)
                        list.Add(item.Cast(elementType));
                }
            }
            else if (ItemsSource is IDictionary dic)
            {
                var elementType = ItemsSource.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments()[1];
                dic.Clear();
                foreach (IWrapper item in dataGrid.ItemsSource)
                    dic.Add(item.Name, item.Cast(elementType));
            }
        }

        public void UpdateTarget()
        {
            ItemsSouceChanged(this, new DependencyPropertyChangedEventArgs(ItemsSourceProperty, null, ItemsSource));
        }

        static bool IsSourceValid(object value)
        {
            if (value != null)
            {
                if (value is IList list)
                {
                    var listType = value.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
                    if (listType != null)
                        return IsElementTypeAvalible(listType.GetGenericArguments()[0]);
                    else
                        return false;
                }
                else if (value is IDictionary)
                {
                    var dicType = value.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                    if (dicType != null && dicType.GetGenericArguments()[0] == typeof(string))
                        return IsElementTypeAvalible(dicType.GetGenericArguments()[1]);
                    else
                        return false;
                }
            }
            return true;
        }

        public IEnumerable ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IEnumerable;
            set => SetValue(ItemsSourceProperty, value);
        }

        static bool IsElementTypeAvalible(Type type)
        {
            if (type.IsEnum) return true;
            switch (type.FullName)
            {
                case "System.Int32":
                case "System.Single":
                case "System.Double":
                case "System.String":
                case "System.Boolean":
                //[DeepWise]
                case "DeepWise.Shapes.Point":
                case "DeepWise.Shapes.Point3D":
                case "DeepWise.Shapes.Vector":
                case "DeepWise.Shapes.Vector3D":
                //[GDI]
                case "System.Drawing.Point":
                case "System.Drawing.PointF":
                //[WinBase]
                case "System.Windows.Point":
                case "System.Windows.Vector":
                //[OpenCv]
                case "OpenCvSharp.Point":
                case "OpenCvSharp.Point2d":
                case "OpenCvSharp.Point2f":
                    return true;
                default:
                    return false;
            }
            
        }

        static Type GetWrapperType(Type elementType)
        {
            switch (elementType.FullName)
            {
                //[Integer]
                case "System.Drawing.Point":
                case "OpenCvSharp.Point":
                    return typeof(Point2DWrapper<int>);

                //[Float]
                case "System.Drawing.PointF":
                case "OpenCvSharp.Point2f":
                    return typeof(Point2DWrapper<float>);
                //[Double]
                case "System.Windows.Point":
                case "DeepWise.Shapes.Point":
                case "OpenCvSharp.Point2d":
                case "System.Windows.Vector":
                case "DeepWise.Shapes.Vector":
                    return typeof(Point2DWrapper<double>);
                case "DeepWise.Shapes.Point3D":
                case "DeepWise.Shapes.Vector3D":
                    return typeof(Point3DWrapper<double>);
                default:
                    return typeof(SingleValueWrapper<>).MakeGenericType(elementType);
                    throw new NotImplementedException();
            }
        }

        public class SingleValueWrapper<T> : IWrapper
        {
            public SingleValueWrapper(string key,T value)
            {
                Name = key;
                Value = value;
            }
            public SingleValueWrapper(T value)
            {
                Value = value;
            }
            public SingleValueWrapper()
            {

            }
            public object Cast(Type type) => Convert.ChangeType(Value, type);
            public string Name { get; set; }
            public T Value { get; set; }
        }
        public class Point2DWrapper<T> : IWrapper 
        {
            public Point2DWrapper() { }
            public Point2DWrapper(dynamic p)
            {
                X = p.X;
                Y = p.Y;
            }
            public Point2DWrapper(string name, object p) : this(p)
            {
                Name = name;
            }
            public object Cast(Type type)
            {
                return Activator.CreateInstance(type, X, Y);
            }
            public string Name { get; set; }
            public T X { get; set; }
            public T Y { get; set; }
        }
        public class SizeWrapper<T> : IWrapper
        {
            public SizeWrapper() { }
            public SizeWrapper(dynamic p)
            {
                Width = p.Width;
                Height = p.Height;
            }
            public SizeWrapper(string name, object p) : this(p)
            {
                Name = name;
            }
            public object Cast(Type type)
            {
                return Activator.CreateInstance(type, Width, Height);
            }
            public string Name { get; set; }
            public T Width { get; set; }
            public T Height { get; set; }
        }
        public class Point3DWrapper<T> : IWrapper
        {
            public Point3DWrapper() { }
            public Point3DWrapper(dynamic p)
            {
                X = p.X;
                Y = p.Y;
                Z = p.Z;
            }
            public Point3DWrapper(string name, object p) : this(p)
            {
                Name = name;
            }
            public string Name { get; set; }
            public T X { get; set; }
            public T Y { get; set; }
            public T Z { get; set; }

            public object Cast(Type type)
            {
                return Activator.CreateInstance(type, X, Y, Z);
            }
        }

        public interface IWrapper
        {
            object Cast(Type type);
            string Name { get; }
        }

        private void dataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            dataGrid.Columns.OfType<DataGridTextColumn>().ToList().ForEach(column =>(column.Binding as System.Windows.Data.Binding).UpdateSourceTrigger = UpdateSourceTrigger.LostFocus);
            if (ItemsSource is IList )
            {
                dataGrid.Columns.Remove(dataGrid.Columns.FirstOrDefault(x => x.Header is string str && str == "Name"));
                
            }
            columnButton.Visibility = IsButtonVisible ? Visibility.Visible : Visibility.Collapsed;
            if (columnButton.Visibility == Visibility.Visible)
            {
                dataGrid.Columns.Remove(columnButton);
                dataGrid.Columns.Add(columnButton);
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is IWrapper wrapper)
            {
                Type elementType;
                if (ItemsSource is IList)
                    elementType = ItemsSource.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)).GetGenericArguments()[0];
                else if (ItemsSource is IDictionary)
                    elementType = ItemsSource.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)).GetGenericArguments()[1];
                else
                    throw new Exception();
                ButtonClicked?.Invoke(sender, new DataGridButtonClickedEventArgs(wrapper.Name, wrapper.Cast(elementType), elementType));
            }
        }

        public static readonly DependencyProperty IsButtonVisibleProperty = DependencyProperty.Register(nameof(IsButtonVisible), typeof(bool), typeof(ValueTable),new PropertyMetadata(ButtonVisibilityPropertyChanged));
        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(nameof(ButtonContent), typeof(object), typeof(ValueTable),new PropertyMetadata("Click"));
        static void ButtonVisibilityPropertyChanged(DependencyObject s,DependencyPropertyChangedEventArgs e)
        {
           (s as ValueTable).columnButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ButtonContent
        {
            get => (object)GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }
        public bool IsButtonVisible
        {
            get=>(bool)GetValue(IsButtonVisibleProperty);
            set => SetValue(IsButtonVisibleProperty, value);
        }
        public event DataGridButtonClickedEventHandller ButtonClicked;
    }
}
