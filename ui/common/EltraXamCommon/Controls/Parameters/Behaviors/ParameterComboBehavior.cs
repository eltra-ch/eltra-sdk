using EltraUiCommon.Controls.Parameters;
using System;
using Xamarin.Forms;

namespace EltraXamCommon.Controls.Parameters.Behaviors
{
    public class ParameterComboBehavior : Behavior<Picker>
    {
        private ParameterComboViewModel _viewModel;

        protected override void OnAttachedTo(Picker picker) 
        {
            picker.SelectedIndexChanged += OnSelectedIndexChanged;
            picker.BindingContextChanged += OnBindingContextChanged;

            base.OnAttachedTo(picker);
        }
        
        protected override void OnDetachingFrom(Picker picker) 
        {
            picker.SelectedIndexChanged -= OnSelectedIndexChanged;
            picker.BindingContextChanged -= OnBindingContextChanged;

            base.OnDetachingFrom(picker);
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            _viewModel.SelectedIndexChanged();
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            _viewModel = (sender as Picker)?.BindingContext as ParameterComboViewModel;            
        }
    }
}
