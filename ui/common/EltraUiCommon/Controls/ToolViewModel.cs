using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraCommon.Contracts.Devices;
using EltraConnector.Agent;
using EltraCommon.Contracts.Channels;
using EltraConnector.UserAgent.Definitions;
using EltraUiCommon.Device.Factory;
using EltraUiCommon.Controls.Definitions;
using System.Collections.Generic;

namespace EltraUiCommon.Controls
{
    public class ToolViewModel : ToolViewBaseModel
    {
        #region Private fields

        private AgentConnector _agent;
        private EltraDevice _device;
                
        private bool _isSetUp;
        private IDeviceVcsFactory _deviceFactory;
        
        private Task _updateViewModelsTask;
        private CancellationTokenSource _updateViewModelsCts;
        
        private DateTime _updateTaskTimestamp;
        
        private List<AutoUpdateRegistration> _autoUpdateTasks;
        private SemaphoreSlim _autoUpdateLock;
        private AutoUpdateMode _autoUpdateMode;

        #endregion

        #region Constructors

        public ToolViewModel()
        {
            const int defaultUpdateInterval = 1000;
            
            InitTasks();

            UpdateInterval = defaultUpdateInterval;
            UpdateViewModels = false;

            IsSupported = true;
            Persistenced = true;
            AutoUpdateMode = AutoUpdateMode.Visibility;
        }

        public ToolViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            const int defaultUpdateInterval = 1000;

            InitTasks();

            AutoUpdateMode = AutoUpdateMode.Visibility;

            UpdateInterval = defaultUpdateInterval;

            if (parent is ToolViewModel toolViewModel)
            {
                Agent = toolViewModel.Agent;
                AutoUpdateMode = toolViewModel.AutoUpdateMode;
            }

            IsSupported = true;
            Persistenced = true;
        }

        #endregion

        #region Properties

        public AgentConnector Agent
        {
            get => _agent;
            set
            {
                if(_agent != value)
                {
                    var newAgent = value;
                    bool equal = false;
                    if(_agent!=null && newAgent != null)
                    {
                        if(_agent.Channel != null && newAgent.Channel != null && _agent.Channel.Id == newAgent.Channel.Id)
                        {
                            equal = true;
                        }
                    }

                    if (!equal)
                    {
                        _agent = value;
                        OnAgentChanged();
                    }
                }
            }
        }

        public string Uuid { get; set; }

        public bool UpdateViewModels { get; set; }

        public int UpdateInterval { get; set; }

        public EltraDevice Device 
        { 
            get => _device; 
            set
            {
                if(_device != value)
                { 
                    _device = value;
                    OnDeviceChanged();
                }
            }
        }

		public bool Persistenced { get; set; }

        public bool CanSetUp { get => !_isSetUp; }

        public AutoUpdateMode AutoUpdateMode 
        { 
            get => _autoUpdateMode;
            set 
            {
                if(_autoUpdateMode != value)
                {
                    _autoUpdateMode = value;

                    OnAutoUpdateModeChanged();
                }                
            }             
        }

        protected List<AutoUpdateRegistration> AutoUpdateTasks => _autoUpdateTasks ?? (_autoUpdateTasks = new List<AutoUpdateRegistration>());

        #endregion

        #region Events

        public event EventHandler DeviceInitialized;
        public event EventHandler RefreshRequested;

        #endregion

        #region Events handling

        private void OnDeviceInitialized()
        {
            DeviceInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void OnRefresh()
        {
            RefreshRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void InitTasks()
        {
            _autoUpdateLock = new SemaphoreSlim(1);

            _updateTaskTimestamp = DateTime.MinValue;
            _updateViewModelsTask = Task.CompletedTask;
            _updateViewModelsCts = new CancellationTokenSource();
        }

        protected override void Init(VirtualCommandSet vcs)
        {
            InitTasks();

            _device = vcs.Device;
            _agent = vcs.Connector;

            base.Init(vcs);
        }

        public virtual void SetUp()
        {
            _isSetUp = true;
        }

    	public virtual void ButtonPressed(string classId)
        {   
        }

        public virtual void ButtonReleased(string classId)
        {
        }

        protected virtual void OnAgentChanged()
        {
            if (Agent != null)
            {
                foreach(var child in SafeChildrenArray)
                {
                    if (child is ToolViewModel toolViewModel)
                    {
                        toolViewModel.Agent = Agent;
                    }
                }

                Agent.StatusChanged += (o, a) => {
                    if(a.Status == AgentStatus.Bound)
                    {
                        Init();
                    }
                };

                if (Agent.Status == AgentStatus.Bound)
                {
                    Init();
                }
            }
        }

        protected virtual void OnDeviceChanged()
        {
            if (Agent != null && Agent.Status == AgentStatus.Bound)
            {
                Init();
            }
        }
		
		public virtual void Reset()
        {
        }

        protected virtual IDeviceVcsFactory GetDeviceFactory()
        {
            IDeviceVcsFactory result = null;

            if(_deviceFactory!=null)
            {
                result = _deviceFactory;
            }
            
            return result;
        }

        protected void SetDeviceFactory(IDeviceVcsFactory deviceVcsFactory)
        {
            _deviceFactory = deviceVcsFactory;
        }

        public void Init()
        {
            if(Agent != null && Device != null && IsSupported)
            { 
                if(UpdateViewModels)
                { 
                    if (IsRunning())
                    {
                        Stop();
                    }
                }

                var channel = Agent.Channel;

                if (channel != null)
                {
                    channel.StatusChanged += (sender, args) =>
                    {
                        IsOnline = args.Status == ChannelStatus.Online;
                    };

                    IsOnline = channel.Status == ChannelStatus.Online;
                }
                else
                {
                    MsgLogger.WriteError($"{GetType().Name} - Init", "channel not defined!");
                }

                VirtualCommandSet agentVcs;

                var deviceFactory = GetDeviceFactory();

                if (deviceFactory != null)
                {
                    agentVcs = deviceFactory.CreateVcs(Agent, Device);
                }
                else
                {
                    agentVcs = new VirtualCommandSet(Agent, Device);
                }

                if (Device.Status == DeviceStatus.Ready)
                {
                    Init(agentVcs);

                    OnDeviceInitialized();
                }
                else
                {
                    Device.StatusChanged += (sender, args) =>
                    {
                        if (Device.Status == DeviceStatus.Ready)
                        {
                            Init(agentVcs);

                            OnDeviceInitialized();
                        }
                    };
                }

                if (UpdateViewModels)
                { 
                    Task.Run(() => { Run(); });
                }
            }
        }

        private async void Run()
        {
            const int updateSessionInterval = 200;

            while (ShouldRun())
            {
                try
                {
                    await UpdateViewModelsTree();
                }
                catch (Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - Run", e);
                }
                
                var watch = new Stopwatch();

                watch.Start();

                while (watch.ElapsedMilliseconds < UpdateInterval && ShouldRun())
                {
                    await Task.Delay(updateSessionInterval);
                }
            }
        }
        
        private bool ShouldRun()
        {
            return !_updateViewModelsCts.IsCancellationRequested;
        }

        private bool IsRunning()
        {
            return !_updateViewModelsTask.IsCompleted;
        }

        private void Stop()
        {
            if (IsRunning())
            {
                _updateViewModelsCts.Cancel();

                _updateViewModelsTask.Wait();
            }
        }

        public virtual void ResetAgent()
        {
            IsOnline = false;

            _agent = null;

            foreach (var child in SafeChildrenArray)
            {
                if(child is ToolViewModel toolViewModel)
                {
                    toolViewModel.ResetAgent();
                }
            }
        }

        public override Task<bool> StartUpdate()
        {
            if (Agent != null)
            {
                var agentChannel = Agent.Channel;

                if (agentChannel != null)
                {
                    IsOnline = agentChannel.Status == ChannelStatus.Online;

                    agentChannel.StatusChanged += (o, e) =>
                    {
                        if (o is Channel channel)
                        {
                            IsOnline = channel.Status == ChannelStatus.Online;
                        }
                    };
                }
            }

            return base.StartUpdate();
        }

        public override Task<bool> StopUpdate()
        {
            IsOnline = false;

            return base.StopUpdate();
        }

        protected virtual Task RegisterAutoUpdate()
        {
            return Task.CompletedTask;
        }

        private void RegisterAutoUpdateAsync()
        {
            var id = Guid.NewGuid();

            var tr = new Thread(new ThreadStart(async () =>
            {
                WaitAllAutoUpdateTasks(id);

                try
                {
                    await RegisterAutoUpdate();
                }
                catch(Exception)
                {
                }
            }));

            AddAutoUpdateTask(tr, id);
        }

        private void WaitAllAutoUpdateTasks(Guid id)
        {
            var awaitableTasks = new List<AutoUpdateRegistration>();

            _autoUpdateLock.Wait();

            try
            {
                AutoUpdateRegistration currentTask = null;

                foreach (var autoUpdateTask in AutoUpdateTasks)
                {
                    if (autoUpdateTask.Id != id)
                    {
                        awaitableTasks.Add(autoUpdateTask);
                    }
                    else
                    {
                        currentTask = autoUpdateTask;
                    }
                }

                AutoUpdateTasks.Clear();

                if (currentTask != null)
                {
                    AutoUpdateTasks.Add(currentTask);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _autoUpdateLock.Release();
            }

            try
            {
                foreach(var t in awaitableTasks)
                {
                    //t.Thread.ThreadState == System.Threading.ThreadState.

                    t.Wait();
                }
            }
            catch (Exception)
            {
            }
        }

        private void AddAutoUpdateTask(Thread t, Guid id)
        {
            var aur = new AutoUpdateRegistration() { Thread = t, Id = id };

            _autoUpdateLock.Wait();

            try
            {
                AutoUpdateTasks.Add(aur);
            }
            catch(Exception)
            {
            }
            finally
            {
                _autoUpdateLock.Release();
            }

            t.Start();
        }

        protected virtual Task UnregisterAutoUpdate()
        {
            return Task.CompletedTask;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (AutoUpdateMode == AutoUpdateMode.Initialization || 
                AutoUpdateMode == AutoUpdateMode.Any)
            {
                UnregisterAutoUpdateAsync();

                RegisterAutoUpdateAsync();
            }

            UpdateAllControlsAsync();
        }

        private void UnregisterAutoUpdateAsync()
        {
            var id = Guid.NewGuid();

            var tr = new Thread(new ThreadStart(async () =>
            {
                WaitAllAutoUpdateTasks(id);

                await UnregisterAutoUpdate();

                return;
            }));

            AddAutoUpdateTask(tr,id);
        }

        protected virtual Task UpdateAllControls()
        {
            return Task.CompletedTask;
        }

        private Task UpdateAllControlsAsync()
        {
            const int minWaitTime = 100;
            const double minUpdateDelayInSec = 3;

            if ((DateTime.Now - _updateTaskTimestamp) > TimeSpan.FromSeconds(minUpdateDelayInSec) && _updateViewModelsTask.Wait(minWaitTime))
            {
                _updateTaskTimestamp = DateTime.Now;

                _updateViewModelsTask = Task.Run(async () =>
                {
                    await UpdateAllControls();
                });
            }

            return _updateViewModelsTask;
        }

        public override bool StartCommunication()
        {
            if (AutoUpdateMode == AutoUpdateMode.Visibility ||
                AutoUpdateMode == AutoUpdateMode.Initialization ||
                AutoUpdateMode == AutoUpdateMode.Any)
            {
                RegisterAutoUpdateAsync();
            }

            UpdateAllControlsAsync();

            return base.StartCommunication();
        }

        public override bool StopCommunication()
        {
            if (AutoUpdateMode == AutoUpdateMode.Visibility ||
                AutoUpdateMode == AutoUpdateMode.Initialization ||
                AutoUpdateMode == AutoUpdateMode.Any)
            {
                UnregisterAutoUpdateAsync();
            }

            return base.StopCommunication();
        }

        public override Task Show()
        {
            if (AutoUpdateMode == AutoUpdateMode.Visibility ||
                AutoUpdateMode == AutoUpdateMode.Any)
            {
                RegisterAutoUpdateAsync();
            }

            UpdateAllControlsAsync();

            return base.Show();
        }

        public override Task Hide()
        {
            if (AutoUpdateMode == AutoUpdateMode.Visibility ||
                AutoUpdateMode == AutoUpdateMode.Any)
            {
                UnregisterAutoUpdateAsync();
            }

            return base.Hide();
        }

        protected virtual void OnAutoUpdateModeChanged()
        {
            foreach(var child in SafeChildrenArray)
            {
                if(child is ToolViewModel viewModel)
                {
                    viewModel.AutoUpdateMode = AutoUpdateMode;
                }
            }
        }

        #endregion
    }
}
