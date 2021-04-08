using System;
using Common.UI.Controls.Auto.Forms.Common;
using Xamarin.Forms;

namespace Common.UI.Controls.Auto.Forms.Converters
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
