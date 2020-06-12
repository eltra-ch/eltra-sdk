using EltraCommon.Logger;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraNavigo.Device.Vcs;
using EltraNavigo.Views.Devices.Relay.Base;
using EltraCloudContracts.Contracts.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using System.IO;

namespace EltraNavigo.Views.PhotoControl
{
    public class PhotoControlViewModel : PhotoToolViewModel
    {
        #region Private fields

        private string[] _controlButtonText;
        private bool[] _isControlButtonEnabled;
        private ImageSource _variable1Image;
        private Parameter _variable1Data;

        #endregion

        #region Constructors

        public PhotoControlViewModel()
        {
            UpdateViewModels = false;

            _controlButtonText = new string[2];
            _isControlButtonEnabled = new bool[2];

            SetControlButtonText(0, "Take Picture");
            EnableControlButton(0, true);

            Title = "Photo Control";
            Image = ImageSource.FromResource("EltraNavigo.Resources.hazardous_32px.png");
            Uuid = "8AE87F6A-A118-4852-AD35-875D45E7DE36";
        }

        #endregion

        #region Commands 

        public ICommand ControlButton1Command => new Command(OnControlButton1Pressed);
        public ICommand ControlButton2Command => new Command(OnControlButton2Pressed);

        #endregion

        #region Properties
       
        public string ControlButton1Text
        {
            get => _controlButtonText[0];
            set => SetProperty(ref _controlButtonText[0], value);
        }
        
        public bool IsControlButton1Enabled
        { 
            get => _isControlButtonEnabled[0];
            set => SetProperty(ref _isControlButtonEnabled[0], value);
        }

        public ImageSource Variable1Image
        {
            get => _variable1Image;
            set => SetProperty(ref _variable1Image, value);
        }

        #endregion

        #region Events handling

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            byte[] buffer = null;

            if(e.NewValue.GetValue(ref buffer) && buffer != null)
            {
                Variable1Image = ImageSource.FromStream(() => new MemoryStream(buffer));
            }
        }

        #endregion

        #region Methods

        private async void OnControlButton1Pressed(object obj)
        {
            await OnButtonPressed(0);
        }

        private async void OnControlButton2Pressed(object obj)
        {
            await OnButtonPressed(1);
        }

        private void SetControlButtonText(int channelIndex, string text)
        {
            _controlButtonText[channelIndex] = text;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged($"ControlButton{channelIndex + 1}Text");
            });
        }

        private void EnableControlButton(int channelIndex, bool enable)
        {
            _isControlButtonEnabled[channelIndex] = enable;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged($"IsControlButton{channelIndex + 1}Enabled");
            });
        }
                
        private async Task OnButtonPressed(int channelIndex)
        {
            IsBusy = true;

            if (PhotoVcs != null)
            {
                try
                {
                    var call = await PhotoVcs.TakePicture((ushort)channelIndex);

                    if (call == null || !call.Result)
                    {                        
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - OnButtonPressed", e);
                }
            }

            IsBusy = false;
        }

        public override Task Show()
        {
            Task.Run(async ()=>
            {
                _variable1Data = await Vcs.GetParameter("PARAM_InternalRecorderBufferVariable1Data");

                if(_variable1Data!=null)
                {
                    _variable1Data.ParameterChanged += OnParameterChanged;
                }

                Vcs.RegisterParameterUpdate("PARAM_InternalRecorderBufferVariable1Data", ParameterUpdatePriority.Medium);
            });
            
            return base.Show();
        }

        public override Task Hide()
        {
            Vcs.UnregisterParameterUpdate("PARAM_InternalRecorderBufferVariable1Data", ParameterUpdatePriority.Medium);

            return base.Hide();
        }

        #endregion
    }
}
