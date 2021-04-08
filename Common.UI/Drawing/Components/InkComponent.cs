using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchTracking;

namespace CrsCommon.Drawing.Components
{
    public class InkComponent : BaseComponent
    {
        public string Paths
        {
            get
            {
                var total = new StringBuilder();
                for(var x=0; x<_completedPaths.Count; x++)
                {
                    if (x != 0)
                        total.Append(";");

                    var path = _completedPaths[x];

                    for(var y=0; y<path.Points.Length; y++)
                    {
                        if (y != 0)
                            total.Append(",");

                        var point = path.Points[y];

                        total.Append($"{point.X},{point.Y}");
                    }
                }

                return total.ToString();
            }
            set
            {
                _completedPaths.Clear();
                if (string.IsNullOrEmpty(value))
                    return;

                var paths = value.Split(';');

                foreach(var path in paths)
                {
                    var points = path.Split(',');

                    SKPath p = new SKPath();
                    for(var i=0; i<points.Length; i += 2)
                    {
                        var pointX = float.Parse(points[i]);
                        var pointY = float.Parse(points[i + 1]);

                        if(i == 0)
                        {
                            p.MoveTo(new SKPoint(pointX, pointY));
                        }
                        else
                        {
                            p.LineTo(new SKPoint(pointX, pointY));
                        }
                    }
                    _completedPaths.Add(p);
                }
            }
        }

        Dictionary<long, SKPath> _inProgressPaths = new Dictionary<long, SKPath>();
        List<SKPath> _completedPaths = new List<SKPath>();

        public InkComponent(Canvas canvas = null):base(canvas, ComponentType.Ink, false)
        {
            _allowScaling = false;
        }

        protected override void HandleTouch(TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(ConvertToPixel(args.Location));
                        _inProgressPaths.Add(args.Id, path);
                        _isInitialized = true;
                        _canvas.Invalidate();
                    }
                    break;

                case TouchActionType.Moved:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = _inProgressPaths[args.Id];
                        path.LineTo(ConvertToPixel(args.Location));
                        _canvas.Invalidate();
                    }
                    break;

                case TouchActionType.Released:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _completedPaths.Add(_inProgressPaths[args.Id]);
                        _inProgressPaths.Remove(args.Id);
                        CalculateExtents();
                        _canvas.Invalidate();

                        OnComponentActionCompleted?.Invoke(args.Location);
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _inProgressPaths.Remove(args.Id);
                        _canvas.Invalidate();
                    }
                    break;
            }
        }

        protected override void HandleTranslate(float translateX, float translateY)
        {
            _completedPaths.ForEach(p =>
            {
                p.Transform(SKMatrix.MakeTranslation(translateX, translateY));
            });
        }

        private void CalculateExtents()
        {
            if (!IsInitialized)
                return;

            float minX = 10000f;
            float minY = 10000f;
            float maxX = 0f;
            float maxY = 0f;

            Action<SKPath> CalcValues = delegate(SKPath path)
            {
                var bounds = path.ComputeTightBounds();
                minX = Math.Min(bounds.Left, minX);
                minY = Math.Min(bounds.Top, minY);
                maxX = Math.Max(bounds.Right, maxX);
                maxY = Math.Max(bounds.Bottom, maxY);
            };

            foreach(var kvp in _inProgressPaths)
            {
                CalcValues(kvp.Value);
            }

            foreach(var p in _completedPaths)
            {
                CalcValues(p);
            }

            var stroke = (float)Math.Ceiling((float)StrokeWidth / 2.0f);

            // also give some space for the strokewidth
            _x = minX - stroke;
            _y = minY - stroke;
            _width = maxX - minX + StrokeWidth;
            _height = maxY - minY + StrokeWidth;
        }

        protected override void HandleDraw(SKCanvas canvas)
        {     
            
            if (IsVisible)
            {
                foreach(SKPath path in _completedPaths)
                {
                    canvas.DrawPath(path, _paint);
                }
            }

            else
            {
                //component is hidden, so make it transparent
                foreach (SKPath path in _completedPaths)
                {
                    canvas.DrawPath(path, _emptyPaint);
                }
            }
            

            foreach (SKPath path in _inProgressPaths.Values)
            {
                canvas.DrawPath(path, _paint);
            }           
        }

        public override bool Contains(SKPoint c)
        {
            var tolerance = 5f; //5-pixel tolerance

            foreach (var path in _completedPaths)
            {
                for (int i = 1; i < path.PointCount; i++)
                {
                    var a = path[i - 1];
                    var b = path[i];

                    //Box with tolerance padding to ensure we are near the line segment
                    var box = new SKRect
                    {
                        Left = Math.Min(a.X, b.X) - tolerance,
                        Right = Math.Max(a.X, b.X) + tolerance,
                        Top = Math.Min(a.Y, b.Y) - tolerance,
                        Bottom = Math.Max(a.Y, b.Y) + tolerance
                    };

                    if (!box.Contains(c))
                        continue;//outside of the bounding box + tolerance of this line segment

                    //https://stackoverflow.com/questions/910882/how-can-i-tell-if-a-point-is-nearby-a-certain-line
                    var quotient = (b.X - a.X) * (a.Y - c.Y) - (a.X - c.X) * (b.Y - a.Y);
                    var divisor = Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
                    var distanceFromLine = quotient / divisor;

                    if (Math.Abs(distanceFromLine) > tolerance)
                        continue;

                    return true;
                }
            }
            return false;
        }

        public override void CopyComponent(BaseComponent source)
        {
            base.CopyComponent(source);

            if (!(source is InkComponent))
                return;

            Paths = (source as InkComponent).Paths;
        }
    }
}
