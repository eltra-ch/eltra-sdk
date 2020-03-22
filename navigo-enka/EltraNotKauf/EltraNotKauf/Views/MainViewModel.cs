using EltraNotKauf.Controls;
using Xamarin.Forms;

namespace EltraNotKauf.Views
{
    class MainViewModel : BaseViewModel
    {
        #region Private fields

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
        
        private MasterViewModel CreateMasterViewModel()
        {
            var result = new MasterViewModel(this);

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
            await MasterViewModel.StartUpdate();
        }

        public async void StopCommunication()
        {
            await MasterViewModel.StopUpdate();
        }

        #endregion
    }
}
