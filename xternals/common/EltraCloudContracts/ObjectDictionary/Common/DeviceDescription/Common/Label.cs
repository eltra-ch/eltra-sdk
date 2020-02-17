namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Common
{
    public class Label
    {
        #region Properties

        public string Lang { get; set; }

        public string Content { get; set; }

        #endregion

        #region Methods

        public virtual bool Parse()
        {
            return false;
        }

        #endregion
    }
}
