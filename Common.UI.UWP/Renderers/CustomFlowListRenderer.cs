using Common.UI.UWP.Renderers;
using Xamarin.Forms.Platform.UWP;

[assembly: Xamarin.Forms.Platform.UWP.ExportRenderer(typeof(DLToolkit.Forms.Controls.FlowListView), typeof(CustomFlowListRenderer))]
namespace Common.UI.UWP.Renderers
{
    public class CustomFlowListRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if (List != null)
                List.SelectionMode = Windows.UI.Xaml.Controls.ListViewSelectionMode.None;
        }
    }
}
