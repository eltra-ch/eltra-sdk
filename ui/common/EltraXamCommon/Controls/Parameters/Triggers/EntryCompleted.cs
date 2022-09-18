using Xamarin.Forms;
using EltraUiCommon.Controls.Parameters;

namespace EltraXamCommon.Controls.Parameters.Triggers
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
