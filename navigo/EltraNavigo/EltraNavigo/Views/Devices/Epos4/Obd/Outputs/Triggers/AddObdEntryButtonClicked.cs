using Xamarin.Forms;

namespace EltraNavigo.Views.Obd.Outputs.Triggers
{
    class AddObdEntryButtonClicked : TriggerAction<Button>
    {
        protected override void Invoke(Button sender)
        {
            if (sender.BindingContext is ObdEntry obdEntry)
            {
                if (obdEntry.Parent is ObdOutputsViewModel viewModel)
                {
                    viewModel.AddObdEntryToObserved(obdEntry);
                }
            }
        }
    }
}
