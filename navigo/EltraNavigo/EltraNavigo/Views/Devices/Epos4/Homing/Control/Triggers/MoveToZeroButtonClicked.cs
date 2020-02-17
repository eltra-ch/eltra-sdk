using Xamarin.Forms;

namespace EltraNavigo.Views.Homing.Control.Triggers
{
    public class DefinePositionButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is HomingControlViewModel viewModel)
            {
                viewModel.DefinePosition();
            }
        }
    }
}
