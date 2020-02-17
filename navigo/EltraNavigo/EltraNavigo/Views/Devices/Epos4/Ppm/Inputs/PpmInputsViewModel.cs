using EltraNavigo.Controls;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Ppm.Inputs
{
    public class PpmInputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private ParameterEditViewModel _targetPosition;
        private ParameterEditViewModel _profileVelocity;
        private ParameterEditViewModel _profileAcceleration;
        private ParameterEditViewModel _profileDeceleration;
        private ParameterEditViewModel _quickStopDeceleration;

        private bool _isAbsoluteTarget;
        private bool _changeImmediately;

        #endregion

        #region Construtors

        public PpmInputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _targetPosition = new ParameterEditViewModel(this, "PARAM_TargetPosition");
            _profileVelocity = new ParameterEditViewModel(this, "PARAM_ProfileVelocity");
            _profileAcceleration = new ParameterEditViewModel(this, "PARAM_ProfileAcceleration");
            _profileDeceleration = new ParameterEditViewModel(this,"PARAM_ProfileDeceleration");
            _quickStopDeceleration = new ParameterEditViewModel(this, "PARAM_QuickstopDeceleration");
        }

        #endregion

        #region Properties

        public ParameterEditViewModel TargetPosition
        {
            get => _targetPosition;
            set => SetProperty(ref _targetPosition, value);
        }

        public ParameterEditViewModel ProfileVelocity
        {
            get => _profileVelocity;
            set => SetProperty(ref _profileVelocity, value);
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

        public bool IsAbsoluteTarget
        {
            get => _isAbsoluteTarget;
            set => SetProperty(ref _isAbsoluteTarget, value);
        }

        public bool ChangeImmediately
        {
            get => _changeImmediately;
            set => SetProperty(ref _changeImmediately, value);
        }

        #endregion
    }
}
