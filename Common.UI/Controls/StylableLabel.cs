using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Controls
{
    public class StylableLabel : Label
    {
        public static readonly BindableProperty TooltipProperty = BindableProperty.Create(nameof(Tooltip), typeof(string), typeof(ImageButton), string.Empty, BindingMode.Default);

        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public static readonly BindableProperty IsHighlightedProperty = BindableProperty.Create(nameof(IsHighlighted), typeof(bool), typeof(StylableLabel), false, BindingMode.OneWayToSource);
        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
        }
    }
}
