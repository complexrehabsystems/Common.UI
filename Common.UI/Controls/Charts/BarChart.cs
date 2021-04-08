using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PropertyChanged;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using TouchTracking;
using Xamarin.Forms;

namespace Common.Controls.Charts
{
    [AddINotifyPropertyChangedInterface]
    public class BarChart : SKCanvasView
    {
        public static readonly BindableProperty DataProperty = BindableProperty.Create(nameof(Data), typeof(IEnumerable<DataPoint>), typeof(BarChart), propertyChanged: OnDataChanged);

        public IEnumerable<DataPoint> Data
        {
            get => (IEnumerable<DataPoint>) GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        private IList<DataPoint> DataList => Data as IList<DataPoint>;

        public static readonly BindableProperty ChartTitleProperty = BindableProperty.Create(nameof(ChartTitleProperty), typeof(string), typeof(BarChart), string.Empty, BindingMode.Default);
        public string ChartTitle
        {
            get => (string) GetValue(ChartTitleProperty);
            set
            {
                SetValue(ChartTitleProperty, value);
                InvalidateSurface();
            }
        }

        public static readonly BindableProperty UnitsProperty = BindableProperty.Create(nameof(UnitsProperty), typeof(string), typeof(BarChart), string.Empty, BindingMode.Default);
        public string Units
        {
            get => (string) GetValue(UnitsProperty);
            set
            {
                SetValue(UnitsProperty, value);
                InvalidateSurface();
            }
        }

        public static readonly BindableProperty TickSizeProperty = BindableProperty.Create(nameof(TickSizeProperty), typeof(float), typeof(BarChart), 50.0f, BindingMode.Default);
        public float TickSize
        {
            get => (float) GetValue(TickSizeProperty);
            set
            {
                SetValue(TickSizeProperty, value);
                InvalidateSurface();
            }
        }

        public static readonly BindableProperty DefaultColorProperty = BindableProperty.Create(nameof(DefaultColor), typeof(Color), typeof(BarChart), Color.FromRgb(50, 50, 50), BindingMode.Default);
        public Color DefaultColor
        {
            get => (Color) GetValue(DefaultColorProperty);
            set
            {
                SetValue(DefaultColorProperty, value);
                InvalidateSurface();
            }
        }

        public static readonly BindableProperty HighlightColorProperty = BindableProperty.Create(nameof(HighlightColor), typeof(Color), typeof(BarChart), Color.FromHex("#148fce"), BindingMode.Default);
        public Color HighlightColor
        {
            get => (Color) GetValue(HighlightColorProperty);
            set
            {
                SetValue(HighlightColorProperty, value);
                InvalidateSurface();
            }
        }

        public static readonly BindableProperty HighlightOnHoverProperty = BindableProperty.Create(nameof(HighlightOnHoverProperty), typeof(bool), typeof(BarChart), true, BindingMode.Default);
        public bool HighlightOnHover
        {
            get => (bool) GetValue(HighlightOnHoverProperty);
            set
            {
                SetValue(HighlightOnHoverProperty, value);
                InvalidateSurface();
            }
        }

        public BarChart()
        {
            PaintSurface += OnCanvasViewPaintSurface;
            InvalidateSurface();
        }

        private SKCanvas _canvas;
        private float _canvasWidth;
        private float _canvasHeight;
        private int NumBars => DataList.Count;

        // Ensure max width of each bar is 1/4th the width of the chart
        private float BarWidthPixels => _canvasWidth / Math.Max(NumBars, 4);

        private Scale HScale;
        private Scale VScale;

        private Dictionary<string, SKPaint> _paints;

        public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (!DataList?.Any() ?? false)
                return;

            if (Math.Abs(_canvasWidth - args.Info.Width) > 0.001 || Math.Abs(_canvasHeight - args.Info.Height) > 0.001)
            {
                _canvasWidth = args.Info.Width;
                _canvasHeight = args.Info.Height;

                // if canvas is new or resized, we must recalculate the fontSizes used in the SKPaints
                InitializePaints();
            }

            _canvas = args.Surface.Canvas;
            _canvas.Clear();

            // Scales are used to map values from an input range (defined by the data) onto an output range (pixels)
            HScale = new Scale()
            {
                InputRange = new Range {Min = 0, Max = Math.Max(NumBars, 4)},
                OutputRange = new Range {Min = 50, Max = _canvasWidth-50}
            };

            VScale = new Scale()
            {
                InputRange = new Range {Min = 0, Max = DataList.Max((_) => _.Value)},
                OutputRange = new Range {Min = _canvasHeight - 60, Max = _canvasHeight / 4}
            };

            RenderAxes();
            RenderBars();
            RenderChartTitle();
        }

        private void RenderBars()
        {
            for (int i = 0; i < DataList.Count; i++)
            {
                
                DataPoint p = DataList[i];

                var x0 = HScale.ConvertToOutputRange(i);
                var y0 = VScale.ConvertToOutputRange(p.Value);

                var x1 = HScale.ConvertToOutputRange(i + 0.7f);
                var y1 = VScale.ConvertToOutputRange(0);

                var bubbleTopOffset = Math.Min(100f, BarWidthPixels / 2);
                var bubbleBottomOffset = Math.Min(40f, BarWidthPixels / 5);

                if (p.Locked)
                {

                    var lockOffset = 40f;

                    RenderLockedBarIndicator(p, x0, y0, x1, y1);

                    if (p.IsHighlighted || p.IsHovered)
                    {                       
                        RenderTopLabel(p, x0, y0 - bubbleTopOffset - lockOffset, x1, y1 - bubbleBottomOffset - lockOffset);
                    }
                }
                else
                {
                    var paint = p.IsHovered || p.IsHighlighted
                    ? _paints["highlightTransparent"]
                    : _paints["defaultTransparent"];

                    _canvas.DrawRect(new SKRect() {Left = x0, Right = x1, Bottom = y1, Top = y0}, paint);

                    if (p.IsHighlighted || p.IsHovered)
                    {
                        RenderTopLabel(p, x0, y0-bubbleTopOffset, x1, y0-bubbleBottomOffset);
                    }                   
                }

                if (!String.IsNullOrEmpty(p.XLabel))
                    RenderText(x0, y1 + 10, x1, y1+40, p.XLabel, JustifyText.CENTER, _paints["defaultFont"]);
            }
        }

        private void RenderChartTitle()
        {
            var midX = _canvasWidth / 2 - 25;
            RenderText(midX-100, 10, midX+100, 40, $"{ChartTitle} ({Units})", JustifyText.CENTER, _paints["titleFont"]);
        }

        private void RenderLockedBarIndicator(DataPoint dataPoint, float x0, float y0, float x1, float y1)
        {

            var lockSideOffset = Math.Min(20f, BarWidthPixels/3);
            var lockTopOffset = 30f;

            RenderText(x0, y0-lockTopOffset, x1-lockSideOffset, y1, "\xe72e", JustifyText.CENTER, _paints["segoeFont"]);
        }

        private void RenderTopLabel(DataPoint dataPoint, float x0, float y0, float x1, float y1)
        {

            var nubSize = (x1 - x0) / 10f;

            // triangle nub
            var path = new SKPath();
            var midX = (x0 + x1) / 2f;
            path.AddPoly(new SKPoint[]
            {
                new SKPoint {X = midX - nubSize, Y = y1},
                new SKPoint {X = midX, Y = y1 + nubSize },
                new SKPoint {X = midX + nubSize, Y = y1},
                new SKPoint {X = midX - nubSize, Y = y1},
            });

            if (dataPoint.Locked)
            {
                _canvas.DrawRoundRect(new SKRect { Left = x0, Right = x1, Top = y0, Bottom = y1 }, 5, 5, _paints["defaultTransparent"]);
                _canvas.DrawPath(path, _paints["defaultTransparent"]);
                RenderText(x0, y0, x1, y1, "Locked", JustifyText.CENTER, _paints["whiteFont"]);
                
            }
            else
            {
                _canvas.DrawRoundRect(new SKRect { Left = x0, Right = x1, Top = y0, Bottom = y1 }, 5, 5, _paints["highlight"]);
                _canvas.DrawPath(path, _paints["highlight"]);
                RenderText(x0, y0, x1, y1, $"{dataPoint.Value} {Units}", JustifyText.CENTER, _paints["whiteFont"]);
                
            }           
        }

        enum JustifyText
        {
            LEFT,
            RIGHT,
            CENTER
        }

        private void RenderText(float x0, float y0, float x1, float y1, string label, JustifyText j, SKPaint p)
        {
            // Calculate offsets to center the text on the screen
            SKRect textBounds = new SKRect();
            _paints["whiteFont"].MeasureText(label, ref textBounds);

            float xText = 0f;
            if (j == JustifyText.CENTER)
            {
                xText = (x0 + x1) / 2 - textBounds.MidX;
            } 
            else if (j == JustifyText.LEFT)
            {
                xText = x0;
            }
            else if (j == JustifyText.RIGHT)
            {
                xText = x1 - textBounds.Width;
            }

            // always center vertically in bounding rect
            float yText = (y0 + y1) / 2 - textBounds.MidY;
            _canvas.DrawText(label, xText, yText, p);
        }

        private void RenderAxes()
        {
            var tick = 0f;
            var top = DataList.Max(_ => _.Value) + TickSize;

            var chartHasData = DataList.Any(_ => _.Value != 0);
        
            if (chartHasData)
            {
                while (tick < top && VScale.ConvertToOutputRange(tick) > 60)
                {
                    var y = VScale.ConvertToOutputRange(tick);
                    _canvas.DrawLine(40, y, _canvasWidth - 25, y, _paints["defaultTransparent"]);
                    RenderText(0, y - 25, 30, y + 25, $"{tick}", JustifyText.RIGHT, _paints["defaultFont"]);
                    tick += TickSize;
                }             
            }
            else
            {
                var y = 340;
                _canvas.DrawLine(40, y, _canvasWidth - 25, y, _paints["defaultTransparent"]);
                RenderText(0, y - 25, 30, y + 25, $"{tick}", JustifyText.RIGHT, _paints["defaultFont"]);

                var midY = y / 2;
                var midX = _canvasWidth / 2 - 25;
                RenderText(midX - 100, midY, midX + 100, midY, "No Data Entered Yet", JustifyText.CENTER, _paints["defaultFont"]);
            }                                      
        }

        public void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Moved && HighlightOnHover)
                for (int i = 0; i < DataList.Count; i++)
                {
                    var x0 = HScale.ConvertToOutputRange(i);
                    var x1 = HScale.ConvertToOutputRange(i + 0.7f);

                    if (x1 < x0)
                    {
                        (x0, x1) = (x1, x0);
                    }

                    var loc = args.Location;

                    DataPoint p = DataList[i];
                    p.IsHovered = loc.X > x0 && loc.X < x1;
                }

            InvalidateSurface();
        }

        private static void OnDataChanged(BindableObject bindable, object oldVal, object newVal)
        {
            var newValue = newVal as IEnumerable<DataPoint>;
            var barChart = bindable as BarChart;

            if (newValue is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += barChart.OnItemsSourceCollectionChanged;
            }

            barChart.InvalidateSurface();
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateSurface();
        }

        private void InitializePaints()
        {
            if (_paints == null)
                _paints = new Dictionary<string, SKPaint>();

            _paints.Clear();

            var fontSize = 24.0f;
            var tmpFont = new SKPaint() { Typeface = SKTypeface.FromFamilyName("Roboto"), TextSize = 32f };
            while (tmpFont.MeasureText("2019/10/06") > BarWidthPixels * 0.8)
            {
                fontSize--;
                tmpFont.TextSize = fontSize;
            }

            _paints.Add("default", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = DefaultColor.ToSKColor(),
                StrokeWidth = 2,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("defaultTransparent", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = DefaultColor.MultiplyAlpha(.5).ToSKColor(),
                StrokeWidth = 2,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("highlight", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = HighlightColor.ToSKColor(),
                StrokeWidth = 2,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("highlightTransparent", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = HighlightColor.MultiplyAlpha(0.8).ToSKColor(),
                StrokeWidth = 2,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("defaultFont", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = DefaultColor.ToSKColor(),
                StrokeWidth = 0,
                Typeface = SKTypeface.FromFamilyName("Roboto"),
                TextSize = fontSize,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("titleFont", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = DefaultColor.ToSKColor(),
                StrokeWidth = 1, // giving a stroke > 0 makes the font appear bold
                Typeface = SKTypeface.FromFamilyName("Roboto"),
                TextSize = 18f,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("whiteFont", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = Color.White.ToSKColor(),
                StrokeWidth = 0f,
                Typeface = SKTypeface.FromFamilyName("Roboto"),
                TextSize = fontSize,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

            _paints.Add("segoeFont", new SKPaint()
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = DefaultColor.ToSKColor(),
                StrokeWidth = 2f,
                Typeface = SKTypeface.FromFamilyName("Segoe MDL2 Assets"),
                TextSize = 30f,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            });

        }

    }
}