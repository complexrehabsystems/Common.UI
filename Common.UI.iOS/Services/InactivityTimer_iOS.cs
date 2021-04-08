using CrsCommon.Interfaces;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace CrsCommon.UI.iOS.Services
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