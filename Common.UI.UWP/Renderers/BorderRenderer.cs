using Common.UWP.Common;
using Common.UWP.Renderers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Common.Controls.Border), typeof(BorderRenderer))]
namespace Common.UWP.Renderers
{
    public class BorderRenderer : ViewRenderer<Common.Controls.Border, FrameworkElement>
    {
        Border _border;
        bool MouseEntered;

        protected override void OnElementChanged(ElementChangedEventArgs<Common.Controls.Border> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                _border = new Border
                {
                    Background = e.NewElement.Color.ToNativeBrush(),
                    BorderBrush = e.NewElement.BorderColor.ToNativeBrush(),
                    CornerRadius = new CornerRadius(e.NewElement.CornerRadius),
                    BorderThickness = new Thickness(e.NewElement.BorderThickness),                    
                };
                e.NewElement.BackgroundColor = Xamarin.Forms.Color.Transparent;
         
                SetNativeControl(_border);
            }

            if (e.OldElement != null)
            {
                // unsubscribe
                _border.PointerEntered -= Border_PointerEntered;
                _border.PointerExited -= Border_PointerExited;
            }

            if (e.NewElement != null)
            {
                // subscribe
                _border.PointerEntered += Border_PointerEntered;
                _border.PointerExited += Border_PointerExited;
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
            else if(e.PropertyName == Common.Controls.Border.HighlightColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if (e.PropertyName == Common.Controls.Border.BorderColorProperty.PropertyName)
            {
                _border.BorderBrush = Element.BorderColor.ToNativeBrush();
            }
            else if(e.PropertyName == Common.Controls.Border.CornerRadiusProperty.PropertyName)
            {
                _border.CornerRadius = new CornerRadius(Element.CornerRadius);
            }
            else if(e.PropertyName == Common.Controls.Border.BorderThicknessProperty.PropertyName)
            {
                _border.BorderThickness = new Thickness(Element.BorderThickness);
            }
            else if (e.PropertyName == Common.Controls.Border.BackgroundColorProperty.PropertyName)
            {
                //_border.Background = Element.BackgroundColor.ToNativeBrush();
                //Element.BackgroundColor = Xamarin.Forms.Color.Transparent;
            }


        }

        private void Border_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!Element.HasMouseOver)
                return;

            MouseEntered = false;
            SetBorderColor();
        }

        private void Border_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!Element.HasMouseOver)
                return;

            MouseEntered = true;
            SetBorderColor();

        }

        void SetBorderColor()
        {
            _border.Background = MouseEntered && Element.HasMouseOver ? Element.HighlightColor.ToNativeBrush() : Element.Color.ToNativeBrush();
        }
    }
}
