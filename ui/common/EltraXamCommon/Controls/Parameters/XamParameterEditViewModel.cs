using EltraUiCommon.Controls;
using EltraUiCommon.Controls.Parameters;
using EltraXamCommon.Framework;

namespace EltraXamCommon.Controls.Parameters
{
    public class XamParameterEditViewModel : ParameterEditViewModel
    {
        public XamParameterEditViewModel(ToolViewBaseModel parent, string uniqueId)
            : base(parent, uniqueId)
        {
            Init(new InvokeOnMainThread());
        }
    }
}
