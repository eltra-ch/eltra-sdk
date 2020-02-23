using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCommon.Logger;
using EltraConnector.SyncAgent;
using EltraMaster.Device.Commands;
using EltraMaster.Device.ParameterConnection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraMaster.Device
{
    public class MasterDevice : EltraDevice
    {
        #region Private fields

        private SyncCloudAgent _cloudAgent;

        #endregion

        #region Constructors

        public MasterDevice(string name, string deviceDescriptionFilePath)
        {
            Name = name;
            DeviceDescriptionFilePath = deviceDescriptionFilePath;

            CreateCommandSet();
        }

        #endregion

        #region Properties

        public MasterDeviceCommunication Communication { get; set; }

        public ParameterConnectionManager ParameterConnectionManager { get; private set; }

        public SyncCloudAgent CloudAgent
        {
            get => _cloudAgent;
            set
            {
                _cloudAgent = value;

                OnCloudAgentChanged();
            }
        }

        public string DeviceDescriptionFilePath { get; set; }

        #endregion

        #region Events

        public event EventHandler Initialized;

        #endregion

        #region Events handling

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, new EventArgs());
        }

        protected virtual void OnCloudAgentChanged()
        {
            CreateCommunication();
            CreateDeviceDescription();
        }

        protected virtual void CreateCommunication()
        {
        }

        private void OnDeviceDescriptionStateChanged(object sender, DeviceDescriptionEventArgs e)
        {
            if (e.State == DeviceDescriptionState.Read)
            {
                AddDeviceTools(e.DeviceDescription as XddDeviceDescriptionFile);

                CreateObjectDictionary();
                CreateConnectionManager();

                OnInitialized();
            }
        }

        #endregion

        #region Methods

        private void CreateCommandSet()
        {
            AddCommand(new RegisterParameterUpdateCommand(this));
            AddCommand(new UnregisterParameterUpdateCommand(this));

            AddCommand(new GetObjectCommand(this));
            AddCommand(new SetObjectCommand(this));
        }

        public override async void CreateDeviceDescription()
        {
            DeviceDescription = new XddDeviceDescriptionFile(this)
            {
                Url = CloudAgent.Url,
                SourceFile = DeviceDescriptionFilePath
            };

            DeviceDescription.StateChanged += OnDeviceDescriptionStateChanged;

            await DeviceDescription.Read();
        }

        private void AddDeviceTools(XddDeviceDescriptionFile xdd)
        {
            if (xdd != null)
            {
                foreach (var deviceTool in xdd.DeviceTools)
                {
                    AddTool(deviceTool);
                }
            }
        }

        private void CreateConnectionManager()
        {
            ParameterConnectionManager = new ParameterConnectionManager(this);
        }

        public override bool CreateObjectDictionary()
        {
            var objectDictionary = new XddDeviceObjectDictionary(this);

            bool result = objectDictionary.Open();

            if (objectDictionary.Open())
            {
                ObjectDictionary = objectDictionary;

                Status = DeviceStatus.Ready;

                result = true;
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - CreateObjectDictionary", "Cannot open object dictionary!");
            }

            return result;
        }


        protected void StartParameterConnectionManager(ref List<Task> tasks)
        {
            var task = ParameterConnectionManager.StartAsync();

            if (task != null)
            {
                tasks.Add(task);
            }
        }


        public bool ReadParameter(Parameter parameter)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                result = ParameterConnectionManager.ReadParameter(parameter);
            }

            return result;
        }

        public bool WriteParameter(Parameter parameter)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                result = ParameterConnectionManager.WriteParameter(parameter);
            }

            return result;
        }

        public bool ReadParameterValue<T>(Parameter parameter, out T value)
        {
            bool result = false;

            value = default;

            if (ParameterConnectionManager != null)
            {
                if (ParameterConnectionManager.ReadParameter(parameter))
                {
                    result = parameter.GetValue(out value);
                }
            }

            return result;
        }

        public bool WriteParameterValue<T>(Parameter parameter, T value)
        {
            bool result = false;

            if (ParameterConnectionManager != null)
            {
                if (parameter.SetValue(value))
                {
                    result = ParameterConnectionManager.WriteParameter(parameter);
                }
            }

            return result;
        }

        protected virtual void StartConnectionManagersAsync(ref List<Task> tasks)
        {

        }

        public override async void RunAsync()
        {
            var tasks = new List<Task>();

            StartParameterConnectionManager(ref tasks);

            StartConnectionManagersAsync(ref tasks);

            await Task.WhenAll(tasks);
        }

        public virtual void Disconnect()
        {
            if (ParameterConnectionManager.IsRunning)
            {
                ParameterConnectionManager.Stop();
            }

            Status = DeviceStatus.Disconnected;
        }

        #endregion
    }
}
