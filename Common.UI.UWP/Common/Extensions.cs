using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Common.UWP.Common
{
    public static class Extensions
    {
        public static Brush ToNativeBrush(this Xamarin.Forms.Color color)
        {
            return new SolidColorBrush(color.ToNativeColor());
        }

        public static Xamarin.Forms.Color ToXamarinFormsColor(this Windows.UI.Color color)
        {
            return new Xamarin.Forms.Color((double)color.R / 255.0, (double)color.G / 255.0, (double)color.B / 255.0, (double)color.A / 255.0);
        }

        public static Windows.UI.Color ToNativeColor(this Xamarin.Forms.Color xamarinColor)
        {
            return new Windows.UI.Color
            {
                A = Convert.ToByte(Math.Max(xamarinColor.A, 0) * 255),
                R = Convert.ToByte(Math.Max(xamarinColor.R, 0) * 255),
                G = Convert.ToByte(Math.Max(xamarinColor.G, 0) * 255),
                B = Convert.ToByte(Math.Max(xamarinColor.B, 0) * 255)
            };
        }

        
    }
}
