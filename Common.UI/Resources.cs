using Xamarin.Forms;

namespace Common
{
    public class Resources
    {
        public static void AddDefaultStyles(ResourceDictionary resources)
        {

            resources.Add("ExpandableViewDefaultToggleButtonStyle", Controls.ExpandableView.DefaultExpandableViewToggleButtonStyle);
            resources.Add("ExpandableViewDefaultBorderStyle", Controls.ExpandableView.DefaultExpandableViewBorderStyle);
            resources.Add("ExpandableViewDefaultHeaderBackgroundStyle", Controls.ExpandableView.DefaultExpandableViewHeaderBackgroundStyle);

            resources.Add("SortButtonDefaultStyle", Controls.SortButton.DefaultSortButtonStyle);
        }
    }
}
