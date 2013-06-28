// (c) Microsoft. All rights reserved

using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class VocabQueryResult : IHealthVaultTypeSerializable
    {
        [XmlElement("code-item")]
        public VocabItem[] Items { get; set; }

        [XmlIgnore]
        public bool HasItems
        {
            get { return !Items.IsNullOrEmpty(); }
        }

        [XmlIgnore]
        public IEnumerable<string> MatchText
        {
            get
            {
                if (HasItems)
                {
                    foreach (VocabItem item in Items)
                    {
                        yield return item.DisplayText;
                    }
                }
            }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Items.ValidateOptional("Items");
        }

        #endregion
    }
}