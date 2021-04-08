using Common.Controls.Auto.Forms.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Common.Controls.Auto.Forms
{
    public class AutoFormsAttribute: AutoFormsFilteredAttribute
    {
        public string Label { get; private set; }

        public string Placeholder { get; private set; }

        public string ItemStyle { get; private set; }
        public string LabelStyleOverride { get; private set; }

        public AutoFormsType Type { get; private set; }

        public string IsVisible { get; set; }

        public string IsEnabled { get; set; }

        public string IsFocused { get; set; }

        public AutoFormsOrientation Orientation { get; private set; }
        public AutoFormsLayoutOptions HorizontalLabelOptions { get; private set; }
        public AutoFormsLayoutOptions HorizontalControlOptions { get; private set; }
        public double ControlWidthRequest { get; private set; }

        public double HeightRequest { get; private set; }

        public double PaddingLeft { get; private set; }

        public double PaddingRight { get; private set; }

        public double PaddingTop { get; private set; }

        public double PaddingBottom { get; private set; }

        public double LayoutHorizontalPercentageOverride { get; private set; }

        public string [] Grouped { get; private set; }

        public AutoFormsAttribute(
            string label = null,
            AutoFormsType type = AutoFormsType.Auto,
            AutoFormsOrientation orientation = AutoFormsOrientation.Horizontal,
            AutoFormsLayoutOptions horizontalLabelOptions = AutoFormsLayoutOptions.Default,
            AutoFormsLayoutOptions horizontalControlOptions = AutoFormsLayoutOptions.Default,
            double controlWidthRequest = -1,
            string itemStyle = null,
            string labelStyleOverride = null,
            string placeholder = null,
            double heightRequest = -1,
            string isVisible = null,
            string isEnabled = null,
            string isFocused = null,
            int filter = 0,
            double paddingLeft = 25,
            double paddingRight = 25,
            double paddingTop = 0,
            double paddingBottom = 20,
            double layoutHorizontalPercentageOverride = -1,
            string [] grouped = null):base(filter)
        {
            Label = label;
            ItemStyle = itemStyle;
            LabelStyleOverride = labelStyleOverride;
            Placeholder = placeholder;
            Type = type;
            HeightRequest = heightRequest;
            IsVisible = isVisible;
            IsEnabled = isEnabled;
            IsFocused = isFocused;
            PaddingLeft = paddingLeft;
            PaddingRight = paddingRight;
            PaddingTop = paddingTop;
            PaddingBottom = paddingBottom;
            LayoutHorizontalPercentageOverride = layoutHorizontalPercentageOverride;
            Grouped = grouped;

            Orientation = orientation;
            HorizontalLabelOptions = horizontalLabelOptions;
            HorizontalControlOptions = horizontalControlOptions;
            ControlWidthRequest = controlWidthRequest;
        }
    }
}
