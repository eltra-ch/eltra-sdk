using EltraNavigoMPlayer.Views.MPlayerControl;
using EltraUiCommon.Controls;
using EltraXamCommon.Plugins;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EltraNavigoMPlayer.Plugins
{   
    public class EltraNavigoMPlayerPlugin : IEltraNavigoPlugin
    {
        public IDialogService DialogService { get; set; }

        public List<ToolViewModel> GetViewModels()
        {
            var result = new List<ToolViewModel>();

            result.Add(new MPlayerControlViewModel());

            return result;
        }

        public UserControl GetView(ToolViewModel viewModel)
        {
            UserControl result = null;

            if (viewModel is MPlayerControlViewModel)
            {
                result = new MPlayerControlView();
            }
            
            return result;
        }
    }
}
