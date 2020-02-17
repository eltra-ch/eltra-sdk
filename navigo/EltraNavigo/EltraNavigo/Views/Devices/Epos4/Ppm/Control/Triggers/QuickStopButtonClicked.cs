using Xamarin.Forms;

namespace EltraNavigo.Views.Ppm.Control.Triggers
{
    class QuickStopButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is PpmControlViewModel viewModel)
            {
                viewModel.QuickStop();
            }
        }
    }
}
