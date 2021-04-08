using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Common.UI.Drawing
{
    public class Canvas
    {
        public float Width => _canvasView.CanvasSize.Width;
        public float Height => _canvasView.CanvasSize.Height;
        public void Invalidate() => _canvasView.InvalidateSurface();
        public void InvokeInvalidate() => Device.BeginInvokeOnMainThread(Invalidate);

        public float WidthScale { get; set; } = 1;
        public float HeightScale { get; set; } = 1;

        private SKCanvasView _canvasView;

        public Canvas(SKCanvasView canvasView)
        {
            _canvasView = canvasView;
        }

        public SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)((_canvasView.CanvasSize.Width/WidthScale) * pt.X / _canvasView.Width),
                               (float)((_canvasView.CanvasSize.Height/HeightScale) * pt.Y / _canvasView.Height));
        }
    }
}
