using System;
using Common.UI.Interfaces;

namespace Common.UI.iOS.Services
{
    public class InactivityTimer_iOS : IInactivityTimer
    {
        public bool CheckInactivity()
        {
            return false;
        }

        public void Initialize(TimeSpan timeout, Action callback)
        {
        }

        public void Reset()
        {
        }

        public void Stop()
        {
        }
    }
}