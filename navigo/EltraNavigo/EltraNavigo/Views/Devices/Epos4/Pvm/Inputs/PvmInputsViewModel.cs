using EltraNavigo.Controls;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Pvm.Inputs
{
    public class PvmInputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private ParameterEditViewModel _targetVelocity;
        private ParameterEditViewModel _profileAcceleration;
        private ParameterEditViewModel _profileDeceleration;
        private ParameterEditViewModel _quickStopDeceleration;

        #endregion

        #region Construtors

        public PvmInputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _targetVelocity = new ParameterEditViewModel(this, "PARAM_TargetVelocity");
            _profileAcceleration = new ParameterEditViewModel(this, "PARAM_ProfileAcceleration");
            _profileDeceleration = new ParameterEditViewModel(this,"PARAM_ProfileDeceleration");
            _quickStopDeceleration = new ParameterEditViewModel(this, "PARAM_QuickstopDeceleration");
        }

        #endregion

        #region Properties

        public ParameterEditViewModel TargetVelocity
        {
            get => _targetVelocity;
            set => SetProperty(ref _targetVelocity, value);
        }

        public ParameterEditViewModel ProfileAcceleration
        {
            get => _profileAcceleration;
            set => SetProperty(ref _profileAcceleration, value);
        }

        public ParameterEditViewModel ProfileDeceleration
        {
            get => _profileDeceleration;
            set => SetProperty(ref _profileDeceleration, value);
        }
        
        public ParameterEditViewModel QuickStopDeceleration
        {
            get => _quickStopDeceleration;
            set => SetProperty(ref _quickStopDeceleration, value);
        }


        #endregion
    }
}
