
using System;
using System.Threading.Tasks;
using EltraCommon.Logger;
using EltraNavigo.Controls;
using EltraNavigo.Controls.Status;
using EltraNavigo.Views.Obd.Inputs;
using EltraNavigo.Views.Obd.Inputs.Events;
using EltraNavigo.Views.Obd.Outputs;
using Xamarin.Forms;

namespace EltraNavigo.Views.Obd
{
    public class ObdViewModel : ToolViewModel
    {
        #region Private fields
        
        private ToolStatusViewModel _toolStatusViewModel;
        private ObdOutputsViewModel _outputsViewModel;
        private ObdInputsViewModel _inputsViewModel;

        #endregion

        #region Constructors

        public ObdViewModel()
        {
            Title = "Object Dictionary";
            Image = ImageSource.FromResource("EltraNavigo.Resources.book-open_32px.png");
            Uuid = "B47D8049-914B-4984-A883-90CF537F0318";
        }

        #endregion

        #region Properties

        public ToolStatusViewModel ToolStatusViewModel => _toolStatusViewModel ?? (_toolStatusViewModel = new ToolStatusViewModel(this));

        public ObdOutputsViewModel OutputsViewModel => _outputsViewModel ?? (_outputsViewModel = new ObdOutputsViewModel(this));

        public ObdInputsViewModel InputsViewModel => _inputsViewModel ?? (_inputsViewModel = CreateInputsViewModel());

        #endregion

        #region Events handling

        private async void OnSearchTextChanged(object sender, SearchTextChangedEventArgs e)
        {
            await OutputsViewModel.SetFilter(e.Text);
        }

        private async void OnSearchTextCanceled(object sender, EventArgs e)
        {
            await OutputsViewModel.ResetFilter();
        }

        #endregion

        #region Methods

        private ObdInputsViewModel CreateInputsViewModel()
        {
            var result = new ObdInputsViewModel(this);

            result.SearchTextChanged += OnSearchTextChanged;
            result.SearchTextCanceled += OnSearchTextCanceled;

            return result;
        }
        
        protected override Task Update()
        {
            return Task.Run(UpdateEnabledStatus);
        }

        public override async Task Show()
        {
            IsBusy = true;

            MsgLogger.WriteDebug($"{GetType().Name} - Show", "Update ObdInputsViewModel");

            await UpdateEnabledStatus();

            await base.Show();

            IsBusy = false;
        }

        private async Task UpdateEnabledStatus()
        {
            if (Vcs != null)
            {
                bool deviceLocked = await Vcs.IsDeviceLocked(Device);

                ToolStatusViewModel.IsEnabled = deviceLocked;
                InputsViewModel.IsEnabled = deviceLocked;
                OutputsViewModel.IsEnabled = deviceLocked;
            }
        }

        #endregion
    }
}
