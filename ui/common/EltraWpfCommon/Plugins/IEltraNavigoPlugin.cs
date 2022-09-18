using EltraUiCommon.Controls;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EltraXamCommon.Plugins
{    
    public interface IEltraNavigoPlugin
    {
        IDialogService DialogService { get; set; }

        List<ToolViewModel> GetViewModels();

        UserControl GetView(ToolViewModel viewModel);
    }
}
