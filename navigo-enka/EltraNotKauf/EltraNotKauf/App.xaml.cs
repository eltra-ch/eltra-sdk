using Xamarin.Forms;
using EltraNotKauf.Views;

namespace EltraNotKauf
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

            string url = "https://notkauf.ch";
            //string url = "http://localhost:5001/";

            CreateIdentity();

            Current.Properties["url"] = url;
            
            MainPage = new MainView();
            
            _viewModel = new MainViewModel();

            MainPage.BindingContext = _viewModel;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        private void CreateIdentity()
        {
            string name = string.Empty;
            string login = string.Empty;
            string password = string.Empty;

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
