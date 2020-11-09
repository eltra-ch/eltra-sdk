using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MPlayerCommon.Contracts
{
    [DataContract]
    public class RadioStationEntry
    {
        #region Private fields

        private List<string> _urls;

        #endregion

        public RadioStationEntry()
        {
            Name = string.Empty;
            Description = string.Empty;
            Genre = string.Empty;
            Country = string.Empty;
            Language = string.Empty;
        }

        #region Properties

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Genre { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public List<string> Urls 
        { 
            get => _urls ?? (_urls = new List<string>());
            set => _urls = value;
        }

        #endregion
    }
}
