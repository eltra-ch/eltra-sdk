using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace EltraNavigoMPlayer.Views.Triggers
{
    class SliderValueChanged : TriggerAction<Slider>
    {
        protected override void Invoke(object sender)
        {
            if (sender is Slider slider)
            {
                if (slider.DataContext is MPlayerControl.MPlayerControlViewModel viewModel)
                {
                    viewModel.SliderVolumeValueChanged(slider.Value);
                }
            }
        }
    }
}
