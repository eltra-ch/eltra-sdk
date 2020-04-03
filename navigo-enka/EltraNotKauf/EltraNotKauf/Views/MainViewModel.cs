﻿using EltraNotKauf.Controls;
using EltraNotKauf.Helpers;
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
            IsMasterPageVisible = false;

            OnPropertyChanged("IsMasterPageVisible");
        }
        
        public async void StartUpdate()
        {
            await MasterViewModel.StartUpdate();
        }

        public async void StopUpdate()
        {
            await MasterViewModel.StopUpdate();
        }

        public async void StartCommunication()
        {
            await MasterViewModel.StartCommunication();
        }

        public async void StopCommunication()
        {
            await MasterViewModel.StopCommunication();
        }

        public void OnStart()
        {
            MasterViewModel.GotoFirstPage();
        }

        #endregion
    }
}