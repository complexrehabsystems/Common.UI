using System;
using System.Collections.Generic;
using System.Text;

namespace CrsCommon.Drawing
{
    public class CanvasDimensions
    {
        public CanvasDimensions(float height, float width)
        {
            Height = height;
            Width = width;
        }
        public float Height { get; set; }
        public float Width { get; set; }
    }
}
