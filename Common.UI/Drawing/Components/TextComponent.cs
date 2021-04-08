using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiaSharp;
using TouchTracking;

namespace Common.Drawing.Components
{
    public class TextComponent:BaseComponent
    {
        public string Text
        {
            get => _text;
            set
            {
                _text = value;               
                _isInitialized = !string.IsNullOrEmpty(value);
                IsEdited = !string.IsNullOrEmpty(value);
                OnComponentActionUpdated?.Invoke();

                if (_canvas != null)
                {
                    UpdateMinimumDimensions();
                    _canvas.Invalidate();
                }
            }
        }
        private string _text;
        public float TextSize
        {
            get => _paint.TextSize;
            set
            {
                _paint.TextSize = value;
                IsEdited = true;
                if (_canvas != null)
                {
                    UpdateMinimumDimensions();
                    _canvas.Invalidate();
                }
            }
        }
        public string FontFamily
        {
            get => _paint.Typeface?.FamilyName ?? "";
            set
            {
                _paint.Typeface = SKTypeface.FromFamilyName(value, SKTypefaceStyle.Normal);
                IsEdited = true;
                if (_canvas != null)
                {
                    UpdateMinimumDimensions();
                    _canvas.Invalidate();
                }
            }
        }

        float _minimumWidth;
        float _minimumHeight;
        Line[] _lines;

        public TextComponent(Canvas canvas = null):base(canvas, ComponentType.Text)
        {
            _x = 40;
            _y = 40;
            _width = 100;
            _height = 40;

            _paint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.Black,
                Typeface = SKTypeface.FromFamilyName("Arial", SKTypefaceStyle.Normal),
                StrokeWidth = 1,
                TextSize = 42,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            };

            UpdateMinimumDimensions();
        }

        public override void CopyOptions(BaseComponent source)
        {
            base.CopyOptions(source);

            if (!(source is TextComponent))
                return;

            TextSize = (source as TextComponent).TextSize;
            FontFamily = (source as TextComponent).FontFamily;
        }

        public override void CopyComponent(BaseComponent source)
        {
            base.CopyComponent(source);

            if (!(source is TextComponent))
                return;

            TextSize = (source as TextComponent).TextSize;
            FontFamily = (source as TextComponent).FontFamily;
            Text = (source as TextComponent).Text;
        }

        public override void Initialize(Canvas canvas)
        {
            base.Initialize(canvas);

            UpdateMinimumDimensions();
        }

        protected override void HandleDraw(SKCanvas canvas)
        {
            if (string.IsNullOrWhiteSpace(_text) || _lines.Count() == 0)
                return;

            var height = _lines.Count() * _minimumHeight;

            var area = _rect;

            var y = area.MidY - height / 2;

            foreach (var line in _lines)
            {
                y += _minimumHeight;
                var x = area.MidX - line.Width / 2;
                if (IsVisible)
                {
                    canvas.DrawText(line.Value, x, y, _paint);
                }
                else
                {
                    canvas.DrawText(line.Value, x, y, _emptyPaint);
                }
                
            }
        }

        protected override void HandleScale()
        {
            UpdateMinimumDimensions(true);
        }

        private void UpdateMinimumDimensions(bool keepHeightMeasurement = false)
        {
            if (_paint == null)
                return;

            SKRect bounds = SKRect.Empty;

            // This reference text allows us to compute reasonable dimensions and line-wrapping defaults
            // for a text box using the current font/size/weight settings specified in _paint.
            _paint.MeasureText("REFERENCE_TEXT", ref bounds);

            _minimumWidth = Math.Max(bounds.Width, 20);
            _minimumHeight = Math.Max(bounds.Height, 10);

            _width = Math.Max(_minimumWidth, _width);

            _lines = SplitLines(Text, _width);

            if(keepHeightMeasurement)
            {
                _height = Math.Max(_minimumHeight, _height);
            }
            else
            {
                _height = _minimumHeight;
            }
            
            _height = Math.Max(_lines.Count() * _minimumHeight, _height);
        }

        public class Line
        {
            public string Value { get; set; }
            public float Width { get; set; }
        }

        private Line[] SplitLines(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return new Line[] { };

            var spaceWidth = _paint.MeasureText(" ");
            var lines = text.Split('\n');

            return lines.SelectMany((line) =>
            {
                var result = new List<Line>();

                var words = line.Split(new[] { " " }, StringSplitOptions.None);

                var lineResult = new StringBuilder();
                float width = 0;
                foreach (var word in words)
                {
                    var wordWidth = _paint.MeasureText(word);
                    var wordWithSpaceWidth = wordWidth + spaceWidth;
                    var wordWithSpace = word + " ";

                    if (width + wordWidth > maxWidth)
                    {
                        result.Add(new Line() { Value = lineResult.ToString(), Width = width });
                        lineResult = new StringBuilder(wordWithSpace);
                        width = wordWithSpaceWidth;
                    }
                    else
                    {
                        lineResult.Append(wordWithSpace);
                        width += wordWithSpaceWidth;
                    }
                }

                result.Add(new Line() { Value = lineResult.ToString(), Width = width });

                return result.ToArray();
            }).ToArray();
        }
    }
}
