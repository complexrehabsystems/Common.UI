using Common.UI.iOS.Renderers;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Common.Controls.Border), typeof(BorderRenderer))]
namespace Common.UI.iOS.Renderers
{
    public class BorderRenderer : ViewRenderer<Common.Controls.Border, UIView>
    {
        UIView _border;
        bool MouseEntered;

        public static int LoadClass = 0;

        protected override void OnElementChanged(ElementChangedEventArgs<Common.Controls.Border> e)
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

            if (e.PropertyName == Common.Controls.Border.ColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if (e.PropertyName == Common.Controls.Border.HighlightColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if (e.PropertyName == Common.Controls.Border.BorderColorProperty.PropertyName)
            {
                _border.Layer.BorderColor = Element.BorderColor.ToCGColor();
            }
            else if (e.PropertyName == Common.Controls.Border.CornerRadiusProperty.PropertyName)
            {
                _border.Layer.CornerRadius = (nfloat)Element.CornerRadius;
            }
            else if (e.PropertyName == Common.Controls.Border.BorderThicknessProperty.PropertyName)
            {
                _border.Layer.BorderWidth = (nfloat)Element.BorderThickness;
            }
            else if (e.PropertyName == Common.Controls.Border.BackgroundColorProperty.PropertyName)
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