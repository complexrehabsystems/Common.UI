using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Common.UI.UWP.Common;
using Common.UI.UWP.Renderers;
using Xamarin.Forms.Platform.UWP;

using CommonBorder = Common.UI.Controls.Border;

[assembly: ExportRenderer(typeof(Common.UI.Controls.Border), typeof(BorderRenderer))]
namespace Common.UI.UWP.Renderers
{
    public class BorderRenderer : ViewRenderer<CommonBorder, FrameworkElement>
    {
        Border _border;
        bool MouseEntered;

        protected override void OnElementChanged(ElementChangedEventArgs<CommonBorder> e)
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

            if (e.PropertyName == CommonBorder.ColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if(e.PropertyName == CommonBorder.HighlightColorProperty.PropertyName)
            {
                SetBorderColor();
            }
            else if (e.PropertyName == CommonBorder.BorderColorProperty.PropertyName)
            {
                _border.BorderBrush = Element.BorderColor.ToNativeBrush();
            }
            else if(e.PropertyName == CommonBorder.CornerRadiusProperty.PropertyName)
            {
                _border.CornerRadius = new CornerRadius(Element.CornerRadius);
            }
            else if(e.PropertyName == CommonBorder.BorderThicknessProperty.PropertyName)
            {
                _border.BorderThickness = new Thickness(Element.BorderThickness);
            }
            else if (e.PropertyName == CommonBorder.BackgroundColorProperty.PropertyName)
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
