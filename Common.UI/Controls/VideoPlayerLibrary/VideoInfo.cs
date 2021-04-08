﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Controls.VideoPlayerLibrary
{
    public class VideoInfo
    {
        public string DisplayName { set; get; }

        public VideoSource VideoSource { set; get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
