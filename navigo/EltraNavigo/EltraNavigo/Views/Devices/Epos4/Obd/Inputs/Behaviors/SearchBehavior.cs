using System;
using Xamarin.Forms;

namespace EltraNavigo.Views.Obd.Inputs.Behaviors
{
    public class SearchBehavior : Behavior<SearchBar>
    {
        private ObdInputsViewModel _obdInputsViewModel;
        public SearchBar AssociatedObject { get; private set; }

        protected override void OnAttachedTo (SearchBar control)
        {
            base.OnAttachedTo (control);

            AssociatedObject = control;
            
            _obdInputsViewModel = control.BindingContext as ObdInputsViewModel;

            control.BindingContextChanged += OnContextChanged;
            control.SearchButtonPressed += OnSearchButtonPressed;
            control.TextChanged += OnSearchTextChanged;
        }
        
        protected override void OnDetachingFrom (SearchBar control)
        {
            base.OnDetachingFrom (control);
            
            control.BindingContextChanged -= OnContextChanged;
            control.SearchButtonPressed -= OnSearchButtonPressed;
            control.TextChanged -= OnSearchTextChanged;
        }
 
        private void OnContextChanged(object sender, EventArgs e)
        {
            if (AssociatedObject != null)
            {
                _obdInputsViewModel = AssociatedObject.BindingContext as ObdInputsViewModel;
            }
        }
        
        private void OnSearchButtonPressed(object sender, EventArgs eventArgs)
        {
            _obdInputsViewModel.SearchButtonPressed(AssociatedObject.Text);
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                _obdInputsViewModel.ResetSearch();
            }
        }
    }
}
