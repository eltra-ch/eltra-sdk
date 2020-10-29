using EltraNavigoStreema.Views.StreemaControl;
using EltraXamCommon.Controls;
using EltraXamCommon.Plugins;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Xamarin.Forms;

namespace EltraNavigoStreema.Plugins
{
    public class EltraNavigoStreemaPlugin : IEltraNavigoPlugin
    {
        public IDialogService DialogService { get; set; }

        public List<ToolViewModel> GetViewModels()
        {
            var result = new List<ToolViewModel>();

            result.Add(new StreemaControlViewModel());

            return result;
        }

        public ContentView GetView(ToolViewModel viewModel)
        {
            ContentView result = null;

            if (viewModel is StreemaControlViewModel)
            {
                result = new StreemaControlView();
            }

            return result;
        }
    }
}
