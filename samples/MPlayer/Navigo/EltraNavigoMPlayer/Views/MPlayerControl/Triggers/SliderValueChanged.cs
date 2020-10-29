using Xamarin.Forms;

namespace EltraNavigoMPlayer.Views.Triggers
{
    class SliderValueChanged : TriggerAction<Slider>
    {
        protected override void Invoke(Slider sender)
        {
            if (sender.BindingContext is MPlayerControl.MPlayerControlViewModel viewModel)
            {
                viewModel.SliderVolumeValueChanged(sender.Value);
            }
        }
    }
}
