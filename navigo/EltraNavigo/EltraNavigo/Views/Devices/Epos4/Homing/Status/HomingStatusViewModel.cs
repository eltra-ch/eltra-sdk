﻿using System.Threading.Tasks;
using EltraNavigo.Controls;
using EltraNavigo.Device.Epos4.Parameters;
using EltraNavigo.Device.Epos4.Parameters.Events;

namespace EltraNavigo.Views.Homing.Status
{
    public class HomingStatusViewModel : ToolViewBaseModel
    {
        #region Private fields

        private readonly StatusWordViewModel _statusWordViewModel;

        private ushort _statusWord;

        #endregion

        #region Constructors

        public HomingStatusViewModel(ToolViewBaseModel parent)
            : base(parent)
        {
            _statusWordViewModel = new StatusWordViewModel(this);

            _statusWordViewModel.StatusWordChanged += OnStatusWordChanged;
        }

        #endregion

        #region Events handling

        private void OnStatusWordChanged(object sender, StatusWordChangedEventArgs e)
        {
            StatusWord = e.StatusWordValue;
        }

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
