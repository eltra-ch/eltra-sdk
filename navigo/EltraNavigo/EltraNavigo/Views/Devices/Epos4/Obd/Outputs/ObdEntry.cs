using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Parameters;
using Xamarin.Forms;

namespace EltraNavigo.Views.Obd.Outputs
{
    public class ObdEntry : ToolViewBaseModel
    {
        #region Private fields

        private readonly Parameter _parameter;

        private string _index;
        private string _subIndex;
        private string _name;
        private string _type;
        private string _access;
        private ParameterControlViewModel _value;
        private List<ToolViewBaseModel> _values;
        
        #endregion

        #region Constructors

        public ObdEntry(ToolViewBaseModel parent, Parameter parameter)
            :base(parent)
        {
            _parameter = parameter;

            InitProperties(parameter);
        }
        
        #endregion

        #region Commands

        public ICommand DeleteObdEntryCommand => new Command( OnDeleteEntryRequested );

        #endregion

        #region Events

        public event EventHandler DeleteEntryRequested;

        #endregion

        #region Events handling

        protected virtual void OnDeleteEntryRequested()
        {
            DeleteEntryRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public string UniqueId => _parameter.UniqueId;

        public string Index 
        { 
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public string SubIndex 
        { 
            get => _subIndex;
            set => SetProperty(ref _subIndex, value);
        }

        public string Name 
        { 
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Type 
        { 
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string Access 
        { 
            get => _access;
            set => SetProperty(ref _access, value);
        }

        public ParameterControlViewModel Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public List<ToolViewBaseModel> Values
        {
            get => _values;
            set => SetProperty(ref _values, value);
        }

        public Parameter ParameterEntry => _parameter;
        
        #endregion

        #region Methods

        private void InitProperties(Parameter parameter)
        {
            Index = $"0x{parameter.Index:X4}";
            SubIndex = $"0x{parameter.SubIndex:X2}";
            Name = parameter.Label;

            switch (parameter.Flags.Access)
            {
                case AccessMode.ReadOnly:
                    Access = "RO";
                    break;
                case AccessMode.ReadWrite:
                    Access = "RW";
                    break;
                case AccessMode.WriteOnly:
                    Access = "WO";
                    break;
            }

            Type = parameter.DataType.Type.ToString();
        }

        public override async Task Show()
        {
            IsBusy = true;

            UpdateValue();

            await base.Show();

            IsBusy = false;
        }

        private void UpdateValue()
        {
            if (_parameter != null)
            {
                CreateValue();

                Value.InitModelData();

                Value.IsEnabled = true;

                Values = new List<ToolViewBaseModel> { Value };
            }
        }

        private void CreateValue()
        {
            if (_parameter.Flags.Access == AccessMode.ReadOnly)
            {
                Value = new ParameterLabelViewModel(this, _parameter.UniqueId) { ShowLabel = false };
            }
            else
            {
                Value = new ParameterEditViewModel(this, _parameter.UniqueId) { ShowLabel = false, ShowStepper = false };
            }
        }
        
        #endregion
    }
}
