using CrsCommon.Controls.VideoPlayerLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Controls
{
    public interface IVideoPlayerController
    {
        VideoStatus Status { set; get; }

        TimeSpan Duration { set; get; }
    }
}
