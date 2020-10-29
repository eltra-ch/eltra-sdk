using System;
using Xamarin.Forms;

namespace EltraNavigoMPlayer.Views.MPlayerControl.Behaviors
{
    class MasterVolumeSliderBehavior : Behavior<Slider>
    {
        Slider _control;
        MPlayerControlViewModel _viewModel;

        protected override void OnAttachedTo(Slider control)
        {
            _control = control;
            _viewModel = control.BindingContext as MPlayerControlViewModel;

            control.BindingContextChanged += OnBindingContextChanged;
            control.ValueChanged += OnValueChanged;

            base.OnAttachedTo(control);
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if(_viewModel!=null)
            {
                _viewModel.SliderVolumeValueChanged(e.NewValue);
            }
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (_control.BindingContext is MPlayerControlViewModel model)
            {
                _viewModel = model;
            }
        }
    }
}
