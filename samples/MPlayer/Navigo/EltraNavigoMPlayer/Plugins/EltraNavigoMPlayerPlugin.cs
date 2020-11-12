using EltraNavigoMPlayer.Views.MPlayerControl;
using EltraUiCommon.Controls;
using EltraXamCommon.Dialogs;
using EltraXamCommon.Plugins;
using MPlayerMaster.Views.Dialogs;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace EltraNavigoMPlayer.Plugins
{
    [Preserve(AllMembers = true)]
    public class EltraNavigoMPlayerPlugin : EltraNavigoPluginService
    {
        #region Private fields

        private MPlayerControlViewModel _mPlayerControlViewModel;
        private MPlayerControlView _mPlayerControlView;
        private StationDialogViewModel _stationDialogViewModel;

        #endregion

        #region Properties

        private MPlayerControlViewModel MPlayerControlViewModel
        {
            get => _mPlayerControlViewModel ?? (_mPlayerControlViewModel = CreateMPlayerControlViewModel());
        }

        private MPlayerControlView MPlayerControlView
        {
            get => _mPlayerControlView ?? (_mPlayerControlView = new MPlayerControlView());
        }

        private StationDialogViewModel StationDialogViewModel
        {
            get => _stationDialogViewModel ?? (_stationDialogViewModel = new StationDialogViewModel());
        }

        #endregion

        #region Methods

        private MPlayerControlViewModel CreateMPlayerControlViewModel()
        {
            var result = new MPlayerControlViewModel();

            result.PluginService = this;

            return result;
        }

        public override List<ToolViewModel> GetViewModels()
        {
            var result = new List<ToolViewModel>();

            result.Add(MPlayerControlViewModel);

            return result;
        }

        public override View ResolveView(ToolViewModel viewModel)
        {
            ContentView result = null;

            if (viewModel is MPlayerControlViewModel)
            {
                result = MPlayerControlView;
            }
            
            return result;
        }

        public override List<XamDialogViewModel> GetDialogViewModels()
        {
            return new List<XamDialogViewModel>() { StationDialogViewModel };
        }

        public override View ResolveDialogView(XamDialogViewModel viewModel)
        {
            View result = null;
            
            if(viewModel is StationDialogViewModel)
            {
                result = new StationDialogView();
            }

            return result;
        }

        #endregion
    }
}
