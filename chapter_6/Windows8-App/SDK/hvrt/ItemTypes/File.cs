// (c) Microsoft. All rights reserved

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HealthVault.ItemTypes
{
    public sealed class File : IItemDataTyped
    {
        internal const string RootElement = "file";
        internal const string TypeIDString = "bd0403c5-4ae2-4b0e-a8db-1888678e4528";

        private ItemProxy m_itemProxy;
        private String255 m_name;

        public File()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public File(string name, string contentType)
            : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }

            Name = name;
            ContentType = new CodableValue(contentType);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("name", Order = 1)]
        public string Name
        {
            get { return (m_name != null) ? m_name.Value : String.Empty; }
            set { m_name = !String.IsNullOrEmpty(value) ? new String255(value) : null; }
        }

        [XmlElement("size", Order = 2)]
        public long Size { get; set; }

        [XmlElement("content-type", Order = 3)]
        public CodableValue ContentType { get; set; }

        #region IItemDataTyped Members

        [XmlIgnore]
        public ItemType Type
        {
            get { return m_itemProxy.Item.Type; }
        }

        [XmlIgnore]
        public ItemKey Key
        {
            get { return m_itemProxy.Item.Key; }
            set { m_itemProxy.Item.Key = value; }
        }

        [XmlIgnore]
        public RecordItem Item
        {
            get { return m_itemProxy; }
        }

        [XmlIgnore]
        public ItemData ItemData
        {
            get { return m_itemProxy.ItemData; }
            set { m_itemProxy.ItemData = value; }
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            m_name.ValidateRequired("Name");
            ContentType.ValidateRequired("ContentType");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public override string ToString()
        {
            return (Name != null) ? Name : string.Empty;
        }

        public static File Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<File>(xml);
        }

        public IAsyncOperation<bool> Display(IRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          Blob blob = await RefreshAndGetDefaultBlobAsync(record, cancelToken);
                          if (blob == null)
                          {
                              return false;
                          }

                          return await blob.DisplayAsync().AsTask(cancelToken);
                      }
                );
        }

        public IAsyncOperation<bool> DownloadAsync(IRecord record, IOutputStream destination)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          Blob blob = await RefreshAndGetDefaultBlobAsync(record, cancelToken);
                          if (blob == null)
                          {
                              return false;
                          }

                          await blob.DownloadAsync(record, destination).AsTask(cancelToken);
                          return true;
                      }
                );
        }

        public IAsyncOperation<bool> DownloadToFileAsync(IRecord record, StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          using (
                              StorageStreamTransaction transaction =
                                  await file.OpenTransactedWriteAsync().AsTask(cancelToken))
                          {
                              return await DownloadAsync(record, transaction.Stream).AsTask(cancelToken);
                          }
                      }
                );
        }

        public IAsyncAction UploadAsync(IRecord record, string contentType, int size, IInputStream stream)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          Size = size;
                          if (string.IsNullOrEmpty(contentType))
                          {
                              contentType = HttpStreamer.OctetStreamMimeType;
                          }

                          ContentType = new CodableValue(contentType);

                          var blobInfo = new BlobInfo(contentType);
                          await record.PutBlobInItem(Item, blobInfo, stream).AsTask(cancelToken);
                      }
                );
        }

        public IAsyncAction UploadFileAsync(IRecord record, StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                          {
                              if (String.IsNullOrEmpty(Name))
                              {
                                  Name = file.Name;
                              }

                              await
                                  UploadAsync(record, stream.ContentType, (int) stream.Size, stream).AsTask(cancelToken);
                          }
                      });
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }

        private async Task<Blob> RefreshAndGetDefaultBlobAsync(IRecord record, CancellationToken cancelToken)
        {
            await Item.RefreshBlobDataAsync(record).AsTask(cancelToken);
            if (!Item.HasBlobData)
            {
                return null;
            }

            return Item.BlobData.DefaultBlob();
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }
    }
}