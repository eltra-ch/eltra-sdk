using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Toast;

namespace EltraNavigo.Views.Homing.OpMode
{
    public class HomingOpModeViewModel : OpModeViewModel
    {
        public HomingOpModeViewModel(ToolViewBaseModel parent, Epos4OperationModes opMode) 
            : base(parent,opMode)
        {
        }

        public override async void ActivateOpMode()
        {
            if (Vcs != null)
            {
                await EposVcs.ActivateHomingMode();

                ToastMessage.ShortAlert("Activate homing mode");
            }
        }
    }
}
