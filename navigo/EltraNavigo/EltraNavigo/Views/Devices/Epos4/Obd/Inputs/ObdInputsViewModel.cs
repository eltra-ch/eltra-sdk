using System;
using EltraNavigo.Controls;
using EltraNavigo.Views.Obd.Inputs.Events;

namespace EltraNavigo.Views.Obd.Inputs
{
    public class ObdInputsViewModel : ToolViewBaseModel
    {
        #region Private fields

        private string _searchText;

        #endregion

        #region Constructors

        public ObdInputsViewModel(ToolViewBaseModel parent)
            : base(parent)
        {            
        }

        #endregion

        #region Properties

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        #endregion

        #region Events

        public event EventHandler<SearchTextChangedEventArgs> SearchTextChanged;
        public event EventHandler SearchTextCanceled;

        #endregion

        #region Event handler

        private void OnSearchTextChanged(string text)
        {
            SearchTextChanged?.Invoke(this, new SearchTextChangedEventArgs(text));
        }

        protected virtual void OnSearchTextCanceled()
        {
            SearchTextCanceled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public void SearchButtonPressed(string text)
        {
            OnSearchTextChanged(text);
        }

        public void ResetSearch()
        {
            OnSearchTextCanceled();
        }

        #endregion

        
    }
}
