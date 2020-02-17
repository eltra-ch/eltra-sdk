using Xamarin.Forms;

namespace EltraNavigo.Views.Pvm.Control.Triggers
{
    class EnableButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is PvmControlViewModel viewModel)
            {
                viewModel.EnableDisable();
            }
        }
    }
}
