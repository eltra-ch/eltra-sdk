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
using EltraNavigo.Controls.Toast;

namespace EltraNavigo.Views.PhotoControl
{
    public class PhotoControlViewModel : PhotoToolViewModel
    {
        #region Private fields

        private string _controlButtonText;
        private string _variable1ImageText;
        private bool _isControlButtonEnabled;
        private ImageSource _variable1Image;
        private Parameter _variable1Data;

        #endregion

        #region Constructors

        public PhotoControlViewModel()
        {
            UpdateViewModels = false;

            SetControlButtonText("Take Picture");
            EnableControlButton(true);

            Title = "Photo Control";
            Image = ImageSource.FromResource("EltraNavigo.Resources.camera_32px.png");
            Uuid = "8AE87F6A-A118-4852-AD35-875D45E7DE36";
        }

        #endregion

        #region Commands 

        public ICommand ControlButtonCommand => new Command(OnControlButtonPressed);
        public ICommand RecordVideoButtonCommand => new Command(OnRecordVideoButtonPressed);

        #endregion

        #region Properties

        public string ControlButtonText
        {
            get => _controlButtonText;
            set => SetProperty(ref _controlButtonText, value);
        }
        public string Variable1ImageText
        {
            get => _variable1ImageText;
            set => SetProperty(ref _variable1ImageText, value);
        }
        

        public bool IsControlButtonEnabled
        { 
            get => _isControlButtonEnabled;
            set => SetProperty(ref _isControlButtonEnabled, value);
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
            UpdateVariable1Image(e.NewValue);
        }

        #endregion

        #region Methods

        private async void OnControlButtonPressed(object obj)
        {
            await OnButtonPressed(0);
        }

        private async void OnRecordVideoButtonPressed(object obj)
        {
            IsBusy = true;

            if (PhotoVcs != null)
            {
                try
                {
                    var call = await PhotoVcs.RecordVideo(5);

                    if (call == null || !call.Result)
                    {
                        ToastMessage.ShortAlert("Error - no video taken!");
                    }
                    else
                    {
                        ToastMessage.ShortAlert("Video");
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - OnButtonPressed", e);
                }
            }

            IsBusy = false;
        }

        private void SetControlButtonText(string text)
        {
            ControlButtonText = text;
        }

        private void EnableControlButton(bool enable)
        {
            IsControlButtonEnabled = enable;
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
                        ToastMessage.ShortAlert("Error - no picture taken!");
                    }
                    else
                    {
                        ToastMessage.ShortAlert("Picture taken!");
                    }
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - OnButtonPressed", e);
                }
            }

            IsBusy = false;
        }

        private void RegisterParameterUpdate()
        {
            Task.Run(async () =>
            {
                _variable1Data = await Vcs.GetParameter("PARAM_InternalRecorderBufferVariable1Data");

                if (_variable1Data != null)
                {
                    _variable1Data.ParameterChanged += OnParameterChanged;
                }

                Vcs.RegisterParameterUpdate("PARAM_InternalRecorderBufferVariable1Data", ParameterUpdatePriority.Medium);
            });
        }

        private void UnregisterParameterUpdate()
        {
            Vcs.UnregisterParameterUpdate("PARAM_InternalRecorderBufferVariable1Data", ParameterUpdatePriority.Medium);
        }

        private void UpdateVariable1Image(ParameterValue variable1DataValue)
        {
            byte[] buffer = null;

            if (variable1DataValue != null)
            {
                if (variable1DataValue.GetValue(ref buffer) && buffer != null)
                {
                    Variable1Image = ImageSource.FromStream(() => new MemoryStream(buffer));
                }

                Variable1ImageText = $"{variable1DataValue.Modified}";
            }
        }

        public override Task Show()
        {
            Task.Run(async () => {
                var variable1DataValue = await Vcs.GetParameterValue("PARAM_InternalRecorderBufferVariable1Data");

                UpdateVariable1Image(variable1DataValue);
            });
            
            return base.Show();
        }

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            if (result)
            {
                RegisterParameterUpdate();
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                UnregisterParameterUpdate();
            }

            return result;
        }

        #endregion
    }
}
