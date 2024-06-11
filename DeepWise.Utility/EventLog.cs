using DeepWise.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DeepWise
{
    public class EventLog : INotifyPropertyChanged
    {
        public static EventLog Default { get; } =  ((Func<EventLog>)(() => 
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            return new EventLog(dir + DateTime.Now.ToString("yyyy-MM-dd"));
        }))();

        /// <summary>
        /// WriteDeubg will only run when "DEBUG" was defined
        /// </summary>
        [Conditional("DEBUG")]
        public static void WriteDeubg(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string member = "")
        {
            var msg = new LogMessage()
            {
                Caption = member,
                DateTime = DateTime.Now,
                Description = message,
                Level =  MessageLevel.Debug,
                StackTrace = $"at {member} in {filePath }:line{lineNumber}",
            };
            Default.Messages.Add(msg);
        }
        public static void Write(Exception ex, object source = null)
        {
            var msg = new LogMessage()
            {
                Caption = ex.GetType().Name,
                DateTime = DateTime.Now,
                Description = ex.Message,
                Level = MessageLevel.Error,
                Source = source,
                StackTrace = ex.StackTrace,
            };
            Default.Messages.Add(msg);
        }
        public static void Write(string message,string caption=null,MessageLevel level = MessageLevel.Info,object source=null,string stackTrace = null)
        {
            var msg = new LogMessage()
            {
                Caption = caption,
                DateTime = DateTime.Now,
                Description = message,
                Level = level,
                Source = source,
                StackTrace = stackTrace,
            };
            Default.Messages.Add(msg);
        }

        public static void ShowWindow()
        {
            var win = new Window();
            win.Title = "EventLog";
            var ml = new Controls.MessageLogView();
            win.Content = ml;
            win.Width = 800;
            win.Height = 600;
            win.Show();
        }
 
        public EventLog(string path)
        {
            Path = path;
            if(System.IO.File.Exists(path))
            {
                var text = File.ReadAllText(path);
                try
                {
                    Debug.Print(path);
                    var jObj = JObject.Parse(text);
                    var msgs = jObj["Messages"].ToString();
                    JsonConvert.PopulateObject(msgs, Messages);
                    File.WriteAllText(Path, msgs.Substring(1, msgs.Length - 2));
                }
                
                catch (Exception ex)
                {
                    JsonConvert.PopulateObject('['+text+']', Messages);
                    //throw ex;
                }

                _errorNumber = Messages.Where(x => x.Level == MessageLevel.Error).Count();
                _warningNumber = Messages.Where(x => x.Level == MessageLevel.Warning).Count();
                _infoNumber = Messages.Where(x => x.Level == MessageLevel.Info).Count();
            }

            ListenList.CollectionChanged += SuscrbeMessage;
            Messages.CollectionChanged += Messages_CollectionChanged;
        }
        [JsonIgnore]
        public string Path { get; }

        public int ErrorNumber => _errorNumber;
        public int WarningNumber => _warningNumber;
        public int InfoNumber => _infoNumber;
        int _infoNumber, _warningNumber, _errorNumber;

        bool _newMessage = false;
        public bool NewMessage
        {
            get => _newMessage;
            set
            {
                if (_newMessage == value) return;
                _newMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewMessage)));
            }
        }

        object _lock = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add && e.NewItems[0] is LogMessage msg)
            {
                NewMessage = true;
                switch (msg.Level)
                {
                    case MessageLevel.Error:
                        Interlocked.Increment(ref _errorNumber);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorNumber)));
                        break;
                    case MessageLevel.Warning:
                        Interlocked.Increment(ref _warningNumber);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WarningNumber)));
                        break;
                    case MessageLevel.Info:
                        Interlocked.Increment(ref _infoNumber);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InfoNumber)));
                        break;
                }
            }
            else if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                _infoNumber = _errorNumber = _warningNumber = 0;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorNumber)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WarningNumber)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InfoNumber)));
            }

            if (e.Action == NotifyCollectionChangedAction.Add && !string.IsNullOrEmpty(Path) )
            {
                lock (_lock)
                {
                    if (File.Exists(Path))
                    {
                        using (StreamWriter w = File.AppendText(Path))
                        {
                            var str = "," + JsonConvert.SerializeObject(e.NewItems[0]);
                            w.Write(str);
                        }
                    }
                    else
                    {
                        File.WriteAllText(Path,  JsonConvert.SerializeObject(e.NewItems[0]));
                    }
                }
            }
        }

        object _addLock = new object();
        void MessageRecieved(object sender, LogMessageEventArgs e)
        {
            lock(_addLock)
            {
                Messages.Add(e.Message);
            }
        }

        private void SuscrbeMessage(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ILogMessageProvider item in e.NewItems)
                        item.MessageWritten += MessageRecieved;
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    foreach (ILogMessageProvider item in e.OldItems)
                        item.MessageWritten -= MessageRecieved;
                    break;
            }
        }

  

        public MTObservableCollection<LogMessage> Messages { get; } = new MTObservableCollection<LogMessage>();
        [JsonIgnore]
        public ObservableCollection<ILogMessageProvider> ListenList { get; } = new ObservableCollection<ILogMessageProvider>();
    }
    public class MTObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler CollectionChanged = this.CollectionChanged;
            if (CollectionChanged != null)
                foreach (NotifyCollectionChangedEventHandler nh in CollectionChanged.GetInvocationList())
                {
                    DispatcherObject dispObj = nh.Target as DispatcherObject;
                    if (dispObj != null)
                    {
                        Dispatcher dispatcher = dispObj.Dispatcher;
                        if (dispatcher != null && !dispatcher.CheckAccess())
                        {
                            dispatcher.BeginInvoke(
                                (Action)(() => nh.Invoke(this,
                                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))),
                                DispatcherPriority.DataBind);
                            continue;
                        }
                    }
                    nh.Invoke(this, e);
                }
        }
    }
 
    public static class EventLogExtension
    {
        /// <summary>
        /// regist event to default event log, So it can recieved message from this。
        /// </summary>
        [Obsolete("just Use EventLog.Write instead")]
        public static void RegistEvent(this ILogMessageProvider messageProvider) => messageProvider.RegistEvent(EventLog.Default);
        [Obsolete("just Use EventLog.Write instead")]
        public static void UnregistEvent(this ILogMessageProvider messageProvider) => messageProvider.UnregistEvent(EventLog.Default);
        /// <summary>
        /// regist event to spacific event log, So it can recieved message from this。
        /// </summary>
        [Obsolete("just Use EventLog.Write instead")]
        public static void RegistEvent(this ILogMessageProvider messageProvider, EventLog log)
        {
            if (!log.ListenList.Contains(messageProvider))
                log.ListenList.Add(messageProvider);
        }
        [Obsolete("just Use EventLog.Write instead")]
        public static void UnregistEvent(this ILogMessageProvider messageProvider, EventLog log)
        {
            if (log.ListenList.Contains(messageProvider))
                log.ListenList.Remove(messageProvider);
        }
    }

    /// <summary>
    /// Indicates can provide message for <see cref="DeepWise.EventLog"/> via <see cref="EventLogExtension.RegistEvent(ILogMessageProvider)"/>
    /// </summary>
    [Obsolete("just Use EventLog.Write instead")]
    public interface ILogMessageProvider
    {
        event EventHandler<LogMessageEventArgs> MessageWritten;
    }

    public class LogMessage
    {
        public LogMessage() { }
        public LogMessage(object source) 
        { 
            if(source == null)throw new ArgumentNullException("source");
            Source = source;
            Caption = source.ToString();
            if(source is Exception ex)
            {
                Description = ex.Message;
                StackTrace = ex.StackTrace;
                Level = MessageLevel.Error;
            }
            else
            {
                Description = source.ToString();
                Level = MessageLevel.Warning;
            }
            DateTime = DateTime.Now;
        }
        public MessageLevel Level { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }

        [JsonConverter(typeof(ToStringJsonConverter))]
        public object Source { get; set; }
        public string StackTrace { get; set; }
    }

    public class ToStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString();
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageLevel : int
    {
        Critical = 50,
        Error = 40,
        Warning = 30, 
        Info = 20, 
        Debug = 10, 
        Notset = 0,
    }

    public class LogMessageEventArgs : EventArgs
    {
        public LogMessageEventArgs(LogMessage message)
        {
            Message = message;
        }

        public LogMessageEventArgs(string caption , object source,string description ="",MessageLevel level = MessageLevel.Error,string stackTrace = null)
        {
            Message = new LogMessage() 
            {
                Caption = caption, 
                Source = source?.ToString() ,
                DateTime = DateTime.Now,
                Description = description,
                Level = level,
                StackTrace = stackTrace
            };
        }

        public LogMessageEventArgs(Exception ex, object source, MessageLevel level = MessageLevel.Error,string stackTrace = null)
        {
            Message = new LogMessage()
            {
                Caption = ex.GetType().Name,
                Source = source?.ToString(),
                DateTime = DateTime.Now,
                Description = ex.Message,
                Level = level,
                StackTrace = stackTrace == null ? ex.StackTrace : stackTrace
            };
        }
        public LogMessage Message { get; }
    }
}