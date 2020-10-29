using Xamarin.Forms;

namespace EltraNavigoStreema.Views.StreemaControl.Triggers
{
    class SliderValueChanged : TriggerAction<Slider>
    {
        protected override void Invoke(Slider sender)
        {
            if (sender.BindingContext is StreemaControlViewModel viewModel)
            {
                viewModel.SliderVolumeValueChanged(sender.Value);
            }
        }
    }
}
