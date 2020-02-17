using EltraNavigo.Controls;
using EltraNavigo.Device.Vcs;

namespace EltraNavigo.Views.Devices.Thermo.Base
{
    public class ThermoToolViewModel : ToolViewModel
    {
        #region Constructors

        public ThermoToolViewModel()
        {
        }

        public ThermoToolViewModel(ToolViewBaseModel parent)
            : base(parent)
        {

        }

        #endregion

        #region Properties

        protected ThermoVcs ThermoVcs => Vcs as ThermoVcs;

        #endregion
    }
}
