// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class OfflinePersonInfo : IValidatable
    {
        public OfflinePersonInfo()
        {
        }

        public OfflinePersonInfo(string id)
        {
            Id = id;
        }

        [XmlElement("offline-person-id")]
        public string Id { get; set; }

        #region IValidatable Members

        public void Validate()
        {
        }

        #endregion
    }
}