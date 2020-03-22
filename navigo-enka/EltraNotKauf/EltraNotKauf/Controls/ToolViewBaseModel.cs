using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EltraCommon.Logger;

namespace EltraNotKauf.Controls
{
    public class ToolViewBaseModel : BaseViewModel
    {
        #region Private fields

        private static object _syncObject = new object();
        private List<ToolViewBaseModel> _children;
        private bool _isVisible;
        private bool _isEnabled;
        private bool _isUpdating;
        private bool _isMandatory;
        private bool _isSupported;

        #endregion

        #region Construtors

        public ToolViewBaseModel()
        {
            _isEnabled = true;
        }

        public ToolViewBaseModel(ToolViewBaseModel parent)
            : base(parent)
        {
            Parent = parent;

            parent.AddChild(this);
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

        private void AddChild(ToolViewBaseModel child)
        {
            lock(_syncObject)
            { 
                Children.Add(child);
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

            return default;
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
            bool result = true;

            if (!IsUpdating)
            {
                IsUpdating = true;

                foreach (var child in SafeChildrenArray)
                {
                    result = await child.StartUpdate();

                    if(!result)
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

        public virtual async Task<bool> StopUpdate()
        {
            bool result = true;

            if (IsUpdating)
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

        #endregion
    }
}
