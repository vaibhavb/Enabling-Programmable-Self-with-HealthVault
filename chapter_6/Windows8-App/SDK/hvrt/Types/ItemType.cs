// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemType : IHealthVaultType
    {
        public ItemType()
        {
            Name = String.Empty;
            ID = String.Empty;
        }

        public ItemType(string typeID) : this()
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }
            ID = typeID;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string ID { get; set; }

        #region IHealthVaultType Members

        public void Validate()
        {
            ID.ValidateRequired("ID");
        }

        #endregion

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }

        public bool ShouldSerializeID()
        {
            return !String.IsNullOrEmpty(ID);
        }
    }
}