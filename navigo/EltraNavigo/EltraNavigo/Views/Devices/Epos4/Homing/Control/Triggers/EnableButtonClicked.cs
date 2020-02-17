using Xamarin.Forms;

namespace EltraNavigo.Views.Homing.Control.Triggers
{
    class EnableButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is HomingControlViewModel viewModel)
            {
                viewModel.EnableDisable();
            }
        }
    }
}
