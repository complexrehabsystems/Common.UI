using System;
using PropertyChanged;

namespace Common.Controls.Charts
{
    [AddINotifyPropertyChangedInterface]
    public class DataPoint
    {
        public DataPoint()
        {
            XLabel = string.Empty;
        }

        public float Value { get; set; }
        public string XLabel { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsHovered { get; set; }
        public int ID { get; set; }
        public bool Locked { get; set; }
    }
}
