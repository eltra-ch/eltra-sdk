using EltraCommon.Contracts.Devices;
using EltraCommon.Contracts.Node;
using EltraCommon.Contracts.Parameters;
using EltraCommon.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCommon.ObjectDictionary.DeviceDescription;
using EltraConnector.Master.Device.Commands;
using EltraConnector.Master.Device.ParameterConnection;
using EltraConnector.SyncAgent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraConnector.Master.Device
{
    public class MasterDevice : EltraDeviceNode
    {
        #region Private fields

        private SyncCloudAgent _cloudAgent;

        #endregion

        #region Constructors

        public MasterDevice(string family, string deviceDescriptionFilePath)
        {
            Family = family;
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

        protected virtual async void OnCloudAgentChanged()
        {
            if (CloudAgent != null)
            {
                CreateCommunication();

                await ReadDeviceDescriptionFile();
            }
        }

        protected virtual void CreateCommunication()
        {
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

        public override async Task<bool> ReadDeviceDescriptionFile()
        {
            var deviceDescriptionFile = new XddDeviceDescriptionFile(this)
            {
                Url = CloudAgent.Url,
                SourceFile = DeviceDescriptionFilePath
            };

            StatusChanged += (sender, args) => 
            {
                if(Status == DeviceStatus.Ready)
                {
                    AddDeviceTools(deviceDescriptionFile);

                    CreateConnectionManager();

                    OnInitialized();
                }
            };

            return await ReadDeviceDescriptionFile(deviceDescriptionFile);
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

        public virtual int GetUpdateInterval(ParameterUpdatePriority priority)
        {
            int result;

            switch (priority)
            {
                case ParameterUpdatePriority.High:
                    result = 10000;
                    break;
                case ParameterUpdatePriority.Medium:
                    result = 30000;
                    break;
                case ParameterUpdatePriority.Low:
                    result = 60000;
                    break;
                case ParameterUpdatePriority.Lowest:
                    result = 180000;
                    break;
                default:
                    result = 60000;
                    break;
            }

            return result;
        }

        #endregion
    }
}
