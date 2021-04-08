using CrsCommon.Controls;
using CrsCommon.UWP.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace CrsCommon.UWP.Renderers
{
    public class CustomWebViewRenderer : WebViewRenderer, IWebViewDelegate
    {
        //CRS-1601 - Summary View Not Showing Up
        // Workaround from https://xamarin.github.io/bugzilla-archives/57/57451/bug.html
        // Overrides https://github.com/xamarin/Xamarin.Forms/blob/afc037ffec4b12434b02d3d9d0f7db30f638aa82/Xamarin.Forms.Platform.UAP/WebViewRenderer.cs#L26
        // Looks like method we are overriding first attempts to change the schema to a local schema when no base url is provided
        // After this happens it then displays the string html
        // Best guess at this point is that the NavigationCompleted is sometimes not fired and thus it doesn't get to the Control.NavigateToString call
        // It looks like we do not need to change the schema in order to display our content 
        void IWebViewDelegate.LoadHtml(string html, string baseUrl)
        {
            if (Element.Source is HtmlWebViewSource && string.IsNullOrEmpty(baseUrl))
                Control.NavigateToString(html);
            else
                LoadHtml(html, baseUrl);
        }
    }
}
