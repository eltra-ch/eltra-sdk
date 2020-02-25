using System;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters.Events;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;

namespace EltraNavigo.Controls.Parameters
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

        #endregion

        #region Events handling

        private void OnParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            UnregisterEvents();

            if (e.Parameter != null)
            {
                string valueAsText = e.Parameter.GetValueAsString();

                Value = valueAsText;
            }            
        }

        #endregion

        #region Methods

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

        public override async void InitModelData()
        {
            if (Vcs != null)
            {
                if (_parameter == null)
                {
                    if (Vcs.SearchParameter(UniqueId) is Parameter parameter)
                    {
                        _parameter = parameter;
                    }
                }

                if (_parameter != null)
                {
                    var parameterValue = await Vcs.GetParameterValue(_parameter.UniqueId);

                    if (parameterValue!=null && _parameter.SetValue(parameterValue))
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
                }
            }
        }

        protected override void OnInitialized()
        {
            InitModelData();
        }

        public override async Task<bool> StartUpdate()
        {
            if (!IsUpdating)
            {
                if (Vcs != null)
                {
                    Vcs.RegisterParameterUpdate(UniqueId);
                }

                InitModelData();
            }

            return await base.StartUpdate();
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                if (Vcs != null && _parameter != null)
                {
                    Vcs.UnregisterParameterUpdate(_parameter?.UniqueId);
                }
            }

            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            if (Vcs != null)
            {
                Vcs.RegisterParameterUpdate(UniqueId);
            }

            RegisterEvents();

            InitModelData();

            await base.Show();

            IsBusy = false;
        }

        public override async Task Hide()
        {
            if (Vcs != null && _parameter != null)
            {
                Vcs.UnregisterParameterUpdate(_parameter?.UniqueId);
            }

            UnregisterEvents();

            await base.Hide();
        }

        public async Task<bool> TextChanged(string newValue)
        {
            bool result = false;

            try
            {
                if (Vcs != null)
                {
                    var oldValue = _parameter.GetValueAsString();

                    if (oldValue != newValue)
                    {
                        if (_parameter != null && _parameter.SetValueAsString(newValue))
                        {
                            if (await Vcs.WriteParameter(_parameter))
                            {
                                InitDoubleValue();
                                InitIntValue();

                                result = true;
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
