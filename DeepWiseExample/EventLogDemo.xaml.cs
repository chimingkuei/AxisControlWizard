using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace DeepWise.Test
{
    /// <summary>
    /// EventLogDemo.xaml 的互動邏輯
    /// </summary>
    [Group("系統"),DisplayName("EventLog(事件檢視器)")]
    [Description("此範例展示如何讓物件繼承ILogMessageProvider介面，並將訊息寫入EventLog中。")]
    public partial class EventLogDemo : Window
    {
        public EventLogDemo()
        {
            InitializeComponent();
        }
        TestDevice TestDevice { get; } = new TestDevice();
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(btn_Debug):
                    {
                        
                        EventLog.WriteDeubg("debug");
                        break;
                    }
                case nameof(btn_Clear):
                    {
                        if(System.IO.File.Exists(EventLog.Default.Path))
                        {
                            if(MessageBox.Show("確定要刪除目前的檔案嗎？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                EventLog.Default.Messages.Clear();
                                System.IO.File.Delete(EventLog.Default.Path);
                            }
                        }
                        break;
                    }
                case nameof(btn_ThreadTest):
                    {
                        (sender as Button).IsEnabled = false;
                        var a = Task.Run(() =>
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                EventLog.Write($"{i}", "A");
                                System.Threading.Thread.Sleep(10);
                            }
                        });
                        await B();
                        await a;
                        
                        (sender as Button).IsEnabled = true;
                    }
                    break;
                case nameof(btn_Error):
                    try
                    {
                        throw new Exception("hey");
                    }
                    catch (Exception ex)
                    {
                        EventLog.Write(ex);
                    }
                    break;
                case nameof(btn_Info):
                    //cross thread operation is also valid
                    _ = Task.Run(() => EventLog.Write("test message", "caption"));
                    break;
                case nameof(btn_Folder):
                    //logs were stored in the "logs" folder under the application directory
                    var dir = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
                    Process.Start(dir);
                    break;
            }

            async Task B()
            {
                for (int i = 0; i < 100; i++)
                {
                    EventLog.Write($"{i}", "B");
                    await Task.Delay(5);
                }
            }
        }
    }

    public class TestDevice : ILogMessageProvider
    {
        public TestDevice()
        {
            //via RegistEvent() the defaul eventLog can recieved message from this device
            this.RegistEvent();
        }
        public event EventHandler<LogMessageEventArgs> MessageWritten;

        protected void SendMessage(string message, string caption = "", MessageLevel level = MessageLevel.Warning)
        {
            if (caption == "") caption = message;
            MessageWritten?.Invoke(this, new LogMessageEventArgs(caption, this, description: message, level: level));

        }
        protected void ThrowMessage(Exception ex)
        {
            MessageWritten?.Invoke(this, new LogMessageEventArgs(ex, this));
        }

        public void ThrowMessage()
        {
            SendMessage("testMessage", "test", MessageLevel.Info);
        }

        public void ThrowErrorMessage()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                ThrowMessage(ex);
            }
        }
    }
}
