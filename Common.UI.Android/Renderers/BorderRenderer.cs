using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Common.UI.Android.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using CommonBorder = Common.UI.Controls.Border;
using CommonImageButton = Common.UI.Controls.ImageButton;
using AndroidView = Android.Views.View;
using System.ComponentModel;
using Android.Graphics.Drawables;

[assembly: ExportRenderer(typeof(CommonBorder), typeof(BorderRenderer))]
namespace Common.UI.Android.Renderers
{
    public class BorderRenderer : ViewRenderer<CommonBorder, global::Android.Views.View>
	{
        protected AndroidView _border;
        protected bool MouseDown;
        protected bool MouseEntered;
        protected bool _pointerCaptured = false;

        public static int LoadClass = 0;


		public BorderRenderer(Context context) : base(context)
        {
            AutoPackage = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CommonBorder> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                _border = new AndroidView(this.Context)
                {
                };

				GradientDrawable shape = new GradientDrawable();
				shape.SetShape(ShapeType.Rectangle);
				shape.SetCornerRadii(new float[] 
				{ 
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
				});
				shape.SetColor(Element.Color.ToAndroid());
				shape.SetStroke((int)Element.BorderThickness,Element.BorderColor.ToAndroid());
				_border.Background = shape;

				if(!e.NewElement.InputTransparent)
                {
                    _border.Touch += _border_Touch;
                }

                SetNativeControl(_border);
            }

        }

        private void _border_Touch(object sender, TouchEventArgs e)
        {
            if (e.Event == null || Element.ButtonPressed == null)
                return;

            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    V_OnTouchBegin();
                    break;
                case MotionEventActions.Up:
                    V_OnTouchEnd();
                    break;
                case MotionEventActions.Cancel:
                    V_OnTouchCancelled();
                    break;
                default:
                    break;
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

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control == null)
                return;

			var shape = _border.Background as GradientDrawable;

			if (e.PropertyName == CommonBorder.ColorProperty.PropertyName)
            {
				SetBackgroundColor(); 
			}
            else if (e.PropertyName == CommonBorder.HighlightColorProperty.PropertyName)
            {
				SetBackgroundColor();
			}
            else if (e.PropertyName == CommonBorder.BorderColorProperty.PropertyName)
            {
				shape.SetStroke((int)Element.BorderThickness, Element.BorderColor.ToAndroid());
			}
            else if (e.PropertyName == CommonBorder.CornerRadiusProperty.PropertyName)
            {
				shape.SetCornerRadii(new float[]
				{
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
				});
			}
            else if (e.PropertyName == CommonBorder.BorderThicknessProperty.PropertyName)
            {
				shape.SetStroke((int)Element.BorderThickness, Element.BorderColor.ToAndroid());
			}
            else if (e.PropertyName == CommonBorder.BackgroundColorProperty.PropertyName)
            {
                //_border.Background = Element.BackgroundColor.ToNativeBrush();
                //Element.BackgroundColor = Xamarin.Forms.Color.Transparent;
            }


        }

		void SetBackgroundColor()
		{
			var shape = _border.Background as GradientDrawable;
			var col = MouseEntered && Element.HasMouseOver ? Element.HighlightColor : Element.Color;
			shape.SetColor(col.ToAndroid());
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