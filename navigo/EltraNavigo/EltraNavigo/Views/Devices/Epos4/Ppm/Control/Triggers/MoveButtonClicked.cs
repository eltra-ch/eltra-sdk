using Xamarin.Forms;

namespace EltraNavigo.Views.Ppm.Control.Triggers
{
    public class MoveButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is PpmControlViewModel viewModel)
            {
                viewModel.SetPosition();
            }
        }
    }
}
