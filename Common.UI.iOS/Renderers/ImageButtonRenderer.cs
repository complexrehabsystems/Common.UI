using Common.UI.iOS.Controls;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using CommonImageButton = Common.UI.Controls.ImageButton;

[assembly: ExportRenderer(typeof(Common.UI.Controls.ImageButton), typeof(Common.UI.iOS.Renderers.ImageButtonRenderer))]
namespace Common.UI.iOS.Renderers
{
    public class ImageButtonRenderer : ViewRenderer<CommonImageButton, UIView>
    {
        protected bool MouseDown;
        protected bool MouseEntered;
        protected bool _pointerCaptured = false;
        public static int LoadClass = 0;

        protected override void OnElementChanged(ElementChangedEventArgs<CommonImageButton> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                var v = new UITouchView
                {
                    //ExclusiveTouch = true,
                    //MultipleTouchEnabled = false,
                    BackgroundColor = Xamarin.Forms.Color.Transparent.ToUIColor(),
                };
                v.Layer.CornerRadius = (nfloat)e.NewElement.CornerRadius;
                //v.Layer.BorderWidth = (nfloat)e.NewElement.BorderThickness;
                //v.Layer.BorderColor = UIColor.Clear.CGColor;
                v.OnTouchBegin += V_OnTouchBegin;
                v.OnTouchCancelled += V_OnTouchCancelled;
                v.OnTouchEnd += V_OnTouchEnd;
                
                SetNativeControl(v);

            }

        }

        private void V_OnTouchEnd()
        {
            ReleaseButtonPressed(canceling: false);
            _pointerCaptured = false;

            
        }

        private void V_OnTouchCancelled()
        {
            ReleaseButtonPressed(canceling: true);
            _pointerCaptured = false;
        }

        private void V_OnTouchBegin()
        {
            if (MouseDown || (!Element?.IsEnabled ?? false))
                return;

            if (!_pointerCaptured)
            {
                _pointerCaptured = false;
            }

            ButtonPressed(true);
        }

        void ButtonPressed(bool pressed)
        {
            if (Element != null)
                Element.ButtonState = pressed ? CommonImageButton.State.Active : CommonImageButton.State.Hover;

            if (pressed == MouseDown)
                return;

            MouseDown = pressed;

            if (pressed)
                Element?.ButtonPressed();
            else
                Element?.ButtonReleased();
        }

        private void ReleaseButtonPressed(bool canceling)
        {
            if (Element != null)
                Element.ButtonState = CommonImageButton.State.Static;

            if (!MouseDown || !Element.IsEnabled)
                return;

            ButtonPressed(false);

            // no need to highlight mouse pressed after selection or capture lost/canceled
            MouseEntered = false;

            Element.IsHighlighted = false;

            if (Element != null)
                Element.ButtonState = CommonImageButton.State.Static;

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