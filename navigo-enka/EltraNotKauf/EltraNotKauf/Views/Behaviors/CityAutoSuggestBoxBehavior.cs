using dotMorten.Xamarin.Forms;
using EltraNotKauf.Views.Contact;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Login.Behaviors
{
    class CityAutoSuggestBoxBehavior : Behavior<AutoSuggestBox>
    {
        private AutoSuggestBox _view;
        private ContactViewModel _contactViewModel;
        
        protected override void OnAttachedTo(AutoSuggestBox view)
        {
            _view = view;

            view.BindingContextChanged += OnPageBindingContextChanged;
            
            view.TextChanged += OnTextChanged;
            view.QuerySubmitted += OnQuerySubmitted;
            view.Focused += OnFocused;

            base.OnAttachedTo(view);
        }

        private void OnFocused(object sender, FocusEventArgs e)
        {
            if(e.IsFocused)
            {
                _view.IsSuggestionListOpen = false;
                _view.ItemsSource = null;
            }
        }

        private void OnQuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (_contactViewModel != null)
            {
                if (e.ChosenSuggestion != null)
                {
                    _contactViewModel.City = e.ChosenSuggestion.ToString();
                }
                else
                {
                    _contactViewModel.City = e.QueryText;
                }
            }

            if (_view != null)
            {
                _view.IsSuggestionListOpen = false;
                _view.ItemsSource = null;

                _view.Unfocus();
            }
        }

        private async void OnTextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var view = (AutoSuggestBox)sender;
            
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(view.Text) || view.Text.Length < 3)
                {
                    view.ItemsSource = null;
                }
                else
                {
                    if (_contactViewModel != null)
                    {
                        await _contactViewModel.UpdateCitySuggestions(view.Text);
                    }                    
                }
            }
        }

        private void OnPageBindingContextChanged(object sender, EventArgs e)
        {
            if (_view.BindingContext is ContactViewModel model)
            {
                _contactViewModel = model;

                _contactViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "City")
            {
                _view.Text = _contactViewModel.City;
            }
            else if(e.PropertyName == "CitySuggestions")
            {
                _view.ItemsSource = _contactViewModel.CitySuggestions;
            }
            else if(e.PropertyName == "Region")
            {
                /*if (_contactViewModel.IsVisible)
                {
                    _view.Text = string.Empty;
                }*/

                _view.ItemsSource = null;
            }
        }
    }
}
