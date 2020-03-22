using System;
using EltraNotKauf.Controls;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Behaviors
{
    public class NavigationPageSelection : Behavior<ListView>
    {
        private MasterViewModel _viewModel;
        private bool _pageChangedEventPending = false;

        protected override void OnAttachedTo(ListView page)
        {
            _viewModel = page.BindingContext as MasterViewModel;

            page.BindingContextChanged += OnBindingContextChanged;
            page.ItemSelected += OnNavigationPageChanged;
            
            base.OnAttachedTo(page);
        }

        protected override void OnDetachingFrom(ListView page)
        {
            page.BindingContextChanged -= OnBindingContextChanged;
            page.ItemSelected -= OnNavigationPageChanged;
            
            base.OnDetachingFrom(page);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            _viewModel = (sender as ListView)?.BindingContext as MasterViewModel;

            if (_pageChangedEventPending)
            {
                _viewModel?.GotoFirstPage();

                _pageChangedEventPending = false;
            }
        }

        private void OnNavigationPageChanged(object sender, SelectedItemChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                var selectedItem = e.SelectedItem;

                _viewModel.ChangePage(selectedItem as ToolViewModel);
            }
            else
            {
                _pageChangedEventPending = true;
            }
        }
    }
}
