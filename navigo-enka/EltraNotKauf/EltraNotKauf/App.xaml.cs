using Xamarin.Forms;
using EltraNotKauf.Views;
using Xamarin.Essentials;
using System;
using EltraNotKauf.Helpers;
using Xamarin.Forms.Xaml;
using EltraCommon.Logger;

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

            Connectivity.ConnectivityChanged += OnConnectivityChanged;

            Current.Properties["url"] = Url;

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

        public string Url
        {
            get
            {
                string result;

                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        result = "http://notkauf.ch";
                        break;
                    case Device.Android:
                    case Device.UWP:
                    case Device.macOS:
                    default:
                        result = "https://notkauf.ch";
                        break;
                }

                return result;
            }
        }

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
