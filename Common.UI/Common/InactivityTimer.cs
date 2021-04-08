using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace Common.Common
{
    public class InactivityTimer : IInactivityTimer
    {
        private MyTimer _timer;
        private TimeSpan _timeout;
        private Action _callback;
        
        public bool CheckInactivity()
        {
            return _timer?.TimerInvoked ?? false;
        }

        public void Initialize(TimeSpan timeout, Action callback)
        {
            _timeout = timeout;
            _callback = callback;

            Reset();
        }

        public void Reset()
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != 1)
                return; // must be on the UI thread

            Stop();

            if (_callback == null)
                return;

            _timer = new MyTimer(_timeout, _callback);
            _timer.Start();
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }
    }

    public class MyTimer
    {
        public bool TimerInvoked => _invoked;

        private readonly TimeSpan timespan;
        private readonly Action callback;
        private bool _invoked;

        private CancellationTokenSource cancellation;

        public MyTimer(TimeSpan timespan, Action callback)
        {
            this.timespan = timespan;
            this.callback = callback;
            this.cancellation = new CancellationTokenSource();
        }

        public void Start()
        {
            _invoked = false;
            CancellationTokenSource cts = this.cancellation; // safe copy
            Device.StartTimer(this.timespan,
                () => 
                {
                    if (cts.IsCancellationRequested) 
                        return false;

                    this.callback?.Invoke();
                    _invoked = true;
                    return false; // or true for periodic behavior
                });
        }

        public void Stop()
        {
            Interlocked.Exchange(ref this.cancellation, new CancellationTokenSource()).Cancel();
        }
    }
}
