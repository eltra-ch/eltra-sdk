namespace EltraCloudContracts.ObjectDictionary.Common.DeviceDescription.Profiles.Application.Units
{
    public class Unit
    {
        #region Properties

        public string UniqueId { get; set; }

        public string Label => GetLabel(RegionalOptions.Language);

        public double Multiplier => GetMultiplier();

        public int DecimalPlaces => GetDecimalPlaces();

        #endregion

        #region Methods

        protected virtual string GetLabel(string language)
        {
            return string.Empty;
        }

        protected virtual double GetMultiplier()
        {
            return 1;
        }

        protected virtual int GetDecimalPlaces()
        {
            return 0;
        }

        #endregion
    }
}
