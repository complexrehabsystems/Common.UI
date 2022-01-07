using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Common.UI.Common;
using Xamarin.Forms;

namespace Common.UI.Controls
{
    public class ImageButton : ContentView
    {
        public enum LayoutOrientation
        {
            Vertical,
            Horizontal,
            HorizontalReversed,
            VerticalReversed
        };

        public enum State
        {
            Static,
            Hover,
            Active,
        };

        #region Events
        public event EventHandler Pressed;
        public event EventHandler Released;
        public event EventHandler Clicked;
        public event EventHandler RepeatedClick;
        #endregion Events

        #region Properties

        public static readonly BindableProperty TooltipProperty = BindableProperty.Create(nameof(Tooltip), typeof(string), typeof(ImageButton), string.Empty, BindingMode.Default);

        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public static readonly BindableProperty ButtonBackgroundColorProperty = BindableProperty.Create(nameof(ButtonBackgroundColor), typeof(Color), typeof(ImageButton), Color.Transparent, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl.Background.Color = (Color)newValue;
            });

        public Color ButtonBackgroundColor
        {
            get { return (Color)GetValue(ButtonBackgroundColorProperty); }
            set { SetValue(ButtonBackgroundColorProperty, value); }
        }

        public static readonly BindableProperty ButtonHighlightColorProperty = BindableProperty.Create(nameof(ButtonHighlightColor), typeof(Color), typeof(ImageButton), Color.FromRgba(1.0, 1.0, 1.0, 0.3), BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl.Highlight.Color = (Color)newValue;
            });

        public Color ButtonHighlightColor
        {
            get { return (Color)GetValue(ButtonHighlightColorProperty); }
            set { SetValue(ButtonHighlightColorProperty, value); }
        }

        public static readonly BindableProperty ButtonBorderColorProperty = BindableProperty.Create(nameof(ButtonBorderColor), typeof(Color), typeof(ImageButton), Color.Transparent, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl.Background.BorderColor = (Color)newValue;
            });

        public Color ButtonBorderColor
        {
            get { return (Color)GetValue(ButtonBorderColorProperty); }
            set { SetValue(ButtonBorderColorProperty, value); }
        }

        public static readonly BindableProperty BorderThicknessProperty = BindableProperty.Create(nameof(BorderThickness), typeof(double), typeof(ImageButton), 1.0, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl.Background.BorderThickness = ctrl.Highlight.BorderThickness = (double)newValue;
            });

        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(ImageButton), 0.0, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl.Background.CornerRadius = ctrl.Highlight.CornerRadius = (double)newValue;
            });

        /// <summary>
        /// Corner Radius
        /// </summary>
        /// <value>corner radius of the button</value>
        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Left Padding
        /// </summary>
        /// <value>space between the left side of the button and the start of the button image/text</value>
        public int LeftPadding
        {
            get { return (int)GetValue(LeftPaddingProperty); }
            set { SetValue(LeftPaddingProperty, value); }
        }

        public static readonly BindableProperty LeftPaddingProperty = BindableProperty.Create(nameof(LeftPadding), typeof(int), typeof(ImageButton), -1, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl._leftPadding = (int)newValue < 0 ? GridLength.Star : (int)newValue;
                //ctrl._leftPadding = (int)newValue;
                ctrl.UpdateControls();
            });

        /// <summary>
        /// Right Padding
        /// </summary>
        /// <value>space between the right side of the button and the end of the button image/text</value>
        public int RightPadding
        {
            get { return (int)GetValue(RightPaddingProperty); }
            set { SetValue(RightPaddingProperty, value); }
        }

        public static readonly BindableProperty RightPaddingProperty = BindableProperty.Create(nameof(RightPadding), typeof(int), typeof(ImageButton), -1, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl._rightPadding = (int)newValue < 0 ? GridLength.Star : (int)newValue;
                ctrl.UpdateControls();
            });

        /// <summary>
        /// Top Padding
        /// </summary>
        /// <value>space between the top of the button and the top of the button image/text</value>
        public int TopPadding
        {
            get { return (int)GetValue(TopPaddingProperty); }
            set { SetValue(TopPaddingProperty, value); }
        }

        public static readonly BindableProperty TopPaddingProperty = BindableProperty.Create(nameof(TopPadding), typeof(int), typeof(ImageButton), -1, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl._topPadding = (int)newValue < 0 ? GridLength.Star : (int)newValue;
                ctrl.UpdateControls();
            });

        /// <summary>
        /// Bottom Padding
        /// </summary>
        /// <value>space between the bottom of the button and the bottom of the button image/text</value>
        public int BottomPadding
        {
            get { return (int)GetValue(BottomPaddingProperty); }
            set { SetValue(BottomPaddingProperty, value); }
        }

        public static readonly BindableProperty BottomPaddingProperty = BindableProperty.Create(nameof(BottomPadding), typeof(int), typeof(ImageButton), -1, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;
                ctrl._bottomPadding = (int)newValue < 0 ? GridLength.Star : (int)newValue;
                ctrl.UpdateControls();
            });

        /// <summary>
        /// TextWidth
        /// </summary>
        /// <value>Fine tune the middle column of the image button's width</value>
        public int TextWidth
        {
            get { return (int)GetValue(TextWidthProperty); }
            set { SetValue(TextWidthProperty, value); }
        }

        public static readonly BindableProperty TextWidthProperty = BindableProperty.Create(nameof(TextWidth), typeof(int), typeof(ImageButton), -1, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._textWidth = (int)newValue < 0 ? GridLength.Auto : (int)newValue;
                    ctrl.UpdateControls();
                });



        public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(ImageSource), typeof(ImageButton), null, BindingMode.TwoWay,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._image.Source = (ImageSource)newValue;
                    ctrl.UpdateControls();
                });

        /// <summary>
        /// Gets or sets the Icon of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }


        /// <summary>
        /// The Text property.
        /// </summary>
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ImageButton), string.Empty, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._label.Text = (string)newValue;
                    ctrl.UpdateControls();
                });

        /// <summary>
        /// Gets or sets the Text of theImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly BindableProperty LineBreakModeTextProperty = BindableProperty.Create(nameof(LineBreakModeText), typeof(LineBreakMode), typeof(ImageButton), LineBreakMode.WordWrap, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._label.LineBreakMode = (LineBreakMode)newValue;
                });

        public LineBreakMode LineBreakModeText
        {
            get { return (LineBreakMode)GetValue(LineBreakModeTextProperty); }
            set { SetValue(LineBreakModeTextProperty, value); }
        }

        /// <summary>
        /// The FontFamily property.
        /// </summary>
        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(ImageButton), null, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._label.FontFamily = (string)newValue;
                });

        /// <summary>
        /// Gets or sets the FontFamily of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// The FontAttributes property.
        /// </summary>
        public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(ImageButton), FontAttributes.None, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._label.FontAttributes = (FontAttributes)newValue;
                });

        /// <summary>
        /// Gets or sets the UnderlineThickness of the ImageButton instance.  If <= 0 no underlining will occur.
        /// </summary>
        /// <value>The thickness of the underlining line.</value>
        public double UnderlineThickness
        {
            get { return (double)GetValue(UnderlineThicknessProperty); }
            set { SetValue(UnderlineThicknessProperty, value); }
        }

        /// <summary>
        /// The FontAttributes property.
        /// </summary>
        public static readonly BindableProperty UnderlineThicknessProperty = BindableProperty.Create(nameof(UnderlineThickness), typeof(double), typeof(ImageButton), 0.0, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._underline.HeightRequest = (double)newValue;
                });

        /// <summary>
        /// Gets or sets the FontAttributes of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public FontAttributes FontAttributes
        {
            get { return (FontAttributes)GetValue(FontAttributesProperty); }
            set { SetValue(FontAttributesProperty, value); }
        }

        /// <summary>
        /// The Command property.
        /// </summary>
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageButton), null, BindingMode.Default, null);

        /// <summary>
        /// Gets or sets the Command of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// The CommandParameter property.
        /// </summary>
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ImageButton), null, BindingMode.Default, null);

        /// <summary>
        /// Gets or sets the CommandParameter of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// The Command property.
        /// </summary>
        public static readonly BindableProperty CommandRepeatProperty = BindableProperty.Create(nameof(CommandRepeat), typeof(ICommand), typeof(ImageButton), null, BindingMode.Default, null);

        /// <summary>
        /// Gets or sets the Command of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public ICommand CommandRepeat
        {
            get { return (ICommand)GetValue(CommandRepeatProperty); }
            set { SetValue(CommandRepeatProperty, value); }
        }

        public static readonly BindableProperty RepeatDelayProperty = BindableProperty.Create(nameof(RepeatDelay), typeof(int), typeof(ImageButton), 300, BindingMode.Default, null);

        /// <summary>
        /// Gets or sets the Command of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public int RepeatDelay
        {
            get { return (int)GetValue(RepeatDelayProperty); }
            set { SetValue(RepeatDelayProperty, value); }
        }

        /// <summary>
        /// The TextColor property.
        /// </summary>
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ImageButton), Color.Black, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._textColor = (Color)newValue;
                    ctrl._label.TextColor = (Color)newValue;
                    ctrl._helperText.TextColor = (Color)newValue;
                    ctrl._underline.BackgroundColor = (Color)newValue;
                });

        /// <summary>
        /// Gets or sets the TextColor of the ImageButton instance.
        /// </summary>
        /// <value>The color of the button.</value>
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        /// <summary>
        /// The TextHighlightColor property.
        /// </summary>
        public static readonly BindableProperty TextHighlightColorProperty = BindableProperty.Create(nameof(TextHighlightColor), typeof(Color), typeof(ImageButton), Color.Black, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._textHighlightColor = (Color)newValue;
                });

        /// <summary>
        /// Gets or sets the TextHighlightColor of the ImageButton instance.
        /// </summary>
        /// <value>The highlighting color of the button text.</value>
        public Color TextHighlightColor
        {
            get { return (Color)GetValue(TextHighlightColorProperty); }
            set { SetValue(TextHighlightColorProperty, value); }
        }

        /// <summary>
        /// The FontSize property.
        /// </summary>
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(ImageButton), Font.Default.FontSize, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._label.FontSize = (double)newValue;
                });

        /// <summary>
        /// Gets or sets the FontSize of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }


        /// <summary>
        /// The InternalSpacing property.
        /// </summary>
        public static readonly BindableProperty InternalSpacingProperty = BindableProperty.Create(nameof(InternalSpacing), typeof(int), typeof(ImageButton), 2, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ImageButton)bindable;

                if (ctrl.Orientation == LayoutOrientation.Vertical || ctrl.Orientation == LayoutOrientation.VerticalReversed)
                    ctrl._layoutRoot.RowSpacing = (int)newValue;
                else
                    ctrl._layoutRoot.ColumnSpacing = (int)newValue;

            });

        /// <summary>
        /// Gets or sets the InternalSpacing of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public int InternalSpacing
        {
            get { return (int)GetValue(InternalSpacingProperty); }
            set { SetValue(InternalSpacingProperty, value); }
        }


        public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(LayoutOrientation), typeof(ImageButton), LayoutOrientation.Vertical, BindingMode.Default,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl.UpdateControls();
                });

        public LayoutOrientation Orientation
        {
            get { return (LayoutOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #region HelperText
        /// <summary>
        /// The Text property.
        /// </summary>
        public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(nameof(HelperText), typeof(string), typeof(ImageButton), string.Empty, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._helperText.Text = (string)newValue;
                    ctrl.UpdateControls();
                });

        /// <summary>
        /// Gets or sets the Text of theImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public string HelperText
        {
            get { return (string)GetValue(HelperTextProperty); }
            set { SetValue(HelperTextProperty, value); }
        }

        /// <summary>
        /// The FontFamily property.
        /// </summary>
        public static readonly BindableProperty HelperTextFontFamilyProperty = BindableProperty.Create(nameof(HelperTextFontFamily), typeof(string), typeof(ImageButton), null, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._helperText.FontFamily = (string)newValue;
                });

        /// <summary>
        /// Gets or sets the FontFamily of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public string HelperTextFontFamily
        {
            get { return (string)GetValue(HelperTextFontFamilyProperty); }
            set { SetValue(HelperTextFontFamilyProperty, value); }
        }

        /// <summary>
        /// The FontAttributes property.
        /// </summary>
        public static readonly BindableProperty HelperTextFontAttributesProperty = BindableProperty.Create(nameof(HelperTextFontAttributes), typeof(FontAttributes), typeof(ImageButton), FontAttributes.None, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._helperText.FontAttributes = (FontAttributes)newValue;
                });

        /// <summary>
        /// Gets or sets the FontAttributes of the ImageButton instance.
        /// </summary>
        /// <value>The color of the buton.</value>
        public FontAttributes HelperTextFontAttributes
        {
            get { return (FontAttributes)GetValue(HelperTextFontAttributesProperty); }
            set { SetValue(HelperTextFontAttributesProperty, value); }
        }

        /// <summary>
        /// The FontSize property.
        /// </summary>
        public static readonly BindableProperty HelperTextFontSizeProperty = BindableProperty.Create(nameof(HelperTextFontSize), typeof(double), typeof(ImageButton), 10.0, BindingMode.Default,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (ImageButton)bindable;
                    ctrl._helperText.FontSize = (double)newValue;
                });
        /// <summary>
        /// Gets or sets the Helper Text FontSize of the ImageButton instance.double
        /// </summary>
        /// <value>The color of the buton.</value>
        public double HelperTextFontSize
        {
            get { return (double)GetValue(HelperTextFontSizeProperty); }
            set { SetValue(HelperTextFontSizeProperty, value); }
        }



        #endregion


        public static readonly BindableProperty ButtonStateProperty = BindableProperty.Create(nameof(ButtonState), typeof(State), typeof(ImageButton), State.Static, BindingMode.OneWayToSource);

        public State ButtonState
        {
            get { return (State)GetValue(ButtonStateProperty); }
            set { SetValue(ButtonStateProperty, value); }
        }

        public static readonly BindableProperty IsHighlightedProperty = BindableProperty.Create(nameof(IsHighlighted), typeof(bool), typeof(ImageButton), false, BindingMode.TwoWay,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ((ImageButton)bindable).SetHighlighted((bool)newValue);
            });
        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        public Border Background { get; set; }
        private Border Highlight { get; set; }

        #endregion
        private Grid _layoutRoot;
        private Image _image;
        private Label _label;
        private Label _helperText;
        private BoxView _underline;
        private Color _textColor = Color.Black;
        private Color? _textHighlightColor = null;

        private float spacer => 4;
        public double TranslationTravel => 2;
        public int RepeatCount { get; private set; }

        private GridLength _leftPadding = GridLength.Star;
        private GridLength _rightPadding = GridLength.Star;
        private GridLength _topPadding = GridLength.Star;
        private GridLength _bottomPadding = GridLength.Star;

        //used if the automatic grid length is too long
        private GridLength _textWidth = GridLength.Auto;
        protected bool ValidImage => _image.Source != null;
        protected bool ValidLabel => string.IsNullOrWhiteSpace(_label.Text) == false;

        private bool _continueTimer;

        public ImageButton()
        {
            RepeatCount = 0;
            BackgroundColor = Xamarin.Forms.Color.Transparent;
            HorizontalOptions = LayoutOptions.StartAndExpand;
            VerticalOptions = LayoutOptions.StartAndExpand;

            _layoutRoot = new Grid
            {

                InputTransparent = Device.RuntimePlatform == Device.Android ? false : true,
                //Padding = new Thickness(spacer),
                RowSpacing = 2,
                ColumnSpacing = 2,
                IsClippedToBounds = false,
                ColumnDefinitions = {
                    new ColumnDefinition{Width = _leftPadding},
                    new ColumnDefinition{Width = _textWidth},
                    new ColumnDefinition{Width = _rightPadding},
                },
                RowDefinitions =
                {
                    new RowDefinition{Height = _topPadding},
                    new RowDefinition{Height = GridLength.Auto},
                    new RowDefinition{Height = GridLength.Auto},
                    new RowDefinition{Height = _bottomPadding}
                }

            };

            Background = new Border
            {
                InputTransparent = Device.RuntimePlatform == Device.Android? false: true,
                CornerRadius = CornerRadius,
                BorderThickness = BorderThickness,
                Color = ButtonBackgroundColor,
                BorderColor = ButtonBorderColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };

            if(Device.RuntimePlatform == Device.Android)
            {
                Background.ButtonPressed = HandleAndroidButtonPressed;
                Background.ButtonReleased = HandleAndroidButtonReleased;
                Background.ButtonSelected = HandleAndroidButtonSelected;
            }

            Highlight = new Border
            {
                InputTransparent = true,
                CornerRadius = CornerRadius,
                BorderThickness = BorderThickness,
                Color = ButtonHighlightColor,
                BorderColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                IsVisible = false,
            };

            _image = new Image
            {
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };

            _label = new Label
            {
                InputTransparent = true,
                LineBreakMode = LineBreakModeText,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                VerticalTextAlignment = TextAlignment.Center,
                //BackgroundColor = Color.Red, // uncomment to debug layout issues
            };

            _helperText = new Label
            {
                InputTransparent = true,
                FontSize = 10,
                LineBreakMode = LineBreakMode.NoWrap,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                //BackgroundColor = Color.Red, // uncomment to debug layout issues
            };

            _underline = new BoxView
            {
                VerticalOptions = LayoutOptions.End,
                HeightRequest = 0
            };

            Content = _layoutRoot;

        }

        private void HandleAndroidButtonPressed()
        {
            IsHighlighted = Background.IsHighlighted;
            ButtonState = Background.ButtonState;
            ButtonPressed();
        }

        private void HandleAndroidButtonReleased()
        {
            IsHighlighted = Background.IsHighlighted;
            ButtonState = Background.ButtonState;
            ButtonReleased();
        }

        private void HandleAndroidButtonSelected()
        {
            IsHighlighted = Background.IsHighlighted;
            ButtonState = Background.ButtonState;
            ButtonSelected();
        }

        private void UpdateControls()
        {
            _layoutRoot.Children.Clear();

            UpdateLayouts();

            _layoutRoot.RowSpacing = 0;
            _layoutRoot.ColumnSpacing = 0;

            bool allowedHelperText = false;

            if (ValidImage && ValidLabel && Orientation == LayoutOrientation.Vertical)
            {
                _layoutRoot.Add(_image, 1, 1);
                _layoutRoot.Add(_underline, 1, 2);
                _layoutRoot.Add(_label, 1, 2);
                _layoutRoot.RowSpacing = InternalSpacing;
            }
            else if (ValidImage && ValidLabel && Orientation == LayoutOrientation.VerticalReversed)
            {
                _layoutRoot.Add(_label, 1, 1);
                _layoutRoot.Add(_underline, 1, 1);
                _layoutRoot.Add(_image, 1, 2);
                _layoutRoot.RowSpacing = InternalSpacing;
            }
            else if (ValidImage && ValidLabel && Orientation == LayoutOrientation.HorizontalReversed)
            {
                _layoutRoot.Add(_label, 1, 1);
                _layoutRoot.Add(_underline, 1, 1);
                _layoutRoot.Add(_image, 2, 1);
                _layoutRoot.ColumnSpacing = InternalSpacing;
            }
            else if (ValidImage && ValidLabel && Orientation == LayoutOrientation.Horizontal)
            {
                _layoutRoot.Add(_image, 1, 1);
                _layoutRoot.Add(_underline, 2, 1);
                _layoutRoot.Add(_label, 2, 1);
                _layoutRoot.ColumnSpacing = InternalSpacing;
            }
            else if(ValidLabel)
            {
                _layoutRoot.Add(_underline, 1, 1);
                _layoutRoot.Add(_label, 1, 1);
                allowedHelperText = true;
            }
            else if (ValidImage)
            {
                _layoutRoot.Add(_image, 1, 1);
                allowedHelperText = true;
            }

            if(allowedHelperText && string.IsNullOrEmpty(_helperText.Text) == false)
            {
                if(Orientation == LayoutOrientation.Vertical)
                {
                    _layoutRoot.Add(_helperText, 1, 2);
                    _layoutRoot.RowSpacing = InternalSpacing;
                }
                else if (Orientation == LayoutOrientation.VerticalReversed)
                {
                    _layoutRoot.Add(_helperText, 1, 1);
                    if (ValidImage)
                    {
                        Grid.SetRow(_image, 2);
                    }
                    else if (ValidLabel)
                    {
                        Grid.SetRow(_label, 2);
                    }

                    _layoutRoot.RowSpacing = InternalSpacing;
                }
                else if(Orientation == LayoutOrientation.HorizontalReversed)
                {
                    _layoutRoot.Add(_helperText, 1, 1);
                    if(ValidImage)
                    {
                        Grid.SetColumn(_image, 2);
                    }
                    else if(ValidLabel)
                    {
                        Grid.SetColumn(_label, 2);
                    }
                    
                    _layoutRoot.ColumnSpacing = InternalSpacing;
                }
                else
                {
                    _layoutRoot.Add(_helperText, 2, 1);
                    _layoutRoot.ColumnSpacing = InternalSpacing;
                }
            }

            Grid.SetColumnSpan(Highlight, _layoutRoot.ColumnDefinitions.Count);
            Grid.SetRowSpan(Highlight, _layoutRoot.RowDefinitions.Count);
            _layoutRoot.Children.Insert(0, Highlight);

            Grid.SetColumnSpan(Background, _layoutRoot.ColumnDefinitions.Count);
            Grid.SetRowSpan(Background, _layoutRoot.RowDefinitions.Count);
            _layoutRoot.Children.Insert(0, Background);

            // uncomment to debug layout issues
            //_layoutRoot.BackgroundColor = Color.Green;
            //_layoutRoot.DebugGrid(Color.Blue);

            this.InvalidateLayout();

        }

        private void UpdateLayouts()
        {
            _layoutRoot.ColumnDefinitions.Clear();
            _layoutRoot.RowDefinitions.Clear();

            GridLength leftPaddingLength = _leftPadding;
            GridLength rightPaddingLength = _rightPadding;
            GridLength topPaddingLength = _topPadding;
            GridLength bottomPaddingLength = _bottomPadding;

            // this is done to handle the case if you want a presized button (so inner content needs to be centered) or
            // have a self growing button based on the inner content size
            if (WidthRequest <= 0 && HorizontalOptions.Alignment != LayoutAlignment.Fill)
            {
                leftPaddingLength = _leftPadding.IsAbsolute ? _leftPadding : 4;
                rightPaddingLength = _rightPadding.IsAbsolute ? _rightPadding : 4;
            }


            if(HeightRequest <= 0 && VerticalOptions.Alignment != LayoutAlignment.Fill)
            {
                topPaddingLength = _topPadding.IsAbsolute ? _topPadding : 4;
                bottomPaddingLength = _bottomPadding.IsAbsolute ? _bottomPadding : 4;
            }

            if (Orientation == LayoutOrientation.Vertical || Orientation == LayoutOrientation.VerticalReversed)
            {
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = leftPaddingLength });
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = _textWidth });
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = rightPaddingLength });

                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = topPaddingLength });
                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = bottomPaddingLength });

                _label.HorizontalTextAlignment = TextAlignment.Center;
            }
            else
            {
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = leftPaddingLength });
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                _layoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = rightPaddingLength });

                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = topPaddingLength });
                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                _layoutRoot.RowDefinitions.Add(new RowDefinition { Height = bottomPaddingLength });
            }

  
        }

        private void SetHighlighted(bool highlighted)
        {
            Highlight.IsVisible = highlighted;
            _label.TextColor = highlighted ? (_textHighlightColor ?? _textColor): _textColor;
            _helperText.TextColor = highlighted ? (_textHighlightColor ?? _textColor) : _textColor;
            _underline.BackgroundColor = highlighted ? (_textHighlightColor ?? _textColor) : _textColor;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == ContentView.HeightRequestProperty.PropertyName)
            {
                _layoutRoot.HeightRequest = HeightRequest;
                UpdateLayouts();
            }
            else if (propertyName == ContentView.WidthRequestProperty.PropertyName)
            {
                _layoutRoot.WidthRequest = WidthRequest;
                UpdateLayouts();
            }
        }


        public void ButtonPressed()
        {
            _layoutRoot.TranslateTo(TranslationTravel, TranslationTravel, 10);

            Pressed?.Invoke(this, EventArgs.Empty);

            RepeatCount = 0;
            _continueTimer = true;

            if (RepeatedClick != null || CommandRepeat != null)
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(RepeatDelay), () =>
                {
                    if (!_continueTimer)
                        return false;

                    RepeatCount++;

                    RepeatedClick?.Invoke(this, EventArgs.Empty);

                    if (CommandRepeat != null && CommandRepeat.CanExecute(RepeatCount))
                        CommandRepeat.Execute(RepeatCount);

                    return true;
                });

                RepeatedClick?.Invoke(this, EventArgs.Empty);

                if (CommandRepeat != null && CommandRepeat.CanExecute(RepeatCount))
                    CommandRepeat.Execute(RepeatCount);
            }
        }

        public void ButtonReleased()
        {
            _layoutRoot.TranslateTo(0, 0, 10);

            _continueTimer = false;

            Released?.Invoke(this, EventArgs.Empty);
        }

        public void ButtonSelected()
        {
            _continueTimer = false;

            if (Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);

            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
