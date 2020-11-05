using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace EltraNavigoMPlayer.Views.MPlayerControl.Behaviors
{
    class MasterVolumeSliderBehavior : Behavior<Slider>
    {
        MPlayerControlViewModel _viewModel;

        protected override void  OnAttached()
        {
            _viewModel = AssociatedObject.DataContext as MPlayerControlViewModel;

            AssociatedObject.DataContextChanged += OnDataContextChanged; ;
            AssociatedObject.ValueChanged += OnValueChanged;

            base.OnAttached();
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null)
            {
                _viewModel.SliderVolumeValueChanged(e.NewValue);
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject.DataContext is MPlayerControlViewModel model)
            {
                _viewModel = model;
            }
        }
    }
}
