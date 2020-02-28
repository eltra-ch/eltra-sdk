using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using EltraCloudContracts.Contracts.Devices;
using EltraCloudContracts.Contracts.Sessions;
using EltraCommon.Logger;
using EltraNavigo.Controls;
using EltraResources;
using Xamarin.Forms;

namespace EltraNavigo.Views.DeviceList
{
    public class DeviceViewModel : ToolViewBaseModel
    {
        #region Private fields

        private string _name;
        private string _version;
        private string _description;
        private ImageSource _image;
        private bool _isDeviceLocked;
        private bool _canLockDevice;
        private Session _session;

        #endregion

        #region Constructors
        
        public DeviceViewModel(ToolViewBaseModel parent, Session session, EltraDevice device)
            : base(parent)
        {
            _session = session;

            Device = device;

            RegisterEvents();

            Initialize();
        }

        #endregion

        #region Commands

        public ICommand InformationCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await OnInformationRequestedAsync();
                });
            }
        }

        #endregion

        #region Properties

        public EltraDevice Device { get; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        public ImageSource Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public bool IsDeviceLocked
        {
            get => _isDeviceLocked;
            set => SetProperty(ref _isDeviceLocked, value);
        }

        public bool CanLockDevice
        {
            get => _canLockDevice;
            set => SetProperty(ref _canLockDevice, value);
        }

        #endregion

        #region Events handling

        private async Task OnInformationRequestedAsync()
        {
            var view = new DeviceInfoView();
            var viewModel = new DeviceInfoViewModel(this, Vcs, _session);

            view.BindingContext = viewModel;

            var navigation = Application.Current.MainPage.Navigation;

            await navigation?.PushModalAsync(view);    
        }

        private void DeviceOnStatusChanged(object sender, EventArgs e)
        {
            Initialize();
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            if(Device!=null && Device.Status == DeviceStatus.Ready)
            {
                Name = Device.Name;
                Description = Device.ProductName;

                Version = $"0x{Device.Version.HardwareVersion:X} 0x{Device.Version.SoftwareVersion:X} 0x{Device.Version.ApplicationNumber:X} 0x{Device.Version.ApplicationVersion:X}";

                CreateImage();
            }
        }

        private void RegisterEvents()
        {
            if (Device != null)
            {
                Device.StatusChanged += DeviceOnStatusChanged;
            }
        }

        private void CreateImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(Device.ProductPicture))
                {
                    var base64Prefix = "data:image/png;base64,";
                    string imageBase64 = Device.ProductPicture;
                    
                    if (imageBase64.StartsWith(base64Prefix))
                    {
                        imageBase64 = imageBase64.Substring(base64Prefix.Length);
                    }

                    Image = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
                }
                else
                {
                    var fileName = DeviceToPictureFileNameConverter();

                    var eltraResource = new EltraResource();
                    string imageResourceName = eltraResource.GetImageSourceName("devices.thumbnails", $"{fileName}");

                    Image = ImageSource.FromResource(imageResourceName, eltraResource.Assembly);
                }
            }
            catch(Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - CreateImage", e);
            }
        }
        
        private string DeviceToPictureFileNameConverter()
        {
            var version = Device.Version;

            var result = $"{version.HardwareVersion:X4}.png";

            return result;
        }

        public override async Task Show()
        {
            IsBusy = true;

            if (Vcs != null)
            {
                CanLockDevice = await Vcs.CanLockDevice(Device);
                IsDeviceLocked = await Vcs.IsDeviceLocked(Device);
            }

            await base.Show();

            IsBusy = false;
        }

        public async void LockToggle(bool toggled)
        {
            bool result = false;

            if (Vcs != null)
            {
                if (toggled)
                {
                    result = await Vcs.LockDevice(Device);
                }
                else
                {
                    result = await Vcs.UnlockDevice(Device);
                }
            }

            if (result)
            {
                IsDeviceLocked = toggled;
            }
        }

        #endregion
    }
}
