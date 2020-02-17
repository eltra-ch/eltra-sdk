using Xamarin.Forms;

namespace EltraNavigo.Views.Pvm.Control.Triggers
{
    public class MoveButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is PvmControlViewModel viewModel)
            {
                viewModel.SetVelocity();
            }
        }
    }
}
