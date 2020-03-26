using dotMorten.Xamarin.Forms;
using EltraNotKauf.Views.Contact;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Contact.Behaviors
{
    class StreetAutoSuggestBoxBehavior : Behavior<AutoSuggestBox>
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
                    _contactViewModel.Street = e.ChosenSuggestion.ToString();
                }
                else
                {
                    _contactViewModel.Street = e.QueryText;
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
                        await _contactViewModel.UpdateStreetSuggestions(view.Text);
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
            if(e.PropertyName == "Street")
            {
                _view.Text = _contactViewModel.Street;
            }
            else if(e.PropertyName == "StreetSuggestions")
            {
                _view.ItemsSource = _contactViewModel.StreetSuggestions;
            }
            else if(e.PropertyName == "Region")
            {
                /*if (_contactViewModel.IsVisible)
                {
                    _view.Text = string.Empty;
                }*/
                _view.ItemsSource = null;
            }
            else if (e.PropertyName == "PostalCode")
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
