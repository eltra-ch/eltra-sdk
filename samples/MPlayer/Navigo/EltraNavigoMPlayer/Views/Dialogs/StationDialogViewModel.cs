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
        private RadioStationEntry _radioStationEntry;
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

        public RadioStationEntry RadioStationEntry
        {
            get => _radioStationEntry;
            set => SetProperty(ref _radioStationEntry, value);
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

        private void OnRequestApply()
        {
            SendCloseRequest(new DialogParameters() { { "result", "apply" } });
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            _parameters = parameters;

            RadioStationEntry = parameters.GetValue<RadioStationEntry>("entry");
        }
        
        #endregion
    }
}
