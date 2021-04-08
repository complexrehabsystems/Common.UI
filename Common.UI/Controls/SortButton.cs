using Common.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Common.Controls
{
    public class SortButton : ImageButton
    {
        public enum SortDirectionEnum
        {
            Ascending,
            Descending
        }

        public class SortState
        {
            public object SortValue { get; private set; }
            public SortDirectionEnum SortDirection { get; private set; }

            public SortState(object sortValue, SortDirectionEnum sortDirection)
            {
                SortValue = sortValue;
                SortDirection = sortDirection;
            }
        }

        /// <summary>
        /// Default properties that can be overriden in derived styles
        /// </summary>
        public readonly static Style DefaultSortButtonStyle = new Style(typeof(ImageButton))
        {
            Setters =
            {
                new Setter { Property = SortButton.OrientationProperty, Value = LayoutOrientation.Horizontal},
                new Setter { Property = SortButton.HelperTextFontFamilyProperty, Value = ViewConstants.FontFamilySegoe },
                new Setter { Property = SortButton.ButtonBackgroundColorProperty, Value = Color.Transparent },
                new Setter { Property = SortButton.ButtonBorderColorProperty, Value = Color.Transparent },
                new Setter { Property = SortButton.ButtonHighlightColorProperty, Value = Color.Transparent }
            }
        };

        public static readonly BindableProperty SortValueProperty = BindableProperty.Create(nameof(SortValue), typeof(object), typeof(SortButton), null, BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var ctrl = (SortButton)bindable;
            });

        public object SortValue
        {
            get { return GetValue(SortValueProperty); }
            set { SetValue(SortValueProperty, value); }
        }

        public static readonly BindableProperty SortedStateProperty = BindableProperty.Create(nameof(SortedState), typeof(SortState), typeof(SortButton), null, BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var ctrl = (SortButton)bindable;
                ctrl.OnSortedStateChanged();
            });

        public SortState SortedState
        {
            get { return (SortState)GetValue(SortedStateProperty); }
            set { SetValue(SortedStateProperty, value); }
        }

        public static readonly BindableProperty IsSortedProperty = BindableProperty.Create(nameof(IsSorted), typeof(bool), typeof(SortButton), false, BindingMode.OneWayToSource);

        public bool IsSorted
        {
            get { return (bool)GetValue(IsSortedProperty); }
            set { SetValue(IsSortedProperty, value); }
        }



        public SortButton()
        {
            Style = DefaultSortButtonStyle;
            Clicked += SortButton_Clicked;
        }

        
        public void SortButton_Clicked(object sender, EventArgs e)
        {
            if (SortValue != null)
            {
                if (SortValue.Equals(SortedState?.SortValue))
                {
                    var newDirection = SortedState.SortDirection == SortDirectionEnum.Ascending ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
                    SortedState = new SortState(SortedState.SortValue, newDirection);
                }
                else
                    SortedState = new SortState(SortValue, SortDirectionEnum.Ascending);
            }
        }

        public void OnSortedStateChanged()
        {
            //TODO: Add animation to the change between asc/desc?
            IsSorted = SortValue.Equals(SortedState?.SortValue);

            HelperText = IsSorted
                ? SortedState?.SortDirection == SortDirectionEnum.Descending ? "  \uEDDC" : "  \uEDDB"
                : "";
        }
    }
}
