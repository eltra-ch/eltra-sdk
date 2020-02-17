using Xamarin.Forms;

namespace EltraNavigo.Views.Homing.Control.Triggers
{
    class QuickStopButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is HomingControlViewModel viewModel)
            {
                viewModel.QuickStop();
            }
        }
    }
}
