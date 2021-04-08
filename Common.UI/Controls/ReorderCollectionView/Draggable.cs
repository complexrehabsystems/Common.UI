using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Common.Controls.ReorderCollectionView
{
    public class Draggable: Grid
    {
        /// <summary>
        /// The Command property.
        /// </summary>
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Draggable), null, BindingMode.Default, null);

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

        public Draggable()
        {
            this.InputTransparent = true;
            this.CascadeInputTransparent = true;
        }
    }
}
