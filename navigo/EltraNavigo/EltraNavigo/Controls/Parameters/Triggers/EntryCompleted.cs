using Xamarin.Forms;

namespace EltraNavigo.Controls.Parameters.Triggers
{
    public class EntryCompleted : TriggerAction<Entry>
    {
        protected override async void Invoke(Entry sender)
        {
            if (sender.BindingContext is ParameterEditViewModel viewModel)
            {
                await viewModel.TextChanged(sender.Text);
            }
        }
    }
}
