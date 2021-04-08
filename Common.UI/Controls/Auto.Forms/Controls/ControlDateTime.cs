using CrsCommon.Controls.Auto.Forms.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Controls.Auto.Forms.Controls
{
    public class ControlDateTime : ControlBase
    {
        public ControlDateTime(ControlConfig config) : base(config)
        {
        }

        protected override View CreateControl(string bindingName, Type fieldType)
        {
            if (fieldType != typeof(DateTime) && 
                fieldType != typeof(DateTimeOffset) &&
                fieldType != typeof(DateTime?) &&
                fieldType != typeof(DateTimeOffset?))
            {
                Debug.WriteLine($"field:{bindingName} error. Wrong type {fieldType.ToString()} should be DateTime or DateTimeOffset");
                return null;
            }

            var t = new DatePicker
            {
                Style = _itemStyle
            };

            t.SetBinding(DatePicker.DateProperty, new Binding(bindingName, BindingMode.TwoWay, new DateTimeConverter(), fieldType));

            return t;
        }
    }
}
