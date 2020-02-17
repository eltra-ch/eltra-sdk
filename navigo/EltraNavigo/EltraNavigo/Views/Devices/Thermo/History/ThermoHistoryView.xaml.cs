using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EltraNavigo.Views.Devices.Thermo.History
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class ThermoHistoryView : ContentView
    {
        public ThermoHistoryView()
        {
            InitializeComponent();

            ForceLayout();
        }
    }
}