using System;
using Common.UI.Drawing.Touch;
using Newtonsoft.Json;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Common.UI.Drawing.Components
{
    public abstract class BaseComponent
    {
        public byte ColorR
        {
            get => _paint?.Color.Red ?? 0;
            set => _paint.Color = Xamarin.Forms.Color.FromRgba(value, _paint.Color.Green, _paint.Color.Blue, _paint.Color.Alpha).ToSKColor();
        }
        public byte ColorG
        {
            get => _paint?.Color.Green ?? 0;
            set => _paint.Color = Xamarin.Forms.Color.FromRgba(_paint.Color.Red, value, _paint.Color.Blue, _paint.Color.Alpha).ToSKColor();
        }
        public byte ColorB
        {
            get => _paint?.Color.Blue ?? 0;
            set => _paint.Color = Xamarin.Forms.Color.FromRgba(_paint.Color.Red, _paint.Color.Green, value, _paint.Color.Alpha).ToSKColor();
        }
        public byte ColorA
        {
            get => _paint?.Color.Alpha ?? 0;
            set => _paint.Color = Xamarin.Forms.Color.FromRgba(_paint.Color.Red, _paint.Color.Green, _paint.Color.Blue, value).ToSKColor();
        }
        public float StrokeWidth
        {
            get => _paint.StrokeWidth;
            set
            {
                _paint.StrokeWidth = value;
                IsEdited = true;
            }
        }
        public float X
        {
            get => _x;
            set => _x = value;

        }
        public float Y
        {
            get => _y;
            set => _y = value;
        }
        public float Width
        {
            get => _width;
            set => _width = value;

        }
        public float Height
        {
            get => _height;
            set => _height = value;
        }

        public BaseComponent PreviousVersion { get; set; }

        public bool IsEdited { get; set; }

        public int DisplayOrder { get; set; }

        public enum ComponentType
        {
            Ink,
            Text,
            Image,
            Unknown
        }

        [JsonIgnore]
        public ComponentType Type => _type;
        [JsonIgnore]
        public bool IsInitialized => _isInitialized;
        [JsonIgnore]
        public bool IsCurrent => _isCurrent;
        [JsonIgnore]
        public bool AllowsScaling => _allowScaling;
        [JsonIgnore]
        public Canvas Canvas
        {
            get => _canvas;
            set => _canvas = value;
        }
        [JsonIgnore]
        public Color Color
        {
            get => _paint?.Color.ToFormsColor() ?? Color.Black;
            set
            {
                IsEdited = true;
                _paint.Color = value.ToSKColor();
            }
        }   
        
        [JsonIgnore]
        public Action<Point> OnComponentActionCompleted { get; set; }

        [JsonIgnore]
        public Action OnComponentActionUpdated { get; set; }

        protected Canvas _canvas;
        protected SKPaint _paint;
        protected ComponentType _type;
        protected bool _isInitialized; // this is a check if the compenent has been set up and is drawing something
        protected float _x;
        protected float _y;
        protected float _width;
        protected float _height;
        protected SKPaint _emptyPaint;

        protected SKRect _rect => SKRect.Create(_x, _y, _width, _height);

        // bounding box
        protected bool _allowScaling;
        protected bool _drawBoundingBox;
        protected bool _isCurrent;
        protected SKPaint boundingPaint;
        protected SKPaint buttonPaint;
        protected SKPaint textPaint;
        protected bool _isTranslating;
        protected bool _isScaling;
        protected SKPoint _startPoint;
        protected const float buttonRadius = 14f;

        protected SKPoint ConvertToPixel(Point pt) => _canvas.ConvertToPixel(pt);

        public bool IsVisible { get; set; }

        public BaseComponent(Canvas canvas = null, ComponentType type = ComponentType.Unknown, bool drawBoundingBox = true)
        {
            _canvas = canvas;
            _type = type;
            _drawBoundingBox = drawBoundingBox;

            _x = _y = 10;
            _width = _height = 100;
            _isInitialized = false;
            _isCurrent = true;
            _allowScaling = true;

            // default
            _paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 4,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            };

            _emptyPaint = new SKPaint
            {
                Color = SKColors.Transparent,
                StrokeWidth = 0
            };

            IsVisible = true;
        }

        public static BaseComponent Create(ComponentType type, Canvas canvas)
        {
            switch(type)
            {
                case ComponentType.Ink:
                    return new InkComponent(canvas);
                case ComponentType.Image:
                    return new ImageComponent(canvas);
                case ComponentType.Text:
                    return new TextComponent(canvas);
                default:
                    throw new Exception("Create Undefined Drawing component");
            }
        }

        public virtual void CopyOptions(BaseComponent source)
        {
            if (source == null)
                return;

            Color = source.Color;
            StrokeWidth = source.StrokeWidth;
        }

        public virtual void CopyComponent(BaseComponent source)
        {
            if (source == null)
                return;

            SetLocation(source.X, source.Y);

            Color = source.Color;
            StrokeWidth = source.StrokeWidth;
            Height = source.Height;
            Width = source.Width;
        }

        public virtual void Initialize(Canvas canvas)
        {
            _canvas = canvas;
            _isInitialized = true;
            _isCurrent = false;
        }

        public void SetItemSelected()
        {
            _drawBoundingBox = true;
            _isCurrent = true;
            _isInitialized = true;
        }

        public void SetCurrent(bool current) => _isCurrent = current;

        public void SetLocation(float x, float y, float width = -1, float height = -1)
        {
            _x = x;
            _y = y;
            _width = width > 0 ? width : _width;
            _height = height > 0 ? height : _height;
            HandleScale();
            _canvas.Invalidate();
        }

        public virtual void Complete()
        {
            _isInitialized = false;
            _isCurrent = false;
            _isTranslating = false;
            _isScaling = false;
            IsEdited = false;

            boundingPaint = null;
            buttonPaint = null;
            textPaint = null;
        }

        public virtual SKRect GetBoundingBox() => new SKRect(_x, _y, _x + _width, _y + _height);

        public virtual SKRect GetScaleBox() => new SKRect(
                            _x + _width - buttonRadius,
                            _y + _height - buttonRadius,
                            _x + _width + buttonRadius,
                            _y + _height + buttonRadius);

        public void OnTouch(TouchActionEventArgs args)
        {
            if (_drawBoundingBox && _isCurrent && _isInitialized)
            {
                switch (args.Type)
                {
                    case TouchActionType.Pressed:
                        _startPoint = ConvertToPixel(args.Location);

                        SKRect box = GetBoundingBox();
                        SKRect scaleButton = GetScaleBox();

                        if(_allowScaling && scaleButton.Contains(_startPoint))
                        {
                            _isScaling = true;
                        }
                        else if(box.Contains(_startPoint))
                        {
                            _isTranslating = true;
                        }
                        break;

                    case TouchActionType.Moved:
                        var point = ConvertToPixel(args.Location);

                        if (point.X <= 0 || point.Y <= 0 || point.X >= _canvas.Width || point.Y >= _canvas.Height)
                            return;

                        var diff = point - _startPoint;

                        if (_isTranslating)
                        {                            
                            _x = _x + diff.X;
                            _y = _y + diff.Y;
                            HandleTranslate(diff.X, diff.Y);
                            
                        }
                        else if(_isScaling)
                        {
                            _width = Math.Max(_width + diff.X, 50);
                            _height = Math.Max(_height + diff.Y, 50);
                            HandleScale();
                        }
                        else
                        {
                            return;
                        }

                        IsEdited = true;

                        _startPoint = point;
                        _canvas.Invalidate();
                        break;

                    case TouchActionType.Released:
                    case TouchActionType.Cancelled:
                        if (!_isScaling && !_isTranslating)
                        {
                            HandleTouch(args);
                            return;
                        }

                        _isScaling = false;
                        _isTranslating = false;
                        OnComponentActionUpdated?.Invoke();
                        break;
                }
            }
            else
            {
                HandleTouch(args);
            }
            
        }

        public void OnDraw(SKCanvas canvas)
        {
            HandleDraw(canvas);

            if (_drawBoundingBox && _isCurrent)
                DrawBoundingBox(canvas);           
        }
        

        protected abstract void HandleDraw(SKCanvas canvas);
        protected virtual void HandleTouch(TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Released)
                OnComponentActionCompleted?.Invoke(args.Location);
        }
        protected virtual void HandleScale() { }

        protected virtual void HandleTranslate(float translateX, float translateY) { }

        protected void DrawBoundingBox(SKCanvas canvas)
        {
            SKPath path = new SKPath();

            path.MoveTo(_x, _y);
            path.LineTo(_x + _width, _y);
            path.LineTo(_x + _width, _y + _height);
            path.LineTo(_x, _y + _height);
            path.LineTo(_x, _y);

            if (boundingPaint == null)
            {
                boundingPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 1,
                    StrokeCap = SKStrokeCap.Butt,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4, 4, 4 }, 2)
                };
            }

            canvas.DrawPath(path, boundingPaint);

            if (_allowScaling == false)
                return;

            if(buttonPaint == null)
            {
                buttonPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = Color.FromRgba(1.0, 1.0, 1.0, 0.6).ToSKColor(),
                    IsAntialias = true
                };
            }

            if (textPaint == null)
            {
                textPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = 1,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round,
                    IsAntialias = true,
                    TextSize = 14,
                    Typeface = SKTypeface.FromFamilyName("Segoe MDL2 Assets", SKTypefaceStyle.Normal)
                };
            }

            canvas.DrawCircle(_x + _width, _y + _height, buttonRadius, buttonPaint);
            canvas.DrawCircle(_x + _width, _y + _height, buttonRadius, textPaint);


            SKRect textBounds = SKRect.Empty;
            textPaint.MeasureText("\uE73F", ref textBounds);

            canvas.Save();
            canvas.RotateDegrees(90, _x + _width, _y + _height);
            canvas.DrawText("\uE73F", _x + _width - textBounds.MidX, _y + _height - textBounds.MidY, textPaint);
            canvas.Restore();

        }

        public virtual bool Contains(SKPoint p) => GetBoundingBox().Contains(p);

        public virtual void Hide()
        {
            IsVisible = false;
        }

        public virtual void Show()
        {
            IsVisible = true;
        }
    }
}
