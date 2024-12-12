using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraUiCommon.Controls.Parameters
{
    public class ParameterEditViewModel : ParameterControlViewModel
    {
        #region Private fields

        const double defaultUnitWidth = 32;

        private string _value;
        private string _unit;
        private bool _showStepper = true;
        private Parameter _parameter;
        
        private long _intMinValue;
        private long _intMaxValue;
        private long _intValue;

        private double _doubleMinValue = double.MinValue;
        private double _doubleMaxValue = double.MaxValue;
        private double _doubleValue;
        private double _doubleIncrement = 1.0;
        private double _unitWidth;
        
        private bool _lockParameterChange;

        #endregion

        #region Constructors

        public ParameterEditViewModel(ToolViewBaseModel parent, string uniqueId) 
            : base(parent, uniqueId)
        {
            IsEnabled = true;
        }

        public ParameterEditViewModel(ToolViewBaseModel parent, ushort index, byte subIndex)
            : base(parent, index, subIndex)
        {
        }

        #endregion

        #region Properties

        public Parameter Parameter
        {
            get => _parameter;
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value, "Unit", 
                new Action(()=> {
                    if (!string.IsNullOrEmpty(Unit))
                    {
                        UnitWidth = defaultUnitWidth;
                    }
                }));
        }

        public long IntMinValue
        {
            get => _intMinValue;
            set => SetProperty(ref _intMinValue, value);
        }

        public long IntMaxValue
        {
            get => _intMaxValue;
            set => SetProperty(ref _intMaxValue, value);
        }

        public long IntValue
        {
            get => _intValue;
            set => SetProperty(ref _intValue, value);
        }

        public double DoubleMinValue
        {
            get => _doubleMinValue;
            set => SetProperty(ref _doubleMinValue, value);
        }

        public double DoubleMaxValue
        {
            get => _doubleMaxValue;
            set => SetProperty(ref _doubleMaxValue, value);
        }

        public double DoubleValue
        {
            get => _doubleValue;
            set => SetProperty(ref _doubleValue, value);
        }
        
        public double DoubleIncrement
        {
            get => _doubleIncrement;
            set => SetProperty(ref _doubleIncrement, value);
        }

        public double UnitWidth
        {
            get => _unitWidth;
            set => SetProperty(ref _unitWidth, value);
        }

        public bool ShowStepper
        {
            get => _showStepper;
            set => SetProperty(ref _showStepper, value);
        }

        private EltraDevice Device
        {
            get => Vcs.Device;
        }

        #endregion

        #region Events

        public event EventHandler<ParameterChangedEventArgs> Changed;

        public event EventHandler<ParameterChangedEventArgs> Edited;

        public event EventHandler<ParameterWrittenEventArgs> Written;

        #endregion

        #region Events handling

        private void OnEdited(ParameterChangedEventArgs e)
        {
            Edited?.Invoke(this, e);
        }

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.Parameter != null && !_lockParameterChange)
            {
                string valueAsText = e.Parameter.GetValueAsString();

                if (Value != valueAsText)
                {
                    Value = valueAsText;

                    Changed?.Invoke(this, e);
                }
            }
        }

        private void OnParameterWritten(object sender, ParameterWrittenEventArgs e)
        {
            Written?.Invoke(sender, e);
        }

        #endregion

        #region Methods

        private void RegisterParameterChangedEvent()
        {
            if (_parameter != null)
            {
                _parameter.ParameterChanged += OnParameterChanged;
            }
        }

        private void UnregisterParameterChangedEvent()
        {
            if (_parameter != null)
            {
                _parameter.ParameterChanged -= OnParameterChanged;
            }
        }

        private void RegisterEvents()
        {
            RegisterParameterChangedEvent();
            RegisterWrittenEvent();
        }

        private void RegisterWrittenEvent()
        {
            if (_parameter != null)
            {
                _parameter.ParameterWritten += OnParameterWritten;
            }
        }

        private void UnregisterEvents()
        {
            UnregisterParameterChangedEvent();
            UnregisterWrittenEvent();
        }

        private void UnregisterWrittenEvent()
        {
            if (_parameter != null)
            {
                _parameter.ParameterWritten -= OnParameterWritten;
            }
        }

        private void UpdateParameter()
        {
            if (_parameter == null)
            {
                if (!string.IsNullOrEmpty(UniqueId))
                {
                    if (Vcs.Device.SearchParameter(UniqueId) is Parameter parameter)
                    {
                        _parameter = parameter;
                    }
                }
                else
                {
                    if (Vcs.Device.SearchParameter(Index, SubIndex) is Parameter parameter)
                    {
                        _parameter = parameter;
                    }
                }
            }
        }

        public override void InitModelData()
        {
            if (Vcs != null && Vcs.Device != null)
            {
                UpdateParameter();

                if (_parameter != null)
                {
                    Task.Run(async ()=> {
                        
                        var parameterValue = await _parameter.ReadValue();

                        return parameterValue;

                    }).ContinueWith((t) => {
                        var parameterValue = t.Result;
                        if (parameterValue != null && _parameter.SetValue(parameterValue))
                        {
                            if (InvokeOnMainThread != null)
                            {
                                InvokeOnMainThread.BeginInvokeOnMainThread(() =>
                                {
                                    UpdateParameterValue();
                                });
                            }
                            else
                            {
                                UpdateParameterValue();
                            }
                        }
                    });
                }
            }
        }

        private void UpdateParameterValue()
        {
            Value = _parameter.GetValueAsString();

            Label = _parameter.Label;

            if (_parameter is XddParameter epos4Parameter)
            {
                Unit = epos4Parameter.Unit.Label;
            }

            InitDoubleValue();
            InitIntValue();

            InitIntRanges();
            InitDoubleRanges();
        }

        protected override void OnVirtualCommandSetChanged()
        {
            if (Vcs != null)
            {
                Vcs.DeviceChanged -= OnVcsDeviceChanged;
                Vcs.DeviceChanged += OnVcsDeviceChanged;

                OnVcsDeviceChanged();
            }
        }

        protected override void OnInitialized()
        {
            InitModelData();
        }

        private void OnVcsDeviceChanged(object sender, EventArgs e)
        {
            OnVcsDeviceChanged();
        }

        private void OnVcsDeviceChanged()
        {
            _parameter = null;

            UpdateParameter();
        }

        public override async Task<bool> StartUpdate()
        {
            const string methodName = "StartUpdate";
            bool result = true;

            if(!IsUpdating)
            {
                result = await base.StartUpdate();

                if (result)
                {
                    UpdateParameter();

                    if (_parameter != null)
                    {
                        _parameter.AutoUpdate();
                    }
                    else
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - {methodName}", $"Parameter uniqueid = '{UniqueId}' or {Index:X4}:{SubIndex:X2} not defined!");
                    }

                    InitModelData();
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
                    try
                    {
                        if (_parameter != null)
                        {
                            _parameter.StopUpdate();
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.Print(e.Message);
                    }
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
            IsBusy = true;

            if (IsVisible)
            {
                UnregisterEvents();

                await base.Hide();
            }

            IsBusy = false;
        }

        public async Task<bool> TextChanged(string newValue)
        {
            bool result = false;

            try
            {
                if (Vcs != null && _parameter != null)
                {
                    var oldValue = _parameter.GetValueAsString();
                    var oldParameterValue = _parameter.ActualValue.Clone();

                    if (oldValue != newValue)
                    {
                        _lockParameterChange = true;

                        if (_parameter.SetValueAsString(newValue))
                        {
                            _lockParameterChange = false;

                            int retryCount = 10;
                            bool writeResult = false;
                            do
                            {
                                writeResult = await _parameter.Write();
                                retryCount--;
                            }
                            while (!writeResult && retryCount > 0);

                            if (writeResult)
                            {
                                var newParameterValue = _parameter.ActualValue;

                                InitDoubleValue();
                                InitIntValue();

                                OnEdited(new ParameterChangedEventArgs(_parameter, oldParameterValue, newParameterValue));

                                result = true;
                            }
                            else
                            {
                                _lockParameterChange = false;
                                _parameter.SetValueAsString(oldValue);
                                result = false;
                            }
                        }
                        else
                        {
                            _lockParameterChange = false;
                            _parameter.SetValueAsString(oldValue);
                            result = false;
                        }
                    }
                    else
                    {
                        result = true;
                    }

                    IsValid = result;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return result;
        }

        private void InitDoubleValue()
        {
            DoubleIncrement = 1;

            if (double.TryParse(Value, out double doubleValue))
            {
                DoubleValue = doubleValue;
            }
        }

        private void InitIntValue()
        {
            if (long.TryParse(Value, out long intValue))
            {
                IntValue = intValue;
            }
        }

        private void InitIntRanges()
        {
            if (_parameter != null)
            {
                long minvalue = 0;
                long maxvalue = 0;

                if (_parameter is XddParameter xddParameter)
                {
                    if (xddParameter.GetRange(ref minvalue, ref maxvalue))
                    {
                        switch (_parameter.DataType.Type)
                        {
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                                {
                                    IntMinValue = minvalue;
                                    IntMaxValue = maxvalue;

                                    DoubleMinValue = IntMinValue;
                                    DoubleMaxValue = IntMaxValue;
                                }
                                break;
                            case TypeCode.Single:
                            case TypeCode.Double:
                                DoubleMinValue = minvalue;
                                DoubleMaxValue = maxvalue;
                                break;
                        }
                    }
                }
            }
        }

        private void InitDoubleRanges()
        {
            if (_parameter != null)
            {
                double minvalue = 0;
                double maxvalue = 0;

                if (_parameter is XddParameter xddParameter)
                {
                    if (xddParameter.GetRange(ref minvalue, ref maxvalue))
                    {
                        switch (_parameter.DataType.Type)
                        {
                            case TypeCode.Single:
                            case TypeCode.Double:
                                DoubleMinValue = minvalue;
                                DoubleMaxValue = maxvalue;
                                break;
                        }
                    }
                }
            }
        }

        public async Task ValueChanged(double value)
        {
            Value = $"{value}";
            
            await TextChanged(Value);            
        }

        #endregion
    }
}
