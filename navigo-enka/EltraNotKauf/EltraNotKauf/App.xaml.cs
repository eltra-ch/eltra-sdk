using Xamarin.Forms;
using EltraNotKauf.Views;
using Xamarin.Essentials;
using System;
using EltraCommon.Logger;
using EltraNotKauf.Helpers;
using Xamarin.Forms.Xaml;

namespace EltraNotKauf
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        #region Private fields

        private readonly MainViewModel _viewModel;

        #endregion

        #region Constructors

        public App()
        {
            InitializeComponent();

#if RELEASE
            MsgLogger.LogLevels = "";
#endif

            string url = "https://notkauf.ch";
            //string url = "http://localhost:5001/";

            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            Current.Properties["url"] = url;

            ThreadHelper.Initialize(Environment.CurrentManagedThreadId);

            MainPage = new MainView();
            
            _viewModel = new MainViewModel();

            MainPage.BindingContext = _viewModel;
        }

        #endregion

        #region Events

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if(e.NetworkAccess == NetworkAccess.Internet)
            {
                _viewModel?.StartCommunication();
            }
            else
            {
                _viewModel?.StopCommunication();
            }
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        protected override void OnStart()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                _viewModel?.StartCommunication();
            }
            else
            {
                _viewModel?.StopCommunication();
            }

            _viewModel?.StartUpdate();

            _viewModel?.OnStart();
        }

        protected override void OnSleep()
        {
            _viewModel?.StopCommunication();

            _viewModel?.StopUpdate();
        }

        protected override void OnResume()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                _viewModel?.StartCommunication();
            }

            _viewModel?.StartUpdate();
        }

        #endregion
    }
}
