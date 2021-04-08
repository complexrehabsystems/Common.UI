using System.Diagnostics;
using Common.UI.Controls;
using Common.UI.UWP.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomBoxView), typeof(CustomBoxViewRenderer))]
namespace Common.UI.UWP.Renderers
{
    public class CustomBoxViewRenderer : BoxViewRenderer
    {
        private bool MouseEntered;

        protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
        {
            base.OnElementChanged(e);

            if (Control != null && e.NewElement != null)
            {
                //HeaderFrame = new Frame
                //{   
                //    Height = e.NewElement.HeaderHeightRequest,
                //    Background = Xamarin.Forms.Color.Red.ToNativeBrush()
                //    //Background = Xamarin.Forms.Color.Transparent.ToNativeBrush()// e.NewElement.ButtonBackgroundColor.ToNativeBrush()
                //};

                //if (e.NewElement.Width >= 0)
                //    HeaderFrame.Width = e.NewElement.Width;

                //SetNativeControl(HeaderFrame);

                // subscribe
                Debug.WriteLine("CustomBoxViewRenderer subscribe");
                Control.PointerEntered += HeaderFrame_PointerEntered;
                Control.PointerExited += HeaderFrame_PointerExited;
            }

            if (Control != null && e.OldElement != null)
            {
                Debug.WriteLine("CustomBoxViewRenderer unsubscribe");
                Control.PointerEntered -= HeaderFrame_PointerEntered;
                Control.PointerExited -= HeaderFrame_PointerExited;
            }


        }

        #region Mouse Events
        private void HeaderFrame_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!MouseEntered || (!Element?.IsEnabled ?? false) )
                return;

            MouseEntered = false;

            if (Element is CustomBoxView cbv)
                cbv.IsMouseOver = false;
        }        

        private void HeaderFrame_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MouseEntered || (!Element?.IsEnabled ?? false) )
                return;

            MouseEntered = true;

            if (Element is CustomBoxView cbv)
                cbv.IsMouseOver = true;
        }
        #endregion

    }
}
