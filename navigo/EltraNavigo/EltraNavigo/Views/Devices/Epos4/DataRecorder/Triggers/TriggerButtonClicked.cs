using Xamarin.Forms;

namespace EltraNavigo.Views.DataRecorder.Triggers
{
    public class TriggerButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is DataRecorderViewModel controlViewModel)
            {
                controlViewModel.TriggerRecorder();
            }
        }
    }
}
