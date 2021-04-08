using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using CrsCommon.Controls;
using CrsCommon.UWP.Renderers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(UrhoView), typeof(UrhoViewRenderer))]
namespace CrsCommon.UWP.Renderers
{   

    public class UrhoViewRenderer:ViewRenderer<UrhoView, ContentControl>
    {
        private bool _pointerHolding = false;
        private uint _pointerId = 0;
        private Windows.Foundation.Point _startPoint;
        private Windows.Foundation.Point _lastPoint;

        protected override void OnElementChanged(ElementChangedEventArgs<UrhoView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                SetNativeControl(new ContentControl());
            }

            if (e.OldElement != null)
            {
                Debug.WriteLine($"UrhoViewRenderer - unsubscribe");
                // unsubscribe
                var coreWindow = CoreWindow.GetForCurrentThread();
                coreWindow.PointerWheelChanged -= CoreWindow_PointerWheelChanged;
                coreWindow.PointerPressed -= CoreWindow_PointerPressed;
                coreWindow.PointerReleased -= CoreWindow_PointerReleased;
                coreWindow.PointerMoved -= CoreWindow_PointerMoved;
            }

            if(e.NewElement != null)
            {
                Debug.WriteLine($"UrhoViewRenderer - subscribe");

                // subscribe
                var coreWindow = CoreWindow.GetForCurrentThread();
                coreWindow.PointerWheelChanged += CoreWindow_PointerWheelChanged;
                coreWindow.PointerPressed += CoreWindow_PointerPressed;
                coreWindow.PointerReleased += CoreWindow_PointerReleased;
                coreWindow.PointerMoved += CoreWindow_PointerMoved;
                coreWindow.PointerCaptureLost += CoreWindow_PointerCaptureLost;
            }
        }

        private void CoreWindow_PointerCaptureLost(CoreWindow sender, PointerEventArgs args)
        {
            Debug.WriteLine($"UrhoViewRenderer - CoreWindow_PointerCaptureLost");
        }

        private void CoreWindow_PointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            args.Handled = false;
            if (!_pointerHolding || !WithinControlBounds(args.CurrentPoint.Position))
                return;

            if (args.CurrentPoint.PointerId != _pointerId)
                return;

            //Debug.WriteLine($"Control_PointerMoved - {args.CurrentPoint.Properties.IsRightButtonPressed}");

            if (args.CurrentPoint.Properties.IsRightButtonPressed == false || args.CurrentPoint.PointerDevice.PointerDeviceType != PointerDeviceType.Mouse)
                return;

            Element.RightMouseClickMoved?.Invoke(
                _lastPoint.X - args.CurrentPoint.Position.X,
                _lastPoint.Y - args.CurrentPoint.Position.Y);

            _lastPoint = args.CurrentPoint.Position;
            args.Handled = true;
        }

        private void CoreWindow_PointerReleased(CoreWindow sender, PointerEventArgs args)
        {
            Debug.WriteLine($"UrhoViewRenderer - CoreWindow_PointerReleased");

            if (_pointerHolding == false || args.CurrentPoint.PointerId != _pointerId)
            {
                args.Handled = false;
                return;
            }
            //Debug.WriteLine("Control_PointerReleased");
            _pointerHolding = false;
            _pointerId = 0;
            args.Handled = true;
        }

        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            Debug.WriteLine($"UrhoViewRenderer - CoreWindow_PointerPressed");
            //Debug.WriteLine("Control_PointerPressed");

            args.Handled = false;

            // detecting multitouch so stop handling events
            if (_pointerHolding && args.CurrentPoint.PointerId != _pointerId)
            {
                _pointerHolding = false;
                _pointerId = 0;
                return;
            }

            _pointerHolding = false;

            CaptureStartPoint();            

            if (!WithinControlBounds(args.CurrentPoint.Position) || args.CurrentPoint.Properties.IsRightButtonPressed == false)
            {
                return;
            }

            args.Handled = true;

            _pointerHolding = true;
            _pointerId = args.CurrentPoint.PointerId;
            _lastPoint = args.CurrentPoint.Position;

            //Debug.WriteLine("Control_PointerPressed Holding");
        }

        private void CoreWindow_PointerWheelChanged(CoreWindow sender, PointerEventArgs args)
        {
            CaptureStartPoint();

            if (!WithinControlBounds(args.CurrentPoint.Position))
                return;

            //Debug.WriteLine("Core MouseWheel Changed");
            Element.MouseWheelPressed?.Invoke(
                args.CurrentPoint.Properties.MouseWheelDelta,
                args.CurrentPoint.Position.X - _startPoint.X, 
                args.CurrentPoint.Position.Y - _startPoint.Y);
        }

        private bool WithinControlBounds(Windows.Foundation.Point pos)
        {
            return (pos.X < _startPoint.X ||
                    pos.Y < _startPoint.Y ||
                    pos.X > _startPoint.X + Control.ActualWidth ||
                    pos.Y > _startPoint.Y + Control.ActualHeight) ? false : true;
        }

        private void CaptureStartPoint()
        {
            var ttv = Control.TransformToVisual(Window.Current.Content);
            _startPoint = ttv.TransformPoint(new Windows.Foundation.Point(0, 0));
        }


    }
}
