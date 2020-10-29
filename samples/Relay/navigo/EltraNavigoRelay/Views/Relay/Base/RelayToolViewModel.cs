using EltraNavigoRelay.Device.Vcs;
using EltraNavigoRelay.Device.Vcs.Factory;
using EltraXamCommon.Controls;
using EltraXamCommon.Device.Factory;

namespace EltraNavigoRelay.Views.Relay.Base
{
    public class RelayToolViewModel : ToolViewModel
    {
        protected RelayVcs RelayVcs => Vcs as RelayVcs;

        protected override IDeviceVcsFactory GetDeviceFactory()
        {
            IDeviceVcsFactory result = base.GetDeviceFactory();

            if (result == null)
            {
                SetDeviceFactory(new RelayVcsFactory());
            }

            return base.GetDeviceFactory();
        }
    }
}
