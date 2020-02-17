using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Toast;

namespace EltraNavigo.Views.Pvm.OpMode
{
    public class PvmOpModeViewModel : OpModeViewModel
    {
        #region Constructors

        public PvmOpModeViewModel(ToolViewBaseModel parent, Epos4OperationModes opMode) 
            : base(parent,opMode)
        {
        }

        #endregion

        #region Methods

        public override async void ActivateOpMode()
        {
            if (EposVcs != null)
            {
                await EposVcs.ActivateProfileVelocityMode();

                ToastMessage.ShortAlert("Activate profile velocity mode");
            }            
        }
        
        #endregion
    }
}
