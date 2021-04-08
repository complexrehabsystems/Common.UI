namespace Common.Controls.Charts
{
    public class Scale
    {
        public Range InputRange { get; set; }
        public Range OutputRange { get; set; }

        public float ConvertToInputRange(float x) => Convert(x, OutputRange, InputRange);
        public float ConvertToOutputRange(float x) => Convert(x, InputRange, OutputRange);

        private float Convert(float x, Range from, Range to)
        {
            var p = from.ToPercentage(x);
            return to.FromPercentage(p);
        }
    }
}
