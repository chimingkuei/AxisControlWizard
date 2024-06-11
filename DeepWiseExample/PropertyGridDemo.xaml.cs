using DeepWise.AccessControls;
using DeepWise.Controls;
using DeepWise.Localization;
using DeepWise.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace DeepWise.Test
{
    /// <summary>
    /// PropertyGridDemo.xaml 的互動邏輯
    /// </summary>
    [Group("UI控制項")]
    [DisplayName("PropertyGrid(屬性檢視器)")]
    [Description("此範例提供如何使用PropertyGrid的控件，以及其提供的功能、擴充屬性。e.g.\r\n[SliderAttribute]\r\n[DisplayNameAttribute]\r\n[OrderAttribute]\r\n等等.......")]
    public partial class PropertyGridDemo : Window
    {
        public PropertyGridDemo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch( (sender as Button).Content as string )
            {
                case "Value1 = 50":
                    (propertyGrid.SelectedObject as PropertyGridDemoObject).Value1 = 50;
                    //You can either implement INotifyPropertyChanged interface for the target classes
                    //or call 'Update' method each time you manual changed the property value behind code
                    break;
                case "Update Value1":
                    propertyGrid.Update("Value1");
                    //propertyGrid.Update() ->use method with no parameters for updates all properties
                    break;
                case "Change Layout":
                    propertyGrid.LabelWidth = new GridLength(3, GridUnitType.Star);
                    propertyGrid.FieldWidth = new GridLength(1, GridUnitType.Star);
                    //Equals to <UI:PropertyGrid  LabelWidth="3*" FieldWidth="1*"/> in Xaml
                    propertyGrid.ItemMargin = new Thickness(10);
                    break;
            }
        }
    }

    //Enum with "Flags" Attribute can provide checkboxes for each flag instead of a single combobox.
    [Flags]
    public enum PropertyGridDemoAnchor
    {
        Top = 0b01,
        Left = 0b10,
        Bottom = 0b100,
        [Display(Name = "右")]
        Right = 0b1000,
    }

    public class PropertyGridDemoObject : INotifyPropertyChanged
    {
        public DemoObject Item { get; } = new DemoObject();

        [Expander]
        public DemoObject Item2 { get; } = new DemoObject();
        
        //Use [ToolButtons] attribute to create several small button for method
        //Notice!! : the EventArgs have to be type in "EnumButtonEventArgs" so we can distinguish which button had been clicked.
        [ToolButtons(ToolButton.Add, ToolButton.Copy, CustomIcon.Number1)]
        public void ToolButtons(object sender, EnumButtonEventArgs e)
        {
            //can use default enum type : DeepWise.Controls.ToolButton
            //or build a custom enum value that containing a [Icon] attribute
            switch (e.Value)
            {
                case ToolButton.Add:
                    MessageBox.Show(e.Value.ToString());
                    break;
                case ToolButton.Copy:
                    MessageBox.Show(e.Value.ToString());
                    break;
                case CustomIcon.Number1:
                    MessageBox.Show(e.Value.ToString());
                    break;
            }
        }

        public PropertyGridDemoAnchor Anchor { get; set; } = PropertyGridDemoAnchor.Left | PropertyGridDemoAnchor.Top;

        //Use [RadioButtons] attribute force to create several radio buttons instead of a combobox
        //Use [WhiteList] attribute to limit 
        [RadioButtons,WhiteList(Visibility.Visible, Visibility.Hidden)]
        public Visibility Visibility 
        {
            get => _visibility;
            set
            {
                _visibility = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Visibility)));
            }
        }
        Visibility _visibility;

        bool _value1Enable, _value2Visible = true;

        public bool Value1Enable
        {
            get => _value1Enable;
            set
            {
                _value1Enable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value1Enable)));
            }
        }

        int _value1 = 140;
        [Slider("Value1Minimum", "Value1Maximum"),Conditional("Value1Enable")]
        public int Value1
        {
            get => _value1;
            set
            {
                _value1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value1)));
            }
        }

        [Browsable(false)]
        public int Value1Minimum => 0;

        int _value1Maximum = 255;
        [DefaultValue(255)]
        public int Value1Maximum
        {
            get => _value1Maximum;
            set
            {
                _value1Maximum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value1Maximum)));
            }
        }

        public bool Value2Visible
        {
            get => _value2Visible;
            set
            {
                _value2Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value2Visible)));
            }
        }

        //Use [Button] attribute on a property can provides a small button right of the field box.
        [Button(nameof(Value2Click), "Click"), Conditional(nameof(Value2Visible), DisableEffect.Hide)]
        public int Value2 { get; set; }

        void Value2Click()
        {
            MessageBox.Show(Value2.ToString());
        }

        [Unit("mm")]
        public int Length { get; set; } = 20;

        [Description("here is sample description...."),DisplayName("Mouse Over Here"),Mark(Mark.Exclamation)]
        public string Description { get; set; } = "";

        [DecimalPlaces(6),DisplayName("DecimalPlaces = 6")]
        public double DecimalPlaces { get; set; } = Math.PI;

        public event PropertyChangedEventHandler PropertyChanged;

        [Order(2), Button]
        public void TestButton(object sender, EventArgs e)
        {
            //the [Button] attribute can supports the following handller formats
            //1. void Method(object sender, EventArgs e)
            //2. void Method(object sender, RoutedEventArgs e)
            //3. void Method() 
            MessageBox.Show("ButtonClicked");
        }

        //use [DisplayName] , [Disply] or [LocalizedDisplayName] to display different text instead original property name.
        [DisplayName("名稱")]
        public string Name { get; set; } = "TestItem";

        //type implemented [TypeConverter] attribute can directly show on textbox(and also parse to a value from input string)
        public System.Windows.Point Location { get; set; }

        //Use [Path] attibute on a string can provides OpenFile,SelectFile,SelectDirectory Dialog for user
        [Path(PathMode.SelectFile)]
        public string SelectFile
        {
            get => _selectFile;
            set
            {
                _selectFile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectFile)));
            }
        }
        public string _selectFile = "";
    }
}