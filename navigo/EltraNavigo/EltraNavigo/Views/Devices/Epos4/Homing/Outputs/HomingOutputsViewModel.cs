using EltraNavigo.Controls;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Homing.Outputs
{
    public class HomingOutputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private ParameterLabelViewModel _demandPosition;
        private ParameterLabelViewModel _actualPosition;
        private ParameterLabelViewModel _actualVelocity;
        private ParameterLabelViewModel _actualCurrent;

        #endregion

        #region Construtors

        public HomingOutputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _demandPosition = new ParameterLabelViewModel(this, "PARAM_PositionDemandValue");
            _actualPosition = new ParameterLabelViewModel(this, "PARAM_PositionActualValue");
            _actualVelocity = new ParameterLabelViewModel(this, "PARAM_VelocityActualValueAveraged");
            _actualCurrent = new ParameterLabelViewModel(this, "PARAM_CurrentActualValueAveraged");
        }

        #endregion

        #region Properties

        public ParameterLabelViewModel DemandPosition
        {
            get => _demandPosition;
            set => SetProperty(ref _demandPosition, value);
        }

        public ParameterLabelViewModel ActualPosition
        {
            get => _actualPosition;
            set => SetProperty(ref _actualPosition, value);
        }

        public ParameterLabelViewModel ActualVelocity
        {
            get => _actualVelocity;
            set => SetProperty(ref _actualVelocity, value);
        }

        public ParameterLabelViewModel ActualCurrent
        {
            get => _actualCurrent;
            set => SetProperty(ref _actualCurrent, value);
        }
        
        #endregion
    }
}
