using EltraUiCommon.Controls;
using EltraUiCommon.Controls.Parameters;
using EltraXamCommon.Framework;

namespace EltraXamCommon.Controls.Parameters
{
    public class XamParameterLabelViewModel : ParameterLabelViewModel
    {
        public XamParameterLabelViewModel(ToolViewBaseModel parent, string uniqueId)
            : base(parent, uniqueId)
        {
            Init(new InvokeOnMainThread());
        }
    }
}
