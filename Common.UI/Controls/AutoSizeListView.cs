using System;
using System.Collections;
using System.Collections.Specialized;
using Xamarin.Forms;

namespace Common.UI.Controls
{
    public class AutoSizeListView : UnclickableListView
    {
        public static readonly BindableProperty MaxRowsBeforeScrollingProperty = BindableProperty.Create(nameof(MaxRowsBeforeScrolling), typeof(int), typeof(AutoSizeListView),
            default(int), BindingMode.OneWay, null, OnMaxRowsBeforeScrollingChanged);

        public static readonly BindableProperty HeaderHeightProperty = BindableProperty.Create(nameof(HeaderHeight), typeof(int), typeof(AutoSizeListView),
            default(int), BindingMode.OneWay, null, OnHeaderHeightChanged);

        public int MaxRowsBeforeScrolling
        {
            get { return (int)GetValue(MaxRowsBeforeScrollingProperty); }
            set { SetValue(MaxRowsBeforeScrollingProperty, value); }
        }

        public int HeaderHeight
        {
            get { return (int)GetValue(HeaderHeightProperty); }
            set { SetValue(HeaderHeightProperty, value); }
        }

        private INotifyCollectionChanged NotifyCollection;

        public AutoSizeListView()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public static void OnMaxRowsBeforeScrollingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as AutoSizeListView;
            if (view == null)
            {
                return;
            }
            view.UpdateHeight();
        }

        public static void OnHeaderHeightChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as AutoSizeListView;
            if (view == null)
            {
                return;
            }
            view.UpdateHeight();
        }

        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ItemsSourceProperty.PropertyName)
            {
                if (NotifyCollection != null)
                    NotifyCollection.CollectionChanged -= ItemsSource_CollectionChanged;
                NotifyCollection = ItemsSource as INotifyCollectionChanged;
                if (NotifyCollection != null)
                    NotifyCollection.CollectionChanged += ItemsSource_CollectionChanged;

                //UpdateHeight();
                Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                {
                    UpdateHeight();
                    return false;
                });
            }
        }

        protected void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //UpdateHeight();
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                UpdateHeight();
                return false;
            });

        }

        private void UpdateHeight()
        {
            var maxRows = MaxRowsBeforeScrolling;
            var dataHeight = 0;
            if (ItemsSource != null)
            {
                //Currently we will only support ICollection derived objects
                // Both ObservableCollection<T> and List<T> satisify this
                var rowCount = (ItemsSource as ICollection)?.Count ?? MaxRowsBeforeScrolling;
                dataHeight = RowHeight * Math.Min(rowCount, MaxRowsBeforeScrolling);
            }
            MinimumHeightRequest = dataHeight + HeaderHeight;
            HeightRequest = dataHeight + HeaderHeight;
        }
    }
}
