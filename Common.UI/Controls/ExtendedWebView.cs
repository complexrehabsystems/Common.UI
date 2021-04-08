using Xamarin.Forms;

namespace Common.UI.Controls
{
    public class ExtendedWebView : WebView
    {
        public static readonly BindableProperty HtmlProperty = BindableProperty.Create(nameof(Html), typeof(string), typeof(ExtendedWebView), string.Empty, BindingMode.Default);

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }
    }
}
