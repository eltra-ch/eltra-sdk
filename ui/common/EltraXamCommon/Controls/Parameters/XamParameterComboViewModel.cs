using EltraUiCommon.Controls;
using EltraUiCommon.Controls.Parameters;
using EltraXamCommon.Framework;

namespace EltraXamCommon.Controls.Parameters
{
    public class XamParameterComboViewModel : ParameterComboViewModel
    {
        public XamParameterComboViewModel(ToolViewBaseModel parent, string uniqueId)
            : base(parent, uniqueId)
        {
            Init(new InvokeOnMainThread());
        }
    }
}
