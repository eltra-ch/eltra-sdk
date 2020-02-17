using System;
using System.Collections;
using System.Threading.Tasks;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraNavigo.Controls;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraNavigo.Device.Epos4.Parameters.Events;

namespace EltraNavigo.Device.Epos4.Parameters
{
    class StatusWordViewModel : ToolViewBaseModel
    {
        #region Constructors
        public StatusWordViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
        }

        #endregion
        
        #region Properties

        private ushort _statusWord;
        private Parameter _statusWordParameter;

        #endregion

        #region Events

        public event EventHandler<StatusWordChangedEventArgs> StatusWordChanged;

        #endregion

        #region Events handling

        private void OnStatusWordChanged(StatusWordChangedEventArgs args)
        {
            StatusWordChanged?.Invoke(this, args);
        }

        #endregion

        #region Properties

        public ushort StatusWord
        { 
            get => _statusWord;
            set
            {
                if(SetProperty(ref _statusWord, value))
                {
                    OnStatusWordChanged(new StatusWordChangedEventArgs() { StatusWordValue = value });
                }
            }
        }
                
        public bool IsQuickStopActive
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x00x 0111
                if (statusBits[0] && statusBits[1] && statusBits[2] && !statusBits[3] &&
                    !statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsOperationEnabled
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x01x 0111
                if (statusBits[0] && statusBits[1] && statusBits[2] && !statusBits[3] &&
                    statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsSwitchOnDisabled
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x10x 0000
                if (!statusBits[0] && !statusBits[1] && !statusBits[2] && !statusBits[3] &&
                    !statusBits[5] && statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsDisabled
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x10x 0000
                if (!statusBits[0] && !statusBits[1] && !statusBits[2] && !statusBits[3] &&
                    !statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsReadyToSwitchOn
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x01x 0001
                if (statusBits[0] && !statusBits[1] && !statusBits[2] && !statusBits[3] &&
                    statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsSwitchOn
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x01x 0011
                if (statusBits[0] && statusBits[1] && !statusBits[2] && !statusBits[3] &&
                    statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsFaultReactionActive
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x00x 1111
                if (statusBits[0] && statusBits[1] && statusBits[2] && statusBits[3] &&
                    !statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        public bool IsFault
        {
            get
            {
                bool result = false;
                var statusBits = new BitArray(BitConverter.GetBytes(_statusWord));

                //xxxx xxxx x00x 1000
                if (!statusBits[0] && !statusBits[1] && !statusBits[2] && statusBits[3] &&
                    !statusBits[5] && !statusBits[6])
                {
                    result = true;
                }

                return result;
            }
        }

        #endregion

        #region Events

        private void OnStatusWordParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            var parameter = e.Parameter;
            
            if (parameter != null)
            {
                if (parameter.GetValue(out ushort statusWord))
                {
                    StatusWord = statusWord;                    
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
                    Vcs.RegisterParameterUpdate("PARAM_StatusWord", ParameterUpdatePriority.Medium);
                }

                await UpdateStatusWordValue();
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
                    Vcs.UnregisterParameterUpdate("PARAM_StatusWord", ParameterUpdatePriority.Medium);
                }
            }

            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            if (Vcs != null)
            {
                Vcs.RegisterParameterUpdate("PARAM_StatusWord", ParameterUpdatePriority.Medium);

                await UpdateStatusWordValue();

                RegisterEvents();
            }

            await base.Show();

            IsBusy = false;
        }

        private async Task UpdateStatusWordValue()
        {
            if (Vcs != null)
            {
                _statusWordParameter = await Vcs.GetParameter("PARAM_StatusWord");

                if (_statusWordParameter != null && _statusWordParameter.GetValue(out ushort statusWord))
                {
                    StatusWord = statusWord;
                }
            }
        }

        private void RegisterEvents()
        {
            if (_statusWordParameter != null)
            {
                _statusWordParameter.ParameterChanged += OnStatusWordParameterChanged;
            }
        }

        private void UnregisterEvents()
        {
            if (_statusWordParameter != null)
            {
                _statusWordParameter.ParameterChanged -= OnStatusWordParameterChanged;
            }
        }

        public override async Task Hide()
        {
            if (Vcs != null)
            {
                UnregisterEvents();

                Vcs.UnregisterParameterUpdate("PARAM_StatusWord", ParameterUpdatePriority.Medium);
            }

            await base.Hide();
        }
        
        #endregion
    }
}
