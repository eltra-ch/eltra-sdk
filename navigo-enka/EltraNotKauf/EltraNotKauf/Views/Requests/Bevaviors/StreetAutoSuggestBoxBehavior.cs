﻿using dotMorten.Xamarin.Forms;
using EltraNotKauf.Views.Contact;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace EltraNotKauf.Views.Requests.Behaviors
{
    class StreetAutoSuggestBoxBehavior : Behavior<AutoSuggestBox>
    {
        private AutoSuggestBox _view;
        private RequestsViewModel _viewModel;
        
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
            if (_viewModel != null)
            {
                if (e.ChosenSuggestion != null)
                {
                    _viewModel.Street = e.ChosenSuggestion.ToString();
                }
                else
                {
                    _viewModel.Street = e.QueryText;
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
                    if (_viewModel != null)
                    {
                        await _viewModel.UpdateStreetSuggestions(view.Text);
                    }                    
                }
            }
        }

        private void OnPageBindingContextChanged(object sender, EventArgs e)
        {
            if (_view.BindingContext is RequestsViewModel model)
            {
                _viewModel = model;

                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_viewModel !=null && _viewModel.IsVisible)
            {
                if (e.PropertyName == "Street")
                {
                    _view.Text = _viewModel.Street;
                }
                else if (e.PropertyName == "StreetSuggestions")
                {
                    _view.ItemsSource = _viewModel.StreetSuggestions;
                }
                else if (e.PropertyName == "Region")
                {
                    _view.ItemsSource = null;
                }
                else if (e.PropertyName == "PostalCode")
                {
                    _view.ItemsSource = null;
                }
            }
        }
    }
}