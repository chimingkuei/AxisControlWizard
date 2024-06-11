using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MotionControllers.Motion
{
    public class MotionControllerMonitor
    {
        ADLINK_Motion cntlr;
        public MotionControllerMonitor(ADLINK_Motion cntlr, Action<Exception,string> writeErrorMessage)
        {
            this.cntlr = cntlr;
            this.writeErrorMessage = writeErrorMessage;
        }
        private Action<Exception,string> writeErrorMessage;
        public int _updateRate = 40;
        [Range(15, 150)]
        public int UpdateRate
        {
            get => _updateRate;
            set
            {
                if (value < 15 || value > 150) throw new ArgumentOutOfRangeException();
                _updateRate = value;
            }
        }

        public bool Start()
        {
            if (cntlr.IsInitialized)
            {
                if(thread==null)
                {
                    thread = new Thread(MonitorStatus);
                    //thread.SetApartmentState(ApartmentState.STA)
                    thread.Priority = ThreadPriority.Highest;
                    thread.Start();
                }
                return true;
            }
            return false;
        }
        Thread thread;
        public ThreadPriority ThreadPriority { get => thread.Priority; set => thread.Priority = value; }
        public void Stop()
        {
            thread?.Abort();
            thread = null;
        }
        public event EventHandler Update;
        private AsyncAutoResetEvent FrameExpired { get; } = new AsyncAutoResetEvent(false);
        public async Task Wait()
        {
             await FrameExpired.WaitAsync();
             await FrameExpired.WaitAsync();
        }
        
        private void MonitorStatus()
        {
            try
            {
                while (cntlr.IsInitialized)
                {
                    Update?.Invoke(this, EventArgs.Empty);
                    Thread.Sleep(UpdateRate);
                    FrameExpired.Set();
                }
            }
            catch (ThreadAbortException ex)
            {

            }
            catch (Exception ex)
            {
                var newEx = new Exception("monitorException", ex);
                writeErrorMessage?.Invoke(newEx,null);
                throw newEx;
            }
        }
    }
}
