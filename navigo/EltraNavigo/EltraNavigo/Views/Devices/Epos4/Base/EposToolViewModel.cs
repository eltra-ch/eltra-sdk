using EltraNavigo.Controls;
using EltraNavigo.Device.Vcs;

namespace EltraNavigo.Views.Devices.Epos4
{
    public class EposToolViewBaseModel : ToolViewBaseModel
    {
        public EposToolViewBaseModel()
        {
        }

        public EposToolViewBaseModel(ToolViewBaseModel parent)
            : base(parent)
        {            
        }

        protected EposVcs EposVcs => Vcs as EposVcs;
    }
}
