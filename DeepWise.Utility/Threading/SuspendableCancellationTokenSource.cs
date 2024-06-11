using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeepWise.Threading
{
    public class SuspendableCancellationTokenSource : CancellationTokenSource
    {
        public int ResumeWatchingInterval { get; set; } = 50;

        public void Suspend()
        {
            if (IsSuspended) throw new InvalidOperationException("Source is already suspended");
            IsSuspended = true;
        }

        public void Resume()
        {
            IsSuspended = false;
        }

        public bool IsSuspended { get; private set; } = false;

        /// <summary>
        /// if source is suspended, this method will be blocked until Resume() is called.
        /// </summary>
        /// <returns>return true if cancellation is requested.</returns>
        public async Task<bool> WaitWhileSuspended()
        {
            while (IsSuspended)
            {
                await Task.Delay(ResumeWatchingInterval, this.Token);
                if (Token.IsCancellationRequested) return true;
            }
            return Token.IsCancellationRequested;
        }

      
    }
}
