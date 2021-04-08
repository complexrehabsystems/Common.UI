using CrsCommon.Drawing.Components;
using Newtonsoft.Json;
using PropertyChanged;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchTracking;
using Xamarin.Forms;

namespace CrsCommon.Drawing
{
    public class DrawingEngine : INotifyPropertyChanged
    {
        public Action ComponentSelected;
        public Action ComponentUpdated;

        private Color _activeColor = Color.Black;

        public Color ActiveColor
        {
            get
            {
                return _activeColor;
            }
            set
            {
                _activeColor = value;
                if (_currentComponent != null)
                {
                    _currentComponent.Color = value;
                    _canvas.Invalidate();
                }
                else if (_selectedComponent != null)
                {
                    _selectedComponent.Color = value;
                    _canvas.Invalidate();
                }

                OnPropertyChanged(new PropertyChangedEventArgs("ActiveColor"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }

        public float ActiveStrokeWidth
        {
            get
            {
                if (!GetActiveComponent(out var component))
                    return 0f;

                return component.StrokeWidth;
            }
            set
            {
                if (!GetActiveComponent(out var component))
                    return;

                component.StrokeWidth = value;
                _canvas.Invalidate();
            }
        }


        public BaseComponent.ComponentType ActiveComponent
        {
            get
            {
                if (!GetActiveComponent(out var component))
                    return BaseComponent.ComponentType.Unknown;

                return component.Type;
            }
        }

        public bool IsActiveComponentInitialized
        {
            get
            {
                if (_selectedComponent != null)
                {
                    return true;
                }
                else if (_currentComponent != null && _currentComponent.IsInitialized)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsDirty { get; set; }

        public DrawingEngine(SKCanvasView canvas)
        {
            _canvas = new Canvas(canvas);
            _components = new List<BaseComponent>();
            _currentComponent = null;
            _selectedComponent = null;
        }

        public void BeginRefresh() => _refreshing = true;

        Canvas _canvas;
        bool _refreshing;
        bool _selectorMode;
        bool _saveSurface;

        SKData _thumbnailData;
        int _thumbnailSize;
        TaskCompletionSource<bool> thumbnailCompletion;
        BaseComponent _currentComponent;
        BaseComponent _selectedComponent;
        List<BaseComponent> _components;

        public void EndRefresh()
        {
            _refreshing = false;
            _canvas.Invalidate();
        }

        public void AddComponent(BaseComponent.ComponentType type, Point? location)
        {
            _selectorMode = false;

            ClearComponent();

            _currentComponent = BaseComponent.Create(type, _canvas);

            _currentComponent.OnComponentActionCompleted = OnComponentActionCompleted;
            _currentComponent.OnComponentActionUpdated = OnComponentActionUpdated;

            // now try and see what the last component of this type has for options and copy it over
            var lastComponent = FindLastComponent(type);

            _currentComponent.CopyOptions(lastComponent);
            _currentComponent.Color = ActiveColor;
            if (location != null)
                _currentComponent.SetLocation((float)location.Value.X, (float)location.Value.Y);

            var currrentHighestDisplayPriority = _components?.OrderByDescending(c => c.DisplayOrder).FirstOrDefault()?.DisplayOrder;

            _currentComponent.DisplayOrder = (currrentHighestDisplayPriority ?? 0) + 1;

            ComponentSelected?.Invoke();

            _canvas.Invalidate();
        }

        public BaseComponent CreateNewVersionOfComponent(BaseComponent previous)
        {
            previous.Hide();

            _selectorMode = false;

            ClearComponent();

            BaseComponent newVersion = BaseComponent.Create(previous.Type, _canvas);
            newVersion.PreviousVersion = previous;
            newVersion.DisplayOrder = previous.DisplayOrder;

            newVersion.CopyComponent(previous);

            _components.Add(newVersion);
            newVersion.IsEdited = false;

            _currentComponent = newVersion;
            _currentComponent.OnComponentActionCompleted = OnComponentActionCompleted;
            _currentComponent.OnComponentActionUpdated = OnComponentActionUpdated;            

            ComponentSelected?.Invoke();

            _canvas.Invalidate();
           
            return newVersion;

        }

        public List<string> GetFontList()
        {
            var fonts = SKFontManager.Default;

            return fonts.GetFontFamilies().OrderBy(x => x).ToList();
        }

        private BaseComponent FindLastComponent(BaseComponent.ComponentType type)
        {
            for(var i=_components.Count-1; i >= 0; i--)
            {
                var c = _components[i];
                if (c.Type == type)
                    return c;
            }

            return null;
        }

        public void SetLocation(float x, float y, float width = -1, float height = -1)
        {
            if (!GetActiveComponent(out var component))
                return;

            IsDirty = true;
            component.SetLocation(x, y, width, height);
        }

        public void SetSelectorMode()
        {
            _selectorMode = true;

            ClearComponent();
        }

        public bool DeleteComponent()
        {
            if (_selectedComponent != null)
            {
                _components.Remove(_selectedComponent);

                //also need to delete all previous versions of the component being deleted
                var previousVersion = _selectedComponent.PreviousVersion;
                while (previousVersion != null)
                {
                    _components.Remove(previousVersion);

                    previousVersion = previousVersion.PreviousVersion;
                }
                
            }
            else if (_currentComponent != null && _currentComponent.IsInitialized)
            {
                _currentComponent = null;
            }
            else
            {
                return false;
            }

            IsDirty = true;
            ClearComponent();
            _canvas.Invalidate();

            SetSelectorMode();

            return true;
        }

        public void BringToFront()
        {
            if (_currentComponent != null)
            {
                // it is the front already
            }

            if (!GetActiveComponent(out var component))
                return;
           
            var highestDisplayPriority = _components?.OrderByDescending(c => c.DisplayOrder).FirstOrDefault()?.DisplayOrder;

            if (component.DisplayOrder >= highestDisplayPriority)
                return;

            IsDirty = true;

            component.DisplayOrder = (highestDisplayPriority + 1) ?? 1;

            _canvas.Invalidate();
        }

        public void SendToBack()
        {
            if(_currentComponent != null && _currentComponent.IsInitialized == false)
            {
                // special case (mostly for ink control because each new path is a new component)
                ClearComponent();

                if (_components.Count == 0)
                    return;

                var saved = _components.Last();
                _selectorMode = true;
                _selectedComponent = saved;
                saved.SetItemSelected();
            }
            else if(_currentComponent != null)
            {
                var saved = _currentComponent;
                ClearComponent();

                _selectorMode = true;
                _selectedComponent = saved;
                saved.SetItemSelected();
            }

            if (!GetActiveComponent(out var component))
                return;

            if (component?.DisplayOrder <= 1)
                return;

            IsDirty = true;

            component.DisplayOrder = 1;

            foreach (var c in _components)
            {
                c.DisplayOrder++;
            }

            _canvas.Invalidate();

        }

        private void ClearComponent()
        {
            bool componentEdited = _currentComponent?.IsEdited ?? true;

            if (_selectedComponent != null)
            {
                _selectedComponent.Complete();
            }

            if (_currentComponent != null && _currentComponent.IsInitialized)
            {
                _currentComponent.Complete();
                _components.Add(_currentComponent);
            }     
            
            //case for if the user doesn't edit a component that they select
            else if (!componentEdited && _currentComponent.PreviousVersion != null)
            {
                _currentComponent.PreviousVersion.Show();
                _components.Remove(_currentComponent);
            }

            if (_currentComponent != null)
                _currentComponent.IsEdited = false;

            _currentComponent = null;
            _selectedComponent = null;
        }

        public void DeleteLastComponent()
        {
            _canvas.Invalidate();

            SetSelectorMode();

            if (_components != null && _components.Count > 0)
            {
                var componentToDelete = _components[_components.Count - 1];

                if (componentToDelete.PreviousVersion != null)
                {
                    componentToDelete.PreviousVersion.Show();
                }

                _components.Remove(componentToDelete);

                IsDirty = true;
            }           
        }

        public async Task SetImageUrl(string url)
        {
            if (!GetActiveComponent(out var component, BaseComponent.ComponentType.Image))
                return;

            IsDirty = true;
            await (component as ImageComponent).LoadUrl(url);
        }

        public async Task SetImageStream(Stream stream)
        {
            if (!GetActiveComponent(out var component, BaseComponent.ComponentType.Image))
                return;

            IsDirty = true;
            await (component as ImageComponent).LoadStream(stream);
        }

        public void SetTextEntry(string text)
        {
            if (!GetActiveComponent(out var component, BaseComponent.ComponentType.Text))
                return;

            IsDirty = true;
            (component as TextComponent).Text = text;
        }

        public void SetTextFontFamily(string familyName)
        {
            if (!GetActiveComponent(out var component, BaseComponent.ComponentType.Text))
                return;

            IsDirty = true;
            (component as TextComponent).FontFamily = familyName;
        }

        public void SetTextFontSize(float fontSize)
        {
            if (!GetActiveComponent(out var component, BaseComponent.ComponentType.Text))
                return;

            IsDirty = true;
            (component as TextComponent).TextSize = fontSize;
        }

        private bool GetActiveComponent(out BaseComponent component, BaseComponent.ComponentType? restrictedType = null)
        {
            component = null;
            if (_currentComponent != null)
            {
                component = _currentComponent;
            }
            else if (_selectedComponent != null)
            {
                component = _selectedComponent;
            }

            if (component == null)
                return false;

            if (restrictedType.HasValue && component.Type != restrictedType)
                component = null;

            return component != null ? true : false;
        }

        public T GetActiveComponent<T>() where T : BaseComponent
        {
            if (!GetActiveComponent(out var component) || !(component is T))
                return null;

            return component as T;
        }

        public void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Pressed)
            {
                IsDirty = true;

                if (_currentComponent is InkComponent)
                {
                    //make sure last selected ink component is cleared when drawing new one
                    _selectedComponent = null;
                }
            }
                            
            if (_selectorMode && args.Type == TouchActionType.Pressed)
            {
                SKPoint point = _canvas.ConvertToPixel(args.Location);

                // if we have a selected component and we want to do a scale then do the check here
                if(_selectedComponent != null && _selectedComponent.AllowsScaling)
                {
                    if(_selectedComponent.GetScaleBox().Contains(point))
                    {
                        _selectedComponent.OnTouch(args);
                        _canvas.Invalidate();
                        return;
                    }
                }

                var previousSelectedComponent = _selectedComponent;

                _selectedComponent = null;

                // clear our selected items
                _components.ForEach(c =>
                {
                    c.Complete();
                });

                var reversedComponents = _components.OrderByDescending(c => c.DisplayOrder);
                // reverse for z order
                foreach (var component in reversedComponents)
                {

                    if(component.Contains(point) && component.IsVisible)
                    {
                        var c = CreateNewVersionOfComponent(component);                        

                        _selectedComponent = c;
                        ActiveColor = _selectedComponent.Color;
                        c.SetItemSelected();
                        c.OnTouch(args);
                        
                        // send the event only if there is a new selected item
                        if(previousSelectedComponent != _selectedComponent)
                            ComponentSelected?.Invoke();                        

                        _canvas.Invalidate();
                        return;
                    }
                }

                ComponentSelected?.Invoke();
                _canvas.Invalidate(); 

            }
            else if (_selectedComponent != null)
            {
                _selectedComponent?.OnTouch(args);
            }
            else
            {
                _currentComponent?.OnTouch(args);

                //enable deletion of ink component while drawing, without jumping into selector mode
                if (args.Type == TouchActionType.Released && _currentComponent is InkComponent)
                {
                    var lastDrawnComponent = FindLastComponent(BaseComponent.ComponentType.Ink);

                    _selectedComponent = lastDrawnComponent;
                    ComponentSelected?.Invoke();
                    _canvas.Invalidate();
                }
            }  
        }

        public void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (_refreshing)
                return;

            SKCanvas canvas = args.Surface.Canvas;
           
            canvas.Clear(SKColors.White);

            float xBounds = 0;
            float yBounds = 0;

            foreach (var c in _components)
            {
                //only make scaling decisions based on visible components
                if(c.IsVisible)
                {
                    if ((c.X + c.Width) > xBounds)
                    {
                        xBounds = c.X + c.Width;
                    }

                    if ((c.Y + c.Height) > yBounds)
                    {
                        yBounds = c.Y + c.Height;
                    }
                }              
            }

            float canvasScalarX = 1;
            float canvasScalarY = 1;

            //only start scaling down if the edge of the canvas reaches the components
            if (xBounds > canvas.DeviceClipBounds.Width)
            {
                canvasScalarX = canvas.DeviceClipBounds.Width / xBounds;
            }

            if (yBounds > canvas.DeviceClipBounds.Height)
            {
                canvasScalarY = canvas.DeviceClipBounds.Height / yBounds;
            }

            //maintain aspect ratio
            var scale = Math.Min(canvasScalarX, canvasScalarY);

            _canvas.WidthScale = scale;
            _canvas.HeightScale = scale;

            canvas.Scale(scale);

            
            //paint components to surface in their display order
            var componentsOrdered = _components.OrderBy(c => c.DisplayOrder);          
            foreach (var c in componentsOrdered)
            {
                c.OnDraw(canvas);
            }           

            //if the _currentComponent was merely selected, it gets drawn in the above loop,
            //but if it was newly created, it's not part of the components yet, but should be drawn still
            if (!_components.Contains(_currentComponent))
            {
                _currentComponent?.OnDraw(canvas);
            }

            if (_saveSurface)
            {
                _saveSurface = false;

                using (var image = args.Surface.Snapshot())
                {
                    if (_thumbnailSize <= 0)
                    {
                        _thumbnailData = image.Encode(SKEncodedImageFormat.Png, 100);
                    }
                    else
                    {
                        //square the image
                        var minLength = Math.Min(image.Width, image.Height);
                        var rect = SKRectI.Create((image.Width - minLength) / 2, (image.Height - minLength) / 2, minLength, minLength);
                        using (var subImage = image.Subset(rect))
                        {
                            using (var bitmap = SKBitmap.FromImage(subImage))
                            {
                                using (var sizedBitmap = bitmap.Resize(new SKImageInfo(_thumbnailSize, _thumbnailSize), SKBitmapResizeMethod.Lanczos3))
                                {
                                    using (var img = SKImage.FromBitmap(sizedBitmap))
                                    {
                                        _thumbnailData = image.Encode(SKEncodedImageFormat.Png, 100);                                        
                                    }
                                }
                                    
                            }
                        }
                        
                    }                    
                }
                thumbnailCompletion.SetResult(true);
            }
        }

        protected void OnComponentActionCompleted(Point location)
        {
            BaseComponent component;
            if (GetActiveComponent(out component))
            {
                var componentType = component.Type;

                ClearComponent();

                AddComponent(componentType, location);
            }
        }

        protected void OnComponentActionUpdated()
        {
            ComponentUpdated?.Invoke();
        }

        public async Task<bool> SaveSnapshot(Stream outputFilestream, int thumbnailSize = -1)
        {
            ClearComponent();

            if (_thumbnailData != null)
            {
                _thumbnailData.Dispose();
                _thumbnailData = null;
            }

            _thumbnailSize = thumbnailSize;
            _saveSurface = true;
            thumbnailCompletion = new TaskCompletionSource<bool>();

            _canvas.InvokeInvalidate();

            // wait for the canvas to draw itself.  thumbnail bitmap needs to be set during the draw portion
            await thumbnailCompletion.Task;

            bool result = true;
            try
            {
                _thumbnailData.SaveTo(outputFilestream);
                _thumbnailData.Dispose();
                _thumbnailData = null;
            }
            catch(Exception)
            {
                result = false;
            }
            return result;
                     
        }

        public async Task<bool> SaveContent(Stream outputStream)
        {
            // handle derived classes!
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string serializedContent = JsonConvert.SerializeObject(_components, settings);

            bool result = true;
            try
            {
                using (var writer = new StreamWriter(outputStream))
                {
                    await writer.WriteAsync(serializedContent);
                }

                IsDirty = false;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }



        public async Task<bool> LoadContent(Stream inputStream)
        {
            _components.Clear();

            bool result = true;

            try
            {
                using (var reader = new System.IO.StreamReader(inputStream))
                {
                    var content = await reader.ReadToEndAsync();

                    JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

                    _components = JsonConvert.DeserializeObject<List<BaseComponent>>(content, settings);
                    if ( _components == null )
                        return false;

                    _components.ForEach(c =>
                    {
                        c.Initialize(_canvas);
                    });

                    SetSelectorMode();
                }

            }
            catch (Exception)
            {
                result = false;
            }

            _canvas.Invalidate();

            return result;          
            
        }
    }
}
