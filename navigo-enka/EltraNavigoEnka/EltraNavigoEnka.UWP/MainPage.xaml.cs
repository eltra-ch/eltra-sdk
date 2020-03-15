namespace EltraNavigo.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            Xamarin.FormsMaps.Init("AiiFY0nwNBSMYNGMryV0jMxxFHFk5_MfODMS80mUamw-BCI67dNGaDpwfH9Bwxom");
            Windows.Services.Maps.MapService.ServiceToken = "AiiFY0nwNBSMYNGMryV0jMxxFHFk5_MfODMS80mUamw-BCI67dNGaDpwfH9Bwxom";

            LoadApplication(new EltraNavigo.App());            
        }
    }
}
