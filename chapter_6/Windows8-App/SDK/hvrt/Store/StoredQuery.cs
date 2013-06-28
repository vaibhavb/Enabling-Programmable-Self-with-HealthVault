// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;
using DateTime = HealthVault.Types.DateTime;

namespace HealthVault.Store
{
    public sealed class StoredQuery : IHealthVaultTypeSerializable
    {
        public StoredQuery()
        {
            Name = String.Empty;
        }

        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        [XmlElement("lastUpdated", Order = 2)]
        public DateTime LastUpdated { get; set; }

        [XmlElement("query", Order = 3)]
        public ItemQuery Query { get; set; }

        [XmlElement("result", Order = 4)]
        public ItemQueryResult Result { get; set; }

        [XmlIgnore]
        public bool HasQuery
        {
            get { return (Query != null); }
        }

        [XmlIgnore]
        public bool HasResult
        {
            get { return (Result != null); }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Query.ValidateOptional("Query");
            Result.ValidateOptional("Result");
        }

        #endregion

        public bool IsStale(int maxAgeInSeconds) // Easier to use int from Javascript
        {
            if (LastUpdated == null)
            {
                return true;
            }

            TimeSpan ts = TimeSpan.FromSeconds(maxAgeInSeconds);
            DateTimeOffset lastUpdated = LastUpdated.Value;
            TimeSpan diff = DateTimeOffset.Now.Subtract(lastUpdated);

            return (diff > ts);
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }
    }
}