using System;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Login.Behaviors
{
    public class PasswordValidationBehavior : Behavior<Entry> {

        private Entry _view;
        private SignViewModel _viewModel;
        
        protected override void OnAttachedTo(Entry entry) 
        {
            _view = entry;
        
            entry.TextChanged += OnEntryTextChanged;
            entry.Completed += OnEntryCompleted;
            entry.BindingContextChanged += OnEntryBindingContextChanged;

            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            if(_viewModel!=null)
            {
                _viewModel.StatusChanged -= OnViewModelStatusChanged;
            }

            entry.TextChanged -= OnEntryTextChanged;
            entry.Completed -= OnEntryCompleted;
            entry.BindingContextChanged -= OnEntryBindingContextChanged;

            base.OnDetachingFrom(entry);
        }

        protected virtual void OnEntryBindingContextChanged(object sender, EventArgs e)
        {
            if (_view.BindingContext is SignViewModel model)
            {
                _viewModel = model;
                _viewModel.StatusChanged += OnViewModelStatusChanged;
            }
        }

        private void OnViewModelStatusChanged(object sender, Events.SignStatusEventArgs e)
        {
            if (e.Status == SignStatus.Failed)
            {
                Shake();
            }
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            _viewModel?.OnPasswordCompleted();

            _view.Focus();
        }

        protected virtual void OnEntryTextChanged(object sender, TextChangedEventArgs args) 
        {
            if (((Entry)sender).BindingContext is SignViewModel viewModel)
            {
                if (_viewModel != viewModel)
                {
                    _viewModel = viewModel;   
                }

                viewModel.OnPasswordChanged(args.NewTextValue);
            }
        }

        private async void Shake()
        {
            uint timeout = 50;

            if (_view != null)
            {
                await _view.TranslateTo(-15, 0, timeout);

                await _view.TranslateTo(15, 0, timeout);

                await _view.TranslateTo(-10, 0, timeout);

                await _view.TranslateTo(10, 0, timeout);

                await _view.TranslateTo(-5, 0, timeout);

                await _view.TranslateTo(5, 0, timeout);

                _view.TranslationX = 0;
            }
        }
    }
}