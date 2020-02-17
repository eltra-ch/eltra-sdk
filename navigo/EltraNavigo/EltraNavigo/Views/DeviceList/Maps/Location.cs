using EltraCloudContracts.Contracts.Sessions;
using EltraNavigo.Controls;
using Xamarin.Forms.Maps;

namespace EltraNavigo.Views.DeviceList.Maps
{
    public class Location : BaseViewModel
    {
        #region Private fields

        private Position _position;
        private string _description;

        #endregion

        public Location(IpLocation ipLocation)
        {
            Description = $"{ipLocation.City}, {ipLocation.Country}";

            Position = new Position(ipLocation.Latitude, ipLocation.Longitude);
        }

        #region Properties

        public Position Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        #endregion
    }
}
