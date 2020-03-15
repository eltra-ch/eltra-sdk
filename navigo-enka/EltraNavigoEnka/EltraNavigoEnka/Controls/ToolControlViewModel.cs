using System.Threading.Tasks;

namespace EltraNavigo.Controls
{
    public class ToolControlViewModel : ToolViewBaseModel
    {
        #region Construtors

        public ToolControlViewModel()
        {
        }

        public ToolControlViewModel(ToolViewBaseModel parent)
            : base(parent)
        {            
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            IsBusy = false;

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            IsBusy = true;

            return result;
        }

        #endregion
    }
}
