using Xamarin.Forms;

namespace EltraNavigo.Controls.Status.Triggers
{
    class ClearFaultClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is ToolStatusViewModel viewModel)
            {
                viewModel.ClearFault();
            }
        }
    }
}
