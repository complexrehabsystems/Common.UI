using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrsCommon.Controls;
using CrsCommon.UWP.Common;
using CrsCommon.UWP.Renderers;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(InputHandler), typeof(CrsCommon.UWP.Renderers.InputHandlerRenderer))]
namespace CrsCommon.UWP.Renderers
{
    public class InputHandlerRenderer : ViewRenderer<InputHandler, FrameworkElement>
    {
        protected Grid LayoutRoot;
        protected bool MouseDown;
        protected bool MultiTouch;
        protected int FingerCount;
        protected Point _lastPoint;
        protected bool _pointerCaptured = false;
        protected Dictionary<uint, Point> _currentMutilPoint = new Dictionary<uint, Point>();
        protected Dictionary<uint, Point> _lastMultiPoint = new Dictionary<uint, Point>();

        protected override void OnElementChanged(ElementChangedEventArgs<InputHandler> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                LayoutRoot = new Grid
                {
                    ColumnSpacing = 0,
                    RowSpacing = 0,
                    Background = Xamarin.Forms.Color.Transparent.ToNativeBrush(),
                    ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.All
                };

                SetNativeControl(LayoutRoot);
            }

            if (e.OldElement != null)
            {
                //Debug.WriteLine("InputHandlerRenderer - unsubscribe");
                // unsubscribe
                LayoutRoot.PointerEntered -= Border_PointerEntered;
                LayoutRoot.PointerPressed -= Border_PointerPressed;
                LayoutRoot.PointerExited -= Border_PointerExited;
                LayoutRoot.PointerReleased -= Border_PointerReleased;
                LayoutRoot.PointerMoved -= Border_PointerMoved;
                LayoutRoot.PointerCanceled -= Border_PointerCanceled;
                LayoutRoot.PointerCaptureLost -= Border_PointerCaptureLost; 
                LayoutRoot.ManipulationStarted -= LayoutRoot_ManipulationStarted;
                LayoutRoot.ManipulationDelta -= LayoutRoot_ManipulationDelta;
                LayoutRoot.ManipulationCompleted -= LayoutRoot_ManipulationCompleted;
                LayoutRoot.PointerWheelChanged -= CoreWindow_PointerWheelChanged;
            }

            if (e.NewElement != null)
            {
                //Debug.WriteLine("InputHandlerRenderer - subscribe");
                // subscribe
                LayoutRoot.PointerEntered += Border_PointerEntered;
                LayoutRoot.PointerPressed += Border_PointerPressed;
                LayoutRoot.PointerExited += Border_PointerExited;
                LayoutRoot.PointerReleased += Border_PointerReleased;
                LayoutRoot.PointerMoved += Border_PointerMoved;
                LayoutRoot.PointerCanceled += Border_PointerCanceled;
                LayoutRoot.PointerCaptureLost += Border_PointerCaptureLost;
                LayoutRoot.ManipulationStarted += LayoutRoot_ManipulationStarted;
                LayoutRoot.ManipulationDelta += LayoutRoot_ManipulationDelta;
                LayoutRoot.ManipulationCompleted += LayoutRoot_ManipulationCompleted;
                LayoutRoot.PointerWheelChanged += CoreWindow_PointerWheelChanged;
            }
        }

        private void LayoutRoot_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            MultiTouch = false;
            FingerCount = 0;
            _lastMultiPoint.Clear();
            _currentMutilPoint.Clear();
        }

        private void LayoutRoot_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (FingerCount != 2 || e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
                return;

            MultiTouch = true;

            if (_lastMultiPoint.Count() != 2 || _currentMutilPoint.Count() != 2)
            {
                SaveMultiTouch();
                return;
            }

            //Debug.WriteLine($"LayoutRoot_ManipulationDelta vel:{e.Velocities.Expansion} scale:{e.Cumulative.Scale}");

            List<Point> points = new List<Point>();

            foreach(var l in _lastMultiPoint)
            {
                points.Add(l.Value);

                if (_currentMutilPoint.TryGetValue(l.Key, out Point p))
                    points.Add(p);
            }

            if(points.Count != 4)
            {
                Debug.WriteLine("Problems");
                return;
            }

            Element.MultiTouchDelta?.Invoke(new InputHandler.MultiTouchEventArgs
            {
                f1StartX = points[0].X,
                f1StartY = points[0].Y,
                f1EndX = points[1].X,
                f1EndY = points[1].Y,
                f2StartX = points[2].X,
                f2StartY = points[2].Y,
                f2EndX = points[3].X,
                f2EndY = points[3].Y,
            });


            SaveMultiTouch();
        }

        void SaveMultiTouch()
        {
            _lastMultiPoint.Clear();

            if (_currentMutilPoint.Count != 2)
                return;

            foreach(var c in _currentMutilPoint)
            {
                _lastMultiPoint[c.Key] = c.Value;
            }
        }


        private void LayoutRoot_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            
        }





        #region Mouse Events

        private void Border_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;

            var point = e.GetCurrentPoint(LayoutRoot).Position;

            if (FingerCount > 0)
            {
                //if (_currentMutilPoint.ContainsKey(e.Pointer.PointerId))
                //    _lastMultiPoint[e.Pointer.PointerId] = _currentMutilPoint[e.Pointer.PointerId];

                _currentMutilPoint[e.Pointer.PointerId] = point;

                //Debug.WriteLine($"Border_PointerMoved id:{e.Pointer.PointerId} xy:{point}");
            }

            if (!MouseDown || FingerCount > 1)
                return;
            
            //Debug.WriteLine($"InputHandlerRenderer - Border_PointerMoved xy:{point.X},{point.Y} dxdy:{point.X-_lastPoint.X},{point.Y-_lastPoint.Y}");

            if (e.GetCurrentPoint(LayoutRoot).Properties.IsRightButtonPressed)
            {
                Element.RightMouseClickMoved?.Invoke(
                _lastPoint.X - point.X,
                _lastPoint.Y - point.Y);
            }
            else
            {
                Element.TouchMove?.Invoke(new InputHandler.TouchEventArgs
                {
                    X = (int)point.X,
                    Y = (int)point.Y,
                    DX = (int)(point.X - _lastPoint.X),
                    DY = (int)(point.Y - _lastPoint.Y),
                });
            }
            
            _lastPoint = point;
            
        }

        private void Border_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MultiTouch = false;
            MouseDown = false;
            e.Handled = true;
            _pointerCaptured = false;
            FingerCount = 0;
            _lastMultiPoint.Clear();
            _currentMutilPoint.Clear();
            //Debug.WriteLine("InputHandlerRenderer - Border_PointerCaptureLost");

            Element.TouchCancelled?.Invoke(new InputHandler.TouchEventArgs());
        }

        private void Border_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MultiTouch = false;
            MouseDown = false;
            e.Handled = true;
            _lastMultiPoint.Clear();
            _currentMutilPoint.Clear();
            FingerCount = 0;

            if (_pointerCaptured)
            {
                LayoutRoot.ReleasePointerCapture(e.Pointer);
                _pointerCaptured = false;
            }

            Element.TouchCancelled?.Invoke(new InputHandler.TouchEventArgs());
            //Debug.WriteLine("InputHandlerRenderer - Border_PointerCanceled");

        }

        private void Border_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _lastMultiPoint.Clear();
            _currentMutilPoint.Clear();
            FingerCount--;
            e.Handled = true;
            MouseDown = false;
            if (FingerCount > 1 || MultiTouch)
            {
                return;
            }

            if (_pointerCaptured)
            {
                LayoutRoot.ReleasePointerCapture(e.Pointer);
                _pointerCaptured = false;
            }

            var point = e.GetCurrentPoint(LayoutRoot).Position;

            //Debug.WriteLine($"InputHandlerRenderer - Border_PointerReleased xy:{point.X},{point.Y} rect:{LayoutRoot.ActualWidth},{LayoutRoot.ActualHeight}");

            if (!e.GetCurrentPoint(LayoutRoot).Properties.IsRightButtonPressed)
            {
                Element.TouchEnd?.Invoke(new InputHandler.TouchEventArgs
                {
                    X = (int)point.X,
                    Y = (int)point.Y,
                });
            }          
        }

        private void Border_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(LayoutRoot).Position;

            _lastMultiPoint[e.Pointer.PointerId] = point;

            FingerCount++;
            if (FingerCount > 1)
                return;
            
            _lastPoint = point;

            //Debug.WriteLine($"InputHandlerRenderer - Border_PointerPressed xy:{point.X},{point.Y} rect:{LayoutRoot.ActualWidth},{LayoutRoot.ActualHeight}");

            e.Handled = true;
            MouseDown = true;

            if (!_pointerCaptured)
            {
                LayoutRoot.CapturePointer(e.Pointer);
                _pointerCaptured = true;
            }           


            //right click handling?

            Element.TouchBegin?.Invoke(new InputHandler.TouchEventArgs
            {
                X = (int)(point.X),
                Y = (int)(point.Y),
            });
        }

        private void Border_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("InputHandlerRenderer - Border_PointerExited");
            Element.MouseExited?.Invoke();
        }

        private void Border_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("InputHandlerRenderer - Border_PointerEntered");
            Element.MouseEntered?.Invoke();
        }

        private void CoreWindow_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (Element == null)
                return;

            var point = e.GetCurrentPoint(LayoutRoot).Position;

            Element.MouseWheelPressed?.Invoke(
                e.GetCurrentPoint(LayoutRoot).Properties.MouseWheelDelta,
                point.X,
                point.Y);
        }


        #endregion



    }
}
