using CrsCommon.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(CrsCommon.UWP.Renderers.ExtendedWebViewRenderer))]
namespace CrsCommon.UWP.Renderers
{
    public class ExtendedWebViewRenderer : ViewRenderer<ExtendedWebView, Windows.UI.Xaml.Controls.WebView>
    {
        private static string[] SetBodyOverFlowHiddenString = new string[] { @"function SetBodyOverFlowHidden() { document.body.style.overflow = 'hidden'; } SetBodyOverFlowHidden();" };
        protected override void OnElementChanged(ElementChangedEventArgs<ExtendedWebView> e)
        {
            try
            {
                base.OnElementChanged(e);

                if (e.OldElement != null && Control != null)
                {
                    Control.NavigationCompleted -= OnWebViewNavigationCompleted;
                }

                if (e.NewElement != null)
                {
                    if (Control == null)
                    {
                        SetNativeControl(new Windows.UI.Xaml.Controls.WebView());

                        if (!string.IsNullOrWhiteSpace(Element.Html))
                        {
                            Control.NavigateToString(Element.Html);
                        }
                    }

                    Control.NavigationCompleted -= OnWebViewNavigationCompleted;
                    Control.NavigationCompleted += OnWebViewNavigationCompleted;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error at ExtendedWebViewRenderer OnElementChanged: " + ex.Message);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            // update this based on your custom webview control and what you want it to do
            if (Element is ExtendedWebView element && e.PropertyName.Equals(nameof(ExtendedWebView.Html)) && !string.IsNullOrWhiteSpace(element.Html))
                Control.NavigateToString(element.Html);
        }

        private async void OnWebViewNavigationCompleted(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess)
                return;

            await Control.InvokeScriptAsync("eval", SetBodyOverFlowHiddenString);
            var heightString = await Control.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
            if (int.TryParse(heightString, out int height))
            {
                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    //Debug.WriteLine($"OnWebViewNavigationCompleted - {height}");
                    Element.HeightRequest = height + 40;
                //});                
            }

            //var widthString = await Control.InvokeScriptAsync("eval", new[] { "document.body.scrollWidth.toString()" });
            //if (int.TryParse(widthString, out int width))
            //{
            //    Element.WidthRequest = width;
            //}
        }
    }
}
