// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    [XmlType("text-search-parameters")]
    public sealed class VocabSearch : IHealthVaultTypeSerializable
    {
        public VocabSearch()
        {
            MaxResults = 25;
        }

        public VocabSearch(string searchText)
            : this()
        {
            Text = new VocabSearchText(searchText);
        }

        [XmlElement("search-string", Order = 1)]
        public VocabSearchText Text { get; set; }

        [XmlElement("max-results", Order = 2)]
        public int MaxResults { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            throw new NotImplementedException();
        }

        public void Validate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}