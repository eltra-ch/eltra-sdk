using EltraCommon.ObjectDictionary.Xdd.DeviceDescription.Profiles.Application.Parameters;
using EltraUiCommon.Controls;
using MPlayerCommon.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EltraNavigoMPlayer.Views.Dialogs
{
    public class RadioStationEntryViewModel : BaseViewModel
    {
        private List<RadioStationUrlViewModel> _urls;
        private XddParameter _stationIdParameter;

        public RadioStationEntryViewModel(RadioStationEntry radioStationEntry, XddParameter stationIdParameter)
        {
            _stationIdParameter = stationIdParameter;

            RadioStationEntry = radioStationEntry;
        }

        public RadioStationEntry RadioStationEntry { get; set; }
                
        public string Name { get => RadioStationEntry.Name; }
        public string Description { get => RadioStationEntry.Description; }
        public string Genre { get => RadioStationEntry.Genre; }
        public string Country { get => RadioStationEntry.Country; }
        public string Language { get => RadioStationEntry.Language; }

        public List<RadioStationUrlViewModel> Urls => _urls ?? (_urls = CreateUrls());

        protected virtual List<RadioStationUrlViewModel> CreateUrls()
        {
            var result = new List<RadioStationUrlViewModel>();

            foreach(var url in RadioStationEntry.Urls)
            {
                var urlViewModel = new RadioStationUrlViewModel(url, _stationIdParameter);

                result.Add(urlViewModel);
            }

            SetDefaultUrl(result);

            return result;
        }

        private void SetDefaultUrl(List<RadioStationUrlViewModel> urls)
        {
            bool isChecked = IsAnyUrlChecked(urls);

            if (!isChecked && urls.Count > 0)
            {
                urls[0].IsChecked = true;
            }
        }

        private bool IsAnyUrlChecked(List<RadioStationUrlViewModel> urls)
        {
            bool isChecked = false;
            foreach (var url in urls)
            {
                if (url.IsChecked)
                {
                    isChecked = true;
                    break;
                }
            }

            return isChecked;
        }

        public async Task<bool> Apply()
        {
            bool result = false;

            IsBusy = true;

            foreach(var url in Urls)
            {
                if(url.IsChecked)
                {
                    result = _stationIdParameter.SetValue(url.Url);

                    if (result)
                    {
                        result = await _stationIdParameter.Write();
                    }

                    break;
                }
            }

            IsBusy = false;

            return result;
        }
    }
}
