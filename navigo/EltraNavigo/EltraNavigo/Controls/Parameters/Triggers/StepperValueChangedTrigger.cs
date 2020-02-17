using Xamarin.Forms;

namespace EltraNavigo.Controls.Parameters.Triggers
{
    class StepperValueChangedTrigger : TriggerAction<Stepper>
    {
        protected override async void Invoke(Stepper sender)
        {
            if (sender.BindingContext is ParameterEditViewModel viewModel)
            {
                double value = sender.Value;

                await viewModel.ValueChanged(value);
            }
        }
    }
}
