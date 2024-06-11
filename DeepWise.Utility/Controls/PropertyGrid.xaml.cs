using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using DeepWise.AccessControls;
using DeepWise.Localization;

namespace DeepWise.Controls
{
    /// <summary>
    /// PropertyGrid.xaml 的互動邏輯
    /// <para>支援的Attributes : </para>
    /// <br>1.<see cref="LocalizedDisplayNameAttribute"/></br>
    /// <br>2.<see cref="DisplayNameAttribute"/></br>
    /// <br>3.<see cref="BrowsableAttribute"/></br>
    /// <br>4.<see cref="ButtonAttribute"/></br>
    /// <br>5.<see cref="SliderAttribute"/></br>
    /// <br>6.<see cref="DefaultValueAttribute"/></br>
    /// <br>7.<see cref="RangeAttribute"/></br>
    /// <br>8.<see cref="OrderAttribute"/></br>
    /// <br>9.<see cref="DecimalPlacesAttribute"/></br>
    /// <br>10.<see cref="RadioButtonsAttribute"/></br>
    /// <br>11.<see cref="ConditionalAttribute"/></br>
    /// <br>12.<see cref="DescriptionAttribute"/></br>
    /// <para>保留的Attributes : </para>
    /// <br><see cref="UpDownButtonAttribute"/></br>
    /// </summary>

    [ContentProperty("SelectedObject")]
    public partial class PropertyGrid : UserControl
    {
        static PropertyGrid()
        {
            //Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"Controls\generic.xaml") });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/dw_base;component/Controls/generic.xaml") });
        }

        public static void Show(object target, string name = "")
        {
            new Window()
            {
                Content = new PropertyGrid()
                {
                    SelectedObject = target
                },
                MinWidth = 250,
                Title = string.IsNullOrEmpty(name) ? target.ToString() : name,
            }.Show();
        }
        public static void ShowDialog(object target, string name = "")
        {
            new Window()
            {
                Content = new PropertyGrid()
                {
                    SelectedObject = target
                },
                MinWidth = 250,
                Title = string.IsNullOrEmpty(name) ? target.ToString() : name,
            }.ShowDialog();
        }
        public PropertyGrid()
        {
            InitializeComponent();
            Loaded += PropertyGrid_Loaded;
        }

        private void PropertyGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                InitializeMembers();
        }

        public static PropertyGrid GetOwnerPropertyGrid(object obj)
        {
            var matchs = Dic.Where(x => x.Value == obj);
            if (matchs.Count() == 1)
            {
                return matchs.First().Key;
            }
            else
                throw new Exception();
        }

        public static readonly DependencyProperty DeclaredOnlyProperty = DependencyProperty.Register(nameof(DeclaredOnly), typeof(bool), typeof(PropertyGrid));
        public bool DeclaredOnly
        {
            get => (bool)GetValue(DeclaredOnlyProperty);
            set => SetValue(DeclaredOnlyProperty, value);
        }
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(GridLength), typeof(PropertyGrid), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));
        public GridLength LabelWidth
        {
            get => (GridLength)GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }
        public static readonly DependencyProperty FieldWidthProperty = DependencyProperty.Register(nameof(FieldWidth), typeof(GridLength), typeof(PropertyGrid), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));
        public GridLength FieldWidth
        {
            get => (GridLength)GetValue(FieldWidthProperty);
            set => SetValue(FieldWidthProperty, value);
        }

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(PropertyGrid), new PropertyMetadata(new Thickness(10, 5, 10, 5)));

        public static PropertyGrid GetCurrentPropertyGrid(object uiElement)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(uiElement as DependencyObject);
            while (!(parent is PropertyGrid))
            {
                parent = VisualTreeHelper.GetParent(parent as DependencyObject);
            }
            return parent as PropertyGrid;
        }

        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }

        public static readonly DependencyProperty DefualtDecimalPlacesProperty = DependencyProperty.Register(nameof(DefualtDecimalPlaces), typeof(object), typeof(PropertyGrid), new PropertyMetadata(4));
        public int DefualtDecimalPlaces
        {
            get => (int)GetValue(DefualtDecimalPlacesProperty);
            set => SetValue(DefualtDecimalPlacesProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(OnSelectedObjectChanged), SelectedObjectValidCallback);
        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public static Dictionary<PropertyGrid, object> Dic { get; } = new Dictionary<PropertyGrid, object>();

        protected static void OnSelectedObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SelectedObjectProperty)
            {
                if (sender is PropertyGrid pg)
                {
                    pg.InitializeMembers();
                    Dic[pg] = e.NewValue;
                }
            }
        }

        private static bool SelectedObjectValidCallback(object value) => !(value != null && value is ValueType);

        #region Refersh
        public void Reset() => InitializeMembers();
        /// <summary>
        /// 手動更新所有欄位的數值
        /// </summary>
        public void Update()
        {
            //throw new NotImplementedException();
            //foreach (var ctrl in Grid.Children.Cast<FrameworkElement>().Where(x => x.Name != null))
            //{
            //    UpdateBinding(ctrl);
            //}
        }
        /// <summary>
        /// 手動更新特定欄位的數值
        /// </summary>
        public void Update(string propertyName)
        {
            //throw new NotImplementedException();
            //foreach (var ctrl in Grid.Children.Cast<FrameworkElement>().Where(x => x.Name == propertyName))
            //{
            //    UpdateBinding(ctrl);
            //}
        }
        #endregion

        void InitializeMembers()
        {
            //if(!DesignerProperties.GetIsInDesignMode(this))
            {
                var list = new List<PropertyFieldControl>();

                if (SelectedObject != null)
                {
                    IEnumerable<MemberInfo> members;
                    if (DeclaredOnly)
                        members = SelectedObject.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(IsTargetMemberAvalible).OrderBy(GetOrder);
                    else
                        members = SelectedObject.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(IsTargetMemberAvalible).OrderBy(GetOrder);
                    foreach (var info in members)
                    {
                        try
                        {
                            var ctrl = new PropertyFieldControl(info, SelectedObject);

                            ctrl.ColumnDefinitions[0].SetBinding(ColumnDefinition.WidthProperty, new Binding("LabelWidth") { Source = this, Mode = BindingMode.OneWay });
                            ctrl.ColumnDefinitions[1].SetBinding(ColumnDefinition.WidthProperty, new Binding("FieldWidth") { Source = this, Mode = BindingMode.OneWay });
                            ctrl.SetBinding(PropertyFieldControl.MarginProperty, new Binding("ItemMargin") { Source = this, Mode = BindingMode.OneWay });
                            #region Other Effects
                            if (info.TryGetCustomAttribute<ConditionalAttribute>(out var conditionalAttribute))
                            {
                                if (conditionalAttribute.Effect == DisableEffect.Disable)
                                    ctrl.SetBinding(FrameworkElement.IsEnabledProperty, new Binding(conditionalAttribute.TargetPropertyName) { Source = SelectedObject, Converter = new ObjectEqualsConverter(), ConverterParameter = conditionalAttribute.Value });
                                else
                                    ctrl.SetBinding(FrameworkElement.VisibilityProperty, new Binding(conditionalAttribute.TargetPropertyName) { Source = SelectedObject, Converter = new ObjectEqualsConverter(), ConverterParameter = conditionalAttribute.Value });
                            }
                            else if (info.TryGetCustomAttribute<AccessLevelAttribute>(out var accessLevelAttribute))
                            {
                                if (!DesignerProperties.GetIsInDesignMode(this))
                                {
                                    ctrl.SetBinding(accessLevelAttribute.Effect == DisableEffect.Disable ? FrameworkElement.IsEnabledProperty : FrameworkElement.VisibilityProperty,
                                      new Binding(nameof(AccessController.CurrentUser))
                                      {
                                          Source = AccessController.Default,
                                          Converter = AccessLevelToBooleanValueConverter.Instance,
                                          ConverterParameter = (accessLevelAttribute.Level),
                                      });
                                }
                            }
                            #endregion
                            //this.listBox.Items.Add(ctrl);
                            list.Add(ctrl);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                if (list.Any(x => x.GroupName != "default"))
                {

                    ICollectionView view = CollectionViewSource.GetDefaultView(list);
                    view.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
                    //view.SortDescriptions.Add(new SortDescription("GroupName", ListSortDirection.Ascending));
                    listBox.ItemsSource = view;
                }
                else
                    listBox.ItemsSource = list;
            }


        }

        #region Helper
        bool IsTargetMemberAvalible(MemberInfo info)
        {
            if (!info.TryGetCustomAttribute<BrowsableAttribute>(out var atr) || atr.Browsable)
            {
                if (info is PropertyInfo pinfo)
                {
                    return true;
                }
                else if (info is MethodInfo minfo)
                    return minfo.TryGetCustomAttribute<ButtonAttribute>(out _) || minfo.TryGetCustomAttribute<ToolButtonsAttribute>(out _);
                else
                    return false;
            }
            else
                return false;
        }

        static int GetOrder(MemberInfo info)
        {
            if (info.TryGetCustomAttribute<OrderAttribute>(out var atr))
                return atr.Order;
            else
                return int.MaxValue;
        }
        #endregion

    }

    public class PropertyFieldControl : Grid
    {
        public List<Point> Ps { get; set; } = new List<Point>();

        static void ShowPointTable(object sender, RoutedEventArgs e)
        {
            var propertyName = (sender as Button).Tag as string;
            var src = (sender as Button).DataContext;
            var pInfo = src.GetType().GetProperty(propertyName);
            var btnAttri = pInfo.GetCustomAttribute<ButtonAttribute>();
            var wnd = new Window()
            {
                Title = pInfo.GetDisplayName(),
                FontSize = 18,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(e.Source as FrameworkElement),
                SizeToContent = SizeToContent.WidthAndHeight,
                DataContext = pInfo.GetValue(src)
            };

            var table = new DictionaryView()
            {
                Width = 300,
                Height = double.NaN,
                MinHeight = 300,
            };
            if (btnAttri != null)
            {

                table.IsSmallButtonVisiable = true;
                var methodInfo = src.GetType().GetMethod(btnAttri.CallbackName, BindingFlags.Public| BindingFlags.NonPublic| BindingFlags.Instance| BindingFlags.Static);
                var d = (DataGridButtonClickedEventHandller)methodInfo.CreateDelegate(typeof(DataGridButtonClickedEventHandller), src);
                table.SmallButtonClicked += d;
            }
            wnd.Content = table;
            wnd.ShowDialog();
        }
        static void ShowCustomEditor(object sender,RoutedEventArgs e)
        {
            var btn = sender as Button;
            var pName = btn.Tag as string;
            var src = btn.DataContext;
            var property = src.GetType().GetProperty(pName);
            var windowType = property.GetCustomAttribute<CustomEditorAttribute>().EditorType;

            //if there is a contructor for the instance than create 
            Window window;
            if(windowType.GetConstructors().Any(x =>
            {
                var _params = x.GetParameters();
                return _params.Length == 1 && _params[0].ParameterType.IsAssignableFrom(property.PropertyType);
            }))
            {
                try
                {

                    window = Activator.CreateInstance(windowType, property.GetValue(src)) as Window;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

            }
            else
                window = Activator.CreateInstance(windowType) as Window;

            window.Name = property.Name;
            window.DataContext = src;
            window.ShowDialog();
        } 

        static void ShowCollectionEditor(object sender,RoutedEventArgs e)
        {
   
            var btn = sender as Button;
            var pName = btn.Tag as string;
            var src = btn.DataContext;
            var pInfo = src.GetType().GetProperty(pName);

            var collection = pInfo.GetValue(src);
            if (collection is List<string> stringList)
            {
                var wind = new Window()
                {
                    Title = pInfo.GetDisplayName(),
                };
                var dataGrid = new DataGrid()
                {
                    CanUserAddRows = true,
                    CanUserDeleteRows = true,
                };
                wind.Content = dataGrid;

                dataGrid.ItemsSource = stringList.Select(x=>new StringWrapper(x)).ToList();

                wind.Closed += (s2, e2) =>
                {
                    stringList.Clear();
                    stringList.AddRange((dataGrid.ItemsSource as List<StringWrapper>).Select(x => x.Text).ToArray());
                };
                wind.ShowDialog();
            }
            else if (collection is ICollection)
            {
                var wind = new CollectionEditor()
                {
                    Title = pInfo.GetDisplayName(),
                    DataContext = collection
                };
                if(pInfo.TryGetCustomAttribute<EditableAttribute>(out var editableAttribute))
                {
                    wind.CanUserEditList = editableAttribute.AllowEdit;
                }
                wind.ShowDialog();
            }
            else
            {
                MessageBox.Show($"集合{collection.GetType().Name}方法尚未實作");
            }

        }

        static void SetPath(object sender, RoutedEventArgs e)
        {
            var propertyName = (sender as Button).Tag as string;
            var src = (sender as Button).DataContext;
            var pInfo = src.GetType().GetProperty(propertyName);

            if (pInfo.GetCustomAttribute<PathAttribute>() is PathAttribute pathAtr)
            {
                switch (pathAtr.Mode)
                {
                    case PathMode.Directory:
                        using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
                        {
                            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
                                pInfo.SetValue(src, dlg.SelectedPath);
                            break;
                        }
                    case PathMode.SaveFile:
                        using (var dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.FileName))
                                pInfo.SetValue(src, dlg.FileName);
                            break;
                        }
                    case PathMode.SelectFile:
                        using (var dlg = new System.Windows.Forms.OpenFileDialog())
                        {
                            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.FileName))
                                pInfo.SetValue(src, dlg.FileName);
                            break;
                        }
                }
            }

            var tbx = ((sender as Button).Parent as Grid).Children.OfType<TextBox>().FirstOrDefault();
            BindingOperations.GetBindingExpression(tbx, TextBox.TextProperty).UpdateTarget();
        }

        static void ResetToDefault(object sender, EventArgs e)
        {
            var propertName = (sender as Button).Tag as string;
            var obj = (sender as Button).DataContext;
            var pInfo = obj.GetType().GetProperty(propertName);

            pInfo.SetValue(obj, pInfo.GetCustomAttribute<DefaultValueAttribute>().Value);
        }
        static void ShowPropertyGrid(object sender, RoutedEventArgs e)
        {
            var _btn = sender as Button;
            var tag = ((object, PropertyInfo))(_btn.Tag);
            var win = new Window()
            {
                Title = tag.Item2.GetDisplayName(),
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 400,
                MinHeight = 400,
                FontSize = 20,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            var collection = tag.Item2.GetValue(tag.Item1);
            win.Content = new PropertyGrid()
            {
                SelectedObject = collection
            };
            win.ShowDialog();
        }

        static bool IsPointTable(Type type)
        {
            return (type.GetInterfaces().Any(x =>
             x.IsGenericType &&
             x.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
             x.GenericTypeArguments[0] == typeof(string) 
             && IsAvalibleType(x.GenericTypeArguments[1])));

            bool IsAvalibleType(Type _type)
            {
                return
                _type == typeof(System.Drawing.Point) ||
                _type == typeof(System.Drawing.Size) ||
                _type == typeof(System.Drawing.PointF) ||
                _type == typeof(System.Drawing.SizeF) ||
                _type == typeof(System.Drawing.Rectangle) ||
                _type == typeof(System.Drawing.RectangleF) ||
                _type == typeof(System.Windows.Point) ||
                _type == typeof(System.Windows.Size) ||
                _type == typeof(System.Windows.Rect) ||
                _type == typeof(System.Windows.Vector) ||
                _type == typeof(DeepWise.Shapes.Point) ||
                _type == typeof(DeepWise.Shapes.Point3D) ||
                _type == typeof(DeepWise.Shapes.Vector);
            }
        }

        static bool IsCollection(Type type)
        {
            if (type.IsArray)
                return true;
            if (typeof(IList).IsAssignableFrom(type))
                return true;
            if (type.GetInterfaces().FirstOrDefault(x =>
            x.IsGenericType &&
            x.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
            x.GenericTypeArguments[0] == typeof(string)) != null)
                return true;

            return false;
        }

        void SetScaleBindingToFontSize(UIElement target, double scaleParameter = 1.0 / 12)
        {
            var binding = new Binding("FontSize")
            {
                Source = this,
                Converter = ScaleConverter.Instance,
                ConverterParameter = scaleParameter,
            };
            var scaleTransform = new ScaleTransform();
            target.RenderTransform = scaleTransform;
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, binding);
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, binding);
        }

        public string GroupName { get; }
        public PropertyFieldControl(MemberInfo memberInfo, object target)
        {

            this.Name = memberInfo.Name;
            DataContext = target;
            
    
                InitializeComponent(memberInfo);
            if (memberInfo.TryGetCustomAttribute<DisplayAttribute>(out var att))
                GroupName = att.GroupName;
            else
                GroupName = "default";
        }

        void InitializeComponent(MemberInfo memberInfo)
        {
            ColumnDefinitions.Add(new ColumnDefinition() { Name = "LabelWidth", Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition() { Name = "FieldWidth", Width = new GridLength(1, GridUnitType.Star) });
            //ColumnDefinitions.Add(new ColumnDefinition() );
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            InitializeLabel(memberInfo);
            if (memberInfo.TryGetCustomAttribute<CustomEditorAttribute>(out var customEditorAttribute))
            {
                if (typeof(Window).IsAssignableFrom(customEditorAttribute.EditorType))
                {
                    var btn = new Button()
                    {
                        Content = " ... ",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = memberInfo.Name,
                    };
                    btn.Click += ShowCustomEditor;
                    Grid.SetColumn(btn, 1);
                    Children.Add(btn);
                }
                else
                {
                    var property = (memberInfo as PropertyInfo);
                    var windowType = customEditorAttribute.EditorType;

                    //if there is a contructor for the instance than create 
                    FrameworkElement customCtrl;
                    if (windowType.GetConstructors().Any(x =>
                    {
                        var _params = x.GetParameters();
                        return _params.Length == 1 && _params[0].ParameterType.IsAssignableFrom(property.PropertyType);
                    }))
                    {
                            customCtrl = Activator.CreateInstance(windowType, property.GetValue(DataContext)) as FrameworkElement;
              
                    }
                    else
                        customCtrl = Activator.CreateInstance(windowType) as FrameworkElement;

                    customCtrl.Name = property.Name;
                    if(!string.IsNullOrWhiteSpace(customEditorAttribute.ContentProperty))
                    {
                        customCtrl.GetType().GetProperty(customEditorAttribute.ContentProperty).SetValue(customCtrl, property.GetValue(DataContext));
                    }

                    if(customEditorAttribute.IsSpanned)
                    {
                        this.RowDefinitions.Add(new RowDefinition());
                        Grid.SetColumnSpan(customCtrl, 2);
                        Grid.SetRow(customCtrl, 1);

                    }
                    else
                    {
                        Grid.SetColumn(customCtrl, 1);
                    }
                    Children.Add(customCtrl);
                }
            }
            else
                InitializeField(memberInfo);
        }
        void InitializeLabel(MemberInfo member)
        {
            var label = new TextBlock() { Name = member.Name, VerticalAlignment = VerticalAlignment.Center };
            if (member.TryGetCustomAttribute<MarkAttribute>(out var markAtt))
            {
                Run run = new Run(member.GetDisplayName());
                label.Inlines.Add(run);

                switch (markAtt.Mark)
                {
                    case Mark.Question:
                        run = new Run(" ❓");
                        run.Foreground = Brushes.Orange;
                        label.Inlines.Add(run);
                        break;
                    case Mark.Exclamation:
                        run = new Run(" ❗");
                        run.Foreground = Brushes.Blue;
                        label.Inlines.Add(run);
                        break;
                    case Mark.Warring:
                        run = new Run(" ⚠️");
                        run.Foreground = Brushes.Red;
                        label.Inlines.Add(run);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
                label.Text = member.GetDisplayName();

            if (member.TryGetCustomAttribute<DescriptionAttribute>(out var descriptionAttribute))
                label.ToolTip = descriptionAttribute.Description;
            else if (member.TryGetCustomAttribute<DisplayAttribute>(out var dpAtt) && !string.IsNullOrWhiteSpace(dpAtt.Description))
                label.ToolTip = dpAtt.Description;

            this.Children.Add(label);
        }
        void InitializeField(MemberInfo member)
        {
            #region Initailize Field
            if (member is PropertyInfo property)
            {
                if (property.TryGetCustomAttribute<CustomEditorAttribute>(out var cAtt) || property.PropertyType.TryGetCustomAttribute<CustomEditorAttribute>(out cAtt))
                {
                    //Content button
                    var btn = new Button()
                    {
                        Content = " ... ",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = (property.GetValue(DataContext), cAtt.EditorType),
                    };

                    btn.Click += (s, e) =>
                    {
                        var _btn = s as Button;
                        var args = ((object, Type))_btn.Tag;
                        var win = Activator.CreateInstance(args.Item2, args.Item1, null) as Window;
                        win.ShowDialog();
                    };
                    Grid.SetColumn(btn, 1);
                    Children.Add(btn);
                }
                else
                {
                    
                    switch (property.PropertyType.Name)
                    {
                        #region Numeric
                        case "Single":
                        case "Double":
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "UInt16":
                        case "UInt32":
                        case "UInt64":
                        case "Point":
                            {
                                var tbx = CreateTextBox(property);

                                List<FrameworkElement> list = new List<FrameworkElement>();
                                if (property.TryGetCustomAttribute<UnitAttribute>(out var unitAttribute))
                                {
                                    var tbkUnit = new TextBlock()
                                    {
                                        Text = unitAttribute.Unit,
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    list.Add(tbkUnit);
                                }
                                if (property.TryGetCustomAttribute<ButtonAttribute>(out var btnAtt))
                                {
                                    var btn = new Button()
                                    {
                                        Content = btnAtt.Text,
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    btn.SetBinding(Button.HeightProperty, new Binding("ActualHeight") { Source = tbx, Mode = BindingMode.OneWay });
                                    var methodInfo = DataContext.GetType().GetMethod(btnAtt.CallbackName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    var paramsInfo = methodInfo.GetParameters();

                                    if (paramsInfo.Length == 0)
                                    {
                                        var action = (Action)methodInfo.CreateDelegate(typeof(Action), DataContext);
                                        btn.Click += (s, e) => action();
                                    }
                                    else if (paramsInfo.Length == 2 && paramsInfo[0].ParameterType == typeof(object) && typeof(EventArgs).IsAssignableFrom(paramsInfo[1].ParameterType))
                                    {
                                        var _delegate = methodInfo.CreateDelegate(typeof(RoutedEventHandler), DataContext);
                                        btn.Click += (s, e) => _delegate.DynamicInvoke(s, e);
                                    }
                                    else
                                    {
                                        throw new Exception($"{btnAtt.CallbackName}的引數不符合規定");
                                    }

                                    list.Add(btn);
                                }
                                if (property.TryGetCustomAttribute<DefaultValueAttribute>(out var defaultAtt))
                                {
                                    var btn = new Button()
                                    {
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Name = property.Name,
                                        ToolTip = "Reset",
                                        Style = Application.Current.Resources["TransBtn"] as Style,
                                        Tag = property.Name,
                                        Content = new Image()
                                        {
                                            Stretch = Stretch.None,
                                            Source = new BitmapImage(new Uri(@"pack://application:,,,/dw_base;component/Resources/icon16_cross.png"))
                                        },
                                    };
                                    btn.SetBinding(Button.IsEnabledProperty, new Binding(Name) { Mode = BindingMode.OneWay, Converter = ObjectIsNotEmptyToBooleanConverter.Instance });
                                    btn.Click += ResetToDefault;

                                    list.Add(btn);
                                }
                                SetLayout(tbx, list.ToArray());

                                if (property.TryGetCustomAttribute<SliderAttribute>(out var sliderAtr))
                                {
                                    RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
                                    Slider slider = new Slider()
                                    {
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Margin = new Thickness(5),
                                        Name = property.Name
                                    };

                                    //var track = slider.Template.FindName("PART_Track", slider) as Track;

                                    //Binding
                                    if (sliderAtr.MaxValue is double || sliderAtr.MaxValue is int)
                                    {
                                        slider.Maximum = Convert.ToDouble(sliderAtr.MaxValue);
                                        slider.Minimum = Convert.ToDouble(sliderAtr.MinValue);
                                        var binding = tbx.GetBindingExpression(TextBox.TextProperty).ParentBinding;
                                        binding.ValidationRules.Add(new RangeRule(slider.Minimum, slider.Maximum));
                                    }
                                    else if (sliderAtr.MaxValue is string)
                                    {
                                        slider.SetBinding(Slider.MaximumProperty, new Binding(sliderAtr.MaxValue as string) { Source = DataContext, });
                                        slider.SetBinding(Slider.MinimumProperty, new Binding(sliderAtr.MinValue as string) { Source = DataContext, });
                                        var binding = tbx.GetBindingExpression(TextBox.TextProperty).ParentBinding;
                                        binding.ValidationRules.Add(new RangeRule(DataContext, (string)sliderAtr.MinValue, (string)sliderAtr.MaxValue));
                                    }
                                    else
                                    {
                                        if (property.GetCustomAttribute<RangeAttribute>() is RangeAttribute range)
                                        {
                                            slider.Maximum = Convert.ToDouble(range.Maximum);
                                            slider.Minimum = Convert.ToDouble(range.Minimum);
                                            var binding = tbx.GetBindingExpression(TextBox.TextProperty).ParentBinding;
                                            binding.ValidationRules.Add(new RangeRule(slider.Minimum, slider.Maximum));
                                        }
                                        else
                                            throw new Exception("SliderAttribute必須搭配引述或者RangAttribute使用");
                                    }
                                    slider.SetBinding(Slider.ValueProperty, new Binding(property.Name) { Source = DataContext, });

                                    Grid.SetRow(slider, 1);
                                    Grid.SetColumn(slider, 0);
                                    Grid.SetColumnSpan(slider, 2);
                                    Children.Add(slider);
                                }
                                else if (property.TryGetCustomAttribute<RangeAttribute>(out var range))
                                {
                                    var binding = tbx.GetBindingExpression(TextBox.TextProperty).ParentBinding;
                                    if (range.Minimum is double || range.Minimum is int)
                                        binding.ValidationRules.Add(new RangeRule(Convert.ToDouble(range.Minimum), Convert.ToDouble(range.Maximum)));
                                }
                            }
                            break;
                        #endregion
                        case "Boolean":
                            {
                                var chk = new CheckBox()
                                {
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Name = property.Name
                                };

                                chk.SetBinding(CheckBox.IsCheckedProperty, new Binding(property.Name)
                                {
                                    Source = DataContext,
                                    Mode = (property.CanWrite && property.SetMethod.IsPublic) ? BindingMode.Default : BindingMode.OneWay,
                                });

                                List<FrameworkElement> list = new List<FrameworkElement>();
                                if (property.TryGetCustomAttribute<UnitAttribute>(out var unitAttribute))
                                {
                                    var tbkUnit = new TextBlock()
                                    {
                                        Text = unitAttribute.Unit,
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    list.Add(tbkUnit);
                                }
                                if (property.TryGetCustomAttribute<ButtonAttribute>(out var btnAtt))
                                {
                                    var btn = new Button()
                                    {
                                        Content = btnAtt.Text,
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };
                                    btn.SetBinding(Button.HeightProperty, new Binding("ActualHeight") { Source = chk, Mode = BindingMode.OneWay });
                                    var methodInfo = DataContext.GetType().GetMethod(btnAtt.CallbackName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    var paramsInfo = methodInfo.GetParameters();

                                    if (paramsInfo.Length == 0)
                                    {
                                        var action = (Action)methodInfo.CreateDelegate(typeof(Action), DataContext);
                                        btn.Click += (s, e) => action();
                                    }
                                    else if (paramsInfo.Length == 2 && paramsInfo[0].ParameterType == typeof(object) && typeof(EventArgs).IsAssignableFrom(paramsInfo[1].ParameterType))
                                    {
                                        var _delegate = methodInfo.CreateDelegate(typeof(RoutedEventHandler), DataContext);
                                        btn.Click += (s, e) => _delegate.DynamicInvoke(s, e);
                                    }
                                    else
                                    {
                                        throw new Exception($"{btnAtt.CallbackName}的引數不符合規定");
                                    }

                                    list.Add(btn);
                                }
                                if (property.TryGetCustomAttribute<DefaultValueAttribute>(out var defaultAtt))
                                {
                                    var btn = new Button()
                                    {
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Name = property.Name,
                                        ToolTip = "Reset",
                                        Style = Application.Current.Resources["TransBtn"] as Style,
                                        Tag = property.Name,
                                        Content = new Image()
                                        {
                                            Stretch = Stretch.None,
                                            Source = new BitmapImage(new Uri(@"pack://application:,,,/dw_base;component/Resources/icon16_cross.png"))
                                        },
                                    };
                                    btn.SetBinding(Button.IsEnabledProperty, new Binding(Name) { Mode = BindingMode.OneWay, Converter = ObjectIsNotEmptyToBooleanConverter.Instance });
                                    btn.Click += ResetToDefault;

                                    list.Add(btn);
                                }

                                SetLayout(chk, list.ToArray());
                                break;
                            }
                        case "String":
                            {
                                TextBox tbx = CreateTextBox(property);
                              
                                if (property.GetCustomAttribute<PathAttribute>() is PathAttribute pathAtr)
                                {
                                    var btn = new Button()
                                    {
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Name = property.Name
                                    };
                                    var canvas = (Grid)(btn.Content = new Grid() { Margin = new Thickness(4) });
                       
                                    canvas.Children.Add(
                                        new Line()
                                        {
                                            Stretch = Stretch.Uniform,
                                            X1 = 0.5,
                                            Y1 = 0,
                                            X2 = 0.5,
                                            Y2 = 1,
                                            Stroke = Brushes.Black,
                                            StrokeThickness = 3,
                                        });
                                    canvas.Children.Add(
                                        new Line()
                                        {
                                            Stretch = Stretch.Uniform,
                                            X1 = 0,
                                            Y1 = 0.5,
                                            X2 = 1,
                                            Y2 = 0.5,
                                            Stroke = Brushes.Black,
                                            StrokeThickness = 3,
                                        });
                                    canvas.SetBinding(Grid.OpacityProperty, new Binding("IsEnabled")
                                    {
                                        Converter = BooleanToOpacityConverter.Instance,
                                        RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                                        //Mode = BindingMode.OneWay
                                    }) ;
                                    tbx.IsReadOnly = true;
                                    tbx.SetBinding(TextBox.ToolTipProperty, new Binding("Text") { Source = tbx, Mode = BindingMode.OneWay, Converter = new EmptyStringToNullConverter() });
                                    btn.SetBinding(Button.HeightProperty, new Binding("ActualHeight") { Source = tbx, Mode = BindingMode.OneWay });
                                    btn.SetBinding(Button.WidthProperty, new Binding("ActualHeight") { Source = tbx, Mode = BindingMode.OneWay });
                                    btn.Click += SetPath;
                                    btn.Tag = property.Name;
                                    SetLayout(tbx, btn);
                                }
                                else if (property.TryGetCustomAttribute<ButtonAttribute>(out var smallButtonAttribute))
                                {
                                    var btn = new Button()
                                    {
                                        Content = smallButtonAttribute.Text,
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };

                                    var methodInfo = DataContext.GetType().GetMethod(smallButtonAttribute.CallbackName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    var paramsInfo = methodInfo.GetParameters();

                                    if (paramsInfo.Length == 0)
                                    {
                                        var action = (Action)methodInfo.CreateDelegate(typeof(Action), DataContext);
                                        btn.Click += (s, e) => action();
                                    }
                                    else if (paramsInfo.Length == 2 && paramsInfo[0].ParameterType == typeof(object) && typeof(EventArgs).IsAssignableFrom(paramsInfo[1].ParameterType))
                                    {
                                        var _delegate = methodInfo.CreateDelegate(typeof(RoutedEventHandler), DataContext);
                                        btn.Click += (s, e) => _delegate.DynamicInvoke(s, e);
                                    }
                                    else
                                    {
                                        throw new Exception($"{smallButtonAttribute.CallbackName}的引數不符合規定");
                                    }

                                    SetLayout(tbx, btn);
                                }
                                else
                                    SetLayout(tbx);

                                break;
                            }
                        case nameof(DeepWise.Data.DetectionResult):
                            {
                                
                                var tbx = new TextBox()
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Foreground = Brushes.White,
                                    TextAlignment = TextAlignment.Center,
                                    FontWeight = FontWeights.Bold,
                                    BorderBrush = Brushes.DimGray,
                                    BorderThickness = new Thickness(1),
                                    Name = property.Name,
                                    IsHitTestVisible = false,
                                };

                                tbx.SetBinding(TextBox.TextProperty, new Binding(property.Name)
                                {
                                    Source = DataContext,
                                    Mode = BindingMode.OneWay,
                                });

                                tbx.SetBinding(TextBox.BackgroundProperty, new Binding(property.Name)
                                {
                                    Source = DataContext,
                                    Mode = BindingMode.OneWay,
                                    Converter = DeepWise.Data.DetectionResult2ColorConverter.Instance,
                                });

                                Grid.SetColumn(tbx, 1);
                                Children.Add(tbx);
                                break;
                            }
                        default:
                            if (property.PropertyType.IsEnum)
                            {

                                if (property.TryGetCustomAttribute<RadioButtonsAttribute>(out var radio))
                                {
                                    IEnumerable<Enum> enums = property.TryGetCustomAttribute<WhiteListAttribute>(out var whiteList) ?
                                        whiteList.Elements.Cast<Enum>() :
                                        enums = Enum.GetValues(property.PropertyType).Cast<Enum>();

                                    if (radio.Direction == RadioButtonsDirection.TopToBottom)
                                    {
                                        foreach (Enum item in enums)
                                        {
                                            //RowDefinitions.Add(new RowDefinition() { Height = new GridLength(this.FontSize * 2) });
                                            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(24 * 2) });
                                            RadioButton rbtn = new RadioButton()
                                            {
                                                Content = item.GetDisplayName(),

                                                Margin = new Thickness(16, 0, 0, 0),
                                                VerticalAlignment = VerticalAlignment.Center,
                                            };

                                            var binding = new Binding(property.Name)
                                            {
                                                Source = DataContext,
                                                Converter = EnumBooleanConverter.Defualt,
                                                ConverterParameter = item,
                                            };
                                            rbtn.SetBinding(RadioButton.IsCheckedProperty, binding);
                                            Children.Add(rbtn);
                                            Grid.SetColumn(rbtn, 0);
                                            Grid.SetColumnSpan(rbtn, 2);
                                            Grid.SetRow(rbtn, RowDefinitions.Count - 1);
                                        }
                                    }
                                    else if (radio.Direction == RadioButtonsDirection.LeftToRight)
                                    {
                                        var wpnl = new DockPanel();
                                        foreach (Enum item in enums)
                                        {
                                            RadioButton rbtn = new RadioButton()
                                            {
                                                Content = item.GetDisplayName(),
                                                VerticalAlignment = VerticalAlignment.Center,
                                                Margin = new Thickness(5),
                                            };
                                            var binding = new Binding(property.Name)
                                            {
                                                Source = DataContext,
                                                Converter = EnumBooleanConverter.Defualt,
                                                ConverterParameter = item,
                                            };
                                            rbtn.SetBinding(RadioButton.IsCheckedProperty, binding);
                                            wpnl.Children.Add(rbtn);
                                        }
                                        Children.Add(wpnl);
                                        Grid.SetColumn(wpnl, 1);
                                        Grid.SetRow(wpnl, RowDefinitions.Count - 1);

                                    }
                                }
                                else if (property.PropertyType.GetCustomAttribute<FlagsAttribute>() is FlagsAttribute flagsAtt)
                                {
                                    IEnumerable<Enum> enums = Enum.GetValues(property.PropertyType).Cast<Enum>();
                                    foreach (Enum item in enums)
                                    {
                                        RowDefinitions.Add(new RowDefinition());
                                        //RowDefinitions.Add(new RowDefinition() { Height = new GridLength(this.FontSize * 2) });
                                        CheckBox rbtn = new CheckBox()
                                        {
                                            Content = item.GetDisplayName(),
                                            Margin = new Thickness(16, 0, 0, 0),
                                            VerticalAlignment = VerticalAlignment.Center,
                                        };
                                        var binding = new Binding(property.Name)
                                        {
                                            Source = DataContext,
                                            Converter = EnumFlagsBooleanConverter.Defualt,
                                            ConverterParameter = (item, DataContext, property),
                                        };
                                        rbtn.SetBinding(CheckBox.IsCheckedProperty, binding);
                                        Children.Add(rbtn);
                                        Grid.SetColumn(rbtn, 0);
                                        Grid.SetColumnSpan(rbtn, 2);
                                        Grid.SetRow(rbtn, RowDefinitions.Count - 1);
                                    }
                                }
                                else
                                {
                                    var cbx = new ComboBox() { VerticalAlignment = VerticalAlignment.Center };
                                    FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TextBlock));
                                    Binding placeBinding = new Binding();
                                    fef.SetBinding(TextBlock.TextProperty, placeBinding);
                                    placeBinding.Converter = new EnumDisplayNameConverter();
                                    var dataTemplate = new DataTemplate();
                                    dataTemplate.VisualTree = fef;
                                    cbx.ItemTemplate = dataTemplate;
                                    cbx.ItemsSource = Enum.GetValues(property.PropertyType);
                                    cbx.SetBinding(ComboBox.SelectedItemProperty, new Binding(property.Name)
                                    {
                                        Source = DataContext,
                                        Mode = property.CanWrite ? BindingMode.Default : BindingMode.OneWay
                                    });
                                    Children.Add(cbx);
                                    Grid.SetColumn(cbx, 2);
                                }
                                break;
                            }
                            else if (IsPointTable(property.PropertyType))
                            {
                                var tbk = new TextBlock()
                                {
                                    Text = "(Collection)",
                                    VerticalAlignment = VerticalAlignment.Center,
                                };
                                Grid.SetColumn(tbk, 2);
                                Children.Add(tbk);

                                //Content button
                                var btn = new Button()
                                {
                                    Content = " ... ",
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Tag = property.Name,
                                };

                                btn.Click += ShowPointTable;

                                Grid.SetColumn(btn, 2);
                                Children.Add(btn);
                                break;
                            }
                            else if (IsCollection(property.PropertyType))
                            {
                                var tbk = new TextBlock()
                                {
                                    Text = "(Collection)",
                                    VerticalAlignment = VerticalAlignment.Center,
                                };
                                Grid.SetColumn(tbk, 2);
                                Children.Add(tbk);

                                //Content button
                                var btn = new Button()
                                {
                                    Content = " ... ",
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Tag = (DataContext, property),
                                };
                                btn.Tag = property.Name;
                                btn.Click += ShowCollectionEditor;

                                Grid.SetColumn(btn, 2);
                                Children.Add(btn);
                                break;
                            }
                            else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                TextBox tbx = CreateTextBox(property);
                                SetLayout(tbx);
                                break;
                            }
                            else if (property.PropertyType.IsValueType)
                            {
                                var tbk = new TextBlock()
                                {
                                    Text = "(no preview)",
                                    VerticalAlignment = VerticalAlignment.Center,
                                };
                                SetLayout(tbk);
                                break;
                            }
                            else
                            {
                                if (property.TryGetCustomAttribute<ExpanderAttribute>(out var expandAtt))
                                {
                                    var expander = new Expander()
                                    {
                                        Name = member.Name,
                                        Header = member.GetDisplayName(),
                                        VerticalAlignment = VerticalAlignment.Center,

                                    };
                                    if (member.TryGetCustomAttribute<DescriptionAttribute>(out var descriptionAttribute))
                                    {
                                        expander.ToolTip = descriptionAttribute.Description;
                                    }

                                    //RowDefinitions[row].Height = new GridLength();
                                    
                                    Grid.SetColumn(expander, 0);
                                    Grid.SetColumnSpan(expander, 2);
                                    Children.Clear();
                                    Children.Add(expander);
                                    var grid = new PropertyGrid() { FlowDirection = FlowDirection.LeftToRight,Margin = new Thickness(10,0,0,0)};
                                    //border.Child = grid;
                                    expander.Content = grid;
                                    //expander.Content = border;

                                    grid.SelectedObject = property.GetValue(DataContext);

                                    break;
                                }
                                else
                                {
                                    //Content button
                                    var btn = new Button()
                                    {
                                        Content = " ... ",
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Tag = (DataContext, property),
                                    };

                                    btn.Click += ShowPropertyGrid;

                                    Grid.SetColumn(btn, 2);
                                    //Grid.SetRow(btn, row);
                                    Children.Add(btn);
                                    break;
                                }
                            }
                    }
                }
            }
            else if (member is MethodInfo method)
            {
                if (method.TryGetCustomAttribute<ButtonAttribute>(out var btnAttri))
                {
                    var btn = new Button() { Name = method.Name, VerticalAlignment = VerticalAlignment.Center };

                    btn.Content = btnAttri.Text;
                    var args = method.GetParameters();
                    if (args.Length == 0)
                    {
                        btn.Tag = Delegate.CreateDelegate(typeof(Action), DataContext, method.Name);
                        RoutedEventHandler routedEventHandler = (s, e) =>
                        {
                            ((s as Button).Tag as Action)();
                        };
                        typeof(Button).GetEvent("Click").AddEventHandler(btn, routedEventHandler);
                    }
                    else if (args.Length == 2)
                    {
                        if (args[0].ParameterType == typeof(object) && args[1].ParameterType == typeof(EventArgs))
                        {
                            btn.Tag = Delegate.CreateDelegate(typeof(EventHandler), DataContext, method.Name);
                            RoutedEventHandler routedEventHandler = (s, e) =>
                            {
                                ((s as Button).Tag as EventHandler)(s, EventArgs.Empty);
                            };
                            typeof(Button).GetEvent("Click").AddEventHandler(btn, routedEventHandler);
                        }
                        else if (args[0].ParameterType == typeof(object) && args[1].ParameterType == typeof(RoutedEventArgs))
                            typeof(Button).GetEvent("Click").AddEventHandler(btn, Delegate.CreateDelegate(typeof(RoutedEventHandler), DataContext, method.Name));
                    }
                    //typeof(Button).GetEvent("Click").AddEventHandler(btn, Delegate.CreateDelegate(typeof(RoutedEventHandler), SelectedObject, method.Name));

                    Grid.SetColumn(btn, 1);
                    Children.Add(btn);
                }
                else if (method.TryGetCustomAttribute<ToolButtonsAttribute>(out var toolButtons))
                {
                    var pnl = new StackPanel()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                    };
                    bool firstBtn = true;
                    var args = method.GetParameters();
                    if (!(args.Length == 2 && args[0].ParameterType == typeof(object) && args[1].ParameterType == typeof(EnumButtonEventArgs)))
                        throw new Exception("方法參數必須為(object,EnumButtonEventArgs)的形式");

                    foreach (Enum value in toolButtons.Buttons)
                    {
                        var btn = new Button()
                        {
                            Margin = firstBtn ? new Thickness() : new Thickness(8, 0, 0, 0),
                        };
                        //btn.Tag = Delegate.CreateDelegate(typeof(EventHandler<EnumButtonEventArgs>), SelectedObject, method.Name);
                        btn.Resources.Add("EnumValue", value);
                        btn.Resources.Add("TargetMethod", Delegate.CreateDelegate(typeof(EventHandler<EnumButtonEventArgs>), DataContext, method.Name));
                        btn.Content = new Image() { 
                            Source = System.Drawing.BitmapExtensions.ToBitmapSource((System.Drawing.Bitmap)value.GetIcon()), 
                            Stretch = Stretch.None,
                       
                        };
                        RoutedEventHandler routedEventHandler = (s, e) =>
                        {
                            (btn.Resources["TargetMethod"] as EventHandler<EnumButtonEventArgs>)(s, new EnumButtonEventArgs((Enum)btn.Resources["EnumValue"]));
                        };
                        typeof(Button).GetEvent("Click").AddEventHandler(btn, routedEventHandler);
                        btn.SetBinding(Button.WidthProperty, new Binding("ActualHeight") { Source = btn });

                        pnl.Children.Add(btn);
                        firstBtn = false;
                    }
                    Grid.SetColumn(pnl, 2);
                    Children.Add(pnl);
                }
            }
#endregion
        }

        void SetLayout(FrameworkElement field,params FrameworkElement[] buttons)
        {
            if(buttons == null || buttons.Length == 0)
            {
                Grid.SetColumn(field, 1);
                Children.Add(field);
            }
            else
            {
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                
                Grid.SetColumn(field, 0);
                grid.Children.Add(field);
                for (int i = 0; i < buttons.Length; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    Grid.SetColumn(buttons[i], i + 1);
                    buttons[i].Margin = new Thickness(4, 0, 0, 0);
                    grid.Children.Add(buttons[i]);
                }
                SetColumn(grid, 1);
                this.Children.Add(grid);
            }
        }

        TextBox CreateTextBox(PropertyInfo property)
        {
            var tbx = new TextBox() { VerticalAlignment = VerticalAlignment.Center };
            tbx.Name = property.Name;
            bool publicWrite = (property.CanWrite && property.SetMethod.IsPublic);
            var binding = new Binding(property.Name)
            {
                Source = DataContext ,
                Mode = publicWrite ? BindingMode.Default : BindingMode.OneWay,
            };

            if(property.TryGetCustomAttribute<DecimalPlacesAttribute>(out var atr))
                if (property.PropertyType == typeof(double) || property.PropertyType == typeof(float))
                    binding.StringFormat = $"N{atr.Number}";

            tbx.SetBinding(TextBox.TextProperty, binding);
            tbx.IsReadOnly = !publicWrite;
            tbx.KeyDown += KeyPressUpdate;
            return tbx;
        }
        static void KeyPressUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tbx = e.Source as TextBox;
                var exp = tbx.GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
        }
    }

    public class ObservableSlider : Slider
    {
        public ObservableSlider()
        {
            var track = this.Template.FindName("PART_Track", this) as Track;
            _thumb = track.Thumb;
        }

        Thumb _thumb;
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            if (_thumb.IsDragging)
                Scroll?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler Scroll;
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumButtonEventArgs : EventArgs
    {
        public EnumButtonEventArgs(Enum value)
        {
            Value = value;
        }
        public Enum Value { get; }
    }

    internal class RightMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(0, 0, (double)value + 4, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckBoxSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var h = (double)value / 18;
            if (h < 1) h = 1;
            return h;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
