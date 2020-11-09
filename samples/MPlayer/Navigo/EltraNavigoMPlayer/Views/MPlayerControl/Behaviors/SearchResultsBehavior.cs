using EltraNavigoMPlayer.Views.MPlayerControl.Station;
using MPlayerCommon.Contracts;
using System;
using Xamarin.Forms;

namespace EltraNavigoMPlayer.Views.MPlayerControl.Behaviors
{
    class SearchResultsBehavior : Behavior<ListView>
    {
        ListView _control;
        MPlayerStationViewModel _viewModel;

        protected override void OnAttachedTo(ListView control)
        {
            _control = control;
            _viewModel = control.BindingContext as MPlayerStationViewModel;

            control.BindingContextChanged += OnBindingContextChanged;
            control.ItemTapped += OnItemTapped;

            base.OnAttachedTo(control);
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            _viewModel?.OnSearchResultTapped(e.Item as RadioStationEntry);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (_control.BindingContext is MPlayerStationViewModel model)
            {
                _viewModel = model;
            }
        }
    }
}
