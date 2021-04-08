using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CrsCommon.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckboxGroup : ContentView
    {
        public static readonly BindableProperty ButtonStyleProperty = BindableProperty.Create(nameof(ButtonStyle), typeof(Style), typeof(CheckboxGroup), null, BindingMode.Default);
        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        public static readonly BindableProperty SelectAllCommandProperty = BindableProperty.Create(nameof(SelectAllCommand), typeof(ICommand), typeof(CheckboxGroup), null, BindingMode.Default, null);
        public ICommand SelectAllCommand
        {
            get { return (ICommand)GetValue(SelectAllCommandProperty); }
            set { SetValue(SelectAllCommandProperty, value); }
        }

        public static readonly BindableProperty UnselectAllCommandProperty = BindableProperty.Create(nameof(UnselectAllCommand), typeof(ICommand), typeof(CheckboxGroup), null, BindingMode.Default, null);
        public ICommand UnselectAllCommand
        {
            get { return (ICommand)GetValue(UnselectAllCommandProperty); }
            set { SetValue(UnselectAllCommandProperty, value); }
        }
        public CheckboxGroup()
        {
            InitializeComponent();
        }
    }
}