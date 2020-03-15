using EltraNavigo.Controls;

namespace EltraNavigo.Views
{
    public class DetailViewModel : BaseViewModel
    {
        #region Private fields

        private MasterViewModel _masterViewModel;

        #endregion

        #region Constructors

        public DetailViewModel(BaseViewModel parent, MasterViewModel masterViewModel)
            : base(parent)
        {
            if (masterViewModel != null)
            {
                MasterViewModel = masterViewModel;

                UpdateTitle();

                MasterViewModel.PageChanged += (sender, args) => { UpdateTitle(); };
            }
        }

        #endregion

        #region Properties

        public MasterViewModel MasterViewModel
        {
            get => _masterViewModel;
            set => SetProperty(ref _masterViewModel, value);
        }
        
        #endregion

        #region Methods
               
        private void UpdateTitle()
        {
            Title = MasterViewModel?.ActiveViewModel?.Title;
        }

        #endregion
    }
}
