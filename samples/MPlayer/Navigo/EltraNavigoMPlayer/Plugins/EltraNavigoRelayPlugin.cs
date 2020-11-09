using EltraNavigoMPlayer.Views.MPlayerControl;
using EltraUiCommon.Controls;
using EltraXamCommon.Dialogs;
using EltraXamCommon.Plugins;
using EltraXamCommon.Plugins.Events;
using MPlayerMaster.Views.Dialogs;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace EltraNavigoMPlayer.Plugins
{
    [Preserve(AllMembers = true)]
    public class EltraNavigoMPlayerPlugin : IEltraNavigoPluginService
    {
        #region Private fields

        private MPlayerControlViewModel _mPlayerControlViewModel;
        private MPlayerControlView _mPlayerControlView;

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

        #endregion

        #region Events

        public event EventHandler<DialogRequestedEventArgs> DialogRequested;

        #endregion

        #region Methods

        private MPlayerControlViewModel CreateMPlayerControlViewModel()
        {
            var result = new MPlayerControlViewModel();

            result.DialogRequested += (o, e) => { OnDialogRequested(e); };

            return result;
        }

        protected virtual void OnDialogRequested(DialogRequestedEventArgs e)
        {
            DialogRequested?.Invoke(this, e);
        }

        public List<ToolViewModel> GetViewModels()
        {
            var result = new List<ToolViewModel>();

            result.Add(MPlayerControlViewModel);

            return result;
        }

        public ContentView GetView(ToolViewModel viewModel)
        {
            ContentView result = null;

            if (viewModel is MPlayerControlViewModel)
            {
                result = MPlayerControlView;
            }
            
            return result;
        }

        public List<XamDialogViewModel> GetDialogViewModels()
        {
            return new List<XamDialogViewModel>() { new StationDialogViewModel() };
        }

        public View ResolveDialogView(XamDialogViewModel viewModel)
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
