using dotMorten.Xamarin.Forms;
using EltraNavigo.Views.Contact;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EltraNavigo.Views.Login.Behaviors
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
                    var suggestions = await GetSuggestions(view.Text);

                    view.ItemsSource = suggestions;
                }
            }
        }

        private async Task<List<string>> GetSuggestions(string text)
        {
            var result = new List<string>();

            if (_contactViewModel != null)
            {
                result = await _contactViewModel.GetCities(text);
            }

            return result;
        }

        private void OnPageBindingContextChanged(object sender, EventArgs e)
        {
            if (_view.BindingContext is ContactViewModel model)
            {
                _contactViewModel = model;
            }
        }
    }
}
