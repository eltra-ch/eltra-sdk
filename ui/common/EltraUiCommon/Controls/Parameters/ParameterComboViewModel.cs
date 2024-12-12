using EltraCommon.Contracts.Devices;
using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraUiCommon.Controls.Parameters
{
    public class ParameterComboViewModel : ToolViewBaseModel
    {
        #region Private fields

        private readonly string _uniqueId;
        private XddParameter _parameter;
        private string _label;
        private List<string> _textRange;
        private List<long> _intRange;
        private int _selectedIndex = -1;

        #endregion

        #region Constructors

        public ParameterComboViewModel(ToolViewBaseModel parent, string uniqueId) 
            : base(parent)
        {
            _uniqueId = uniqueId;
        }

        #endregion

        #region Properties

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public void SelectedIndexChanged()
        {
            WriteValue();
        }

        public List<string> TextRange
        {
            get => _textRange ?? (_textRange = new List<string>());
            set => SetProperty(ref _textRange, value);
        }
        
        public List<long> IntRange
        {
            get => _intRange ?? (_intRange = new List<long>());
            set => SetProperty(ref _intRange, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public int IntValue { get; set; }

        private EltraDevice Device
        {
            get => Vcs.Device;
        }

        #endregion

        #region Methods

        protected override async void OnInitialized()
        {
            if (Device != null)
            {
                if (Device.SearchParameter(_uniqueId) is XddParameter parameter)
                {
                    _parameter = parameter;

                    if (_parameter != null)
                    {
                        var parameterValue = await _parameter.ReadValue();

                        if (_parameter.SetValue(parameterValue))
                        {
                            Label = _parameter.Label;
                            
                            foreach(var allowedValue in _parameter.AllowedValues)
                            {
                                var intRange = new List<long>();
                                var range = new List<string>();

                                allowedValue.GetEnumRange(ref intRange);
                                allowedValue.GetEnumRange(ref range);

                                TextRange = range;
                                IntRange = intRange;
                            }
                        }
                    }
                }
            }
        }

        private bool FindValue(int index, out long value)
        {
            bool result = false;
            int counter = 0;

            value = 0;

            foreach(var val in IntRange)
            {
                if(counter == index)
                {
                    value = val;
                    result = true;
                    break;
                }

                counter++;
            }

            return result;
        }

        private int FindIndex(int intValue)
        {
            int result = -1;
            int counter = 0;

            foreach(var val in IntRange)
            {
                if(val == intValue)
                {
                    result = counter;
                    break;
                }

                counter++;
            }

            return result;
        }

        private void WriteValue()
        {
            if(FindValue(SelectedIndex, out long value))
            {
                IntValue = (int)value;

                if(_parameter != null)
                {
                    _parameter.SetValue((sbyte)IntValue);
                }
            }
        }

        private void ReadValue()
        {
            if(_parameter!=null && _parameter.GetValue(out sbyte val))
            {
                IntValue = val;

                SelectedIndex = FindIndex(val);
            }
        }

        public override async Task<bool> StartUpdate()
        {
            bool result = await base.StartUpdate();

            if (result)
            {
                if (_parameter != null)
                {
                    _parameter.AutoUpdate();
                }

                ReadValue();
            }

            return result;
        }

        public override async Task<bool> StopUpdate()
        {
            bool result = await base.StopUpdate();

            if (result)
            {
                if (_parameter != null)
                {
                    _parameter.StopUpdate();
                }
            }

            return result;
        }

        public override async Task Show()
        {
            await base.Show();

            IsBusy = true;

            if (_parameter != null)
            {
                _parameter.AutoUpdate();
            }

            ReadValue();

            IsBusy = false;
        }

        public override async Task Hide()
        {
            if (_parameter != null && Vcs!=null)
            {
                _parameter.StopUpdate();
            }

            await base.Hide();
        }

        #endregion
    }
}
