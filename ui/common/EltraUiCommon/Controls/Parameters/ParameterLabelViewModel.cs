using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraUiCommon.Controls.Parameters
{
    public class ParameterLabelViewModel : ParameterControlViewModel
    {
        #region Private fields

        private string _value;
        private string _unit;
        private Parameter _parameter;

        #endregion

        #region Constructors

        public ParameterLabelViewModel(ToolViewBaseModel parent, string uniqueId) 
            : base(parent, uniqueId)
        {
            IsEnabled = true;
        }

        public ParameterLabelViewModel(ToolViewBaseModel parent, ushort index, byte subIndex)
            : base(parent, index, subIndex)
        {
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
            if (Vcs != null && Vcs.Device != null)
            {
                var device = Vcs.Device;
                Parameter parameter = null;

                if (!string.IsNullOrEmpty(UniqueId))
                {
                    parameter = device.SearchParameter(UniqueId) as Parameter;
                }
                else
                {
                    parameter = device.SearchParameter(Index, SubIndex) as Parameter;
                }

                if (parameter != null)
                {
                    _parameter = parameter;

                    _parameter.DateTimeFormat = DateTimeFormat;

                    var valueAsText = _parameter.GetValueAsString();

                    if (InvokeOnMainThread != null)
                    {
                        InvokeOnMainThread.BeginInvokeOnMainThread(() =>
                        {
                            UpdateParameterValue(parameter, valueAsText);
                        });
                    }
                    else
                    {
                        UpdateParameterValue(parameter, valueAsText);
                    }
                }
            }
        }

        private void UpdateParameterValue(Parameter parameter, string valueAsText)
        {
            Value = valueAsText;
            Label = _parameter.Label;

            if (parameter is XddParameter epos4Parameter)
            {
                Unit = epos4Parameter.Unit.Label;
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
                    _parameter?.AutoUpdate();

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
                    _parameter?.StopUpdate();
                }
            }
            
            return result;
        }

        public override async Task Show()
        {
            if (!IsVisible)
            {
                IsBusy = true;

                await base.Show();

                RegisterEvents();

                IsBusy = false;
            }
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

            if (Vcs != null && _parameter != null)
            {
                var parameterValue = await _parameter.ReadValue();

                if (parameterValue != null)
                {
                    if (_parameter != null)
                    {
                        if (_parameter.SetValue(parameterValue))
                        {
                            var valueAsText = _parameter.GetValueAsString();

                            if (InvokeOnMainThread != null)
                            {
                                InvokeOnMainThread.BeginInvokeOnMainThread(() =>
                                {
                                    UpdateParameterValue(_parameter, valueAsText);
                                });
                            }
                            else
                            {
                                UpdateParameterValue(_parameter, valueAsText);
                            }

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
