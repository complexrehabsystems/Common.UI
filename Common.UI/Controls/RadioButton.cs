using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Common.Constants;

namespace Common.Controls
{
    public class RadioButton : ContentView
    {
        public static readonly BindableProperty SelectedProperty = BindableProperty.Create(nameof(Selected), typeof(bool), typeof(RadioButton), false, BindingMode.TwoWay,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (RadioButton)bindable;
                ctrl.Label.Text = (bool)newValue ? Checkmark : Empty;
                ctrl.Label.TextColor = (bool)newValue ? ViewConstants.ColorBlue : Color.Black;
            });

        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        protected Label Label => Content as Label;

        protected const double size = 40;

        public static readonly string Empty = "\uECCA";
        public static readonly string Checkmark = "\uECCB";

        public RadioButton()
        {
            Content = new Label
            {
                HeightRequest = size,
                WidthRequest = size,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 24,
                Text = Selected ? Checkmark : Empty,
                FontAttributes = FontAttributes.None,
                FontFamily = "Segoe MDL2 Assets",
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = new Command(SelectItem)
                    }
                }
            };
        }

        public void SelectItem()
        {
            if (Selected)
                return;

            Selected = true;

            var children = GetParentsChildren();
            if (children == null || children.Count == 0)
                return;

            int selectedIndex = 0;
            int indexCounter = 0;

            // turn the other radio buttons off but also figure out the selected radio index in the list
            foreach(var ele in children)
            {        
                if (ele == this)
                {
                    selectedIndex = indexCounter;
                    continue;
                }

                if (ele is RadioButton rb)
                {
                    rb.Selected = false;
                    indexCounter++;
                }
            }

            if (this.Parent is RadioGroup group)
            {
                group.SelectedIndex = selectedIndex;
            }
            else if(this.Parent?.Parent is RadioGroup grp)
            {
                grp.SelectedIndex = selectedIndex;
            }

        }

        List<View> GetParentsChildren()
        {
            if (this.Parent is Grid grid)
            {
                return grid.Children.ToList();
            }
            else if (this.Parent is StackLayout stack)
            {
                return stack.Children.ToList();
            }
            else if(this.Parent?.Parent is Grid grd)
            {
                return grd.Children.ToList();
            }
            else if (this.Parent?.Parent is StackLayout stk)
            {
                return stk.Children.ToList();
            }
            else
            {
                return null;
            }
        }

    }
}
