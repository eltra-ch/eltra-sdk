using System.Diagnostics;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraNavigo.Controls.Parameters
{
    public class ParameterLabelViewModel : ParameterControlViewModel
    {
        #region Private fields

        private readonly string _uniqueId;        
        private string _value;
        private string _unit;
        private Parameter _parameter;

        #endregion

        #region Constructors

        public ParameterLabelViewModel(ToolViewBaseModel parent, string uniqueId) 
            : base(parent, uniqueId)
        {
            _uniqueId = uniqueId;
            IsEnabled = true;
        }

        #endregion

        #region Properties

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public string DateTimeFormat { get; set; }

        public Parameter Model => _parameter;

        #endregion

        #region Events

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter != null)
            {
                string valueAsText = e.Parameter.GetValueAsString();

                Value = valueAsText;
            }
        }

        #endregion

        #region Methods

        public override void InitModelData()
        {
            if (Vcs != null)
            {
                if (Vcs.SearchParameter(_uniqueId) is Parameter parameter)
                {
                    _parameter = parameter;

                    _parameter.DateTimeFormat = DateTimeFormat;

                    var valueAsText = _parameter.GetValueAsString();

                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        Value = valueAsText;
                        Label = _parameter.Label;

                        if (parameter is XddParameter epos4Parameter)
                        {
                            Unit = epos4Parameter.Unit.Label;
                        }
                    });
                }
            }
        }

        protected override void OnInitialized()
        {
            InitModelData();
        }

        public override async Task<bool> StartUpdate()
        {
            bool result = true;

            if (!IsUpdating)
            {
                result = await base.StartUpdate();

                if (result)
                {
                    Vcs?.RegisterParameterUpdate(UniqueId);

                    await UpdateParameterValue();
                }
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = true;

            if (IsUpdating)
            {
                result = await base.StopUpdate();

                if (result)
                {
                    Vcs?.UnregisterParameterUpdate(UniqueId);
                }
            }
            
            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            if (!IsVisible)
            {
                RegisterEvents();

                await base.Show();
            }

            IsBusy = false;
        }

        public override async Task Hide()
        {
            if (IsVisible)
            {
                UnregisterEvents();

                await base.Hide();
            }
        }

        private async Task<bool> UpdateParameterValue()
        {
            bool result = false;

            if (Vcs != null)
            {
                var parameterValue = await Vcs.GetParameterValue(UniqueId);

                if (parameterValue != null)
                {
                    if (_parameter != null)
                    {
                        if (_parameter.SetValue(parameterValue))
                        {
                            var valueAsText = _parameter.GetValueAsString();

                            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                            {
                                Value = valueAsText;
                                Label = _parameter.Label;

                                if (_parameter is XddParameter epos4Parameter)
                                {
                                    Unit = epos4Parameter.Unit.Label;
                                }
                            });

                            result = true;
                        }
                        else
                        {
                            Debug.Fail($"set parameter '{UniqueId}' value failed!");
                        }
                    }
                    else
                    {
                        Debug.Fail($"get parameter '{UniqueId}' object failed!");
                    }
                }
                else
                {
                    Debug.Fail($"get parameter '{UniqueId}' value failed!");
                }
            }

            return result;
        }

        private void RegisterEvents()
        {
            if (_parameter != null)
            {
                _parameter.ParameterChanged += OnParameterChanged;
            }
        }

        private void UnregisterEvents()
        {
            if (_parameter != null)
            {
                _parameter.ParameterChanged -= OnParameterChanged;
            }
        }

        #endregion
    }
}
