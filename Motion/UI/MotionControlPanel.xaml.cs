using MotionControllers.Motion;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MotionControllers.UI
{
    /// <summary>
    /// Interaction logic for MotionControlPanel.xaml
    /// </summary>
    public partial class MotionControlPanel : UserControl
    {
        public MotionControlPanel()
        {
            InitializeComponent();
        }

        public readonly static DependencyProperty AxisControllerProperty = DependencyProperty.Register(nameof(AxisController), typeof(AxisMotionController), typeof(MotionControlPanel));
        public AxisMotionController AxisController
        {
            get => (AxisMotionController)GetValue(AxisControllerProperty);
            set => SetValue(AxisControllerProperty, value);
        }

        private async void OnMoveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is AxisMotionController cntlr)
            {
                try
                {
                    switch ((sender as Button).Content as string)
                    {
                        case "-REL":
                            cntlr.MoveRelative(-cntlr.DistanceRelative);
                            break;
                        case "+REL":
                            cntlr.MoveRelative(cntlr.DistanceRelative);
                            break;
                        case "-JOG":
                        case "+JOG":
                            //event are triggering in PreviewMouseUP、Down
                            break;
                        case "ABS MOVE":
                            //
                            break;
                        case "HOME":
                            _ = cntlr.MoveHome();
                            break;
                    }
                }catch (Exception ex)
                {
                    DeepWise.MessageBox.Show(ex);
                }
            }
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ADLINK_Motion cntlr && e.LeftButton == MouseButtonState.Pressed)
            {
                switch ((sender as Button).Content)
                {
                    case "+JOG":
                        cntlr.StartJogMotion(AxisController.AxisID, 0);
                        break;
                    case "-JOG":
                        cntlr.StartJogMotion(AxisController.AxisID, 1);
                        break;

                }
            }
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                switch ((sender as Button).Content)
                {
                    case "+JOG":
                    case "-JOG":
                        AxisController?.StopJogMotion();
                        break;
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ADLINK_Motion controller)
            {
                var wind = new Window()
                {
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Title = "I/O Table",
                    DataContext = controller,
                    Width = 600,
                    Height = 600,
                    Owner = Window.GetWindow(this),
                };
                wind.Content = new IOTable() { Foreground = new SolidColorBrush(Colors.White) };
                var task = new TaskCompletionSource<object>();
                wind.Closed += (s, a) => task.SetResult(null);
                wind.Closed += (s2, e2) =>
                {
                    if (controller.IOTable.HasChanged())
                    {
                        if (MessageBox.Show("要儲存目前的設定嗎?", "IO表已變更", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            controller.IOTable.Save();
                        }
               
                    }
                    
                        
                };
                (sender as Button).IsEnabled = false;
                wind.Show();
                await task.Task;
                (sender as Button).IsEnabled = true;
            }
        }
        private void Button_Click_1(object sender, EventArgs e)
        {
            if (DataContext is ADLINK_Motion controller)
                try
                {
                    controller?.StopMoveAllEmergency();
                }
                catch
                {
                }
        }

        private void BTN_Servo_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ADLINK_Motion controller)
            {
                try
                {
                    switch ((sender as Button).Name)
                    {
                        case nameof(BTN_Servo):
                            AxisController.SetServo(!AxisController.GetMotionIOStatus(MotionIOStatus.SVON));
                            break;
                        case nameof(BTN_Save):
                            controller.Axes.Save();
                            break;
                        case nameof(BTN_AlarmReset):
                            AxisController.ResetAlarm();
                            //MessageBox.Show("not implement yet");
                            break;
                        case nameof(BTN_SaveAsFile):
                            using (var dlg = new System.Windows.Forms.SaveFileDialog())
                            {
                                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    try
                                    {
                                        controller?.SaveParameterToFile(dlg.FileName);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                            }
                            break;
                        case nameof(BTN_LoadFromFile):
                            using (var dlg = new System.Windows.Forms.OpenFileDialog())
                            {
                                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    try
                                    {
                                        controller?.LoadParamFromFile(dlg.FileName);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                            }
                            break;
                        case nameof(BTN_Pos0):
                            if (MessageBox.Show("確認要將回饋位置設置為0(但馬達不會移動)？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                            {
                                AxisController.Position.SetPosition(0);
                            }
                            break;


                    }
                }
                catch(Exception ex)
                {
                    DeepWise.MessageBox.Show(ex);
                }
            }
        }

        private void BTN_Servo_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}