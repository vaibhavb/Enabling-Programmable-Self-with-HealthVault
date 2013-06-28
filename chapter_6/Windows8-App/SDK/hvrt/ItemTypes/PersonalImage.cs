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
    public sealed class PersonalImage : IItemDataTyped
    {
        internal const string RootElement = "personal-image";
        internal const string TypeIDString = "a5294488-f865-4ce3-92fa-187cd3b58930";

        private ItemProxy m_itemProxy;

        public PersonalImage()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        #region IItemDataTyped

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

        #endregion

        #region IItemDataTyped Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static PersonalImage Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<PersonalImage>(xml);
        }

        public IAsyncOperation<bool> Display(IRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(cancelToken => Task.Run(async () =>
                {
                    Blob blob = await RefreshAndGetDefaultBlobAsync(record, cancelToken);
                    if (blob == null)
                    {
                        return false;
                    }

                    return await blob.DisplayAsync().AsTask(cancelToken);
                }));
        }

        public IAsyncOperation<bool> DownloadAsync(IRecord record, IOutputStream destination)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(cancelToken => Task.Run(async () =>
                {
                    Blob blob = await RefreshAndGetDefaultBlobAsync(record, cancelToken);
                    if (blob == null)
                    {
                        return false;
                    }

                    await blob.DownloadAsync(record, destination).AsTask(cancelToken);
                    return true;
                }));
        }

        public IAsyncOperation<bool> DownloadToFileAsync(IRecord record, StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            return AsyncInfo.Run(cancelToken => Task.Run(async () =>
                {
                    using (
                        StorageStreamTransaction transaction =
                            await file.OpenTransactedWriteAsync().AsTask(cancelToken))
                    {
                        return await DownloadAsync(record, transaction.Stream).AsTask(cancelToken);
                    }
                }));
        }

        public IAsyncAction UploadAsync(IRecord record, string contentType, IInputStream stream)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return AsyncInfo.Run(cancelToken => Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(contentType))
                    {
                        contentType = HttpStreamer.OctetStreamMimeType;
                    }

                    var blobInfo = new BlobInfo(contentType);
                    await record.PutBlobInItem(Item, blobInfo, stream).AsTask(cancelToken);
                }));
        }

        public IAsyncAction UploadFileAsync(IRecord record, StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            return AsyncInfo.Run(cancelToken => Task.Run(async () =>
                {
                    using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                    {
                        await UploadAsync(record, stream.ContentType, stream).AsTask(cancelToken);
                    }
                }));
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
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
    }
}