using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Common.UI.Controls;
using Common.UI.UWP.Renderers;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(UnclickableListView), typeof(UnclickableListViewRenderer))]
namespace Common.UI.UWP.Renderers
{
    public class UnclickableListViewRenderer : ListViewRenderer
    {
        private ScrollViewer _scrollViewer = null;
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if (List != null && e.NewElement != null)
            {
                List.SelectionMode = Windows.UI.Xaml.Controls.ListViewSelectionMode.None;
                List.IsItemClickEnabled = false;
                List.ContainerContentChanging += List_ContainerContentChanging;
            }
            else if (List != null && e.OldElement != null)
            {
                if (_scrollViewer != null)
                {
                    _scrollViewer.ViewChanged -= SV_ViewChanged;
                    _scrollViewer = null;
                }

                List.ContainerContentChanging -= List_ContainerContentChanging;
            }
        }

        private void SV_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!(Element is UnclickableListView el))
                return;

            var itemStackPanel = FindVisualChild<ItemsStackPanel>(List);
            object firstVisibleItem = null;
            var index = 0;

            if ((_scrollViewer.VerticalOffset + List.ActualHeight) == itemStackPanel.ActualHeight)
            {
                //bottom of list, highlight last item regardless
                firstVisibleItem = Element.ItemsSource.Cast<object>().LastOrDefault();
            }
            else
            {
                foreach (var item in Element.ItemsSource)
                {
                    if (index == itemStackPanel.FirstVisibleIndex)
                    {
                        firstVisibleItem = item;
                        break;
                    }
                    index++;
                }
            }

            el.FirstVisibleItem = firstVisibleItem;
        }

        private void List_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (_scrollViewer != null)
                _scrollViewer.ViewChanged -= SV_ViewChanged;

            _scrollViewer = FindVisualChild<ScrollViewer>(List);
            if (_scrollViewer != null)
                _scrollViewer.ViewChanged += SV_ViewChanged;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj, int depth = 0) where childItem : DependencyObject
        {
            if (obj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child, depth + 1);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}

