using System.Threading.Tasks;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraNavigo.Views.Devices.Epos4;

namespace EltraNavigo.Controls
{
    public class OpModeViewModel : EposToolViewBaseModel
    {
        #region Private fields

        private string _operationMode;
        private bool _isActivateEnabled;
        private readonly Epos4OperationModes _opMode;
        private Parameter _operationModeParameter;

        #endregion

        #region Constructors

        public OpModeViewModel(ToolViewBaseModel parent, Epos4OperationModes opMode)
            : base(parent)
        {
            _opMode = opMode;
        }

        #endregion

        #region Properties

        public string OperationMode
        {
            get => _operationMode;
            set => SetProperty(ref _operationMode, value);
        }

        public bool IsActivateEnabled
        {
            get => _isActivateEnabled;
            set => SetProperty(ref _isActivateEnabled, value);
        }

        #endregion

        #region Events

        private void OnOperationModeChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter != null)
            {
                var operationModeParameter = e.Parameter;

                if (operationModeParameter.GetValue(out sbyte operationModeValue))
                {
                    IsActivateEnabled = operationModeValue != (sbyte)_opMode;
                }

                if (operationModeParameter.GetValue(out string operationMode))
                {
                    OperationMode = operationMode;
                }
            }
        }

        #endregion

        #region Methods

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            if (result)
            {
                if (Vcs != null)
                {
                    Vcs.RegisterParameterUpdate("PARAM_ModesOfOperationDisplay", ParameterUpdatePriority.High);
                }

                await UpdateOperationModeValue();

                if (_operationModeParameter != null)
                {
                    _operationModeParameter.ParameterChanged += OnOperationModeChanged;
                }
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                if (_operationModeParameter != null)
                {
                    _operationModeParameter.ParameterChanged -= OnOperationModeChanged;
                }

                if (Vcs != null)
                {
                    Vcs.UnregisterParameterUpdate("PARAM_ModesOfOperationDisplay", ParameterUpdatePriority.High);
                }
            }

            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            if (Vcs != null)
            {
                Vcs.RegisterParameterUpdate("PARAM_ModesOfOperationDisplay", ParameterUpdatePriority.High);
            }

            await UpdateOperationModeValue();

            if (_operationModeParameter != null)
            {
                _operationModeParameter.ParameterChanged += OnOperationModeChanged;
            }

            await base.Show();

            IsBusy = false;
        }

        private async Task UpdateOperationModeValue()
        {
            if (Vcs != null)
            {
                _operationModeParameter = await Vcs.GetParameter("PARAM_ModesOfOperationDisplay");

                if (_operationModeParameter != null)
                {
                    if (_operationModeParameter.GetValue(out sbyte operationModeValue))
                    {
                        IsActivateEnabled = operationModeValue != (sbyte)_opMode;
                    }

                    if (_operationModeParameter.GetValue(out string operationMode))
                    {
                        OperationMode = operationMode;
                    }
                }
            }
        }

        public override async Task Hide()
        {
            if (Vcs != null)
            {
                if (_operationModeParameter != null)
                {
                    _operationModeParameter.ParameterChanged -= OnOperationModeChanged;
                }

                Vcs.UnregisterParameterUpdate("PARAM_ModesOfOperationDisplay", ParameterUpdatePriority.High);
            }

            await base.Hide();
        }

        public virtual void ActivateOpMode()
        {
        }

        #endregion
    }
}
