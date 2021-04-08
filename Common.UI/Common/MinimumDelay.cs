using System;
using System.Threading.Tasks;

namespace Common.UI.Common
{
    public class MinimumDelay
    {
        DateTime _startTime;
        int _minimumMilliseconds;

        public MinimumDelay(int minimumMilliseconds)
        {
            _minimumMilliseconds = minimumMilliseconds;
            _startTime = DateTime.Now;
        }

        public async Task Delay()
        {
            var t = (DateTime.Now - _startTime);

            if (t.TotalMilliseconds >= _minimumMilliseconds)
                return;

            await Task.Delay(_minimumMilliseconds - (int)(t.TotalMilliseconds));
        }
    }
}
