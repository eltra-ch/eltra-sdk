using Xamarin.Forms;

namespace EltraNavigo.Views.Obd.Outputs.Triggers
{
    public class CollapseParameterSelectionButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is ObdOutputsViewModel viewModel)
            {
                viewModel.CollapseParameterSelection();
            }
        }
    }
}
