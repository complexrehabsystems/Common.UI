using Common.Common;
using Common.Constants;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Common.Controls
{
    public class ExpandableView : StackLayout
    {
        public const string ExpandAnimationName = "ExpandAnimationName";

        /// <summary>
        /// Default properties that can be overriden in derived styles
        /// </summary>
        public readonly static Style DefaultExpandableViewToggleButtonStyle = new Style(typeof(ImageButton))
        {
            Setters =
            {
                new Setter { Property = ImageButton.FontSizeProperty, Value = 12 },
                new Setter { Property = ImageButton.FontFamilyProperty, Value = ViewConstants.FontFamilySegoe },
                new Setter { Property = ImageButton.LeftPaddingProperty, Value = 25 },
                new Setter { Property = ImageButton.RightPaddingProperty, Value = 25 },
                new Setter { Property = ImageButton.ButtonBackgroundColorProperty, Value = Color.Transparent },
                new Setter { Property = ImageButton.ButtonBorderColorProperty, Value = Color.Transparent },
                new Setter { Property = ImageButton.ButtonHighlightColorProperty, Value = Color.Transparent }
            }
        };

        public readonly static Style DefaultExpandableViewBorderStyle = new Style(typeof(Border))
        {
            Setters =
            {
                new Setter { Property = Border.BorderColorProperty, Value = Color.FromRgb(221, 221, 221) },
                new Setter { Property = Border.ColorProperty, Value = Color.Transparent }
            }
        };

        public readonly static Style DefaultExpandableViewHeaderBackgroundStyle = new Style(typeof(CustomBoxView))
        {
            Triggers =
            {
                new Trigger(typeof(CustomBoxView))
                {
                    Property = CustomBoxView.IsMouseOverProperty, Value = true,
                    Setters = { new Setter { Property = BackgroundColorProperty, Value = ViewConstants.ColorExtraTransparentBlue } }
                },
                new Trigger(typeof(CustomBoxView))
                {
                    Property = CustomBoxView.IsMouseOverProperty, Value = false,
                    Setters = { new Setter { Property = BackgroundColorProperty, Value = Color.Transparent } }
                }
            }
        };

        public event EventHandler Tapped;
        public event EventHandler IsExpandedChanged;

        public static readonly BindableProperty CollapsedViewProperty = BindableProperty.Create(nameof(CollapsedView), typeof(View), typeof(ExpandableView), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            (bindable as ExpandableView).SetCollapsedView(oldValue as View);
            (bindable as ExpandableView).UpdateView();
        });

        public static readonly BindableProperty ExpandedViewProperty = BindableProperty.Create(nameof(ExpandedView), typeof(View), typeof(ExpandableView), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            (bindable as ExpandableView).SetExpandedView(oldValue as View);
            (bindable as ExpandableView).UpdateView();
        });

        public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(ExpandableView), default(bool), BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            //need to reset the view and pick up on updates from the children(ie IsVisible) of the ExpandedView
            if ((bool)newValue)
                (bindable as ExpandableView).SetExpandedView((bindable as ExpandableView).ExpandedView);

            (bindable as ExpandableView).UpdateView();      
        });

        public static readonly BindableProperty IsHeaderHighlightedProperty = BindableProperty.Create(nameof(IsHeaderHighlighted), typeof(bool), typeof(ExpandableView), default(bool), BindingMode.TwoWay);

        public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(ExpandableView), default(bool), BindingMode.TwoWay);

        public static readonly BindableProperty IsTouchToExpandEnabledProperty = BindableProperty.Create(nameof(IsTouchToExpandEnabled), typeof(bool), typeof(ExpandableView), true);

        public static readonly BindableProperty IsFullHeaderTouchToCollapseEnabledProperty = BindableProperty.Create(nameof(IsFullHeaderTouchToCollapseEnabled), typeof(bool), typeof(ExpandableView), true);

        public static readonly BindableProperty ExpandAnimationLengthProperty = BindableProperty.Create(nameof(ExpandAnimationLength), typeof(uint), typeof(ExpandableView), 250u);

        public static readonly BindableProperty CollapseAnimationLengthProperty = BindableProperty.Create(nameof(CollapseAnimationLength), typeof(uint), typeof(ExpandableView), 250u);

        public static readonly BindableProperty ExpandAnimationEasingProperty = BindableProperty.Create(nameof(ExpandAnimationEasing), typeof(Easing), typeof(ExpandableView), Easing.Linear);

        public static readonly BindableProperty CollapseAnimationEasingProperty = BindableProperty.Create(nameof(CollapseAnimationEasing), typeof(Easing), typeof(ExpandableView), Easing.Linear);

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ExpandableView), default(object));

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ExpandableView), default(ICommand));
        public static readonly BindableProperty HeaderHeightRequestProperty = BindableProperty.Create(nameof(HeaderHeightRequest), typeof(double), typeof(ExpandableView), 50.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            (bindable as ExpandableView)._toggleLabel.HeightRequest = (double)newValue;
            (bindable as ExpandableView)._header.HeightRequest = (double)newValue;
        });

        public static readonly BindableProperty BorderStyleProperty = BindableProperty.Create(nameof(BorderStyle), typeof(Style), typeof(ExpandableView), DefaultExpandableViewBorderStyle, propertyChanged: (bindable, oldValue, newValue) =>
        {
            (bindable as ExpandableView)._border.Style = (Style)newValue;
        });
        public static readonly BindableProperty ToggleStyleProperty = BindableProperty.Create(nameof(ToggleStyle), typeof(Style), typeof(ExpandableView), DefaultExpandableViewToggleButtonStyle, propertyChanged: (bindable, oldValue, newValue) =>
        {
            (bindable as ExpandableView)._toggleLabel.Style = (Style)newValue;
        });


        private bool _shouldIgnoreAnimation;
        private double _lastVisibleHeight = -1;
        private double _startHeight;
        private double _endHeight;
        private Grid _grid;
        private readonly ImageButton _toggleLabel;
        private Border _border;
        private InputHandler _header;
        public static readonly string ChevronDown = "\uE96E";
        private readonly ExpandableViewHeaderViewModel _headerContext = new ExpandableViewHeaderViewModel();


        public ExpandableView()
        {
            this.IsClippedToBounds = true;

            _grid = new Grid()
            {
                RowSpacing = 0,
                ColumnSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition{Width = GridLength.Auto},
                }
            };

            _border = new Border
            {
                Style = DefaultExpandableViewBorderStyle
            };

            _header = new InputHandler
            {
                //Style = DefaultExpandableViewHeaderBackgroundStyle,
                InputTransparent = false,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.Transparent,
                //BindingContext = _headerContext,
            };
            _header.MouseEntered += () =>
            {
                //Debug.WriteLine("_header.MouseEntered");
                _headerContext.IsMouseOver = true;
            };
            _header.MouseExited += () =>
            {
                //Debug.WriteLine("_header.MouseExited");
                _headerContext.IsMouseOver = false;
            };
            //_header.SetBinding(CustomBoxView.IsMouseOverProperty, nameof(ExpandableViewHeaderViewModel.IsMouseOver));

            _headerContext.IsMouseOverChanged += SetHeaderHighlighted;

            _header.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (!IsExpanded || IsFullHeaderTouchToCollapseEnabled)
                    {
                        if (IsTouchToExpandEnabled)
                            IsExpanded = !IsExpanded;

                        Command?.Execute(CommandParameter ?? (object)IsExpanded);
                        Tapped?.Invoke(this, EventArgs.Empty);
                    }
                })
            });

            _toggleLabel = new ImageButton()
            {
                Text = ChevronDown,
                InputTransparent = false,
                Style = DefaultExpandableViewToggleButtonStyle,
                BindingContext = _headerContext
            };
            _toggleLabel.SetBinding(ImageButton.IsHighlightedProperty, nameof(ExpandableViewHeaderViewModel.IsMouseOver));
            _toggleLabel.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (IsTouchToExpandEnabled)
                        IsExpanded = !IsExpanded;

                    Command?.Execute(CommandParameter ?? (object)IsExpanded);
                    Tapped?.Invoke(this, EventArgs.Empty);
                })
            });
            _grid.Add(_border, 0, 2, 0, 1);
            _grid.Add(_header, 0, 1, 0, 1);
            _grid.Add(_toggleLabel, 1, 0);

            Children.Add(_grid);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            _headerContext.ChildContext = BindingContext;
        }

        public View CollapsedView
        {
            get => GetValue(CollapsedViewProperty) as View;
            set => SetValue(CollapsedViewProperty, value);
        }

        public View ExpandedView
        {
            get => GetValue(ExpandedViewProperty) as View;
            set => SetValue(ExpandedViewProperty, value);
        }

        public bool IsExpanded
        {
            get =>(bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public bool IsHeaderHighlighted
        {
            get => (bool)GetValue(IsHeaderHighlightedProperty);
            set => SetValue(IsHeaderHighlightedProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public bool IsTouchToExpandEnabled
        {
            get => (bool)GetValue(IsTouchToExpandEnabledProperty);
            set => SetValue(IsTouchToExpandEnabledProperty, value);
        }

        public bool IsFullHeaderTouchToCollapseEnabled
        {
            get => (bool)GetValue(IsFullHeaderTouchToCollapseEnabledProperty);
            set => SetValue(IsFullHeaderTouchToCollapseEnabledProperty, value);
        }

        public uint ExpandAnimationLength
        {
            get => (uint)GetValue(ExpandAnimationLengthProperty);
            set => SetValue(ExpandAnimationLengthProperty, value);
        }

        public uint CollapseAnimationLength
        {
            get => (uint)GetValue(CollapseAnimationLengthProperty);
            set => SetValue(CollapseAnimationLengthProperty, value);
        }

        public Easing ExpandAnimationEasing
        {
            get => (Easing)GetValue(ExpandAnimationEasingProperty);
            set => SetValue(ExpandAnimationEasingProperty, value);
        }

        public Easing CollapseAnimationEasing
        {
            get => (Easing)GetValue(CollapseAnimationEasingProperty);
            set => SetValue(CollapseAnimationEasingProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ICommand Command
        {
            get => GetValue(CommandProperty) as ICommand;
            set => SetValue(CommandProperty, value);
        }

        public double HeaderHeightRequest
        {
            get => (double)GetValue(HeaderHeightRequestProperty);
            set => SetValue(HeaderHeightRequestProperty, value);
        }

        public Style BorderStyle
        {
            get => (Style)GetValue(BorderStyleProperty);
            set => SetValue(BorderStyleProperty, value);
        }

        public Style ToggleStyle
        {
            get => (Style)GetValue(ToggleStyleProperty);
            set => SetValue(ToggleStyleProperty, value);
        }

        private void UpdateView()
        {
            if (ExpandedView == null || (!IsExpanded && !ExpandedView.IsVisible))
            {
                IsExpandedChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            ExpandedView.SizeChanged -= OnExpandedViewSizeChanged;

            var isExpanding = ExpandedView.AnimationIsRunning(ExpandAnimationName);
            ExpandedView.AbortAnimation(ExpandAnimationName);

            _startHeight = ExpandedView.IsVisible
                ? Math.Max(ExpandedView.Height - (ExpandedView is Layout el ? el.Padding.Top + el.Padding.Bottom : 0), 0)
                : Math.Max(CollapsedView.Height - (CollapsedView is Layout cl ? cl.Padding.Top + cl.Padding.Bottom : 0), 0);

            if (IsExpanded)
            {
                ExpandedView.IsVisible = true;
                CollapsedView.IsVisible = false;
            }

            _endHeight = _lastVisibleHeight;

            var shouldInvokeAnimation = true;

            if (IsExpanded)
            {
                if (_endHeight <= 0)
                {
                    shouldInvokeAnimation = false;
                    ExpandedView.HeightRequest = -1;
                    ExpandedView.SizeChanged += OnExpandedViewSizeChanged;
                }
            }
            else
            {
                _lastVisibleHeight = _startHeight = !isExpanding
                                    ? Math.Max(ExpandedView.Height - (ExpandedView is Layout lay ? lay.Padding.Top + lay.Padding.Bottom : 0), -1)
                                    : _lastVisibleHeight;
                _endHeight = 0;
            }

            _shouldIgnoreAnimation = Height < 0;

            if (shouldInvokeAnimation)
            {
                InvokeAnimation();
            }
            IsExpandedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetCollapsedView(View oldView) => SetView(oldView, CollapsedView, true);
        private void SetExpandedView(View oldView) => SetView(oldView, ExpandedView);

        private void SetView(View oldView, View newView, bool isCollapsedView = false)
        {
            if (oldView != null)
                _grid.Children.Remove(oldView);

            if (newView != null)
            {
                _grid.Add(newView, 0, 2, 0, 1);


                if (isCollapsedView && IsFullHeaderTouchToCollapseEnabled)
                {
                    // have to put this above the collapsed view because we need to handle taps
                    _grid.Children.Remove(_header);
                    _grid.Add(_header, 0, 0);
                }

                //Move the arrow label to the end of the children to allow it's tap gesture to fire
                _grid.Children.Remove(_toggleLabel);
                _grid.Add(_toggleLabel, 1, 0);

                
            }
        }

        private void OnExpandedViewSizeChanged(object sender, EventArgs e)
        {
            if (ExpandedView.Height <= 0) return;
            ExpandedView.SizeChanged -= OnExpandedViewSizeChanged;
            ExpandedView.HeightRequest = 0;
            _endHeight = ExpandedView.Height;
            InvokeAnimation();
        }

        private void InvokeAnimation()
        {
            if (_shouldIgnoreAnimation)
            {
                ExpandedView.HeightRequest = _endHeight;
                ExpandedView.IsVisible = IsExpanded;
                CollapsedView.IsVisible = !IsExpanded;
                _toggleLabel.Rotation = IsExpanded ? -180.0 : 0.0;
                return;
            }

            var length = ExpandAnimationLength;
            var easing = ExpandAnimationEasing;
            if (!IsExpanded)
            {
                length = CollapseAnimationLength;
                easing = CollapseAnimationEasing;
            }

            if (_lastVisibleHeight > 0)
            {
                length = Math.Max((uint)(length * (Math.Abs(_endHeight - _startHeight) / _lastVisibleHeight)), 1);
            }

            this.Animate(ExpandAnimationName, new Animation(v => ExpandedView.HeightRequest = v, _startHeight, _endHeight), 16, length, easing, (value, interrupted) =>
            {
                if (!interrupted && !IsExpanded)
                {
                    ExpandedView.IsVisible = false;
                    CollapsedView.IsVisible = true;
                }
                else if (IsExpanded)
                {
                    ExpandedView.HeightRequest = -1;//Allow for adjustments based on visibility of controls after we've animated the expansion
                }
            });
            _toggleLabel.RotateTo(IsExpanded ? -180 : 0, length, easing);
        }

        public void SetHeaderHighlighted(object sender, bool highlighted)
        {
            IsHeaderHighlighted = highlighted;
        }
    }

    [AddINotifyPropertyChangedInterfaceAttribute]
    public class ExpandableViewHeaderViewModel
    {
        public EventHandler<bool> IsMouseOverChanged;

        public bool IsMouseOver { get; set; }
        public object ChildContext { get; set; }

        public void OnIsMouseOverChanged()
        {
            IsMouseOverChanged.Invoke(this, IsMouseOver);
        }
    }
}
