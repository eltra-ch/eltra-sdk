using System;
using System.Runtime.Serialization;
using EltraCommon.Logger;
using EltraCloudContracts.Contracts.CommandSets;
using EltraCloudContracts.ObjectDictionary.Common;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Parameters;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Factory;
using EltraCloudContracts.ObjectDictionary.Factory;
using EltraCloudContracts.Contracts.ToolSet;
using EltraCloudContracts.ObjectDictionary.Common.DeviceDescription;
using EltraCloudContracts.ObjectDictionary.DeviceDescription.Events;
using EltraCloudContracts.ObjectDictionary.DeviceDescription;

namespace EltraCloudContracts.Contracts.Devices
{
    [DataContract]
    public class EltraDevice
    {
        #region Private fields

        private string _productName;
        private DeviceVersion _version;
        private DeviceCommandSet _commandSet;
        private DeviceToolSet _toolSet;
        private Dd _deviceDescription;
        private DeviceIdentification _deviceIdentification;
        private DeviceStatus _status;
        
        #endregion

        #region Constructors

        public EltraDevice()
        {
            Modified = DateTime.Now;
            Created = DateTime.Now;
        }

        #endregion

        #region Events

        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Events handling

        private void OnDeviceDescriptionFileStateChanged(object sender, DeviceDescriptionEventArgs e)
        {
            
        }

        #endregion

        #region Properties

        [DataMember]
        public string SessionUuid { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DeviceIdentification Identification
        {
            get => _deviceIdentification ?? (_deviceIdentification = new DeviceIdentification());
            set => _deviceIdentification = value;
        }

        [DataMember]
        public DeviceVersion Version
        {
            get => _version ?? (_version = new DeviceVersion());
            set => _version = value;
        }

        [DataMember]
        public DeviceCommandSet CommandSet => _commandSet ?? (_commandSet = new DeviceCommandSet());

        [DataMember]
        public DeviceToolSet ToolSet => _toolSet ?? (_toolSet = new DeviceToolSet());

        [DataMember]
        public DeviceStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;

                    OnStatusChanged();
                }
            }
        }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [IgnoreDataMember]
        public DeviceObjectDictionary ObjectDictionary { get; set; }

        [DataMember]
        public string ProductName  
        {
            get => _productName;
            set => _productName = value;
        }

        [IgnoreDataMember]
        public Dd DeviceDescription
        {
            get => _deviceDescription;
            set => _deviceDescription = value;
        }

        [IgnoreDataMember]
        public string ProductPicture { get; set; }

        #endregion

        #region Methods

        public bool AddCommand(DeviceCommand command)
        {
            bool result = CommandSet.AddCommand(command);

            return result;
        }
        
        public DeviceCommand FindCommand(DeviceCommand command)
        {
            var result = CommandSet.FindCommandByName(command.Name);

            return result;
        }

        public DeviceCommand FindCommand(string commandName)
        {
            DeviceCommand result = null;

            try
            {
                result = CommandSet.FindCommandByName(commandName);
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Device - FindCommand", e);
            }
            
            return result;
        }

        public bool AddTool(DeviceTool tool)
        {
            tool.Device = this;

            bool result = ToolSet.AddTool(tool);

            return result;
        }

        public DeviceTool FindTool(string uuid)
        {
            DeviceTool result = null;

            try
            {
                result = ToolSet.FindToolByUuid(uuid);
            }
            catch (Exception e)
            {
                MsgLogger.Exception("Device - FindTool", e);
            }

            return result;
        }

        public DeviceTool FindTool(DeviceTool tool)
        {
            var result = ToolSet.FindToolByUuid(tool.Uuid);

            return result;
        }

        public virtual void RunAsync()
        {
        }

        public virtual async void ReadDeviceDescriptionFile()
        {
            var deviceDescriptionFile = DeviceDescriptionFactory.CreateDeviceDescriptionFile(this);

            deviceDescriptionFile.StateChanged += OnDeviceDescriptionFileStateChanged;

            await deviceDescriptionFile.Read();
        }

        public virtual bool CreateObjectDictionary()
        {
            bool result = false;

            lock (this)
            {
                if (Status != DeviceStatus.Ready || ObjectDictionary == null)
                {
                    ObjectDictionary = ObjectDictionaryFactory.CreateObjectDictionary(this);

                    if (ObjectDictionary != null)
                    {
                        if (ObjectDictionary.Open())
                        {
                            Status = DeviceStatus.Ready;
                            result = true;
                        }
                        else
                        {
                            MsgLogger.WriteError("Device - CreateObjectDictionary", "Cannot open object dictionary!");
                            ObjectDictionary = null;
                        }
                    }
                }
            }

            return result;
        }

        public ParameterBase SearchParameter(string uniqueId)
        {
            ParameterBase result = null;

            if (ObjectDictionary!=null)
            {
                result = ObjectDictionary.SearchParameter(uniqueId);
            }

            return result;
        }

        public ParameterBase SearchParameter(ushort index, byte subIndex)
        {
            ParameterBase result = null;

            if (ObjectDictionary != null)
            {
                result = ObjectDictionary.SearchParameter(index, subIndex);
            }

            return result;
        }

        public bool CreateDeviceDescription(DeviceDescriptionFile deviceDescriptionFile)
        {
            bool result = false;
            var content = deviceDescriptionFile?.Content;

            if (content != null)
            {
                var deviceDescription = DeviceDescriptionFactory.CreateDeviceDescription(this, deviceDescriptionFile);

                if (deviceDescription.Parse())
                {
                    DeviceDescription = deviceDescription;

                    result = true;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - CreateDeviceDescription", "Parsing device description failed!");
                }
            }

            return result;
        }

        #endregion
    }
}
