using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraXamCommon.Controls;
using EltraXamCommon.Controls.Parameters;
using EltraXamCommon.Controls.Toast;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace EltraNavigoStreema.Views.Devices.Streema.StreemaControl.Station
{
    public class StreemaStationViewModel : ToolViewModel
    {
        #region Private fields

        private int _stationIndex;
        private int _activeStationValue;
        private string _controlButtonText;
        private XddParameter _activeStationParameter;
        private ImageSource _controlButtonImage;
        private bool _isStationEditVisible;
        private ParameterEditViewModel _stationIdParameter;
        private ParameterEditViewModel _stationVolumeScalingParameter;
        private bool _isActiveStation;

        #endregion

        #region Constructors

        public StreemaStationViewModel(ToolViewBaseModel parent, int stationIndex)
            : base(parent)
        {
            _stationIndex = stationIndex;

            _stationIdParameter = new ParameterEditViewModel(this, $"PARAM_Station_{stationIndex+1}_Id");   
            _stationVolumeScalingParameter = new ParameterEditViewModel(this, $"PARAM_Station_{stationIndex+1}_VolumeScaling");

            DeviceInitialized += OnDeviceInitialized;
        }

        #endregion

        #region Commands 

        public ICommand ControlButtonCommand => new Command(OnControlButtonPressed);
        
        public ICommand EditButtonCommand => new Command(OnEditButtonPressed);

        #endregion

        #region Properties

        public bool IsActiveStation
        {
            get => _isActiveStation;
            set => SetProperty(ref _isActiveStation, value);
        }

        public int ActiveStationValue
        {
            get => _activeStationValue;
            set => SetProperty(ref _activeStationValue, value);
        }

        public ImageSource ControlButtonImage
        {
            get => _controlButtonImage;
            set => SetProperty(ref _controlButtonImage, value);
        }

        public string ControlButtonText
        {
            get => _controlButtonText;
            set => SetProperty(ref _controlButtonText, value);
        }

        public bool IsStationEditVisible
        {
            get => _isStationEditVisible;
            set => SetProperty(ref _isStationEditVisible, value);
        }

        public ParameterEditViewModel StationIdParameter
        {
            get => _stationIdParameter;
        }

        public ParameterEditViewModel StationVolumeScalingParameter
        {
            get => _stationVolumeScalingParameter;
        }

        #endregion

        #region Events handling

        private void OnDeviceInitialized(object sender, EventArgs e)
        {
            InitializeActiveStationParameter();

            InitializeStationParameter();
        }

        private void OnEditButtonPressed(object obj)
        {
            IsStationEditVisible = !IsStationEditVisible;
        }

        private void OnActiveStationParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter is Parameter activeStationParameter)
            {
                if (activeStationParameter.GetValue(out int activeStationValue))
                {
                    ActiveStationValue = activeStationValue;

                    IsActiveStation = ActiveStationValue == (_stationIndex + 1);
                }
            }
        }

        private void OnStationImageParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter.GetValue(out string imageUrl))
            {
                ControlButtonImage = ImageSource.FromUri(new Uri(imageUrl));
            }
        }

        private void OnStationLabelParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter.GetValue(out string label))
            {
                ControlButtonText = label;
            }
        }

        private void OnStationParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (IsActiveStation)
            {
                ActiveSelection(0);
            }
        }

        private void OnControlButtonPressed(object obj)
        {
            ActiveSelection(_stationIndex + 1);
        }

        #endregion

        #region Methods

        public override void SetUp()
        {
            base.SetUp();
        }

        public override Task Show()
        {
            var result = base.Show();

            StationIdParameter.InitModelData();
            
            StationVolumeScalingParameter.InitModelData();
            
            return result;
        }

        private void InitializeStationParameter()
        {
            ushort index = (ushort)(0x4000 + (ushort)_stationIndex);
            var urlStationParameter = Device.SearchParameter(index, 0x01) as XddParameter;
            var labelStationParameter = Device.SearchParameter(index, 0x02) as XddParameter;
            var imageStationParameter = Device.SearchParameter(index, 0x03) as XddParameter;

            if (urlStationParameter != null && labelStationParameter != null && imageStationParameter != null)
            {
                labelStationParameter.AutoUpdate();
                imageStationParameter.AutoUpdate();

                labelStationParameter.ParameterChanged += OnStationLabelParameterChanged;
                imageStationParameter.ParameterChanged += OnStationImageParameterChanged;
                urlStationParameter.ParameterChanged += OnStationParameterChanged;

                Task.Run(async () =>
                {

                    await urlStationParameter.UpdateValue();
                    await labelStationParameter.UpdateValue();
                    await imageStationParameter.UpdateValue();

                }).ContinueWith((t) =>
                {
                    if (urlStationParameter.GetValue(out string url))
                    {
                        ControlButtonText = url;
                    }

                    if (labelStationParameter.GetValue(out string label))
                    {
                        ControlButtonText = label;
                    }

                    if (imageStationParameter.GetValue(out string imageurl))
                    {
                        try
                        {
                            ControlButtonImage = ImageSource.FromUri(new Uri(imageurl));
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
            }
        }

        private void InitializeActiveStationParameter()
        {
            _activeStationParameter = Device.SearchParameter(0x4100, 0x00) as XddParameter;

            if (_activeStationParameter != null)
            {
                _activeStationParameter.ParameterChanged += OnActiveStationParameterChanged;

                Task.Run(async () =>
                {

                    await _activeStationParameter.UpdateValue();

                }).ContinueWith((t) =>
                {

                    if (_activeStationParameter.GetValue(out int activeStationValue))
                    {
                        ActiveStationValue = activeStationValue;

                        IsActiveStation = ActiveStationValue == (_stationIndex + 1);
                    }
                });
            }
        }

        private async void ActiveSelection(int index)
        {
            if (_activeStationParameter != null && _activeStationParameter.SetValue(index))
            {
                if (!await _activeStationParameter.Write())
                {
                    ToastMessage.ShortAlert($"Activate Button {index + 1} failed!");
                }
            }
        }

        #endregion
    }
}
