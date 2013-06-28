// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.ItemTypes;
using Windows.Foundation;

namespace HealthVault.Types
{
    [XmlType("thing")]
    public sealed class RecordItem : IHealthVaultType
    {
        private ItemDataBlob m_blobs;
        private ItemData m_data;

        public RecordItem()
        {
            State = String.Empty;
            UpdatedEndDate = DateTime.FromSystemDateTime(DateTimeOffset.MaxValue);
        }

        public RecordItem(string typeID, IItemDataTyped typedData) : this()
        {
            Type = new ItemType(typeID);
            Data = new ItemData(typedData);
        }

        [XmlElement("thing-id", Order = 1)]
        public ItemKey Key { get; set; }

        [XmlElement("type-id", Order = 2)]
        public ItemType Type { get; set; }

        [XmlElement("thing-state", Order = 3)]
        public string State { get; set; }

        [XmlElement("flags", Order = 4)]
        public int Flags { get; set; }

        [XmlElement("eff-date", Order = 5)]
        public DateTime EffectiveDate { get; set; }

        [XmlElement("created", Order = 6)]
        public Audit Created { get; set; }

        [XmlElement("updated", Order = 7)]
        public Audit Updated { get; set; }

        [XmlElement("data-xml", Order = 8)]
        public ItemData Data
        {
            get { return m_data; }
            set
            {
                m_data = value;
                if (m_data != null)
                {
                    m_data.Item = this;
                }
            }
        }

        [XmlElement("blob-payload", Order = 9)]
        public ItemDataBlob BlobData
        {
            get { return m_blobs; }
            set { m_blobs = value; }
        }

        [XmlElement("updated-end-date", Order = 10)]
        public DateTime UpdatedEndDate { get; set; }

        [XmlIgnore]
        public string ID
        {
            get { return (this.Key != null) ? this.Key.ID : null;}
        }

        [XmlIgnore]
        public bool HasKey
        {
            get { return Key != null; }
        }

        [XmlIgnore]
        public bool HasData
        {
            get { return (m_data != null); }
        }

        [XmlIgnore]
        public bool HasTypedData
        {
            get { return (HasData && Data.HasTyped); }
        }

        [XmlIgnore]
        public bool HasBlobData
        {
            get { return m_blobs != null; }
        }

        [XmlIgnore]
        public bool IsReadOnly
        {
            get { return (Flags & 0x16) != 0; }
        }

        //-----------------------------
        //
        // Convenience methods and properties
        //
        //-----------------------------
        [XmlIgnore]
        public IItemDataTyped TypedData
        {
            get
            {
                if (m_data == null)
                {
                    return null;
                }

                return (IItemDataTyped) m_data.Typed;
            }
            set { EnsureData().Typed = value; }
        }

        [XmlIgnore]
        public ItemDataCommon CommonData
        {
            get
            {
                if (m_data == null)
                {
                    return null;
                }

                return m_data.Common;
            }
            set { EnsureData().Common = value; }
        }

        #region IHealthVaultType Members

        public void Validate()
        {
            Key.ValidateOptional("Key");
            Type.ValidateOptional("Type");
            m_data.ValidateOptional("Data");
            m_blobs.ValidateOptional("BlobData");
        }

        #endregion

        public IAsyncAction RefreshBlobDataAsync(IRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          RecordItem item = await record.GetItemAsync(Key, ItemSectionType.Blobs).AsTask(cancelToken);
                          if (item != null)
                          {
                              m_blobs = item.m_blobs;
                          }
                      });
        }

        public void AddOrUpdateBlob(Blob blob)
        {
            EnsureBlobData().AddOrUpdateBlob(blob);
        }

        public bool IsVersion(string version)
        {
            if (HasKey)
            {
                return Key.IsVersion(version);
            }

            return false;
        }

        public RecordItem ShallowClone()
        {
            var item = new RecordItem();
            item.Key = Key;
            item.Type = Type;
            item.State = State;
            item.Flags = Flags;
            item.EffectiveDate = EffectiveDate;
            item.Created = Created;
            item.Updated = Updated;
            item.m_data = m_data;
            item.m_blobs = m_blobs;

            return item;
        }

        public RecordItem DeepClone()
        {
            string xml = this.Serialize();
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            return RecordItem.Deserialize(xml);
        }

        public RecordItem CloneForEdit()
        {
            return this.DeepClone();
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public static RecordItem Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<RecordItem>(xml);
        }

        public static string SerializeMultiple(IList<RecordItem> items)
        {
            return items.ToXml();
        }

        public static IList<RecordItem> DeserializeMultiple(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<List<RecordItem>>(xml);
        }

        public static IList<IItemDataTyped> DeserializeToTypedItems(string xml)
        {
            IList<RecordItem> recordItems = DeserializeMultiple(xml);
            List<IItemDataTyped> items = recordItems.Select(i => i.TypedData).ToList();
            return items;
        }

        public static string SerializeTypedItems(IList<IItemDataTyped> items)
        {
            List<RecordItem> recordItems = items.Select(i => i.Item).ToList();
            return SerializeMultiple(recordItems);
        }

        internal ItemData EnsureData()
        {
            if (m_data == null)
            {
                m_data = new ItemData();
            }

            return m_data;
        }

        internal ItemDataBlob EnsureBlobData()
        {
            if (m_blobs == null)
            {
                m_blobs = new ItemDataBlob();
            }

            return m_blobs;
        }

        internal DateTime EnsureEffectiveDate()
        {
            if (this.EffectiveDate == null)
            {
                DateTime effDate = null;
                if (this.HasTypedData)
                {
                    effDate = this.TypedData.GetDateForEffectiveDate();
                }
                if (effDate == null)
                {
                    effDate = DateTime.Now();
                }
                this.EffectiveDate = effDate;
            }

            return this.EffectiveDate;
        }

        internal void UpdateEffectiveDate()
        {
            this.EffectiveDate = null;
            this.EnsureEffectiveDate();
        }

        internal KeyValuePair<ItemKey, IItemDataTyped> GetTypedDataWithKey()
        {
            return new KeyValuePair<ItemKey, IItemDataTyped>(Key, TypedData);
        }

        internal void PrepareForUpdate()
        {
            EffectiveDate = null;
            Updated = null;
        }

        internal void PrepareForNew()
        {
            EffectiveDate = null;
            Updated = null;
            Created = null;
            Key = null;
        }

        public bool ShouldSerializeState()
        {
            return !String.IsNullOrEmpty(State);
        }
    }
}