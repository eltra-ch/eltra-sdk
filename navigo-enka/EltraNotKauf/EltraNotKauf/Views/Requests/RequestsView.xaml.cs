using EltraNotKauf.Controls.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EltraNotKauf.Views.Requests
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RequestsView : ContentView
    {
        public RequestsView()
        {
            InitializeComponent();

            var entries = new List<Entry>();
            var pickers = new List<Picker>();

            UwpHelper.DeepSearch(Children.ToList(), ref entries);
            UwpHelper.DeepSearch(Children.ToList(), ref pickers);

            UwpHelper.FixElements(entries);
            UwpHelper.FixElements(pickers);
        }
    }
}