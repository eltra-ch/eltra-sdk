using Xamarin.Forms;

namespace EltraNavigo.Views.Pvm.Control.Triggers
{
    public class HaltButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is PvmControlViewModel viewModel)
            {
                viewModel.Halt();
            }
        }
    }
}
