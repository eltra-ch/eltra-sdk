using Xamarin.Forms;

namespace EltraNavigo.Controls.Triggers
{
    class ActivateOpModeButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is OpModeViewModel viewModel)
            {
                viewModel.ActivateOpMode();
            }
        }
    }
}
