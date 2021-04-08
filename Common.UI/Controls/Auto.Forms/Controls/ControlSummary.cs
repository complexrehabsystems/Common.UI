using CrsCommon.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Controls.Auto.Forms
{
    public class ControlSummary : ControlBase
    {
        protected object _bindingContext;

        protected Grid _controlGrid;

        public ControlSummary(ControlConfig config, object bindingContext) : base(config)
        {
            _bindingContext = bindingContext;
        }

        protected override View InitializeControl()
        {
            var g = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.StartAndExpand,
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition {Width = GridLength.Auto },
                    new ColumnDefinition {Width = new GridLength(3, GridUnitType.Star)},
                },
                RowDefinitions =
                {
                    new RowDefinition {Height = GridLength.Auto },
                }
            };

            var label = CreateLabel(Label, LabelStyle, _attribute.Orientation, 0, 0) as Xamarin.Forms.Label;
            label.VerticalOptions = LayoutOptions.Start;
            label.VerticalTextAlignment = TextAlignment.Start;

            g.Children.Add(label);

            var box = new BoxView
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.FillAndExpand,
                WidthRequest = 2,
                HeightRequest = 4, // NOTE: have to set a minimum here or else the layout will default to a minimum amount
                BackgroundColor = _config.SeparatorColor,
            };
            Grid.SetColumn(box, 1);
            g.Children.Add(box);

            _controlGrid = new Grid
            {
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                RowDefinitions =
                {
                    new RowDefinition {Height = GridLength.Auto },
                }
            };
            Grid.SetColumn(_controlGrid, 2);
            g.Children.Add(_controlGrid);

            var v = CreateControl(BindingName, _propertyType);
            _controlGrid.Children.Add(v);            

            return g;
        }

        public override void SetGroupedProperty(PropertyInfo property)
        {
            if (_config.Attribute.Type != AutoFormsType.Group)
                return;

            var props = AttributeHelper.GetAttributes<AutoFormsAttribute>(property);
            if (props == null)
                return;

            var attribute = ControlBase.GetFilteredAttribute(_config.Filter, props);
            if (attribute == null)
                return;

            string format = null;

            if(string.IsNullOrEmpty(attribute.Label) == false)
            {
                format = attribute.Label +": {0}";
            }

            var v = CreateItemLabel(property.Name, property.PropertyType, LabelStyle, format);
            if (v == null)
                return;

            _controlGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(v, _controlGrid.RowDefinitions.Count - 1);

            _controlGrid.Children.Add(v);
        }

        protected override View CreateControl(string bindingName, Type fieldType)
        {
            Debug.WriteLine($"ControlSummary.CreateControl: {bindingName}");
            var isGenericType = fieldType.IsGenericType;
            Style style = null;
            object obj = null;

            if (isGenericType)
            {
                var genericTypeDefinition = fieldType.GetGenericTypeDefinition();

                Debug.WriteLine($"isGenericType {isGenericType}, type def: {genericTypeDefinition}");

                if (genericTypeDefinition == typeof(IList<>) || genericTypeDefinition == typeof(ObservableCollection<>))
                {
                    var g = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        RowSpacing = 2,
                    };

                    var list = _bindingContext as System.Collections.IEnumerable;
                    if (list == null)
                        return g;

                    foreach(var item in list)
                    {
                        var props = AttributeHelper.GetPropertyAttributes<AutoFormsAttribute>(item.GetType());

                        foreach (var p in props)
                        {
                            var property = p.Item1;
                            var attribute = ControlBase.GetFilteredAttribute(_config.Filter, p.Item2);

                            if (property == null || attribute == null)
                                continue;

                            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                            if (string.IsNullOrEmpty(attribute.ItemStyle) == false &&
                                Application.Current.Resources.TryGetValue(attribute.ItemStyle, out obj))
                                style = obj as Style;
                            else
                                style = null;

                            var l = CreateItemLabel(property.Name, property.PropertyType, style ?? _config.LabelStyle);
                            l.BindingContext = item;
                            Grid.SetRow(l, g.RowDefinitions.Count - 1);
                            g.Children.Add(l);
                        }

                        // add a spacer between items
                        g.RowDefinitions.Add(new RowDefinition { Height = 4 });
                    }

                    // don't need that last spacer
                    if(g.RowDefinitions.Count >= 1)
                        g.RowDefinitions.RemoveAt(g.RowDefinitions.Count - 1);

                    return g;

                }
                    
            }

            return CreateItemLabel(bindingName, fieldType, _itemStyle ?? _config.LabelStyle);
        }

        protected Label CreateItemLabel(string binding, Type propertyType, Style style, string format = null)
        {
            var l = new Label
            {
                Style = style,
                VerticalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.Start,
                HorizontalOptions = LayoutOptions.Fill,
                HorizontalTextAlignment = TextAlignment.Start,
                LineBreakMode = LineBreakMode.WordWrap,
            };
            l.SetBinding(Xamarin.Forms.Label.TextProperty, new Binding(binding, BindingMode.Default, new DisplayConverter(), propertyType)
            {
                StringFormat = format
            });

            return l;
        }

    }
}
