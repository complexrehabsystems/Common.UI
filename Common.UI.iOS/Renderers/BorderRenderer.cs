using Common.UI.iOS.Renderers;
using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using CommonBorder = Common.UI.Controls.Border;

[assembly: ExportRenderer(typeof(CommonBorder), typeof(BorderRenderer))]
namespace Common.UI.iOS.Renderers
{
    public class BorderRenderer : ViewRenderer<CommonBorder, UIView>
    {
        UIView _border;
        bool MouseEntered;

        public static int LoadClass = 0;

        protected override void OnElementChanged(ElementChangedEventArgs<CommonBorder> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                _border = new UIView
                {
                    BackgroundColor = e.NewElement.Color.ToUIColor(),
                };
                _border.Layer.BorderColor = e.NewElement.BorderColor.ToCGColor();
                _border.Layer.CornerRadius = (nfloat)e.NewElement.CornerRadius;
                _border.Layer.BorderWidth = (nfloat)e.NewElement.BorderThickness;

                //e.NewElement.BackgroundColor = Xamarin.Forms.Color.Transparent;

                SetNativeControl(_border);
            }

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control == null)
                return;

            if (e.PropertyName == CommonBorder.ColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if (e.PropertyName == CommonBorder.HighlightColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if (e.PropertyName == CommonBorder.BorderColorProperty.PropertyName)
            {
                _border.Layer.BorderColor = Element.BorderColor.ToCGColor();
            }
            else if (e.PropertyName == CommonBorder.CornerRadiusProperty.PropertyName)
            {
                _border.Layer.CornerRadius = (nfloat)Element.CornerRadius;
            }
            else if (e.PropertyName == CommonBorder.BorderThicknessProperty.PropertyName)
            {
                _border.Layer.BorderWidth = (nfloat)Element.BorderThickness;
            }
            else if (e.PropertyName == CommonBorder.BackgroundColorProperty.PropertyName)
            {
                //_border.Background = Element.BackgroundColor.ToNativeBrush();
                //Element.BackgroundColor = Xamarin.Forms.Color.Transparent;
            }


        }

        void SetBorderColor()
        {
            _border.BackgroundColor = MouseEntered && Element.HasMouseOver ? Element.HighlightColor.ToUIColor() : Element.Color.ToUIColor();
        }
    }


}