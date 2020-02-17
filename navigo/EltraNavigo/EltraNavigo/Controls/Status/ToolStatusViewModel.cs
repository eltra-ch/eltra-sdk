using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraConnector.UserAgent.Vcs;
using EltraNavigo.Device.Epos4.Parameters;
using EltraNavigo.Device.Vcs;
using EltraNavigo.Resources;
using EltraNavigo.Views.Devices.Epos4;

namespace EltraNavigo.Controls.Status
{
    public class ToolStatusViewModel : EposToolViewBaseModel
    {
        #region Private fields

        private StatusWordViewModel _statusWordViewModel;
        private List<ErrorHistoryEntryViewModel> _errorHistory;
        private bool _errorHistoryRefreshing;
        private double _errorHistoryListHeight;
        private double _errorHistoryListWidth;
        private string _statusText;
        private bool _isClearFaultButtonVisible;
        
        #endregion

        #region Construtors

        public ToolStatusViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _errorHistoryListHeight = 0;
            _statusWordViewModel = new StatusWordViewModel(this);
        }

        #endregion

        #region Properties

        public double RowHeight = 40;
        
        public List<ErrorHistoryEntryViewModel> ErrorHistory
        {
            get => _errorHistory ?? (_errorHistory = new List<ErrorHistoryEntryViewModel>());
            set => SetProperty(ref _errorHistory, value);
        }

        public bool ErrorHistoryRefreshing
        {
            get => _errorHistoryRefreshing;
            set => SetProperty(ref _errorHistoryRefreshing, value);
        }

        public double ErrorHistoryListHeight
        {
            get => _errorHistoryListHeight;
            set => SetProperty(ref _errorHistoryListHeight, value);
        }

        public double ErrorHistoryListWidth
        {
            get => _errorHistoryListWidth;
            set => SetProperty(ref _errorHistoryListWidth, value);
        }
        
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsClearFaultButtonVisible
        {
            get => _isClearFaultButtonVisible;
            set => SetProperty(ref _isClearFaultButtonVisible, value);
        }

        #endregion

        #region Methods

        protected override async Task Update()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Update", "Update ToolStatusViewModel");

            await UpdateErrorHistory();

            if (!_statusWordViewModel.IsFault)
            {
                StatusText = _statusWordViewModel.IsOperationEnabled
                    ? AppResources.Enabled
                    : AppResources.Disabled;

                IsClearFaultButtonVisible = false;
            }
            else
            {
                StatusText = AppResources.Fault;
                
                IsClearFaultButtonVisible = true;
            }
        }

        private async Task UpdateErrorHistory()
        {
            if (Vcs != null)
            {
                var numberOfErrors = await Vcs.GetParameterValue("PARAM_ErrorHistory_NumberOfErrors");
                byte numberOfErrorsValue = 0;

                if (numberOfErrors != null && numberOfErrors.GetValue(ref numberOfErrorsValue))
                {
                    var errorHistory = new List<ErrorHistoryEntryViewModel>();

                    if (numberOfErrorsValue > 0)
                    {
                        int maxDescriptionSize = 0;

                        for (byte i = 0; i < numberOfErrorsValue; i++)
                        {
                            var errorHistoryParameter = await Vcs.GetParameter($"PARAM_ErrorHistory_{i + 1}");
                            if (errorHistoryParameter != null)
                            {
                                var entry = new ErrorHistoryEntryViewModel();

                                if (errorHistoryParameter.GetStructVariableValue("SErrorHistory_ErrorCode",
                                    out uint errorCode))
                                {
                                    if (errorHistoryParameter.GetStructVariableValue("SErrorHistory_ErrorCode",
                                        out string description))
                                    {
                                        entry.ErrorCode = errorCode;
                                        entry.ErrorDescription = description;

                                        if (description.Length > maxDescriptionSize)
                                        {
                                            maxDescriptionSize = description.Length;
                                        }

                                        errorHistory.Add(entry);
                                    }
                                }
                            }
                        }

                        ErrorHistoryListHeight = RowHeight * ErrorHistory.Count;
                        ErrorHistoryListWidth = maxDescriptionSize * 8;
                    }
                    
                    ErrorHistoryRefreshing = true;

                    ErrorHistory = errorHistory;
                    
                    ErrorHistoryRefreshing = false;
                }
            }
        }

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            if (result)
            {
                if (Vcs != null)
                {
                    Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_NumberOfErrors");
                    Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_1");
                    Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_2");
                    Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_3");
                    Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_4");
                    Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_5");
                }

                await UpdateErrorHistory();
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                if (Vcs != null)
                {
                   Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_NumberOfErrors");
                   Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_1");
                   Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_2");
                   Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_3");
                   Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_4");
                   Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_5");
                }
            }

            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            if (Vcs != null)
            {
                Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_NumberOfErrors");
                Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_1");
                Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_2");
                Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_3");
                Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_4");
                Vcs.RegisterParameterUpdate("PARAM_ErrorHistory_5");
            }

            await base.Show();

            IsBusy = false;
        }

        public override async Task Hide()
        {
            if (Vcs != null)
            {
                Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_NumberOfErrors");
                Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_1");
                Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_2");
                Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_3");
                Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_4");
                Vcs.UnregisterParameterUpdate("PARAM_ErrorHistory_5");
            }

            await base.Hide();
        }

        #endregion

        public async void ClearFault()
        {
            await EposVcs.ClearFault();
        }
    }
}
