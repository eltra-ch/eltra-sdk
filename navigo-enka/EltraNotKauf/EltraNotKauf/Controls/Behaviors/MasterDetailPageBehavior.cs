using EltraNotKauf.Views;
using System;
using Xamarin.Forms;

namespace EltraNotKauf.Controls.Behaviors
{
    public class MasterDetailPageBehavior : Behavior<MasterDetailPage>
    {
        #region Private fields

        private MasterDetailPage _view;
        private MainViewModel _viewModel;

        #endregion

        #region Methods

        protected override void OnAttachedTo(MasterDetailPage view)
        {
            _view = view;

            _view.BindingContextChanged += OnBindingContextChanged;

            base.OnAttachedTo(view);
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (_view.BindingContext is MainViewModel model)
            {
                _viewModel = model;

                _viewModel.MasterViewModel.PageChanged += OnDetailPageChanged;
            }
        }

        private void OnDetailPageChanged(object sender, EventArgs e)
        {
            var activeViewModel = _viewModel.MasterViewModel.ActiveViewModel;

            //_view.Detail = new NavigationPage((Page)Activator.CreateInstance(typeof(DetailView)));

            //_view.Detail.BindingContext = _viewModel.DetailViewModel;
        }

        #endregion
    }
}
