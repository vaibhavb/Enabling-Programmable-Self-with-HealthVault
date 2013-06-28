// (c) Microsoft. All rights reserved

using System.Collections.Generic;
using HealthVault.ItemTypes;
using HealthVault.Types;
using Windows.Foundation;
using Windows.Storage.Streams;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation
{
    public interface IRecord
    {
        string ID { get; }
        bool IsCustodian { get; }
        string PersonID { get; }
        string DisplayName { get; }
        string Name { get; }
        int RelationshipType { get; }
        string Relationship { get; }

        //--------------------------------------
        //
        // Standard CRUD operations
        //
        //--------------------------------------
        IAsyncOperation<ItemDataTypedList> GetAsync(ItemQuery query);
        IAsyncOperation<ItemKey> PutAsync(IItemDataTyped data);
        IAsyncOperation<IList<ItemKey>> PutRawAsync(string xml);
        IAsyncOperation<IList<ItemKey>> PutMultipleAsync(IList<IItemDataTyped> data);
        IAsyncOperation<ItemKey> UpdateAsync(IItemDataTyped data);
        IAsyncOperation<IList<ItemKey>> UpdateMultipleAsync(IList<IItemDataTyped> data);
        IAsyncOperation<ItemKey> NewAsync(IItemDataTyped data);
        IAsyncOperation<IList<ItemKey>> NewMultipleAsync(IList<IItemDataTyped> data);
        IAsyncOperation<IList<ItemKey>> NewRawAsync(string xml);
        IAsyncAction RemoveAsync(ItemKey key);
        IAsyncAction RemoveMultipleAsync(IList<ItemKey> keys);

        IAsyncAction DownloadBlob(Blob blob, IOutputStream destination);
        IAsyncOperation<Blob> UploadBlob(BlobInfo blobInfo, IInputStream source);
        IAsyncOperation<ItemKey> PutBlobInItem(RecordItem item, BlobInfo blobInfo, IInputStream source);

        //-----------------------------------
        //
        // Advanced versions of the above
        //
        //------------------------------------
        IAsyncOperation<RecordItem> GetItemAsync(ItemKey key, ItemSectionType sections);
        IAsyncOperation<RecordItem> GetItemAsync(ItemKey key, string versionType, ItemSectionType sections);
        IAsyncOperation<ItemQueryResult> GetItemsAsync(ItemQuery query);
        //
        // GetAllItems returns only when all recordItems AND pending items have been retrieved from HealthVault
        //
        IAsyncOperation<IList<RecordItem>> GetAllItemsAsync(ItemQuery query);
        IAsyncOperation<IList<ItemQueryResult>> ExecuteQueriesAsync(IList<ItemQuery> queries);

        IAsyncOperation<ItemKey> PutItemAsync(RecordItem item);
        IAsyncOperation<IList<ItemKey>> PutItemsAsync(IList<RecordItem> items);

        IAsyncOperation<ItemKey> UpdateItemAsync(RecordItem item);
        IAsyncOperation<IList<ItemKey>> UpdateMultipleItemAsync(IList<RecordItem> items);

        IAsyncOperation<ItemKey> NewItemAsync(RecordItem item);
        IAsyncOperation<IList<ItemKey>> NewMultipleItemAsync(IList<RecordItem> items);
        //-----------------------------------
        //
        // Key retrieval
        //
        //------------------------------------
        IAsyncOperation<IList<ItemKey>> GetKeysAsync(IList<ItemFilter> filters);
        IAsyncOperation<IList<ItemKey>> GetKeysAsync(IList<ItemFilter> filters, int maxResults);
        IAsyncOperation<IList<PendingItem>> GetKeysAndDateAsync(IList<ItemFilter> filters, int maxResults);
        //-----------------------------------
        //
        // Remove Authorization
        //
        //------------------------------------
        IAsyncAction RemoveApplicationRecordAuthorizationAsync();

        IAsyncOperation<QueryPermissionsResponse> QueryPermissionsAsync(
            QueryPermissionsRequestParams requestParams);
    }
}