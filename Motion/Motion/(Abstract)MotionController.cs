using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeepWise;
namespace MotionControllers.Motion
{
    public abstract class MotionController : INotifyPropertyChanged , ILogMessageProvider
    {
        public event EventHandler<LogMessageEventArgs> MessageWritten;
        protected void WriteMessage(string caption,string message,MessageLevel level = MessageLevel.Error,string stackTrace=null)
        {
            MessageWritten?.Invoke(this, new DeepWise.LogMessageEventArgs(new DeepWise.LogMessage()
            {
                Caption = caption,
                Description = message,
                Level = level,
                DateTime = DateTime.Now,
                StackTrace = stackTrace
            }));
        }
        protected void WriteMessage(Exception ex,string stackTrace = null)
        {
            MessageWritten?.Invoke(this, new DeepWise.LogMessageEventArgs(ex,this,stackTrace: stackTrace));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _name = "unnamed";
        public virtual string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }
        protected static int BoardID_Status = -1;

        public MotionController(string Name)  
        {
            this.RegistEvent();

            this.Name = Name; 

        }

    }
}
