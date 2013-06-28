// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    [XmlType("thing-id")]
    public sealed class ItemKey : IHealthVaultTypeSerializable
    {
        public ItemKey()
        {
            ID = String.Empty;
            Version = String.Empty;
        }

        public ItemKey(string id, string version) : this()
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id");
            }
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("version");
            }
            ID = id;
            Version = version;
        }

        [XmlText]
        public string ID { get; set; }

        [XmlAttribute("version-stamp")]
        public string Version { get; set; }

        [XmlIgnore]
        public bool HasVersion
        {
            get { return !string.IsNullOrEmpty(Version); }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            ID.ValidateRequired("ID");
            Version.ValidateRequired("Version");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public bool IsVersion(string version)
        {
            return Version.SafeEquals(version, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", ID, Version);
        }

        public override bool Equals(object obj)
        {
            var key = obj as ItemKey;
            if (key == null)
            {
                return base.Equals(obj);
            }

            return EqualsKey(key);
        }

        public bool EqualsKey(ItemKey key)
        {
            if (key == null)
            {
                return false;
            }

            if (ReferenceEquals(this, key))
            {
                return true;
            }

            return (
                ID.SafeEquals(key.ID, StringComparison.Ordinal) &&
                    Version.SafeEquals(key.Version, StringComparison.Ordinal)
                );
        }

        public static int Compare(ItemKey key1, ItemKey key2)
        {
            if (key1 == null)
            {
                if (key2 == null)
                {
                    return 0;
                }

                return -1;
            }

            if (key2 == null)
            {
                return 1;
            }

            int cmp = string.CompareOrdinal(key1.ID, key2.ID);
            if (cmp == 0)
            {
                cmp = string.CompareOrdinal(key1.Version, key2.Version);
            }

            return cmp;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static ItemKey NewKey()
        {
            string id = Guid.NewGuid().ToString("D");
            return new ItemKey {ID = id, Version = id};
        }

        const char LocalKeyPrefix = 'L';
        public static ItemKey NewLocalKey()
        {
            ItemKey key = NewKey();
            key.ID = LocalKeyPrefix + key.ID;
            return key;
        }

        public bool IsLocal
        {
            get 
            { 
                return (!string.IsNullOrEmpty(this.ID) && this.ID[0] == LocalKeyPrefix); 
            }
        }

        public bool ShouldSerializeID()
        {
            return !String.IsNullOrEmpty(ID);
        }

        public bool ShouldSerializeVersion()
        {
            return !String.IsNullOrEmpty(Version);
        }
    }
}