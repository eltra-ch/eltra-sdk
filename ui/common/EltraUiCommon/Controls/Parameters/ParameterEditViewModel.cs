using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
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

        #endregion

        #region Constructors

        public ParameterEditViewModel(ToolViewBaseModel parent, string uniqueId) 
            : base(parent, uniqueId)
        {
            IsEnabled = true;
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
            UnregisterChangedEvent();

            if (e.Parameter != null)
            {
                string valueAsText = e.Parameter.GetValueAsString();

                if (Value != valueAsText)
                {
                    Value = valueAsText;

                    Changed?.Invoke(this, e);
                }
            }

            RegisterChangedEvent();
        }

        private void OnParameterWritten(object sender, ParameterWrittenEventArgs e)
        {
            Written?.Invoke(sender, e);
        }

        #endregion

        #region Methods

        private void RegisterChangedEvent()
        {
            if (_parameter != null)
            {
                _parameter.ParameterChanged += OnParameterChanged;
            }
        }

        private void UnregisterChangedEvent()
        {
            if (_parameter != null)
            {
                _parameter.ParameterChanged -= OnParameterChanged;
            }
        }

        private void RegisterEvents()
        {
            RegisterChangedEvent();
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
            UnregisterChangedEvent();
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
                if (Vcs.Device.SearchParameter(UniqueId) is Parameter parameter)
                {
                    _parameter = parameter;
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
        }

        protected override void OnInitialized()
        {
            InitModelData();
        }

        public override async Task<bool> StartUpdate()
        {
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
                        Debug.Print($"Parameter {UniqueId} not defined!");
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
                if (Vcs != null)
                {
                    var oldValue = _parameter.GetValueAsString();
                    var oldParameterValue = _parameter.ActualValue.Clone();

                    if (oldValue != newValue)
                    {
                        if (_parameter != null)
                        {
                            if (_parameter.SetValueAsString(newValue))
                            {
                                if (await _parameter.Write())
                                {
                                    var newParameterValue = _parameter.ActualValue;

                                    InitDoubleValue();
                                    InitIntValue();

                                    OnEdited(new ParameterChangedEventArgs(_parameter, oldParameterValue, newParameterValue) );

                                    result = true;
                                }
                                else
                                {
                                    _parameter.SetValueAsString(oldValue);
                                    result = false;
                                }
                            }
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
                Console.WriteLine(e);
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
                switch (_parameter.DataType.Type)
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        long minvalue = 0;
                        long maxvalue = 0;

                        if (_parameter is XddParameter epos4Parameter)
                        {
                            if (epos4Parameter.GetRange(ref minvalue, ref maxvalue))
                            {
                                IntMinValue = minvalue;
                                IntMaxValue = maxvalue;

                                DoubleMinValue = IntMinValue;
                                DoubleMaxValue = IntMaxValue;
                            }
                        }

                        break;
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
