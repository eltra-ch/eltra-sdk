using EltraUiCommon.Controls;
using EltraUiCommon.Controls.Parameters;
using EltraWpfCommon.Framework;

namespace EltraWpfCommon.Controls.Parameters
{
    public class WpfParameterComboViewModel : ParameterComboViewModel
    {
        public WpfParameterComboViewModel(ToolViewBaseModel parent, string uniqueId)
            : base(parent, uniqueId)
        {
            Init(new InvokeOnMainThread());
        }
    }
}
