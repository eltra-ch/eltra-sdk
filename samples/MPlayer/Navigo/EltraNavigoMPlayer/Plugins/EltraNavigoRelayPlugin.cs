using EltraNavigoMPlayer.Views.MPlayerControl;
using EltraXamCommon.Controls;
using EltraXamCommon.Plugins;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Xamarin.Forms;

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

        public ContentView GetView(ToolViewModel viewModel)
        {
            ContentView result = null;

            if (viewModel is MPlayerControlViewModel)
            {
                result = new MPlayerControlView();
            }
            
            return result;
        }
    }
}
