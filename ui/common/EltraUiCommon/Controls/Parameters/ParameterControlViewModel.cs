namespace EltraUiCommon.Controls.Parameters
{
    public class ParameterControlViewModel : ToolViewBaseModel
    {
        #region Private fields

        private string _uniqueId;
        private bool _showLabel = true;
        private string _label;
        private bool _isValid = true;
        
        private readonly ushort _index;
        private readonly byte _subIndex;

        #endregion

        #region Constructors

        public ParameterControlViewModel(ToolViewBaseModel parent, string uniqueId) 
            : base(parent)
        {
            UniqueId = uniqueId;
        }

        public ParameterControlViewModel(ToolViewBaseModel parent, ushort index, byte subIndex)
            : base(parent)
        {
            _index = index;
            _subIndex = subIndex;
        }

        #endregion

        #region Properties

        public string UniqueId
        {
            get => _uniqueId;
            set { _uniqueId = value; OnPropertyChanged("UniqueId");}
        }

        public bool ShowLabel
        {
            get => _showLabel;
            set
            {
                _showLabel = value; 
                OnPropertyChanged("ShowLabel");
            }
        }

        public ushort Index => _index;

        public byte SubIndex => _subIndex;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        #endregion

        #region Methods

        public virtual void InitModelData()
        {
        }

        #endregion
    }
}
