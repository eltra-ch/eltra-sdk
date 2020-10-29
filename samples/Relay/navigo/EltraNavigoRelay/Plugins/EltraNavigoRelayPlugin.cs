using EltraNavigoRelay.Views.Relay.RelayControl;
using EltraXamCommon.Controls;
using EltraXamCommon.Plugins;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Xamarin.Forms;

namespace EltraNavigoRelay.Plugins
{
    public class EltraNavigoRelayPlugin : IEltraNavigoPlugin
    {
        public IDialogService DialogService { get; set; }

        public List<ToolViewModel> GetViewModels()
        {
            var result = new List<ToolViewModel>();

            result.Add(new RelayControlViewModel());

            return result;
        }

        public ContentView GetView(ToolViewModel viewModel)
        {
            ContentView result = null;

            if (viewModel is RelayControlViewModel)
            {
                result = new RelayControlView();
            }

            return result;
        }
    }
}
