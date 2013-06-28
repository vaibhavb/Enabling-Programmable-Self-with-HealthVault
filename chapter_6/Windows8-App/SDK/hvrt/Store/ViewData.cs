// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;
using DateTime = HealthVault.Types.DateTime;

namespace HealthVault.Store
{
    public sealed class ViewData : IHealthVaultTypeSerializable
    {
        private LazyField<ViewKeyCollection> m_keys;
        private ItemQuery m_query;

        public ViewData()
        {
            Name = String.Empty;
        }

        public ViewData(ItemQuery query)
            : this(query, query.Name)
        {
        }

        public ViewData(ItemQuery query, string name) : this()
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }

            Query = query;
            Name = name;
        }

        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        [XmlElement("lastUpdated", Order = 2)]
        public DateTime LastUpdated { get; set; }

        [XmlElement("query", Order = 3)]
        public ItemQuery Query 
        { 
            get { return m_query;}
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Query");
                }
                m_query = value;
            }
        }

        [XmlElement("keys", Order = 4)]
        public ViewKeyCollection Keys
        {
            get { return m_keys.Value; }
            set { m_keys.Value = value; }
        }

        [XmlIgnore]
        public bool HasName
        {
            get { return !string.IsNullOrEmpty(Name); }
        }

        [XmlIgnore]
        public bool HasQuery
        {
            get { return (Query != null); }
        }

        [XmlIgnore]
        public int KeyCount
        {
            get { return m_keys.HasValue ? m_keys.Value.Count : 0; }
        }

        [XmlIgnore]
        public bool HasKeys
        {
            get { return (KeyCount > 0); }
        }
        
        internal StringCollection TypeVersions
        {
            get { return m_query.View.TypeVersions;}
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Query.ValidateRequired("Query");
            if (HasKeys)
            {
                Keys.ValidateRequired("Keys");
            }
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

        public static ViewData Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ViewData>(xml);
        }
        
        public ViewKey KeyAtIndex(int index)
        {
            this.ValidateIndex(index);
            return m_keys.Value[index];
        }

        public void ValidateIndex(int index)
        {
            if (index < 0 || index >= KeyCount)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }
    }
}