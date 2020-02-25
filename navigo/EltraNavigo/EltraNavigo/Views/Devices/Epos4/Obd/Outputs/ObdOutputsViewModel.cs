using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraNavigo.Controls;
using EltraNavigo.Views.Obd.Outputs.Events;

namespace EltraNavigo.Views.Obd.Outputs
{
    public class ObdOutputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private string _filter;
        private List<ObdEntry> _obdEntries;
        private List<ObdEntry> _backupEntries;
        private List<ObdEntry> _observedObdEntries;
        private bool _isSearchActive;
        private string _collapseParameterSelectionText;
        private bool _isObservedVisible;
        private bool _isEditable = true;

        #endregion

        #region Constructors

        public ObdOutputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _observedObdEntries = new List<ObdEntry>();
        }

        #endregion

        #region Properties

        public List<ObdEntry> ObdEntries
        {
            get => _obdEntries;
            set => SetProperty(ref _obdEntries, value);
        }

        public List<ObdEntry> ObservedObdEntries
        {
            get => _observedObdEntries;
            set => SetProperty(ref _observedObdEntries, value);
        }

        public bool IsSearchActive
        {
            get => _isSearchActive;
            set => SetProperty(ref _isSearchActive, value);
        }

        public string CollapseParameterSelectionText
        {
            get => _collapseParameterSelectionText;
            set => SetProperty(ref _collapseParameterSelectionText, value);
        }

        public bool IsObservedVisible
        {
            get => _isObservedVisible;
            set => SetProperty(ref _isObservedVisible, value);
        }

        public bool IsEditable
        {
            get => _isEditable;
            set => SetProperty(ref _isEditable, value);
        }

        #endregion

        #region Events

        public event EventHandler<ObdEventAddedEventArgs> EntryAdded;
        public event EventHandler<ObdEventAddedEventArgs> EntryRemoved;

        #endregion

        #region Events handling

        protected virtual void OnEntryAdded(ObdEventAddedEventArgs e)
        {
            EntryAdded?.Invoke(this, e);
        }

        protected virtual void OnEntryRemoved(ObdEventAddedEventArgs e)
        {
            EntryRemoved?.Invoke(this, e);
        }

        private async void OnDeleteEntryRequested(object sender, EventArgs e)
        {
            if (sender is ObdEntry observedObdEntry)
            {
                var obdEntry = FindObdEntry(ObservedObdEntries, observedObdEntry);
                if (obdEntry != null)
                {
                    var observedObdEntries = new List<ObdEntry>(ObservedObdEntries);
                    var obdEntries = new List<ObdEntry>(ObdEntries);

                    observedObdEntries.Remove(obdEntry);
                    
                    await obdEntry.Hide();

                    obdEntries.Add(obdEntry);

                    ObdEntries = obdEntries;
                    ObservedObdEntries = observedObdEntries;

                    OnEntryRemoved(new ObdEventAddedEventArgs { Entry = observedObdEntry });
                }
            }
        }

        #endregion

        #region Methods

        private void CreateParameters(List<ParameterBase> parameters, ref List<ObdEntry> obdEntries)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is XddParameter param)
                {
                    var searchPattern = $"{param.Index};0x{param.Index:X4};{param.Label}".ToLower();

                    if (searchPattern.Contains(_filter.ToLower()) && param.VisibleBy("operator"))
                    {
                        var entry = CreateObdEntry(param);

                        obdEntries.Add(entry);
                    }
                }

                if (parameter is StructuredParameter structuredParameter)
                {
                    CreateParameters(structuredParameter.Parameters, ref obdEntries);
                }
            }
        }

        private ObdEntry CreateObdEntry(Parameter parameter)
        {
            var entry = new ObdEntry(this, parameter);

            return entry;
        }

        public async Task SetFilter(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                _filter = filter;

                var device = Vcs?.Device;
                var obd = device?.ObjectDictionary;

                if (obd != null)
                {
                    var obdEntries = new List<ObdEntry>();
                    
                    CreateParameters(obd.Parameters, ref obdEntries);

                    await ShowObdEntries(ObdEntries, false);
                    
                    foreach (var obdEntry in obdEntries)
                    {
                        obdEntry.DeleteEntryRequested += OnDeleteEntryRequested;
                    }

                    ObdEntries = obdEntries;
                    
                    CollapseParameterSelectionText = "˄";
                    IsSearchActive = true;
                    IsObservedVisible = false;
                }
            }
        }

        private async Task ShowObdEntries(List<ObdEntry> obdEntries, bool show)
        {
            if (obdEntries != null)
            {
                foreach (var obdEntry in obdEntries)
                {
                    if (show)
                    {
                        await obdEntry.Show();
                    }
                    else
                    {
                        await obdEntry.Hide();
                    }
                }
            }
        }

        public async Task ResetFilter()
        {
            await ShowObdEntries(ObdEntries, false);
            
            ObdEntries = new List<ObdEntry>();

            IsSearchActive = false;
        }

        private ObdEntry FindObdEntry(List<ObdEntry> obdEntries, ObdEntry obdEntry)
        {
            ObdEntry result = null;

            foreach (var observableEntry in obdEntries)
            {
                if (observableEntry.UniqueId == obdEntry.UniqueId)
                {
                    result = observableEntry;
                    break;
                }
            }

            return result;
        }

        public async void AddObdEntryToObserved(ObdEntry obdEntry)
        {
            var entry = FindObdEntry(ObdEntries, obdEntry);

            if (entry != null)
            {
                var obdEntries = new List<ObdEntry>(ObdEntries);
                var observedObdEntries = new List<ObdEntry>(ObservedObdEntries);

                obdEntries.Remove(entry);
                observedObdEntries.Add(entry);

                ObdEntries = obdEntries;
                ObservedObdEntries = observedObdEntries;

                IsObservedVisible = true;

                await entry.Show();

                OnEntryAdded(new ObdEventAddedEventArgs { Entry = entry });
            }
        }

        public void CollapseParameterSelection()
        {
            if (CollapseParameterSelectionText == "˄")
            {
                CollapseParameterSelectionText = "˅";

                _backupEntries = ObdEntries;
                
                ObdEntries = new List<ObdEntry>();
            }
            else
            {
                CollapseParameterSelectionText = "˄";

                ObdEntries = new List<ObdEntry>(_backupEntries);
            }
        }

        #endregion
    }
}
