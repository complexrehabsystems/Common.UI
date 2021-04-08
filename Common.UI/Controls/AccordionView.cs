using System;
using System.Collections;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Common.UI.Controls
{
    public class AccordionView : Grid
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AccordionView), propertyChanged: OnItemsSourceChanged);
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(AccordionView));
        public static readonly BindableProperty CanExpandMultipleProperty = BindableProperty.Create(nameof(CanExpandMultiple), typeof(bool), typeof(AccordionView), false);
        public static readonly BindableProperty SelectionIndexProperty = BindableProperty.Create(nameof(SelectionIndex), typeof(int), typeof(AccordionView), -1, propertyChanged: OnSelectionIndexChanged);

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public bool CanExpandMultiple
        {
            get { return (bool)GetValue(CanExpandMultipleProperty); }
            set { SetValue(CanExpandMultipleProperty, value); }
        }

        public int SelectionIndex
        {
            get { return (int)GetValue(SelectionIndexProperty); }
            set { SetValue(SelectionIndexProperty, value); }
        }

        static void OnSelectionIndexChanged(BindableObject bindable, object oldVal, object newVal)
        {
            int newIndex = (int)newVal;

            var layout = (AccordionView)bindable;

            for (int i = 0; i < layout.Children.Count; i++)
            {
                var child = layout.Children[i];
                if (child is ExpandableView view)
                {
                    view.IsExpanded = i == newIndex ? true : false;
                }
            }
        }

        static void OnItemsSourceChanged(BindableObject bindable, object oldVal, object newVal)
        {
            IEnumerable newValue = newVal as IEnumerable;
            var layout = (AccordionView)bindable;

            var existingCollection = oldVal as INotifyCollectionChanged;
            if (existingCollection != null)
            {
                existingCollection.CollectionChanged -= layout.OnItemsSourceCollectionChanged;
            }

            var observableCollection = newValue as INotifyCollectionChanged;
            if (observableCollection != null)
            {
                observableCollection.CollectionChanged += layout.OnItemsSourceCollectionChanged;
            }

            layout.Children.Clear();
            layout.SelectionIndex = -1;
            if (newValue != null)
            {
                foreach (var item in newValue)
                {
                    layout.Children.Add(layout.CreateChildView(item));
                }
            }
        }

        View CreateChildView(object item)
        {
            if (ItemTemplate is DataTemplateSelector)
            {
                var dts = ItemTemplate as DataTemplateSelector;
                var itemTemplate = dts.SelectTemplate(item, null);
                itemTemplate.SetValue(BindableObject.BindingContextProperty, item);
                return (View)itemTemplate.CreateContent();
            }
            else
            {
                ItemTemplate.SetValue(BindableObject.BindingContextProperty, item);
                return (View)ItemTemplate.CreateContent();
            }
        }

        void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Children.Clear();
            }

            if (e.OldItems != null && Children.Count > e.OldStartingIndex)
            {
                Children.RemoveAt(e.OldStartingIndex);
            }

            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var item = e.NewItems[i];
                    var view = CreateChildView(item);
                    Children.Insert(e.NewStartingIndex + i, view);
                }
            }
        }

        public AccordionView()
        {
            //Default the spacing when created, allowing consumers to override through bindings should they chose
            ColumnSpacing = 0;
            RowSpacing = 0;

            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        protected void UpdateControls()
        {
            RowDefinitions.Clear();

            if (Children.Count == 0)
                return;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Grid.SetColumn(child, 1);
                Grid.SetRow(child, i);
            }
        }


        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            UpdateControls();
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            if (child is ExpandableView view)
            {
                view.IsExpandedChanged -= ExpandableView_IsExpandedChanged;
                view.IsExpandedChanged += ExpandableView_IsExpandedChanged;
            }

            UpdateControls();
        }

        protected override void OnChildRemoved(Element child)
        {
            base.OnChildRemoved(child);

            if (child is ExpandableView view)
            {
                view.IsExpandedChanged -= ExpandableView_IsExpandedChanged;
            }

            UpdateControls();
        }

        private void ExpandableView_IsExpandedChanged(object sender, EventArgs e)
        {
            if (!CanExpandMultiple && sender is ExpandableView sendingView && sendingView.IsExpanded)
            {
                int selectedIndex = -1;
                for(int i=0; i<Children.Count; i++)
                {
                    var child = Children[i];
                    if (child == sendingView)
                    {
                        selectedIndex = i;
                    }
                    else if (child is ExpandableView view)
                    {
                        view.IsExpanded = false;
                    }
                }
                SelectionIndex = selectedIndex;
            }
        }
    }
}
