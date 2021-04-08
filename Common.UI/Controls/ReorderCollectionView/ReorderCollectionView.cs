using CrsCommon.Controls.ReorderCollectionView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TouchTracking;
using Xamarin.Forms;
using static CrsCommon.Controls.InputHandler;
using CrsCommon.Common;

namespace CrsCommon.Controls.ReorderCollectionView
{
    public class ReorderCollectionView : ContentView
    {
        public ItemsLayoutOrientation Orientation { get; set; }
        public DataTemplate DropTargetItemTemplate { get; set; }
        public DataTemplate GroupItemTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        public int DragHandleSize { get; set; }

        #region ItemsSource Bindable Property

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<object>), typeof(ReorderCollectionView), default(ObservableCollection<object>), propertyChanged: OnItemsSourcePropertyChanged);

        [HandleProcessCorruptedStateExceptions]
        private static void OnItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            try
            {
                var control = ((ReorderCollectionView)bindable);
                var previous = (ObservableCollection<object>)oldValue;
                if (previous != null) previous.CollectionChanged -= control.ItemsSource_CollectionChanged;
                var value = (ObservableCollection<object>)newValue;
                if (value != null) value.CollectionChanged += control.ItemsSource_CollectionChanged;
                //Device.InvokeOnMainThreadAsync(control.LoadChildren);
                Task.Delay(50);
                control.LoadChildren();
            }
            catch (AccessViolationException ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }

        }

        public ObservableCollection<object> ItemsSource
        {
            get { return (ObservableCollection<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        #endregion

        #region ItemTappedCommand Bindable Property

        public static readonly BindableProperty ItemTappedCommandProperty = BindableProperty.Create(nameof(ItemTappedCommand), typeof(ICommand), typeof(ReorderCollectionView), default(ICommand));

        public ICommand ItemTappedCommand
        {
            get { return (ICommand)GetValue(ItemTappedCommandProperty); }
            set { SetValue(ItemTappedCommandProperty, value); }
        }

        #endregion

        #region ItemDroppedOnTargetCommand Bindable Property

        public static readonly BindableProperty ItemDroppedOnTargetCommandProperty = BindableProperty.Create(nameof(ItemDroppedOnTargetCommand), typeof(ICommand), typeof(ReorderCollectionView), default(ICommand));

        public ICommand ItemDroppedOnTargetCommand
        {
            get { return (ICommand)GetValue(ItemDroppedOnTargetCommandProperty); }
            set { SetValue(ItemDroppedOnTargetCommandProperty, value); }
        }

        #endregion

        private ScrollView _scrollView;
        private AbsoluteLayout _absoluteLayout;
        private InputHandler _inputHandler;

        // Dragging
        private Point touchLocation;
        private List<View> draggingViews;
        private Rectangle originRect;
        private bool isDragging;
        private double lastDistance;
        private bool isDraggingLeftUp;
        private bool draggingIsGroup;
        private int dropIndex;
        private double nearbound;
        private double farbound;
        private const int minDragDistance = 15;
        double _scrollAmount = 40;
        bool _lastScrollDirectionUp = true;
        double _maxDragDistance = 0.0;
        Draggable _draggedControl = null;

        private void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            LoadChildren();
        }

        public ReorderCollectionView()
        {
            

            _absoluteLayout = new AbsoluteLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                InputTransparent = true,
                CascadeInputTransparent = false,
            };

            _inputHandler = new InputHandler
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                
            };
            _inputHandler.TouchBegin += GrabBegin;
            _inputHandler.TouchEnd += GrabEnd;
            _inputHandler.TouchMove += GrabMoved;
            _inputHandler.TouchCancelled += GrabCancelled;

            var g = new Grid
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                Children = { _inputHandler, _absoluteLayout }
            };

            _scrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = g
            };

            Content = _scrollView;
        }

        public void GrabBegin(TouchEventArgs args)
        {
            _scrollAmount = 10;
            _lastScrollDirectionUp = true;
            touchLocation = new Point(args.X, args.Y);
            draggingViews = null;
            isDragging = false;
            _maxDragDistance = 0.0;
            _draggedControl = null;
            foreach (var view in _absoluteLayout.Children)
            {
                var rect = AbsoluteLayout.GetLayoutBounds(view);

                if (rect.Contains(touchLocation))
                {
                    // if we have a draggable controls then we only accept a drag if our touch is on one of them otherwise fail
                    var children = view.GetChildren<Draggable>();
                    if(children?.Count() > 0)
                    {
                        bool dragAccepted = false;
                        foreach(var c in children)
                        {
                            var ptScreen = c.RelativeTo(Content);
                            var r = new Rectangle(ptScreen.X, ptScreen.Y, c.Bounds.Width, c.Bounds.Height);
                            if(r.Contains(touchLocation))
                            {
                                _draggedControl = c;
                                dragAccepted = true;
                                break;
                            }
                        }
                        if (!dragAccepted)
                            continue;
                    }

                    draggingIsGroup = view.BindingContext is IReorderGroupItem;
                    if (draggingIsGroup)
                    {
                        var canMove = ((IReorderGroupItem)view.BindingContext).CanMove;
                        if (!canMove)
                            continue;
                    }

                    // found one, register for pending drag operation
                    draggingViews = new List<View>();
                    draggingViews.Add(view);

                    var item = view.BindingContext;
                    var idx = ItemsSource.IndexOf(item);
                    originRect = rect;

                    if (draggingIsGroup)
                    {
                        // if we're on a group header row,
                        // add all views that belong to the same group to the selection
                        for (int i = idx + 1; i < ItemsSource.Count && !(ItemsSource[i] is IReorderGroupItem); i++)
                        {
                            var childitem = ItemsSource[i];
                            var childview = _absoluteLayout.Children.Single(v => v.BindingContext == childitem);
                            rect = AbsoluteLayout.GetLayoutBounds(childview);
                            draggingViews.Add(childview);
                            originRect.Bottom = rect.Bottom;
                            originRect.Right = rect.Right;
                        }
                    }

                    // notify vm of item tapped
                    if (ItemTappedCommand != null)
                        ItemTappedCommand.Execute(item);

                    // highlite the item if supported
                    if (item is IReorderActiveItem activeItem)
                        activeItem.IsActive = true;

                }
            }
        }

        public void GrabEnd(TouchEventArgs args)
        {
            // stop highlight
            foreach (var child in ItemsSource)
                if (child is IReorderActiveItem activeItem)
                    activeItem.IsActive = false;

            // when releasing
            if (isDragging)
            {
                var item = draggingViews[0].BindingContext;
                var fromindex = ItemsSource.IndexOf(item);

                var target = dropIndex;
                dropIndex = -1;
                draggingViews = null;
                isDragging = false;

                // drop index should be within bounds
                var isTargetOutOfBounds =  target < 0 || target >= ItemsSource.Count;

                if ( isTargetOutOfBounds || fromindex == target)
                {
                    // don't reorder the collection, but do reset the layout
#if DebugOrdering
                        System.Diagnostics.Debug.WriteLine($"Keeping item at {fromindex}");
#endif
                    LayoutChildren(0, 0, Width, Height);
                }
                else if (ItemsSource[target] is IReorderDropTargetItem)
                {
                    // drop the item on a drop target row
#if DebugOrdering
                        System.Diagnostics.Debug.WriteLine($"Executing item dropped command {fromindex}");
#endif
                    ItemDroppedOnTargetCommand?.Execute(item);
                }
                else
                {

                    // move the items in the collection, children will be reloaded
#if DebugOrdering
                        System.Diagnostics.Debug.WriteLine($"Moving {fromindex} => {target}");
#endif
                    ItemsSource.Move(fromindex, target);
                }

                // MessagingCenter.Send<ReorderCollectionView>(this, "DoneDragging"); This causes intermittent crashes
            }   // I don't think I need it anymore, but need to be sure so leaving line here for now.
            
            if (_maxDragDistance < 5.0 && _draggedControl != null && _draggedControl.Command != null)
            {
                // if it was more a click then we should try and run the draggable command
                _draggedControl.Command.Execute(_draggedControl); // we pass in the view in case we need location to put up a popup menu
                _draggedControl = null;
            }
        }

        public void GrabMoved(TouchEventArgs args)
        {
            if (draggingViews != null)
            {

                var distance = Orientation == ItemsLayoutOrientation.Horizontal ?
                                    args.X - touchLocation.X :
                                    args.Y - touchLocation.Y;

                _maxDragDistance = Math.Max(_maxDragDistance, Math.Abs(distance));

                isDraggingLeftUp = distance <= lastDistance;
                lastDistance = distance;

                if (!isDragging)
                {
                    // start dragging only after covering the minimum distance
                    // this is to support tapping an item if we're not dragging
                    if (Math.Abs(distance) < minDragDistance)
                        return;

                    // bring selected views to the front
                    foreach (var view in draggingViews)
                        _absoluteLayout.RaiseChild(view);

                    isDragging = true;
                }

                // calculate the new location, make sure coordinates fall within the required range
                var location = originRect.Location;
                if (Orientation == ItemsLayoutOrientation.Horizontal)
                {
                    location.X += distance;
                    if (location.X < nearbound) location.X = nearbound;
                    if (location.X > farbound) location.X = farbound;
                }
                else
                {
                    location.Y += distance;
                    if (location.Y < nearbound) location.Y = nearbound;
                    if (location.Y > farbound) location.Y = farbound;
                }

                // assign new locations to each of the views in selection
                // (ie 1 if not dragging a group)
                foreach (var view in draggingViews)
                {

                    var size = AbsoluteLayout.GetLayoutBounds(view).Size;
                    var newbounds = new Rectangle(location, size);
                    if (Orientation == ItemsLayoutOrientation.Horizontal)
                        location.X += size.Width;
                    else
                        location.Y += size.Height;

                    AbsoluteLayout.SetLayoutBounds(view, newbounds);

                }

                if (Orientation == ItemsLayoutOrientation.Vertical)
                {
                    // handle vertical grabbed auto scrolling
                    double threshold_low = 40;
                    double threshold_high = _scrollView.Height - 40;

                    var scroll_check = args.Y - _scrollView.ScrollY;

                    if (scroll_check < threshold_low)
                    {
                        if (!_lastScrollDirectionUp)
                            _scrollAmount = 10;
                        _scrollAmount = Math.Max(100, _scrollAmount + 5);
                        double scroll = Math.Max(0, _scrollView.ScrollY - _scrollAmount);
                        _scrollView.ScrollToAsync(_scrollView.ScrollX, scroll, true);
                        _lastScrollDirectionUp = true;
                        if (scroll == 0)
                            _scrollAmount = 10;
                    }
                    else if (scroll_check > threshold_high)
                    {
                        if (_lastScrollDirectionUp)
                            _scrollAmount = 10;
                        _scrollAmount = Math.Max(100, _scrollAmount + 5);
                        double scroll = Math.Min(_scrollView.ContentSize.Height, _scrollView.ScrollY + _scrollAmount);
                        _scrollView.ScrollToAsync(_scrollView.ScrollX, scroll, true);
                        _lastScrollDirectionUp = false;

                        if (scroll == _scrollView.ContentSize.Height)
                            _scrollAmount = 10;
                    }
                }

                LayoutChildren(0, 0, Width, Height);
            }
        }

        public void GrabCancelled(TouchEventArgs args)
        {
            GrabEnd(args);
        }

        [HandleProcessCorruptedStateExceptions]
        private void LoadChildren()
        {

            if (ItemsSource == null)
                return;

            layingout = true;
            try
            {
                _absoluteLayout.Children.Clear();
            }
            catch (System.AccessViolationException ex)
            {
                Debug.WriteLine($"Got an access violation: {ex.StackTrace}");
            }


            foreach (var item in ItemsSource)
            {
                var template = (item is IReorderDropTargetItem) ? DropTargetItemTemplate :
                               (item is IReorderGroupItem) ? GroupItemTemplate : ItemTemplate;
                var view = (View)template.CreateContent();
        
                view.BindingContext = item;
                _absoluteLayout.Children.Add(view);
            }

            layingout = false;

            if (Width >= 0 && Height >= 0)
                try
                {
                    LayoutChildren(0, 0, Width, Height);
                }
                catch (System.AccessViolationException ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }

        }

        private SizeRequest lastmeasure;
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (!isDragging) lastmeasure = base.OnMeasure(widthConstraint, heightConstraint);
            return lastmeasure;
        }

        private Size GetGroupSize(View view, bool GroupBounds)
        {
            if (!GroupBounds)
                return AbsoluteLayout.GetLayoutBounds(view).Size;

            var viewidx = _absoluteLayout.Children.IndexOf(view);

            // find top of group
            int startindex = viewidx;
            while (startindex >= 0 && !(Children[startindex].BindingContext is IReorderGroupItem))
                startindex--;

            if (startindex < 0)
                return new Size(0, 0); // item not in a group (IE above the start of any group)

            // find the bottom of the group
            int endindex = viewidx;
            while (endindex < _absoluteLayout.Children.Count - 1 && !(Children[endindex + 1].BindingContext is IReorderGroupItem))
                endindex++;

            // determine size of the full group
            var size = new Size(0, 0);
            for (int i = startindex; i <= endindex; i++)
            {
                var viewsize = AbsoluteLayout.GetLayoutBounds(Children[i]).Size;
                if (Orientation == ItemsLayoutOrientation.Horizontal)
                    size.Width += viewsize.Width;
                else
                    size.Height += viewsize.Height;
            }

            //System.Diagnostics.Debug.Write($"{viewidx}={startindex}-{endindex}:{size}");

            return size;

        }

        private bool layingout;
        [HandleProcessCorruptedStateExceptions]
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            if (layingout)
                return;

            try
            {

                layingout = true;

                // calculate the current bounds of the dragging views if any
                var dragbounds = new Rectangle();
                if (isDragging)
                {
                    var bounds = AbsoluteLayout.GetLayoutBounds(draggingViews[0]);
                    dragbounds = new Rectangle(bounds.Left, bounds.Top, originRect.Width, originRect.Height);
                }

                var location = new Point(0, 0);
                var requiregroup = isDragging && draggingIsGroup;
                nearbound = requiregroup ? -1 : 0;

                // determine item sizes, and dropindex
                // to determine drop location, assume that the whole group of items wasn't in the list
                // then figure out where it would go based on the groups's near or far bounds (depending on drag direction) 
                var staticviewsizes = new List<double>();
                dropIndex = -1;
                var firstpossibledropindex = -1;
                var firstgroupindex = -1;
                var lastgroupindex = -1;
                var waitingfordroplocation = false;
                foreach (var item in ItemsSource)
                {

                    var view = _absoluteLayout.Children.SingleOrDefault(v => v.BindingContext == item);
                    var isgroupitem = item is IReorderGroupItem;

                    double viewsize = -1;

                    // only layout views that aren't participating in the dragging operation
                    if (!isDragging || !draggingViews.Contains(view))
                    {

                        if (isgroupitem && firstgroupindex == -1) firstgroupindex = staticviewsizes.Count;
                        if (isgroupitem) lastgroupindex = staticviewsizes.Count;

                        var groupsize = GetGroupSize(view, requiregroup);
                        var sizeRequest = isDragging ?
                                            AbsoluteLayout.GetLayoutBounds(view).Size : // don't recalculate the requested size during dragging
                                            view.Measure(Width, Height, MeasureFlags.IncludeMargins).Request;

                        if (waitingfordroplocation)
                            if (isgroupitem || !(requiregroup))
                            {
                                // when moving groups of items out, we identify the first view after 
                                // the groups we're moving, but this may not itself be a group.
                                // the correct drop index is assigned at first occurance of an eligible group
                                dropIndex = staticviewsizes.Count;
                                waitingfordroplocation = false;
                            }

                        if (Orientation == ItemsLayoutOrientation.Horizontal)
                        {

                            viewsize = sizeRequest.Width;
                            staticviewsizes.Add(viewsize);

                            if (isDragging && (isgroupitem || !(requiregroup)))
                            {

                                // items can be dropped at 0, but groups can't
                                if (firstpossibledropindex == -1)
                                    firstpossibledropindex = staticviewsizes.Count - 1;

                                if (isDraggingLeftUp)
                                {
                                    // when dragging left
                                    // set the drop index to the left of this view
                                    // if its the first view where the left side of the dragging group 
                                    // is to the left of the middle of this layout group
                                    var threshold = location.X + groupsize.Width / 2;

#if DebugOrdering
                                    System.Diagnostics.Debug.WriteLine($"{staticviewsizes.Count - 1} {dragbounds.Left} < {threshold}");
#endif

                                    if (dropIndex == -1 && dragbounds.Left < threshold)
                                        dropIndex = staticviewsizes.Count - 1;
                                }
                                else
                                {
                                    // when dragging right
                                    // set the drop index to the right of this view
                                    // if the rigth side of the dragging group 
                                    // would be to the right of the middle of this layout group
                                    var threshold = location.X + dragbounds.Width + groupsize.Width / 2;

#if DebugOrdering
                                    System.Diagnostics.Debug.WriteLine($"{staticviewsizes.Count} {dragbounds.Right} > {threshold}");
#endif

                                    if (dragbounds.Right > threshold)
                                        waitingfordroplocation = true;

                                }

                            }

                            location.X += viewsize;

                        }
                        else
                        {

                            viewsize = sizeRequest.Height;
                            staticviewsizes.Add(viewsize);

                            if (isDragging && (isgroupitem || !(requiregroup)))
                            {

                                // items can be dropped at 0, but groups can't
                                if (firstpossibledropindex == -1)
                                    firstpossibledropindex = staticviewsizes.Count - 1;

                                if (isDraggingLeftUp)
                                {
                                    // when dragging up
                                    // set the drop index to above this view
                                    // if its the first view where the top of the dragging group 
                                    // is to the top of the middle of this layout group
                                    var threshold = location.Y + groupsize.Height / 2;

#if DebugOrdering
                                    System.Diagnostics.Debug.WriteLine($"Loc: {staticviewsizes.Count - 1} {dragbounds.Top} < {threshold}");
#endif

                                    if (dropIndex == -1 && dragbounds.Top < threshold)
                                    {
                                        dropIndex = staticviewsizes.Count - 1;
                                        if (dragbounds.Top < (threshold - 40))
                                        {
                                            //double scroll = Math.Max(_scrollView.ScrollY - 30, 0);
                                            //_scrollView.ScrollToAsync(_scrollView.ScrollX, scroll, true);
                                            MessagingCenter.Send<ReorderCollectionView>(this, "Scroll");
                                        }

                                    }

                                }
                                else
                                {
                                    // when dragging down
                                    // set the drop index to below this view
                                    // if the bottom side of the dragging group 
                                    // would be below the middle of this layout group
                                    var threshold = location.Y + dragbounds.Height + groupsize.Height / 2;

#if DebugOrdering
                                    System.Diagnostics.Debug.WriteLine($"{staticviewsizes.Count} {dragbounds.Bottom} > {threshold}");
#endif

                                    if (dragbounds.Bottom > threshold)
                                        waitingfordroplocation = true;

                                }

                            }

                            location.Y += viewsize;

                        }

                    }

                }

                // if no drop target found, set to the default based on the dragging direction
                if (isDragging && dropIndex == -1)
                    dropIndex = isDraggingLeftUp ? staticviewsizes.Count : firstpossibledropindex;

                if (waitingfordroplocation)
                {
                    if (requiregroup)
                        dropIndex = lastgroupindex;
                    else
                        dropIndex = staticviewsizes.Count;
                }

                // if we do have groups, and we're moving an item to above the first group,
                // activate the drop target. assumption here is that the drop target is index 0                
                if (firstgroupindex >= dropIndex && dropIndex > 0 && !requiregroup)
                    dropIndex = 0;

#if DebugOrdering
                System.Diagnostics.Debug.WriteLine($"Dropindex: {dropIndex}");
#endif

                // apply bounds
                location = new Point();
                var staticviewindex = 0;
                bool lastgroupcouldmove = true;
                foreach (var item in ItemsSource)
                {

                    var view = _absoluteLayout.Children.SingleOrDefault(v => v.BindingContext == item);
                    var isdroptargetitem = item is IReorderDropTargetItem;
                    var isgroupitem = item is IReorderGroupItem;

                    if (isgroupitem) lastgroupcouldmove = ((IReorderGroupItem)item).CanMove;

                    // nearbound is the minimum X, or Y that can be assigned to a dragged view
                    if (nearbound == -1 && isgroupitem)
                        nearbound = (Orientation == ItemsLayoutOrientation.Horizontal) ? location.X : location.Y;

                    // highlite the drop target
                    if (isdroptargetitem)
                        ((IReorderDropTargetItem)item).IsActive = (staticviewindex == dropIndex);

                    if (!isDragging || !draggingViews.Contains(view))
                    {

                        var viewsize = staticviewsizes[staticviewindex];

                        if (Orientation == ItemsLayoutOrientation.Horizontal)
                        {

                            // droplocation, add some room
                            if (staticviewindex == dropIndex && !isdroptargetitem)
                                location.X += dragbounds.Width;

                            var viewbounds = new Rectangle(location, new Size(viewsize, Height));
                            AbsoluteLayout.SetLayoutBounds(view, viewbounds);
                            location.X += viewsize;

                            // droptarget has spacing at the end
                            if (staticviewindex == dropIndex && isdroptargetitem)
                                location.X += dragbounds.Width;

                        }
                        else
                        {

                            // droplocation, add some room
                            if (staticviewindex == dropIndex && !isdroptargetitem)
                                location.Y += dragbounds.Height;

                            var viewbounds = new Rectangle(location, new Size(Width, viewsize));
                            AbsoluteLayout.SetLayoutBounds(view, viewbounds);
                            location.Y += viewsize;

                            // droptarget has spacing at the end
                            if (staticviewindex == dropIndex && isdroptargetitem)
                                location.Y += dragbounds.Height;

                        }

                        // farbound is the furtherst coordinate that can be assigned during a dragging operation
                        if (!requiregroup || lastgroupcouldmove)
                            farbound = Orientation == ItemsLayoutOrientation.Horizontal ? location.X : location.Y;

                        staticviewindex++;

                    }

                }

            }
            finally
            {
                layingout = false;
            }

        }

    }
}