using System;
using Xamarin.Forms;
using static Common.UI.Controls.ImageButton;

namespace Common.UI.Controls
{
    public class Border : View
    {
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(Border), Color.Transparent, BindingMode.Default);

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(Border), Color.Black, BindingMode.Default);

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly BindableProperty BorderThicknessProperty = BindableProperty.Create(nameof(BorderThickness), typeof(double), typeof(Border), 1.0, BindingMode.Default, null, null);

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(Border), 0.0, BindingMode.Default, null, null);

        /// <summary>
        /// Corner Radius
        /// </summary>
        /// <value>corner radius of the button</value>
        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly BindableProperty HighlightColorProperty = BindableProperty.Create(nameof(HighlightColor), typeof(Color), typeof(Border), Xamarin.Forms.Color.Gray, BindingMode.Default);

        public Color HighlightColor
        {
            get { return (Color)GetValue(HighlightColorProperty); }
            set { SetValue(HighlightColorProperty, value); }
        }

        public static readonly BindableProperty HasMouseOverProperty = BindableProperty.Create(nameof(HasMouseOver), typeof(bool), typeof(Border), false, BindingMode.Default);

        public bool HasMouseOver
        {
            get { return (bool)GetValue(HasMouseOverProperty); }
            set { SetValue(HasMouseOverProperty, value); }
        }

        public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(nameof(ButtonState), typeof(State), typeof(ImageButton), State.Static, BindingMode.OneWayToSource);

        public State ButtonState
        {
            get { return (State)GetValue(ButtonStateProperty); }
            set { SetValue(ButtonStateProperty, value); }
        }

        public static readonly BindableProperty IsHighlightedProperty = BindableProperty.Create(nameof(IsHighlighted), typeof(bool), typeof(ImageButton), false, BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                //((ImageButton)bindable).SetHighlighted((bool)newValue);
            });
        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        public Action ButtonPressed;
        public Action ButtonReleased;
        public Action ButtonSelected;
    }
}
