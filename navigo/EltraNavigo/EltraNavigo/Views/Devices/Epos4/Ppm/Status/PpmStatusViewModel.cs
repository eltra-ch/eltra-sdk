using EltraNavigo.Device.Epos4.Parameters.Events;
using EltraNavigo.Controls;
using EltraNavigo.Device.Epos4.Parameters;
using System.Threading.Tasks;

namespace EltraNavigo.Views.Ppm.Status
{
    public class PpmStatusViewModel : ToolViewBaseModel
    {
        #region Private fields

        private readonly StatusWordViewModel _statusWordViewModel;

        private ushort _statusWord;

        #endregion

        #region Constructors

        public PpmStatusViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _statusWordViewModel = new StatusWordViewModel(this);

            _statusWordViewModel.StatusWordChanged += OnStatusWordChanged;
        }

        #region Events handling

        private void OnStatusWordChanged(object sender, StatusWordChangedEventArgs e)
        {
            StatusWord = e.StatusWordValue;
        }

        #endregion

        #endregion

        #region Properties

        public ushort StatusWord 
        { 
            get => _statusWord;
            set => SetProperty(ref _statusWord, value); 
        }

        #endregion

        #region Methods

        public override async Task Show()
        {
            IsBusy = true;

            await base.Show();
            
            StatusWord = _statusWordViewModel.StatusWord;

            IsBusy = false;
        }

        #endregion
    }
}
