using EltraNavigo.Controls;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Homing.Inputs
{
    public class HomingInputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private ParameterComboViewModel _homingMethod;
        private ParameterEditViewModel _homeOffsetMoveDistance;
        private ParameterEditViewModel _speedForSwitchSearch;
        private ParameterEditViewModel _speedForZeroSearch;
        private ParameterEditViewModel _homingAcceleration;
        private ParameterEditViewModel _homePosition;
        private ParameterEditViewModel _quickStopDeceleration;
                
        #endregion

        #region Construtors

        public HomingInputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _homingMethod = new ParameterComboViewModel(this, "PARAM_HomingMethod");
            _homeOffsetMoveDistance = new ParameterEditViewModel(this, "PARAM_HomeOffset");
            _speedForSwitchSearch = new ParameterEditViewModel(this, "PARAM_SpeedForSwitchSearch");
            _speedForZeroSearch = new ParameterEditViewModel(this, "PARAM_SpeedForZeroSearch");
            _homingAcceleration = new ParameterEditViewModel(this,"PARAM_HomingAcceleration");
            _homePosition = new ParameterEditViewModel(this,"PARAM_HomePosition");
            _quickStopDeceleration = new ParameterEditViewModel(this, "PARAM_QuickstopDeceleration");
        }

        #endregion

        #region Properties

        public ParameterEditViewModel HomeOffsetMoveDistance
        {
            get => _homeOffsetMoveDistance;
            set => SetProperty(ref _homeOffsetMoveDistance, value);
        }

        public ParameterEditViewModel SpeedForSwitchSearch
        {
            get => _speedForSwitchSearch;
            set => SetProperty(ref _speedForSwitchSearch, value);
        }

        public ParameterEditViewModel SpeedForZeroSearch
        {
            get => _speedForZeroSearch;
            set => SetProperty(ref _speedForZeroSearch, value);
        }

        public ParameterEditViewModel HomingAcceleration
        {
            get => _homingAcceleration;
            set => SetProperty(ref _homingAcceleration, value);
        }

        public ParameterEditViewModel HomePosition
        {
            get => _homePosition;
            set => SetProperty(ref _homePosition, value);
        }
        
        public ParameterEditViewModel QuickStopDeceleration
        {
            get => _quickStopDeceleration;
            set => SetProperty(ref _quickStopDeceleration, value);
        }

        public ParameterComboViewModel HomingMethod
        {
            get => _homingMethod;
            set => SetProperty(ref _homingMethod, value);
        }

        #endregion
    }
}
