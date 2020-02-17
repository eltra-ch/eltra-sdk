using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EltraNavigo.Views.DeviceList
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceInfoView : ContentPage
    {
        public DeviceInfoView()
        {
            InitializeComponent();
        }

        /*private void OnDismissButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync(false);
        }*/
    }
}