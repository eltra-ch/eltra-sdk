using EltraNavigo.Controls;
using EltraNavigo.Device.Vcs;

namespace EltraNavigo.Views.Devices.Relay.Base
{
    public class RelayToolViewModel : ToolViewModel
    {
        protected RelayVcs RelayVcs => Vcs as RelayVcs;

    }
}
