using EltraNavigo.Controls;
using EltraNavigo.Device.Vcs;

namespace EltraNavigo.Views.Devices.Relay.Base
{
    public class PhotoToolViewModel : ToolViewModel
    {
        protected PhotoVcs PhotoVcs => Vcs as PhotoVcs;

    }
}
