using DeepWise.Localization;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace DeepWise.Test
{
    /// <summary>
    /// PropertyGridDemo.xaml 的互動邏輯
    /// </summary>
    [Group("系統"), DisplayName("Localization(多語系)")]
    [Description("多語言範例(地區化)，透過建立不同語系之resx資源檔案，在UI中顯示地區化的語言文字。此範例包含兩個部分:\r\n1.xaml中的地區化\r\n2.PropertyGrid控件中的地區化")]
    public partial class LocalizationDemo : Window
    {
        public LocalizationTestClass Data { get => (LocalizationTestClass)GetValue(DataProperty); set => SetValue(DataProperty, value); }
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(LocalizationTestClass), typeof(LocalizationDemo));
        public LocalizationDemo()
        {
            InitializeComponent();
            Data = new LocalizationTestClass();
            propertyGrid.SelectedObject = Data;
            foreach(ComboBoxItem item in cbx_Language.Items)
            {
                if (item.Tag is string str && str== LocalizeDictionary.CurrentCulture.Name)
                {
                    cbx_Language.SelectedItem = item;
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var text = TestLocalizationDirectionEnum.Left.GetDisplayName();
            MessageBox.Show(text);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var name = (e.AddedItems[0] as ComboBoxItem).Tag as string;
            if (LocalizeDictionary.CurrentCulture.Name == name) return;
            LocalizeDictionary.CurrentCulture = new CultureInfo(name);
            propertyGrid.Reset();
        }
    }
    //[地區化基本概念]============================================================================
    //透過資源檔的不同地區後綴來讀取不同的表定義，再透過Key去尋找對應的值e.g.
    //LocalizationDemoResources.resx  (可不定義內容但一定要有默認資源檔)
    //LocalizationDemoResources.en-US.resx (英文資源檔)
    //LocalizationDemoResources.zh-TW.resx (中文資源檔)

    //[Xaml中的地區化]============================================================================
    //藉由在Xaml中加入 :
    //1. xmlns:lex="clr-namespace:DeepWise.Localization"
    //此行為固定不變
    //2. lex:LocalizeDictionary.DesignCulture="en-US"
    //用來切換Designer的語言預覽 ex "en-US", "zh-TW"
    //3. lex:ResxLocalizationProvider.DefaultAssembly="DeepWiseTest"
    //用來指定組件的名稱(組件名與預設命名空間相同時可省略此行)
    //4. lex:ResxLocalizationProvider.DefaultDictionary="DeepWise.Test.LocalizationDemoResources"
    //用來指定目標資源檔
    //
    //將上述加入xaml後即可透過下面語法綁定至對應語言資源檔的Table表
    //Title="{lex:Loc Title}" 

    //[PropertyGrid中的地區化]====================================================================
    //藉由 LocalizedDisplayNameAttribute 來實作

    public enum TestLocalizationDirectionEnum
    {
        [LocalizedDisplayName("TestLocalizationDirectionEnum.Up", typeof(LocalizationDemoResources))]
        Up,
        [LocalizedDisplayName("TestLocalizationDirectionEnum.Down", typeof(LocalizationDemoResources))]
        Down,
        [LocalizedDisplayName("TestLocalizationDirectionEnum.Left", typeof(LocalizationDemoResources))]
        Left,
        [LocalizedDisplayName("TestLocalizationDirectionEnum.Right", typeof(LocalizationDemoResources))]
        Right
    }


    
    public class LocalizationTestClass 
    {

        [LocalizedDisplayName("Threshold",typeof(LocalizationDemoResources))]
        public int Threshold { get; set; }

        [LocalizedDisplayName("Direction", typeof(LocalizationDemoResources))]
        public TestLocalizationDirectionEnum Direction { get; set; }

    }
}
