using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using CommonButton = Common.UI.Controls.ImageButton;
using AndroidView = Android.Views.View;
using AndroidRectShape = Android.Graphics.Drawables.Shapes.RectShape;
using Android.Graphics.Drawables;
using System.ComponentModel;
using Common.UI.Android.Renderers;

[assembly: ExportRenderer(typeof(CommonButton), typeof(MyImageButtonRenderer))]
namespace Common.UI.Android.Renderers
{
    public class MyImageButtonRenderer : ViewRenderer<CommonButton, AndroidView>
    {
        protected AndroidView _btn;
        bool MouseEntered;

        public static int LoadClass = 0;

		public MyImageButtonRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CommonButton> e)
		{
			base.OnElementChanged(e);

			if (Control == null && e.NewElement != null)
			{
				_btn = new AndroidView(this.Context)
				{
				};

				GradientDrawable shape = new GradientDrawable();
				shape.SetShape(ShapeType.Rectangle);
				shape.SetCornerRadii(new float[]
				{
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
				});
				shape.SetColor(Element.ButtonBackgroundColor.ToAndroid());
				shape.SetStroke((int)Element.BorderThickness, Element.ButtonBorderColor.ToAndroid());
				_btn.Background = shape;

				SetNativeControl(_btn);
			}

		}


		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (Control == null)
				return;

			var shape = _btn.Background as GradientDrawable;

			if (e.PropertyName == CommonButton.ButtonBackgroundColorProperty.PropertyName)
			{
				SetBackgroundColor();
			}
			else if (e.PropertyName == CommonButton.ButtonHighlightColorProperty.PropertyName)
			{
				SetBackgroundColor();
			}
			else if (e.PropertyName == CommonButton.ButtonBorderColorProperty.PropertyName)
			{
				shape.SetStroke((int)Element.BorderThickness, Element.ButtonBorderColor.ToAndroid());
			}
			else if (e.PropertyName == CommonButton.CornerRadiusProperty.PropertyName)
			{
				shape.SetCornerRadii(new float[]
				{
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
					(float)Element.CornerRadius,
				});
			}
			else if (e.PropertyName == CommonButton.BorderThicknessProperty.PropertyName)
			{
				shape.SetStroke((int)Element.BorderThickness, Element.ButtonBorderColor.ToAndroid());
			}
			else if (e.PropertyName == CommonButton.BackgroundColorProperty.PropertyName)
			{
				//_btn.Background = Element.BackgroundColor.ToNativeBrush();
				//Element.BackgroundColor = Xamarin.Forms.Color.Transparent;
			}


		}

		void SetBackgroundColor()
		{
			var shape = _btn.Background as GradientDrawable;
			var col = MouseEntered /*&& Element.HasMouseOver*/ ? Element.ButtonHighlightColor : Element.ButtonBackgroundColor;
			shape.SetColor(col.ToAndroid());
		}
	}
}