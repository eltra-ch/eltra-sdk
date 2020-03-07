using EltraConnector.UserAgent;
using EltraCloudContracts.Contracts.Users;
using EltraNavigo.Controls;
using Xamarin.Forms;

namespace EltraNavigo.Views
{
    class MainViewModel : BaseViewModel
    {
        #region Private fields

        private DeviceAgent _agent;

        private MasterViewModel _masterViewModel;
        private DetailViewModel _detailViewModel;

        private bool _isMasterPageVisible;

        #endregion

        #region Properties

        public string Url
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("url"))
                {
                    result = Application.Current.Properties["url"] as string;
                }

                return result;
            }
        }

        public string Uuid
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("uuid"))
                {
                    result = Application.Current.Properties["uuid"] as string;
                }

                return result;
            }
        }

        public string Login
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("login"))
                {
                    result = Application.Current.Properties["login"] as string;
                }

                return result;
            }
        }

        public string Name
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("name"))
                {
                    result = Application.Current.Properties["name"] as string;
                }

                return result;
            }
        }

        public string Password
        {
            get
            {
                string result = string.Empty;

                if (Application.Current.Properties.ContainsKey("password"))
                {
                    result = Application.Current.Properties["password"] as string;
                }

                return result;
            }
        }

        public DeviceAgent Agent => _agent ?? (_agent = CreateAgent());

        public MasterViewModel MasterViewModel
        {
            get => _masterViewModel ?? (_masterViewModel = CreateMasterViewModel());
            set => SetProperty(ref _masterViewModel, value);
        }

        public DetailViewModel DetailViewModel => _detailViewModel ?? (_detailViewModel = new DetailViewModel(this, MasterViewModel));

        public bool IsMasterPageVisible
        {
            get => _isMasterPageVisible;
            set => SetProperty(ref _isMasterPageVisible, value);
        }

        #endregion

        #region Methods

        private DeviceAgent CreateAgent()
        {
            const uint timeout = 60;
            const uint updateInterval = 30;
            
            var agent = new DeviceAgent(Url, Uuid, new UserAuthData { Login = Login, Password = Password, Name = Name }, updateInterval, timeout);

            return agent;
        }

        private MasterViewModel CreateMasterViewModel()
        {
            var result = new MasterViewModel(this);

            result.Agent = Agent;

            result.PageChanged += (sender, args) => 
            {
                HideMasterView();        
            };

            return result;
        }

        private void HideMasterView()
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                IsMasterPageVisible = false;

                OnPropertyChanged("IsMasterPageVisible");
            });
        }
        
        public async void StartCommunication()
        {
            if(_agent==null)
            { 
                _agent = CreateAgent();

                MasterViewModel.Agent = _agent;
            }

            LockOnStart();

            await MasterViewModel.StartUpdate();
        }

        private void LockOnStart()
        {
            MasterViewModel.LockOnStart();
        }

        public async void StopCommunication()
        {
            UnlockOnStop();

            if (_agent!=null)
            {
                MasterViewModel.ResetAgent();

                _agent.Dispose();
                _agent = null;
            }
            
            await MasterViewModel.StopUpdate();
        }

        private void UnlockOnStop()
        {
            MasterViewModel.UnlockOnStop();
        }

        #endregion
    }
}
