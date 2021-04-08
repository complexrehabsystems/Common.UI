using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Controls
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
