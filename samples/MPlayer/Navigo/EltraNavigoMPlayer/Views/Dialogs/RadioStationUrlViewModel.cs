using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraUiCommon.Controls;

namespace EltraNavigoMPlayer.Views.Dialogs
{
    public class RadioStationUrlViewModel : BaseViewModel
    {
        private string _url;
        private bool _isChecked;
        private XddParameter _stationIdParameter;

        public RadioStationUrlViewModel(string url, XddParameter stationIdParameter)
        {
            _stationIdParameter = stationIdParameter;
            _url = url;

            UpdateCheckedFlag();
        }

        private void UpdateCheckedFlag()
        {
            if(_stationIdParameter != null && _stationIdParameter.GetValue(out string url))
            {
                if(Url == url)
                {
                    IsChecked = true;
                }
            }
        }

        public string Url { get => _url; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
    }
}