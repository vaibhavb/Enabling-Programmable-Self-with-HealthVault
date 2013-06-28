// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemKeyList : IHealthVaultTypeSerializable
    {
        [XmlElement("thing-id")]
        public ItemKey[] Keys { get; set; }

        [XmlIgnore]
        public bool HasKeys
        {
            get { return !Keys.IsNullOrEmpty(); }
        }

        [XmlIgnore]
        public ItemKey FirstKey
        {
            get { return HasKeys ? Keys[0] : null; }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Keys.ValidateRequired("Keys");
        }

        #endregion
    }
}