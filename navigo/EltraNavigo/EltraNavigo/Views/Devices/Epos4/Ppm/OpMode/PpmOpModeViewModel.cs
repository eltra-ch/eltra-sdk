using EltraCloudContracts.ObjectDictionary.Epos4.Definitions;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Toast;

namespace EltraNavigo.Views.Ppm.OpMode
{
    public class PpmOpModeViewModel : OpModeViewModel
    {
        public PpmOpModeViewModel(ToolViewBaseModel parent, Epos4OperationModes opMode) 
            : base(parent,opMode)
        {
        }

        public override async void ActivateOpMode()
        {
            if (Vcs != null)
            {
                await EposVcs.ActivateProfilePositionMode();

                ToastMessage.ShortAlert("Activate profile position mode");
            }            
        }
    }
}
