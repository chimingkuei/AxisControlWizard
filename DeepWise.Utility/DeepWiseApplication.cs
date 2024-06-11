using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeepWise
{
#if TEST
    public class DeepWiseApplication : Application
    {
        public DeepWiseApplication()
        {
            
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DeepWise.EventLog.Default.Messages.Add(new LogMessage() { Caption = "系統開啟", DateTime = DateTime.Now, Level = MessageLevel.Info });
        }
    }
#endif
}
