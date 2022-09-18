using EltraXamCommon.Controls;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace EltraXamCommon.Plugins
{
    [Preserve(AllMembers = true)]
    public interface IEltraNavigoPlugin
    {
        IDialogService DialogService { get; set; }

        List<ToolViewModel> GetViewModels();

        ContentView GetView(ToolViewModel viewModel);
    }
}
