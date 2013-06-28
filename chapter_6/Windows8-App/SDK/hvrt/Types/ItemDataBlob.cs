// (c) Microsoft. All rights reserved

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Serialization;
using HealthVault.Foundation;
using Windows.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemDataBlob : IHealthVaultTypeSerializable
    {
        private BlobCollection m_blobs;

        [XmlElement("blob")]
        public BlobCollection Blobs
        {
            get
            {
                if (m_blobs == null)
                {
                    m_blobs = new BlobCollection();
                }

                return m_blobs;
            }
            set { m_blobs = value; }
        }

        [XmlIgnore]
        public bool HasBlobs
        {
            get { return !m_blobs.IsNullOrEmpty(); }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            m_blobs.ValidateRequired("Blobs");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public void AddOrUpdateBlob(Blob blob)
        {
            if (blob == null)
            {
                throw new ArgumentNullException("blob");
            }

            if (HasBlobs)
            {
                int existingIndex = m_blobs.IndexOfBlobNamed(blob.Name);
                if (existingIndex >= 0)
                {
                    m_blobs.RemoveAt(existingIndex);
                }
            }

            Blobs.Add(blob);
        }

        public Blob DefaultBlob()
        {
            return GetBlobNamed(string.Empty);
        }

        public Blob GetBlobNamed(string name)
        {
            if (!HasBlobs)
            {
                return null;
            }

            int index = m_blobs.IndexOfBlobNamed(name);
            if (index < 0)
            {
                return null;
            }

            return m_blobs[index];
        }

        public static ItemDataBlob Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ItemDataBlob>(xml);
        }

        public static IAsyncOperation<ItemDataBlob> GetBlobDataAsync(IRecord record, ItemKey key)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          RecordItem item = await record.GetItemAsync(key, ItemSectionType.Blobs).AsTask(cancelToken);
                          return (item != null) ? item.BlobData : null;
                      });
        }
    }
}