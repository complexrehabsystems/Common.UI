using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using System.Reactive.Linq;

namespace Common.UWP.Services
{
    public class InactivityTimer_UWP : IInactivityTimer
    {
        private MyTimer _timer;
        private TimeSpan _timeout;
        private Action _callback;
        IDisposable observable = null;
        private bool _paused = true;
        private DateTime _lastPointerMoved;

        public bool CheckInactivity()
        {
            return _timer?.TimerInvoked ?? false;
        }

        public void Initialize(TimeSpan timeout, Action callback)
        {
            _timeout = timeout;
            _callback = callback;

            _lastPointerMoved = DateTime.UtcNow;
            var coreWindow = CoreWindow.GetForCurrentThread();

            coreWindow.PointerPressed -= CoreWindow_PointerPressed;
            coreWindow.KeyDown -= CoreWindow_KeyDown;
            coreWindow.PointerMoved -= CoreWindow_PointerMoved;

            coreWindow.PointerPressed += CoreWindow_PointerPressed;
            coreWindow.KeyDown += CoreWindow_KeyDown;
            coreWindow.PointerMoved += CoreWindow_PointerMoved;

            Reset();
        }

        

        public void Reset()
        {
            if (_callback == null)
                return;

            //Debug.WriteLine("InactivityTimer re-initializing timer");

            _paused = false;

            ResetTimer();

        }

        public void Stop()
        {
            if (_timer != null)
            {
                //Debug.WriteLine("InactivityTimer stopping timer");
                _timer.Stop();
                _timer = null;
            }

            _paused = true;

        }

        protected void ResetTimer()
        {
            //Debug.WriteLine($"InactivityTimer ResetTimer timer {_paused}");
            if (_paused)
                return;

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            if (_callback == null)
                return;

            _timer = new MyTimer(_timeout, _callback);
            _timer.Start();
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            ResetTimer();
        }

        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            ResetTimer();
        }

        private void CoreWindow_PointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            var span = DateTime.UtcNow - _lastPointerMoved;
            if (span.TotalSeconds < 1)
                return;

            _lastPointerMoved = DateTime.UtcNow;
            ResetTimer();
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

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (o, e) =>
            {
                if (cts.IsCancellationRequested)
                {
                    dispatcherTimer.Stop();
                    return;
                }

                this.callback?.Invoke();
                _invoked = true;
                dispatcherTimer.Stop();
            };
            dispatcherTimer.Interval = this.timespan;
            dispatcherTimer.Start();
        }

        public void Stop()
        {
            Interlocked.Exchange(ref this.cancellation, new CancellationTokenSource()).Cancel();
        }
    }
}
