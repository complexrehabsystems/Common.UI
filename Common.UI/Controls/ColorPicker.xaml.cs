using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Common.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorPicker : ContentView
    {
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(ColorPicker), Color.Black, BindingMode.Default,
            propertyChanging: (bindable, oldValue, newValue) =>
            {
                var ctrl = (ColorPicker)bindable;
                var vm = ctrl.BindingContext as ColorPickerViewModel;
                vm?.SelectColor((Color)newValue);
            });

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        public ColorPicker()
        {
            InitializeComponent();
            BindingContext = new ColorPickerViewModel();
        }

        public void OnColorTapped(object sender, EventArgs args)
        {
            TappedEventArgs tappedArgs = null;
            if (args is TappedEventArgs t)
            {
                tappedArgs = t;
            }

            Color = (tappedArgs?.Parameter as ColorViewModel)?.Color ?? Color.Black;
        }

        [AddINotifyPropertyChangedInterface]
        public class ColorPickerViewModel
        {
            private ObservableCollection<ColorViewModel> _availableColors = new ObservableCollection<ColorViewModel>
            {
                new ColorViewModel { Color = Color.FromRgb(246, 64, 44), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(235, 20, 96), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(156, 26, 177), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(102, 51, 185), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(61, 77, 183), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(70, 175, 74), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(0, 150, 135), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(0, 187, 213), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(0, 166, 246), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(16, 147, 245), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(136, 196, 64), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(204, 221, 30), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(255, 236, 22), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(255, 152, 0), Selected = false },
                new ColorViewModel { Color = Color.Black, Selected = true },
                new ColorViewModel { Color = Color.FromRgb(94, 124, 139), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(157, 157, 157), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(122, 85, 71), Selected = false },
                new ColorViewModel { Color = Color.FromRgb(255, 85, 5), Selected = false }
            };

            public ObservableCollection<ColorViewModel> Colors
            {
                get { return _availableColors; }
            }

            public void SelectColor(Color? color)
            {
                foreach (var colorVM in _availableColors)
                {
                    colorVM.Selected = colorVM.Color == color;
                }
            }
        }


        [AddINotifyPropertyChangedInterface]
        public class ColorViewModel
        {
            public Color Color { get; set; }
            public bool Selected { get; set; }
        }
    }
}