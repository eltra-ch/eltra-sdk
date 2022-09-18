using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Contracts.Devices;
using EltraCommon.Logger;
using EltraConnector.Agent;
using EltraCommon.Contracts.Channels;

namespace EltraUiCommon.Controls
{
    public class ToolViewBaseModel : BaseViewModel
    {
        #region Private fields

        private static object _syncObject = new object();
        private List<ToolViewBaseModel> _children;
        private bool _isVisible;
        private bool _isEnabled;
        private bool _isUpdating;
        private bool _isRefreshing;
        private bool _isConnected;
        private bool _isMandatory;
        private bool _isSupported;
        private bool _isNavigable;
        private bool _isInitialized;
        private bool _isOnline;

        #endregion

        #region Construtors

        public ToolViewBaseModel()
        {
            _isNavigable = true;
            _isInitialized = false;
        }

        public ToolViewBaseModel(ToolViewBaseModel parent)
            : base(parent)
        {
            Parent = parent;

            Vcs = parent.Vcs;

            parent.AddChild(this);
        }

        #endregion

        #region Properties

        protected VirtualCommandSet Vcs { get; private set; }

        private List<ToolViewBaseModel> Children => _children ?? (_children = new List<ToolViewBaseModel>() );

        protected ToolViewBaseModel[] SafeChildrenArray
        {
            get
            {
                ToolViewBaseModel[] result;

                lock (_syncObject)
                {
                    result = Children.ToArray();
                }

                return result;
            }
        }

        public bool IsUpdating
        {
            get => _isUpdating; 
            private set => SetProperty(ref _isUpdating, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, "IsEnabled", OnChanged);
        }

        public bool IsSupported
        {
            get => _isSupported;
            set => SetProperty(ref _isSupported, value);
        }

        public bool IsMandatory
        {
            get => _isMandatory;
            set => SetProperty(ref _isMandatory, value);
        }

        public bool IsNavigable
        {
            get => _isNavigable;
            set => SetProperty(ref _isNavigable, value);
        }

        public bool IsInitialized => _isInitialized;

        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        #endregion

        #region Events

        public event EventHandler VisibilityChanged;

        #endregion

        #region Events handling

        protected virtual void OnInitialized()
        {
        }

        private void OnChanged()
        {
            foreach (var child in SafeChildrenArray)
            {
                child.IsEnabled = IsEnabled;
            }
        }

        private void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeviceChannelStatusChanged(ChannelStatus channelStatus)
        {
            switch (channelStatus)
            {
                case ChannelStatus.Online:
                    GoingOnline();
                    break;
                case ChannelStatus.Offline:
                    GoingOffline();
                    break;
            }
        }

        #endregion

        #region Methods

        protected virtual void Init(VirtualCommandSet vcs)
        {
            if (vcs != null && (Vcs != vcs || !_isInitialized))
            {
                _isInitialized = false;

                Vcs = vcs;

                if (Vcs.DeviceStatus == DeviceStatus.Ready)
                {
                    foreach (var child in SafeChildrenArray)
                    {
                        child.Init(Vcs);
                    }
                }

                Vcs.DeviceStatusChanged += (sender, args) =>
                {
                    foreach (var child in SafeChildrenArray)
                    {
                        child.Init(Vcs);
                    }
                };

                Vcs.DeviceChannelStatusChanged += (sender, args) =>
                {
                    var channelStatus = args.Status;

                    OnDeviceChannelStatusChanged(channelStatus);
                };

                OnInitialized();

                _isInitialized = true;
            }
        }

        private void AddChild(ToolViewBaseModel child)
        {
            lock(_syncObject)
            { 
                Children.Add(child);

                Task.Run(()=> {
                    if (Vcs != null && Vcs.DeviceStatus == DeviceStatus.Ready)
                    {
                        child.Init(Vcs);
                    }
                });
            }

            OnPropertyChanged("Children");
        }

        protected async Task UpdateViewModelsTree()
        {
            if (IsVisible)
            {   
                foreach (var child in SafeChildrenArray)
                {
                    await child.UpdateViewModelsTree();
                }
                
                if (IsUpdating)
                {
                    await Update();
                }
            }
        }

        protected virtual Task Update()
        {
            MsgLogger.WriteDebug($"{GetType().Name} - Update", "Update ToolViewBaseModel");

            return Task.CompletedTask;
        }

        public virtual async Task Show()
        {
            IsBusy = true;

            if (!IsVisible)
            {
                foreach (var child in SafeChildrenArray)
                {
                    await child.Show();
                }

                IsVisible = true;

                OnVisibilityChanged();

                await StartUpdate();
            }

            IsBusy = false;
        }

        public virtual async Task Hide()
        {
            IsBusy = true;

            if (IsVisible)
            {
                await StopUpdate();

                foreach (var child in SafeChildrenArray)
                {
                    await child.Hide();
                }

                IsVisible = false;

                OnVisibilityChanged();
            }

            IsBusy = false;
        }

        public virtual async Task<bool> StartUpdate()
        {
            bool result = Vcs != null;

            if (!IsUpdating && result)
            {
                IsUpdating = true;

                foreach (var child in SafeChildrenArray)
                {
                    result = await child.StartUpdate();

                    if(!result)
                    {
                        IsUpdating = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public virtual async Task<bool> StopUpdate()
        {
            bool result = Vcs != null;

            if (IsUpdating && result)
            {
                IsUpdating = false;
                
                result = true;

                foreach (var child in SafeChildrenArray)
                {
                    result = await child.StopUpdate();

                    if (!result)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public virtual bool StartCommunication()
        {
            bool result = true;

            if (!IsConnected)
            {
                IsConnected = true;

                foreach (var child in SafeChildrenArray)
                {
                    result = child.StartCommunication();

                    if (!result)
                    {
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public virtual bool StopCommunication()
        {
            bool result = true;

            if (IsConnected)
            {
                IsConnected = false;

                result = true;

                foreach (var child in SafeChildrenArray)
                {
                    result = child.StopCommunication();

                    if (!result)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        protected virtual void GoingOnline()
        {
            IsOnline = true;
        }

        protected virtual void GoingOffline()
        {
            IsOnline = false;
        }

        #endregion
    }
}
