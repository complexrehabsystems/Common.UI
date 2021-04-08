using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using Xamarin.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Common.Common;
using System.Windows.Input;
using Common.Controls.Auto.Forms.Attributes;
using Common.Controls.Auto.Forms.Controls;
using Common.Controls.Auto.Forms.Validation;

/* Example usage for the AutoForms Control
 
     public class TestAutoForms
    {
        public enum PatientOptions
        {
            [Description("Male")]
            Male,
            [Description("Female")]
            Female,
            [Description("Shark")]
            Shark,
        }

        public enum PatientOptionsOther
        {
            [Description("I am a man")]
            Male,
            [Description("I am a woman")]
            Female,
        }

        [AutoForms("Testing an Editor")]
        public string TestEditor { get; set; }

        [AutoForms("Try a checkbox")]
        public bool TestBoolean { get; set; } = true;

        [AutoForms("Combo box", AutoFormsType.Combo)]
        public List<string> PickerStrings { get; set; } = new List<string>{ "First", "Second", "Third" };

        [AutoForms("Combo box")]
        public int SelectedComboBoxItem { get; set; } = 2;

        [AutoForms("Large Text With Placeholder", AutoFormsType.EntryLarge, placeholder:"Enter your text here")]
        public string LargeTextWithPlaceholder { get; set; }

        [AutoForms("Radio button one", AutoFormsType.Radio)]
        public PatientOptions child { get; set; } = PatientOptions.Male;

        [AutoForms("Radio button two", AutoFormsType.Radio)]
        public PatientOptions child1 { get; set; } = PatientOptions.Female;

        [AutoForms("Radio button three", AutoFormsType.Radio)]
        public PatientOptions child2 { get; set; } = PatientOptions.Shark;

        [AutoForms("Radio button four", AutoFormsType.Radio)]
        public PatientOptions child3 { get; set; } = PatientOptions.Female;

        [AutoForms("Radio button two", AutoFormsType.Radio, orientation: AutoFormsOrientation.Vertical)]
        public PatientOptionsOther Mother { get; set; } = PatientOptionsOther.Female;
    }
     * */

namespace Common.Controls.Auto.Forms
{

    public class AutoForms : ContentView
    {
        #region Properties
        public static readonly BindableProperty ValidationProperty = BindableProperty.Create(nameof(Validation), typeof(AutoFormsValidation), typeof(AutoForms), null, BindingMode.OneWayToSource, null, null);

        public AutoFormsValidation Validation
        {
            get { return (AutoFormsValidation)GetValue(ValidationProperty); }
            set { SetValue(ValidationProperty, value); }
        }

        public static readonly BindableProperty LabelStyleProperty = BindableProperty.Create(nameof(LabelStyle), typeof(Style), typeof(AutoForms), null, BindingMode.Default, null, null);

        public Style LabelStyle
        {
            get { return (Style)GetValue(LabelStyleProperty); }
            set { SetValue(LabelStyleProperty, value); }
        }

        public static readonly BindableProperty EditorStyleProperty = BindableProperty.Create(nameof(EditorStyle), typeof(Style), typeof(AutoForms), null, BindingMode.Default, null, null);

        public Style EditorStyle
        {
            get { return (Style)GetValue(EditorStyleProperty); }
            set { SetValue(EditorStyleProperty, value); }
        }

        public static readonly BindableProperty SeparatorColorProperty = BindableProperty.Create(nameof(SeparatorColor), typeof(Color), typeof(AutoForms), Color.FromRgb(179, 179, 179), BindingMode.Default, null, null);

        public Color SeparatorColor
        {
            get { return (Color)GetValue(SeparatorColorProperty); }
            set { SetValue(SeparatorColorProperty, value); }
        }

        public static readonly BindableProperty SummaryModeProperty = BindableProperty.Create(nameof(SummaryMode), typeof(bool), typeof(AutoForms), false, BindingMode.Default, null, null);

        public bool SummaryMode
        {
            get { return (bool)GetValue(SummaryModeProperty); }
            set { SetValue(SummaryModeProperty, value); }
        }

        public static readonly BindableProperty FilterTypeProperty = BindableProperty.Create(nameof(FilterType), typeof(string), typeof(AutoForms), null, BindingMode.Default);

        public string FilterType
        {
            get { return (string)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        public static readonly BindableProperty FilterProperty = BindableProperty.Create(nameof(Filter), typeof(string), typeof(AutoForms), null, BindingMode.Default,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var ctrl = (AutoForms)bindable;
                ctrl.RebuildForms();
            });

        public double RowSpacing
        {
            get { return (double)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly BindableProperty LayoutHorizontalPercentageProperty = BindableProperty.Create(nameof(LayoutHorizontalPercentage), typeof(double), typeof(AutoForms), 0.3, BindingMode.Default);

        public double LayoutHorizontalPercentage
        {
            get { return (double)GetValue(LayoutHorizontalPercentageProperty); }
            set { SetValue(LayoutHorizontalPercentageProperty, value); }
        }
        #endregion

        protected Grid LayoutRoot => (Content is ScrollView ? ((ScrollView)Content).Content as Grid : Content as Grid);

        public AutoForms()
        {
            Content = new ScrollView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Content = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    RowSpacing = 0,
                }
            };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            RebuildForms();     

        }

        void RebuildForms()
        {
            // first we need to check if we want a scollable view or we just fill the content
            InitializeContent();

            var g = LayoutRoot;

            g.Children.Clear();
            g.RowDefinitions.Clear();

            if (BindingContext == null)
                return;

            BuildForms(BindingContext);
        }

        void InitializeContent()
        {
            Content = new ScrollView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Content = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    RowSpacing = 0,
                }
            };

            if (BindingContext == null)
                return;

            bool hasSpannedList = false;
            var props = BindingContext.GetPropertyAttributes<AutoFormsListAttribute>();

            if(props != null)
            {

                int filter = GetFilter();

                foreach (var prop in props)
                {
                    var property = prop.Item1;
                    var attribute = ControlBase.GetFilteredAttribute(filter, prop.Item2);

                    if (property == null || attribute == null)
                        continue;

                    if (prop.Item2.Any(x => x.SpanListView) )
                    {
                        hasSpannedList = true;
                        break;
                    }
                }
            }

            if (!hasSpannedList)
                return;

            // if we have a spanned list we don't need that top Scrollview because we're going to fill with a list control
            Content = new Grid
            {
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                RowSpacing = 0,
            };


        }

        void BuildForms(object binding)
        {
            if (binding == null)
                return;

            // this is our way for the binding class to communicate back to this control
            Validation = new AutoFormsValidation(ClearValidation, CheckValidation);

            var fullProps = binding.GetType().GetProperties().ToList();

            var props = binding.GetPropertyAttributes<AutoFormsAttribute>();

            ControlBase item = null;            

            int filter = GetFilter();

            List<string> usedItems = new List<string>();

            foreach (var prop in props)
            {
                var property = prop.Item1;
                var attribute = ControlBase.GetFilteredAttribute(filter, prop.Item2);

                if (property == null || attribute == null)
                    continue;

                // this is a safety so we don't re-use grouped items
                if (usedItems.Any(x => x == property.Name))
                    continue;

                var config = new ControlConfig(LayoutHorizontalPercentage, attribute, LabelStyle, EditorStyle, SeparatorColor, property, filter);

                var type = binding.GetType();

                item = null;

                // we might have to recurse
                if(property.PropertyType.IsClass &&
                   property.PropertyType != type && // no infinite recursion allowed
                   property.PropertyType.FullName.StartsWith("System") == false)
                {
                    var obj = property.GetValue(binding);
                    BuildForms(obj);
                    continue;
                }

                var horizontalGroupAttribute = ControlBase.GetFilteredAttribute<AutoFormsHorizontalGroupAttribute>(filter, property);

                if (SummaryMode)
                {
                    var obj = property.GetValue(binding);

                    item = new ControlSummary(config, obj);
                }
                else if (horizontalGroupAttribute != null)
                {
                    item = new ControlHorizontalGroup(config);
                }
                else
                {
                    item = ControlBase.CreateControl(config);                    
                }

                if (item == null)
                    continue;

                var addItems = new List<ControlBase>
                {
                    item
                };

                usedItems.Add(property.Name);

                item.Initialize();

                if (attribute.Grouped != null && attribute.Grouped.Length > 0)
                {
                    foreach(var groupedItem in attribute.Grouped)
                    {
                        var p = fullProps.FirstOrDefault(x => x.Name == groupedItem);
                        if (p == null)
                            continue;

                        usedItems.Add(p.Name);

                        ControlBase newItem = null;

                        switch(attribute.Type)
                        {
                            case AutoFormsType.Radio:
                                newItem = new ControlRadio(true, config);
                                break;
                            default:
                                item.SetGroupedProperty(p);
                                break;
                        }

                        if(newItem != null)
                        {
                            newItem.Initialize();
                            addItems.Add(newItem);
                        }
                    }

                    
                }
                
                foreach(var i in addItems)
                {
                    //Debug.WriteLine($"AutoForms.BuildForms field:{i.BindingName} label:{i.Label}");                    

                    i.BindingContext = binding;

                    var listAttrib = i.Attribute as AutoFormsListAttribute;

                    AddAutoGridItem(i, listAttrib?.SpanListView ?? false);

                    if(i.HasValidation())
                    {
                        AddAutoGridItem (new ControlValidation(i, UpdateLayout));
                    }
                }

                AddAutoGridItem(new BoxView
                {
                    HeightRequest = 10 // used to fix spacing issues
                });
            }
        }

        protected void AddAutoGridItem(View item, bool hasSpan = false)
        {
            if(hasSpan)
            {
                LayoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }
            else
            {
                LayoutRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            

            Grid.SetRow(item, LayoutRoot.RowDefinitions.Count - 1);
            LayoutRoot.Children.Add(item);
        }

        protected int GetFilter()
        {
            if (string.IsNullOrEmpty(Filter))
                return 0;

            // if the filter is just a direct integer instead of an enum string then just return that value
            if (int.TryParse(Filter, out int val))
                return val;

            if (string.IsNullOrEmpty(FilterType))
                return 0;

            var enumType = EnumHelper.GetEnumType(FilterType);
            if (enumType == null)
                return 0;

            foreach (var key in Enum.GetValues(enumType))
            {
                var name = Enum.GetName(enumType, key);
                if (name == Filter)
                {
                    return Convert.ToInt32(key);
                }
            }

            return 0;
        }

        public void ClearValidation()
        {
            foreach(var v in LayoutRoot.Children)
            {
                var vv = v as ControlList;
                if(vv != null)
                {
                    vv.ClearValidation();
                    continue;
                }

                var validation = v as ControlValidation;
                if (validation == null)
                    continue;

                validation.Clear();
            }
        }

        public bool CheckValidation()
        {
            bool isValid = true;
            View firstInvalidItem = null;
            foreach (var v in LayoutRoot.Children)
            {
                var vv = v as ControlList;
                if (vv != null )
                {
                    View view = vv.CheckValidation();
                    if (view != null)
                    {
                        isValid = false;

                        if (firstInvalidItem == null)
                            firstInvalidItem = view;
                    }
                    continue;
                }

                var validation = v as ControlValidation;
                if (validation == null)
                    continue;

                if(!validation.IsValid())
                {
                    isValid = false;

                    if (firstInvalidItem == null)
                        firstInvalidItem = v;
                }
            }

            if(firstInvalidItem != null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var scroll = Content as ScrollView;

                    await scroll.ScrollToAsync(firstInvalidItem, ScrollToPosition.Center, true);
                });
            }

            return isValid;
        }

        public void UpdateLayout() => (Content as ScrollView).ForceLayout();
    }
}
