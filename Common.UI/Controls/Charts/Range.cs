namespace Common.UI.Controls.Charts
{
    public class Range
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public float ToPercentage(float x)
        {
            return (x - Min) / (Max - Min);
        }

        public float FromPercentage(float p)
        {
            return Min + p * (Max - Min);
        }
    }
}
