using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Common.UI.Interfaces;
using Xamarin.Forms.Platform.UWP;

namespace Common.UI.UWP.Services
{
    public class PopupMenu_UWP:IPopupMenu
    {
        public string folderPath => ApplicationData.Current.LocalFolder.Path;

        Popup _popup;

        public async Task<int> LaunchMenu(List<string> menuItems, Xamarin.Forms.View parent)
        {
            if (menuItems == null || menuItems.Count == 0)
                return -1;

            var menu = new Windows.UI.Popups.PopupMenu();

            Windows.Foundation.Point point = new Windows.Foundation.Point(0, 0);

            if (parent != null)
            {
                // have to turn it into the UWP visual control and extract the screen coordinates
                var visualElement = parent.GetOrCreateRenderer()?.ContainerElement;
                if (visualElement != null)
                {
                    GeneralTransform transform = visualElement.TransformToVisual(null);
                    point = transform.TransformPoint(new Windows.Foundation.Point());
                }
            }

            for(var i=0; i<menuItems.Count; i++)
            {
                var item = menuItems[i];
                menu.Commands.Add(new Windows.UI.Popups.UICommand(item));
            }

            var response = await menu.ShowAsync(point);
            if (response == null || string.IsNullOrEmpty(response.Label))
                return -1;

            return menuItems.IndexOf(response.Label);

            
        }

        public async Task<string> LaunchInputTextMenu(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }

        public async Task<Stream> LaunchFilePicker(bool isImage)
        {
            // Create and initialize the FileOpenPicker
            FileOpenPicker openPicker;

            if (isImage)
            {
                openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                };

                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
            }
            else
            {
                openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                };

                openPicker.FileTypeFilter.Add("*");

            }


            // Get a file and return a Stream 
            StorageFile storageFile = await openPicker.PickSingleFileAsync();

            if (storageFile == null)
            {
                return null;
            }

            IRandomAccessStreamWithContentType raStream = await storageFile.OpenReadAsync();
            return raStream.AsStreamForRead();
        }

        public void CancelPopup()
        {
            if(_popup != null)
            {
                _popup.IsOpen = false;
                _popup = null;
            }
        }

        public void ShowContentPopup(Xamarin.Forms.View content, int width, int height, Xamarin.Forms.View parent)
        {
            CancelPopup();

            _popup = new Popup();
            _popup.IsLightDismissEnabled = true;

            if (parent != null)
            {
                // have to turn it into the UWP visual control and extract the screen coordinates
                var parentElement = parent.GetOrCreateRenderer()?.ContainerElement;
                if (parentElement != null)
                {
                    GeneralTransform transform = parentElement.TransformToVisual(null);
                    Windows.Foundation.Point point = transform.TransformPoint(new Windows.Foundation.Point());
                    _popup.HorizontalOffset = point.X;
                    _popup.VerticalOffset = point.Y;
                }
            }

            var border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Windows.UI.Xaml.Thickness(1)
            };
            Grid.SetColumnSpan(border, 3);
            Grid.SetRowSpan(border, 3);

            var visualElement = content.GetOrCreateRenderer().ContainerElement;
            Grid.SetColumn(visualElement, 1);
            Grid.SetRow(visualElement, 1);

            float spacing = 8f;

            var grid = new Windows.UI.Xaml.Controls.Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.White),
                RowSpacing = 4,
                RowDefinitions =
                {
                    new Windows.UI.Xaml.Controls.RowDefinition{Height = new Windows.UI.Xaml.GridLength(spacing)},
                    new Windows.UI.Xaml.Controls.RowDefinition{Height = new Windows.UI.Xaml.GridLength(height)},
                    new Windows.UI.Xaml.Controls.RowDefinition{Height = new Windows.UI.Xaml.GridLength(spacing)},
                },
                ColumnDefinitions =
                {
                    new Windows.UI.Xaml.Controls.ColumnDefinition{Width = new Windows.UI.Xaml.GridLength(spacing)},
                    new Windows.UI.Xaml.Controls.ColumnDefinition{Width = new Windows.UI.Xaml.GridLength(width)},
                    new Windows.UI.Xaml.Controls.ColumnDefinition{Width = new Windows.UI.Xaml.GridLength(spacing)},
                },
                Children =
                {
                    border,
                    visualElement
                }
            };
            grid.SizeChanged += delegate
            {
                // this is done in case the popup window show's outside the bounds of the main window

                var window = ApplicationView.GetForCurrentView();

                if (_popup.HorizontalOffset + grid.ActualWidth > window.VisibleBounds.Width)
                {
                    _popup.HorizontalOffset = window.VisibleBounds.Width - grid.ActualWidth - 10;
                }
            };

            _popup.Child = grid;
            _popup.IsOpen = true;            

        }
    }
}
