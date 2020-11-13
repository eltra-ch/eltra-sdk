using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraNavigoMPlayer.Views.Dialogs;
using EltraXamCommon.Dialogs;
using MPlayerCommon.Contracts;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace MPlayerMaster.Views.Dialogs
{
    public class StationDialogViewModel : XamDialogViewModel
    {
        #region Private fields

        private bool _isBusy;
        private RadioStationEntryViewModel _radioStationEntryViewModel;
        private IDialogParameters _parameters;

        #endregion

        #region Constructors

        public StationDialogViewModel()
        {
            CloseCommand = new DelegateCommand(() => OnRequestClose());
            ApplyCommand = new DelegateCommand(() => OnRequestApply());
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public RadioStationEntryViewModel RadioStationEntryViewModel
        {
            get => _radioStationEntryViewModel;
            set => SetProperty(ref _radioStationEntryViewModel, value);
        }

        #endregion

        #region Commands

        public DelegateCommand CloseCommand { get; }
        public DelegateCommand ApplyCommand { get; }

        #endregion

        #region Events handling

        #endregion

        #region Methods

        private void OnRequestClose()
        {
            SendCloseRequest();
        }

        private async void OnRequestApply()
        {
            IsBusy = true;

            bool result = await RadioStationEntryViewModel.Apply();

            IsBusy = false;

            SendCloseRequest(new DialogParameters() { { "command", "apply" }, { "result", result } });
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            _parameters = parameters;
            
            var radioStationEntry = parameters.GetValue<RadioStationEntry>("entry");
            var stationParameterEntry = parameters.GetValue<XddParameter>("stationIdParameter");

            if (radioStationEntry != null)
            {
                RadioStationEntryViewModel = new RadioStationEntryViewModel(radioStationEntry, stationParameterEntry);
            }
        }
        
        #endregion
    }
}
