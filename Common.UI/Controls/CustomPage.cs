using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Common.Controls
{
    

    public class CustomPage:ContentPage
    {
        public static readonly BindableProperty TitleBarTextColorProperty = BindableProperty.Create(nameof(TitleBarTextColor), typeof(Color), typeof(CustomPage), Color.Black, BindingMode.Default);

        public Color TitleBarTextColor
        {
            get { return (Color)GetValue(TitleBarTextColorProperty); }
            set { SetValue(TitleBarTextColorProperty, value); }
        }

        public static readonly BindableProperty TitleBarColorProperty = BindableProperty.Create(nameof(TitleBarColor), typeof(Color), typeof(CustomPage), Color.White, BindingMode.Default);

        public Color TitleBarColor
        {
            get { return (Color)GetValue(TitleBarColorProperty); }
            set { SetValue(TitleBarColorProperty, value); }
        }

        public Action OnDetached;

        private bool _appeared;

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (this.Parent == null)
            {
                if(_appeared)
                {
                    _appeared = false;
                    OnDetached?.Invoke();
                }
            }
            else
            {
                _appeared = true;
            }
        }


    }
}
