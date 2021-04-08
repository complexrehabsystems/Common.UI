using Xamarin.Forms;

namespace Common.Controls
{
    public class UnclickableListView : ListView
    {
        private bool _skipScrollTo = false;

        public static readonly BindableProperty FirstVisibleItemProperty = BindableProperty.Create(nameof(FirstVisibleItem), typeof(object), typeof(UnclickableListView), null, BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var ctrl = bindable as UnclickableListView;
                if (ctrl != null && !ctrl._skipScrollTo)
                    ctrl.ScrollTo(newValue, ScrollToPosition.Start, false);
            });

        public object FirstVisibleItem
        {
            get { return GetValue(FirstVisibleItemProperty); }
            set
            {
                //set should only be used by the renderer and we do not want to trigger a ScrollTo in that case
                _skipScrollTo = true;
                SetValue(FirstVisibleItemProperty, value);
                _skipScrollTo = false;
            }
        }
    }
}

