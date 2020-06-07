using System;
using Xamarin.Forms;
using EltraNavigo.Views;
using EltraCommon.Logger;

namespace EltraNavigo
{
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

            CreateIdentity();

            Current.Properties["url"] = Url;
            
            MainPage = new MainView();
            
            _viewModel = new MainViewModel();

            MainPage.BindingContext = _viewModel;
        }

        #endregion

        #region Properties

        public string Url
        {
            get
            {
                string result;

                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.iOS:
                        result = "http://eltra.ch";
                        break;
                    case Xamarin.Forms.Device.Android:
                    case Xamarin.Forms.Device.UWP:
                    case Xamarin.Forms.Device.macOS:
                    default:
                        result = "https://eltra.ch";
                        break;
                }

                return result;
            }
        }

        public string Uuid
        {
            get
            {
                string uuid = string.Empty;

                if (Current.Properties.ContainsKey("uuid"))
                {
                    uuid = Current.Properties["uuid"] as string;
                }

                return uuid;
            }
            set
            {
                Current.Properties["uuid"] = value;
            }
        }

        private int RandomNumber
        {
            get
            {
                const int maxValue = 999;
                var randomSessionId = new Random();
                
                return randomSessionId.Next(0, maxValue);
            }            
        }

        #endregion

        #region Methods

        private void GenerateRandomSessionUuid()
        {
            string uuid = Uuid;
            int i = uuid.IndexOf('#');

            if(i>0)
            {
                uuid = uuid.Substring(0, i);
            }

            if (string.IsNullOrEmpty(uuid))
            {
                Uuid = $"{Guid.NewGuid().ToString()}#{RandomNumber}";
            }
            else
            {
                Uuid = $"{uuid}#{RandomNumber}";
            }
        }

        private void CreateIdentity()
        {
            string name = string.Empty;
            string login = string.Empty;
            string password = string.Empty;

            GenerateRandomSessionUuid();

            if (Current.Properties.ContainsKey("name"))
            { 
                name = Current.Properties["name"] as string;
            }

            if (string.IsNullOrEmpty(name))
            {
                Current.Properties["name"] = "Navigo user";
            }

            if (Current.Properties.ContainsKey("login"))
            { 
                login = Current.Properties["login"] as string;
            }

            if (string.IsNullOrEmpty(login))
            {
                Current.Properties["login"] = "navigo@eltra.ch";
            }

            if (Current.Properties.ContainsKey("password"))
            { 
                password = Current.Properties["password"] as string;
            }

            if (string.IsNullOrEmpty(password))
            {
                Current.Properties["password"] = "navigo@eltra.ch";
            }
        }

        protected override void OnStart()
        {
            _viewModel?.StartCommunication();
        }

        protected override void OnSleep()
        {
            _viewModel?.StopCommunication();
        }

        protected override void OnResume()
        {
            _viewModel?.StartCommunication();
        }

        #endregion
    }
}
