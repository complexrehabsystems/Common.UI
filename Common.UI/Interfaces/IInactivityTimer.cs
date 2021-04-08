using System;

namespace Common.UI.Interfaces
{
    public interface IInactivityTimer
    {
        void Initialize(TimeSpan timeout, Action callback);
        void Reset();
        void Stop();
        bool CheckInactivity();
    }
}
