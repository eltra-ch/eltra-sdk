using EltraNavigo.Views.About;
using EltraNavigo.Views.DataRecorder;
using EltraNavigo.Views.DeviceList;
using EltraNavigo.Views.Devices.Thermo.Control;
using EltraNavigo.Views.Devices.Thermo.History;
using EltraNavigo.Views.Devices.Thermo.Overview;
using EltraNavigo.Views.Devices.Thermo.Settings;
using EltraNavigo.Views.Homing;
using EltraNavigo.Views.Login;
using EltraNavigo.Views.Obd;
using EltraNavigo.Views.PhotoControl;
using EltraNavigo.Views.Ppm;
using EltraNavigo.Views.Pvm;
using EltraNavigo.Views.RelayControl;
using Xamarin.Forms;

namespace EltraNavigo.Views.Selectors
{
    public class DetailPageSelector : DataTemplateSelector
    {
        public DataTemplate LoginView { get; set; }
        public DataTemplate DeviceListView { get; set; }
        public DataTemplate ObdView { get; set; }
        public DataTemplate PvmView { get; set; }
        public DataTemplate PpmView { get; set; }
        public DataTemplate HomingView { get; set; }
        public DataTemplate DataRecorderView { get; set; }
        public DataTemplate RelayControlView { get; set; }
        public DataTemplate PhotoControlView { get; set; }
        public DataTemplate ThermoControlView { get; set; }
        public DataTemplate ThermoOverviewView { get; set; }
        public DataTemplate ThermoHistoryView { get; set; }
        public DataTemplate ThermoSettingsView { get; set; }

        public DataTemplate AboutView { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate result = LoginView;

            if (item is LoginViewModel)
            {
                result = LoginView;
            }
            else if (item is DeviceListViewModel)
            {
                result = DeviceListView;
            }
            else if (item is ObdViewModel)
            {
                result = ObdView;
            }
            else if (item is PvmViewModel)
            {
                result = PvmView;
            }
            else if (item is PpmViewModel)
            {
                result = PpmView;
            }
            else if (item is HomingViewModel)
            {
                result = HomingView;
            }
            else if (item is DataRecorderViewModel)
            {
                result = DataRecorderView;
            }
            else if (item is RelayControlViewModel)
            {
                result = RelayControlView;
            }
            else if (item is PhotoControlViewModel)
            {
                result = PhotoControlView;
            }
            else if (item is ThermoControlViewModel)
            {
                result = ThermoControlView;
            }
            else if (item is ThermoOverviewViewModel)
            {
                result = ThermoOverviewView;
            }
            else if (item is ThermoHistoryViewModel)
            {
                result = ThermoHistoryView;
            }
            else if (item is ThermoSettingsViewModel)
            {
                result = ThermoSettingsView;
            }
            else if (item is AboutViewModel)
            {
                result = AboutView;
            }

            return result;
        }
    }
}
