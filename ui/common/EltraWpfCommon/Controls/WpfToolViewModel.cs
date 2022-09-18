using EltraUiCommon.Controls;
using EltraWpfCommon.Framework;
using System.Windows.Media;

namespace EltraWpfCommon.Controls
{
    public class WpfToolViewModel : ToolViewModel
    {
        #region Constructors

        public WpfToolViewModel()
        {
            Init(new InvokeOnMainThread());
        }

        public WpfToolViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public ImageSource Image { get; set; } 

        #endregion
    }
}
