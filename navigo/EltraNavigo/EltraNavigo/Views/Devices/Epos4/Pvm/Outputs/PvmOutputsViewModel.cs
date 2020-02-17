using EltraNavigo.Controls;
using EltraNavigo.Controls.Parameters;

namespace EltraNavigo.Views.Pvm.Outputs
{
    public class PvmOutputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private ParameterLabelViewModel _demandVelocity;
        private ParameterLabelViewModel _actualVelocity;
        private ParameterLabelViewModel _actualCurrent;

        #endregion

        #region Construtors

        public PvmOutputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _demandVelocity = new ParameterLabelViewModel(this, "PARAM_VelocityDemandValue");
            _actualVelocity = new ParameterLabelViewModel(this, "PARAM_VelocityActualValueAveraged");
            _actualCurrent = new ParameterLabelViewModel(this, "PARAM_CurrentActualValueAveraged");
        }

        #endregion

        #region Properties

        public ParameterLabelViewModel DemandVelocity
        {
            get => _demandVelocity;
            set => SetProperty(ref _demandVelocity, value);
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
