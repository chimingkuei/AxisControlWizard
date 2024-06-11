using DeepWise.AccessControls;
using DeepWise.Controls;
using DeepWise.Data;
using DeepWise.Localization;
using DeepWise.Shapes;
using Microsoft.Windows.Design.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
    [DisplayName("MainWindow(主視窗)")]
    [Description("此範例展示一個標準的主視窗模板")]
    public partial class MainWindowDemo : Window
    {
        public MainWindowDemo()
        {
            InitializeComponent();
           
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
                case nameof(btn_ClosePop):
                    {
                        pop.IsOpen = false;
                        break;
                    }
                case nameof(btn_EventLog):
                    {
                        pop.IsOpen = !pop.IsOpen;
                        EventLog.Default.NewMessage = false;
                        break;
                    }
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
                                AccessController.Default.CurrentUser = null;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("確認要關閉程式嗎？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                this.Close();
        }
    }
    public class DRadioButton :RadioButton
    {
        public DRadioButton()
        {
         
            Loaded += DRadioButton_Loaded;
        }

        private void DRadioButton_Loaded(object sender, RoutedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(this))
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                layer.Add(new SimpleCircleAdorner(this));
            }
            Loaded -= DRadioButton_Loaded;
        }
        
    }
    public class SimpleCircleAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public SimpleCircleAdorner(UIElement adornedElement) : base(adornedElement)
        {
          
        }
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            (AdornedElement as DRadioButton).IsChecked = true;
            base.OnMouseLeftButtonDown(e);
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            System.Windows.Rect adornedElementRect = new System.Windows.Rect(this.AdornedElement.DesiredSize);

            drawingContext.DrawRectangle(Brushes.Red, null, adornedElementRect);
        }
    }
}
