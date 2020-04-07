using System.Threading.Tasks;

namespace EltraNotKauf.Controls
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

        public override bool StartUpdate()
        {
            bool result = base.StartUpdate();

            IsBusy = false;

            return result;
        }

        public override bool StopUpdate()
        {
            bool result = base.StopUpdate();

            IsBusy = true;

            return result;
        }

        #endregion
    }
}
