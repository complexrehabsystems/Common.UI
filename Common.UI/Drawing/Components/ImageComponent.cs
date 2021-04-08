using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TouchTracking;

namespace Common.Drawing.Components
{
    public class ImageComponent : BaseComponent
    {
        public string Base64ImageData
        {
            get => _base64ImageData;
            set => _base64ImageData = value;
        }
        public bool LockAspectRatio
        {
            get => _lockAspectRatio;
            set
            {
                _lockAspectRatio = value;

                if (_lockAspectRatio)
                {
                    AspectScale();
                    _canvas?.Invalidate();
                }
            }
        }
        
        protected SKBitmap _bitmap;
        protected string _url;
        protected string _base64ImageData;
        protected bool _lockAspectRatio;

        public ImageComponent(Canvas canvas = null): base(canvas, ComponentType.Image)
        {
            _lockAspectRatio = true;
            _bitmap = null;
        }

        public override void Initialize(Canvas canvas)
        {
            base.Initialize(canvas);

            if (string.IsNullOrEmpty(Base64ImageData))
                return;

            LoadSavedImage();
        }

        public void LoadSavedImage()
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                ToMemoryStream(_base64ImageData, memStream);

                using (SKManagedStream skStream = new SKManagedStream(memStream))
                {
                    _bitmap = SKBitmap.Decode(skStream);
                    _isInitialized = true;
                    IsEdited = true;
                }
            }
        }

        public async Task LoadStream(Stream stream)
        {
            _bitmap = null;
            _isInitialized = false;
            IsEdited = true;
            _canvas.Invalidate();

            using (MemoryStream memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                _base64ImageData = ToBase64Image(memStream);

                using (SKManagedStream skStream = new SKManagedStream(memStream))
                {
                    _bitmap = SKBitmap.Decode(skStream);
                    
                    ResetScale(false);
                    OnComponentActionUpdated?.Invoke();
                }
            }

            _canvas.InvokeInvalidate();
        }

        public void ResetScale(bool invalidate = true)
        {
            if (_bitmap == null || _canvas == null)
                return;

            var padding = 10;
            var maxWidth = _canvas.Width - (padding * 2);
            var maxHeight = _canvas.Height - (padding * 2);

            _width = _bitmap.Width;
            _height = _bitmap.Height;
            _isInitialized = true;
            IsEdited = true;

            // if the image is larger than the canvas let's scale it right down but keep the aspect ratio
            if (_width > maxWidth)
            {
                _width = maxWidth;
                _height = maxWidth * _bitmap.Height / _bitmap.Width;
            }
            if (_height > maxHeight)
            {
                _width = maxHeight * _bitmap.Width / _bitmap.Height;
                _height = maxHeight;
            }

            //reposition the image to be completely visible
            var overflow = (_x + _width) - (maxWidth + padding/*left padding*/);
            if (overflow > 0)
                _x -= overflow;

            overflow = (_y + _height) - (maxHeight + padding/*top padding*/);
            if (overflow > 0)
                _y -= overflow;

            //check left side overflow
            _x = Math.Max(_x, padding);
            _y = Math.Max(_y, padding);

            if (invalidate)
                _canvas.Invalidate();
        }

        public async Task LoadUrl(string url)
        {
            _url = url;
            _bitmap = null;
            _isInitialized = false;
            IsEdited = true;
            _canvas.Invalidate();

            Uri uri = new Uri(url);
            WebRequest request = WebRequest.Create(uri);

            var response = await request.GetResponseAsync();

            using (Stream stream = response.GetResponseStream())
            {
                await LoadStream(stream);
            }
          
        }

        protected string ToBase64Image(MemoryStream stream)
        {
            var bytes = new byte[stream.Length];

            stream.Read(bytes, 0, (int)stream.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return Convert.ToBase64String(bytes);
        }

        protected void ToMemoryStream(string base64Image, MemoryStream stream)
        {
            var bytes = Convert.FromBase64String(base64Image);

            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
        }

        protected override void HandleDraw(SKCanvas canvas)
        {
            if (_bitmap == null)
            {
                return;
            }

            if (IsVisible)
            {
                //if image is added before canvas is ready, reset it to proper scale when drawing
                if (_height < 0 || _width < 0)
                {
                    ResetScale();
                }
                canvas.DrawBitmap(_bitmap, new SKRect(_x, _y, _x + _width, _y + _height));
            }
            else
            {
                canvas.DrawBitmap(_bitmap, new SKRect(0,0,0,0));
            }
                 
        }

        protected override void HandleScale()
        {
            if (!IsInitialized || !IsCurrent)
                return;

            AspectScale();
            
        }

        protected void AspectScale()
        {
            if (!_lockAspectRatio || _bitmap == null)
                return;

            // aspect scale based on the dominant dimension
            if (_bitmap.Width > _bitmap.Height)
            {
                _height = _width * _bitmap.Height / _bitmap.Width;
            }
            else
            {
                _width = _height * _bitmap.Width / _bitmap.Height;
            }
        }

        public override void CopyComponent(BaseComponent source)
        {
            base.CopyComponent(source);

            _bitmap = (source as ImageComponent)._bitmap;
            _base64ImageData = (source as ImageComponent)._base64ImageData;
            _lockAspectRatio = (source as ImageComponent)._lockAspectRatio;
        }
    }
}
