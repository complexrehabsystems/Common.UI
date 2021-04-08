using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Common.UI.Controls;
using Common.UI.UWP.Common;
using Xamarin.Forms.Platform.UWP;
using ImageButtonRenderer = Common.UI.UWP.Renderers.ImageButtonRenderer;

[assembly: ExportRenderer(typeof(ImageButton), typeof(ImageButtonRenderer))]
namespace Common.UI.UWP.Renderers
{
    public class ImageButtonRenderer : ViewRenderer<ImageButton, FrameworkElement>
    {
        protected Grid LayoutRoot;
        protected bool MouseDown;
        protected bool MouseEntered;
        protected bool _pointerCaptured = false;

        protected override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                LayoutRoot = new Grid
                {
                    ColumnSpacing = 0,
                    RowSpacing = 0,
                    CornerRadius = new CornerRadius(e.NewElement.CornerRadius),
                    //BorderBrush = e.NewElement.ButtonBorderColor.ToNativeBrush(),
                    Background = Xamarin.Forms.Color.Transparent.ToNativeBrush(),// e.NewElement.ButtonBackgroundColor.ToNativeBrush(),
                    BorderThickness = new Thickness(e.NewElement.BorderThickness),
                };

                
                if (string.IsNullOrEmpty(Element.Tooltip) == false)
                {                    
                    ToolTipService.SetToolTip(LayoutRoot, Element.Tooltip);
                    ToolTipService.SetPlacement(LayoutRoot, Windows.UI.Xaml.Controls.Primitives.PlacementMode.Bottom);                    
                }

                SetNativeControl(LayoutRoot);
            }

            if (e.OldElement != null)
            {
                // unsubscribe
                LayoutRoot.PointerEntered -= Border_PointerEntered;
                LayoutRoot.PointerPressed -= Border_PointerPressed;
                LayoutRoot.PointerExited -= Border_PointerExited;
                LayoutRoot.PointerReleased -= Border_PointerReleased;
                LayoutRoot.PointerCanceled -= Border_PointerCanceled;
                LayoutRoot.PointerCaptureLost -= Border_PointerCaptureLost;
            }

            if (e.NewElement != null)
            {
                // subscribe
                LayoutRoot.PointerEntered += Border_PointerEntered;
                LayoutRoot.PointerPressed += Border_PointerPressed;
                LayoutRoot.PointerExited += Border_PointerExited;
                LayoutRoot.PointerReleased += Border_PointerReleased;
                LayoutRoot.PointerCanceled += Border_PointerCanceled;
                LayoutRoot.PointerCaptureLost += Border_PointerCaptureLost;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control == null)
                return;

            if (e.PropertyName == ImageButton.TooltipProperty.PropertyName)
            { 
                if (string.IsNullOrEmpty(Element.Tooltip) == false)
                {
                    ToolTipService.SetToolTip(LayoutRoot, Element.Tooltip);
                    ToolTipService.SetPlacement(LayoutRoot, Windows.UI.Xaml.Controls.Primitives.PlacementMode.Bottom);
                }
                else 
                    ToolTipService.SetToolTip(LayoutRoot, null);
            }
        }

        #region Mouse Events
        private void Border_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ReleaseButtonPressed(canceling: true);
        }

        private void Border_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ReleaseButtonPressed(canceling: true);

            if (_pointerCaptured)
            {
                LayoutRoot.ReleasePointerCapture(e.Pointer);
                _pointerCaptured = false;
            }

        }

        private void Border_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ReleaseButtonPressed(canceling: false);

            if (_pointerCaptured)
            {
                LayoutRoot.ReleasePointerCapture(e.Pointer);
                _pointerCaptured = false;
            }
        }

        private void Border_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MouseDown || (!Element?.IsEnabled ?? false) )
                return;

            if (!_pointerCaptured)
            {
                LayoutRoot.CapturePointer(e.Pointer);
                _pointerCaptured = false;
            }            

            ButtonPressed(true);
        }
        
        private void Border_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ButtonPressed(false);

            if(Element != null)
                Element.ButtonState = ImageButton.State.Static;

            if (!MouseEntered || (!Element?.IsEnabled ?? false) )
                return;

            MouseEntered = false;

            Element.IsHighlighted = false;
        }        

        private void Border_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MouseEntered || (!Element?.IsEnabled ?? false) )
                return;

            MouseEntered = true;

            ButtonPressed(false);

            Element.IsHighlighted = true;
        }

        void ButtonPressed(bool pressed)
        {
            if(Element != null)
                Element.ButtonState = pressed ? ImageButton.State.Active : ImageButton.State.Hover;

            if (pressed == MouseDown)
                return;

            MouseDown = pressed;

            if (pressed)
                Element?.ButtonPressed();
            else
                Element?.ButtonReleased();
        }
        #endregion

        private void ReleaseButtonPressed(bool canceling)
        {
            if (Element != null)
                Element.ButtonState = ImageButton.State.Static;

            if (!MouseDown || !Element.IsEnabled)
                return;

            ButtonPressed(false);

            // no need to highlight mouse pressed after selection or capture lost/canceled
            MouseEntered = false;

            Element.IsHighlighted = false;

            if (Element != null)
                Element.ButtonState = ImageButton.State.Static;

            if (!canceling)
            {
                // relay button pressed event
                //   ** Do this last because it *might trigger teardown of this element (e.g. on navigation)
                //      on a separate thread, which leads to null ref badness
                Element?.ButtonSelected();
            }
        }

    }
}
