using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrsCommon.Controls;
using CrsCommon.UWP.Common;
using CrsCommon.UWP.Renderers;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(StylableLabel), typeof(StylableLabelRenderer))]
namespace CrsCommon.UWP.Renderers
{
    public class StylableLabelRenderer : LabelRenderer
    {
        protected bool MouseEntered;

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // unsubscribe
                Control.PointerEntered -= Control_PointerEntered;
                Control.PointerExited -= Control_PointerExited;
            }

            if (Control != null)
            {
                // subscribe
                Control.PointerEntered += Control_PointerEntered;
                Control.PointerExited += Control_PointerExited;

                var el = Element as StylableLabel;
                if (el != null && string.IsNullOrEmpty(el.Tooltip) == false)
                {
                    ToolTipService.SetToolTip(Control, el.Tooltip);
                    ToolTipService.SetPlacement(Control, Windows.UI.Xaml.Controls.Primitives.PlacementMode.Bottom);
                }

            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var el = Element as StylableLabel;

            if (el != null && e.PropertyName == StylableLabel.TooltipProperty.PropertyName)
            {
                if (string.IsNullOrEmpty(el.Tooltip) == false)
                {
                    ToolTipService.SetToolTip(Control, el.Tooltip);
                    ToolTipService.SetPlacement(Control, Windows.UI.Xaml.Controls.Primitives.PlacementMode.Bottom);
                }
                else
                    ToolTipService.SetToolTip(Control, null);
            }
        }

        #region Mouse Events
        private void Control_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!MouseEntered || (!Element?.IsEnabled ?? false) )
                return;

            MouseEntered = false;

            if (Element is StylableLabel label)
                label.SetHighlighted(false);
        }        

        private void Control_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MouseEntered || (!Element?.IsEnabled ?? false) )
                return;

            MouseEntered = true;

            if (Element is StylableLabel label)
                label.SetHighlighted(true);
        }
        #endregion

    }
}
