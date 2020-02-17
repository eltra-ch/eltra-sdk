using System;

namespace EltraNavigo.Controls.Status
{
    public class ErrorHistoryEntryViewModel : BaseViewModel, IEquatable<ErrorHistoryEntryViewModel>
    {
        #region Private fields

        private uint _errorCode;
        private string _errorCodeAsString;
        private string _errorDescription;

        #endregion

        #region Properties

        public uint ErrorCode
        {
            get => _errorCode;
            set => SetProperty(ref _errorCode, value, "ErrorCode", OnErrorCodeChanged);
        }
        
        public string ErrorCodeAsString
        {
            get => _errorCodeAsString;
            set => SetProperty(ref _errorCodeAsString, value);
        }

        public string ErrorDescription
        {
            get => _errorDescription;
            set => SetProperty(ref _errorDescription, value);
        }

        #endregion

        #region Methods

        private void OnErrorCodeChanged()
        {
            ErrorCodeAsString = $"0x{ErrorCode:X4}";
        }

        public bool Equals(ErrorHistoryEntryViewModel other)
        {
            if (other is null)
                return false;

            return this.ErrorCode == other.ErrorCode && this.ErrorDescription == other.ErrorDescription;
        }

        public override int GetHashCode() => (ErrorCode, ErrorDescription).GetHashCode();

        #endregion
    }
}
