using EltraNotKauf.Controls;

namespace EltraNotKauf.Views.Common
{
    public class AssignedToViewModel : BaseViewModel
    {
        private string _name;
        private string _city;
        private string _phone;

        public AssignedToViewModel()
        {   
        }

        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value);   
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }
    }
}
