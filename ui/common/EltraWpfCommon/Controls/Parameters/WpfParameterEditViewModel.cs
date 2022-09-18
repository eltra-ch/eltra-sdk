using EltraUiCommon.Controls;
using EltraUiCommon.Controls.Parameters;
using EltraWpfCommon.Framework;

namespace EltraWpfCommon.Controls.Parameters
{
    public class WpfParameterEditViewModel : ParameterEditViewModel
    {
        public WpfParameterEditViewModel(ToolViewBaseModel parent, string uniqueId)
            : base(parent, uniqueId)
        {
            Init(new InvokeOnMainThread());
        }
    }
}
