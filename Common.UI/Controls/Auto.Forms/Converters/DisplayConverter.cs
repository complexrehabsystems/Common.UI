using CrsCommon.Controls.Auto.Forms.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CrsCommon.Controls.Auto.Forms
{
    public class DisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {           
            return value.Convert((Type)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return StringHelper.ConvertBack(value as string, targetType);
        }
    }
}
