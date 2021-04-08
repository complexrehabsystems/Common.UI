using Common.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace Common.Controls
{
    public class ExtendedSlider : Slider
    {
        /// <summary>
        /// The current step value property
        /// </summary>
        public static readonly BindableProperty CurrentStepValueProperty = BindableProperty.Create(nameof(StepValue), typeof(double), typeof(ExtendedSlider), 0.0, BindingMode.Default, null, null);

        /// <summary>
        /// Gets or sets the step value.
        /// </summary>
        /// <value>The step value.</value>
        public double StepValue
        {
            get { return (double)GetValue(CurrentStepValueProperty); }

            set { SetValue(CurrentStepValueProperty, value); }
        }

        public static readonly BindableProperty IsHoldingThumbProperty = BindableProperty.Create(nameof(IsHoldingThumb), typeof(bool), typeof(ExtendedSlider), false, BindingMode.OneWayToSource);

        public bool IsHoldingThumb
        {
            get { return (bool)GetValue(IsHoldingThumbProperty); }
            set { SetValue(IsHoldingThumbProperty, value); }
        }

        private MyTimer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSlider"/> class.
        /// </summary>
        public ExtendedSlider()
        {
            ValueChanged += OnSliderValueChanged;
        }

        /// <summary>
        /// Handles the <see cref="E:SliderValueChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ValueChangedEventArgs"/> instance containing the event data.</param>
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            StopTimer();

            IsHoldingThumb = true;

            _timer = new MyTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                StopTimer();

                IsHoldingThumb = false;

            });
            _timer.Start();

            if (StepValue <= 0)
                return;

            var newStep = Math.Round(e.NewValue / StepValue);

            Value = newStep * StepValue;            
        }

        void StopTimer()
        {
            if (_timer == null)
                return;

            _timer.Stop();
            _timer = null;
            
        }
    }
}
