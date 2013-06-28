// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Types;
using HealthVault.ItemTypes;
using HealthVault.Types;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace HealthVault.Foundation
{
    internal class RecordImpl : IRecord
    {
        private readonly string m_personID;
        private readonly Record m_record;
        private readonly RecordReference m_recordRef;

        internal RecordImpl(Record record, string personID)
        {
            m_record = record;
            m_personID = personID;
            m_recordRef = new RecordReference(personID, m_record.ID);
        }

        internal HealthVaultClient Client { get; set; }

        #region IRecord Members

        public string ID
        {
            get { return m_record.ID; }
        }

        public bool IsCustodian
        {
            get { return m_record.IsCustodian; }
        }

        public string PersonID
        {
            get { return m_personID; }
        }

        public string DisplayName
        {
            get { return m_record.DisplayName; }
        }

        public string Name
        {
            get { return m_record.Name; }
        }

        public int RelationshipType
        {
            get { return m_record.RelationshipType; }
        }

        public string Relationship
        {
            get { return m_record.Relationship; }
        }

        public IAsyncOperation<ItemDataTypedList> GetAsync(ItemQuery query)
        {
            query.ValidateRequired("query");
            //
            // Ensure the user is querying for typed information
            //
            if (!query.View.ReturnsTypedData)
            {
                throw new ArgumentException("query");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          ItemQueryResult[] results = await ExecuteQueriesAsync(cancelToken, new[] {query});
                          if (results.IsNullOrEmpty())
                          {
                              return null;
                          }

                          ItemQueryResult result = results[0];
                          return new ItemDataTypedList(this, query.View, result.Items, result.PendingKeys);
                      });
        }

        public IAsyncOperation<RecordItem> GetItemAsync(ItemKey key, ItemSectionType sections)
        {
            return GetItemAsync(key, null, sections);
        }

        public IAsyncOperation<RecordItem> GetItemAsync(
            ItemKey key,
            string versionType,
            ItemSectionType sections)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            ItemQuery query = ItemQuery.QueryForKey(key);
            query.View = new ItemView(sections);

            if (!String.IsNullOrEmpty(versionType))
            {
                query.View.TypeVersions.Add(versionType);
            }

            return AsyncInfo.Run(
                async cancelToken =>
                {
                    ItemQueryResult result = await GetItemsAsync(query).AsTask(cancelToken);
                    return result.FirstItem;
                }
                );
        }

        public IAsyncOperation<ItemQueryResult> GetItemsAsync(ItemQuery query)
        {
            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          ItemQueryResult[] results = await ExecuteQueriesAsync(cancelToken, new[] {query});
                          return (results.IsNullOrEmpty()) ? null : results[0];
                      });
        }

        public IAsyncOperation<IList<RecordItem>> GetAllItemsAsync(ItemQuery query)
        {
            return AsyncInfo.Run(cancelToken => GetAllItemsAsync(cancelToken, query));
        }

        public IAsyncOperation<IList<ItemQueryResult>> ExecuteQueriesAsync(IList<ItemQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries");
            }

            return
                AsyncInfo.Run<IList<ItemQueryResult>>(
                    async cancelToken => { return await ExecuteQueriesAsync(cancelToken, queries.ToArray()); });
        }

        public IAsyncOperation<IList<ItemKey>> PutRawAsync(string xml)
        {
            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          IList<ItemKey> keys = await ExecutePutThingsRawAsync(cancelToken, xml);
                          return keys;
                      }
                );
        }

        public IAsyncOperation<IList<ItemKey>> NewRawAsync(string xml)
        {
            // For new items there should not be any item keys,
            // created, or updated dates because these will be new items
            var loadSettings = new XmlLoadSettings {ProhibitDtd = true};
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml, loadSettings);

            // Do not convert these to a foreach loop since the collection 
            // is modified by the remove
            XmlNodeList nodes = xmlDoc.GetElementsByTagName("thing-id");
            for (int i = (int) nodes.Length - 1; i >= 0; i--)
            {
                IXmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            nodes = xmlDoc.GetElementsByTagName("updated");
            for (int i = (int) nodes.Length - 1; i >= 0; i--)
            {
                IXmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            nodes = xmlDoc.GetElementsByTagName("created");
            for (int i = (int) nodes.Length - 1; i >= 0; i--)
            {
                IXmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            // Our put things xml does not support <things> or <ArrayOfThings>
            // We just want to serialize <thing> repeatedly
            nodes = xmlDoc.GetElementsByTagName("thing");
            var parsedXml = new StringBuilder();
            for (int i = 0; i < nodes.Length; i++)
            {
                parsedXml.Append(nodes[i].GetXml());
            }

            return PutRawAsync(parsedXml.ToString());
        }

        public IAsyncOperation<ItemKey> PutAsync(IItemDataTyped data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return PutItemAsync(data.Item);
        }

        public IAsyncOperation<IList<ItemKey>> PutMultipleAsync(IList<IItemDataTyped> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            var items = new RecordItem[data.Count];
            for (int i = 0, count = data.Count; i < count; ++i)
            {
                items[0] = data[i].Item;
            }

            return PutItemsAsync(items);
        }

        public IAsyncOperation<ItemKey> PutItemAsync(RecordItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          IList<ItemKey> keys = await ExecutePutThingsAsync(cancelToken, new[] {item});
                          return (keys != null) ? keys[0] : null;
                      }
                );
        }

        public IAsyncOperation<IList<ItemKey>> PutItemsAsync(IList<RecordItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            return AsyncInfo.Run(cancelToken => ExecutePutThingsAsync(cancelToken, items.ToArray()));
        }

        public IAsyncOperation<ItemKey> UpdateAsync(IItemDataTyped data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            data.Item.PrepareForUpdate();
            return PutAsync(data);
        }

        public IAsyncOperation<IList<ItemKey>> UpdateMultipleAsync(IList<IItemDataTyped> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            for (int i = 0, count = data.Count; i < count; ++i)
            {
                data[i].Item.PrepareForUpdate();
            }

            return PutMultipleAsync(data);
        }

        public IAsyncOperation<ItemKey> NewAsync(IItemDataTyped data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            data.Item.PrepareForNew();
            return PutAsync(data);
        }

        public IAsyncOperation<IList<ItemKey>> NewMultipleAsync(IList<IItemDataTyped> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            for (int i = 0, count = data.Count; i < count; ++i)
            {
                data[i].Item.PrepareForNew();
            }

            return PutMultipleAsync(data);
        }

        public IAsyncAction RemoveAsync(ItemKey key)
        {
            return RemoveMultipleAsync(new[] {key});
        }

        public IAsyncAction RemoveMultipleAsync(IList<ItemKey> keys)
        {
            keys.ValidateRequired("keys");

            return
                AsyncInfo.Run(
                    cancelToken => Client.RecordMethods.RemoveThingsAsync(m_recordRef, keys.ToArray(), cancelToken));
        }

        //-----------------------------------------
        //
        // Keys
        //
        //-----------------------------------------
        public IAsyncOperation<IList<ItemKey>> GetKeysAsync(IList<ItemFilter> filters)
        {
            return GetKeysAsync(filters, -1);
        }

        public IAsyncOperation<IList<ItemKey>> GetKeysAsync(IList<ItemFilter> filters, int maxResults)
        {
            filters.ValidateRequired("filters");

            ItemQuery query = ItemQuery.QueryForKeys(filters, maxResults);
            return AsyncInfo.Run<IList<ItemKey>>(
                async cancelToken =>
                      {
                          ItemQueryResult result = await GetItemsAsync(query);
                          return result.AllKeys.ToArray();
                      });
        }

        public IAsyncOperation<IList<PendingItem>> GetKeysAndDateAsync(IList<ItemFilter> filters, int maxResults)
        {
            filters.ValidateRequired("filter");

            ItemQuery query = ItemQuery.QueryForKeys(filters, maxResults);
            return AsyncInfo.Run<IList<PendingItem>>(
                async cancelToken =>
                      {
                          ItemQueryResult result = await GetItemsAsync(query);
                          if (!result.HasPending)
                          {
                              return null;
                          }

                          return result.PendingItems.ToArray();
                      });
        }

        //----------------------------------------
        //
        // BLOBS
        //
        //-----------------------------------------
        public IAsyncAction DownloadBlob(Blob blob, IOutputStream destination)
        {
            blob.ValidateRequired("blob");
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            Stream stream = destination.AsStreamForWrite();
            return AsyncInfo.Run(cancelToken => Client.Streamer.DownloadAsync(new Uri(blob.Url), stream, cancelToken));
        }

        public IAsyncOperation<Blob> UploadBlob(BlobInfo blobInfo, IInputStream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            blobInfo.ValidateRequired("blobInfo");

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          Stream stream = source.AsStreamForRead();
                          Uri blobUri =
                              await
                                  Client.RecordMethods.UploadBlobStreamAsync(
                                      m_recordRef, stream, blobInfo.ContentType, cancelToken);

                          return new Blob(blobInfo, (int) stream.Length, blobUri.AbsoluteUri);
                      }
                );
        }

        public IAsyncOperation<ItemKey> PutBlobInItem(RecordItem item, BlobInfo blobInfo, IInputStream source)
        {
            item.ValidateRequired("item");

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          Blob blob = await UploadBlob(blobInfo, source).AsTask(cancelToken);

                          item.AddOrUpdateBlob(blob);
                          return await UpdateItemAsync(item).AsTask(cancelToken);
                      }
                );
        }

        public IAsyncOperation<ItemKey> UpdateItemAsync(RecordItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            item.PrepareForUpdate();
            return PutItemAsync(item);
        }

        public IAsyncOperation<IList<ItemKey>> UpdateMultipleItemAsync(IList<RecordItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            for (int i = 0, count = items.Count; i < count; ++i)
            {
                items[i].PrepareForUpdate();
            }

            return PutItemsAsync(items);
        }

        public IAsyncOperation<ItemKey> NewItemAsync(RecordItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            item.PrepareForNew();
            return PutItemAsync(item);
        }

        public IAsyncOperation<IList<ItemKey>> NewMultipleItemAsync(IList<RecordItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            for (int i = 0, count = items.Count; i < count; ++i)
            {
                items[i].PrepareForNew();
            }

            return PutItemsAsync(items);
        }

        public IAsyncAction RemoveApplicationRecordAuthorizationAsync()
        {
            return
                AsyncInfo.Run(
                    cancelToken => Client.RecordMethods.RemoveApplicationRecordAuthorizationAsync(m_recordRef, cancelToken));
        }

        public IAsyncOperation<QueryPermissionsResponse> QueryPermissionsAsync(QueryPermissionsRequestParams requestParams)
        {
            if (requestParams == null)
            {
                throw new ArgumentNullException("requestParams");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                {
                    return await Client.RecordMethods.QueryPermissionsAsync<QueryPermissionsResponse>(
                        m_recordRef,
                        requestParams,
                        cancelToken);
                });
        }

        #endregion

        internal async Task<IList<RecordItem>> GetAllItemsAsync(CancellationToken cancelToken, ItemQuery query)
        {
            var itemsFound = new List<RecordItem>();
            while (true)
            {
                ItemQueryResult[] results = await ExecuteQueriesAsync(cancelToken, new[] {query});
                if (results.IsNullOrEmpty())
                {
                    break;
                }

                ItemQueryResult result = results[0];
                if (result.HasItems)
                {
                    itemsFound.AddRange(result.Items);
                }
                if (!result.HasPending)
                {
                    break;
                }
                //
                // Issue a fresh query for pending items
                //
                ItemQuery pendingQuery = ItemQuery.QueryForPending(result.PendingItems);
                pendingQuery.View = query.View;
                query = pendingQuery;
            }

            return itemsFound;
        }

        internal async Task<ItemQueryResult[]> ExecuteQueriesAsync(CancellationToken cancelToken, ItemQuery[] queries)
        {
            queries.ValidateRequired<ItemQuery>("queries");

            RecordQueryResponse queryResponse =
                await Client.RecordMethods.GetThingsAsync<RecordQueryResponse>(m_recordRef, queries, cancelToken);
            return queryResponse.Results;
        }

        internal async Task<IList<ItemKey>> ExecutePutThingsRawAsync(CancellationToken cancelToken, string xml)
        {
            if (String.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException(xml);
            }

            ItemKeyList keyList =
                await Client.RecordMethods.PutThingsRawAsync<ItemKeyList>(m_recordRef, xml, cancelToken);
            if (keyList == null || !keyList.HasKeys)
            {
                return null;
            }

            return keyList.Keys;
        }

        internal async Task<IList<ItemKey>> ExecutePutThingsAsync(CancellationToken cancelToken, RecordItem[] items)
        {
            items.ValidateRequired<RecordItem>("items");

            ItemKeyList keyList =
                await Client.RecordMethods.PutThingsAsync<ItemKeyList>(m_recordRef, items, cancelToken);
            if (keyList == null || !keyList.HasKeys)
            {
                return null;
            }

            return keyList.Keys;
        }

        public override string ToString()
        {
            return m_record.ToString();
        }
    }
}