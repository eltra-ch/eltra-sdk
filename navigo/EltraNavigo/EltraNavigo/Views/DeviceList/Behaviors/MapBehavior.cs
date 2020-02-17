using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace EltraNavigo.Views.DeviceList.Behaviors
{
    public class MapBehavior : Behavior<Map>
    {
        private Map _associatedObject;
        private DeviceInfoViewModel _viewModel;

        protected override void OnAttachedTo(Map control)
        {
            base.OnAttachedTo(control);

            _associatedObject = control;

            _viewModel = control.BindingContext as DeviceInfoViewModel;

            control.BindingContextChanged += OnContextChanged;

            RegisterEvents();
        }

        private void OnContextChanged(object sender, EventArgs e)
        {
            _viewModel = _associatedObject?.BindingContext as DeviceInfoViewModel;

            RegisterEvents();
        }

        protected override void OnDetachingFrom(Map control)
        {
            base.OnDetachingFrom(control);

            control.BindingContextChanged += OnContextChanged;

            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            if(_viewModel!=null)
            {
                _viewModel.LocationChanged += OnLocationChanged;
            }
        }

        private void UnregisterEvents()
        {
            if (_viewModel != null)
            {
                _viewModel.LocationChanged -= OnLocationChanged;
            }
        }

        private void OnLocationChanged(object sender, Events.LocationChangedEventArgs e)
        {
            _associatedObject?.MoveToRegion(MapSpan.FromCenterAndRadius(
                          e.Position,
                          Distance.FromMiles(1)));
        }
    }
}
