using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Common.Controls
{
    public class CustomBoxView : BoxView
    {
        public static readonly BindableProperty IsMouseOverProperty = BindableProperty.Create(nameof(IsMouseOver), typeof(bool), typeof(CustomBoxView), false, BindingMode.TwoWay);

        public bool IsMouseOver
        {
            get { return (bool)GetValue(IsMouseOverProperty); }
            set { SetValue(IsMouseOverProperty, value); }
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(StylableLabel), false, BindingMode.TwoWay);
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
    }
}