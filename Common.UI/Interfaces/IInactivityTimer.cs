using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Interfaces
{
    public interface IInactivityTimer
    {
        void Initialize(TimeSpan timeout, Action callback);
        void Reset();
        void Stop();
        bool CheckInactivity();
    }
}
