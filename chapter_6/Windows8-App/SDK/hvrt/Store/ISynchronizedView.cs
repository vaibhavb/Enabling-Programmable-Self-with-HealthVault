using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.Foundation;
using HealthVault.Foundation;
using HealthVault.Types;
using HealthVault.ItemTypes;

namespace HealthVault.Store
{
    public enum SynchronizedViewReadAheadMode
    {
        Page,
        Sequential,
    }

    public interface ISynchronizedView
    {
        IRecord Record { get; }
        int KeyCount { get;}
        ViewKeyCollection Keys { get; }
        HealthVault.Types.DateTime LastUpdated { get; }

        int ReadAheadChunkSize { get; set; }
        SynchronizedViewReadAheadMode ReadAheadMode { get; set; }

        /// <summary>
        /// The keys for which items are now available
        /// </summary>
        event EventHandler<IList<ItemKey>> ItemsAvailable;
        /// <summary>
        /// Passes the keys which were not found
        /// </summary>
        event EventHandler<IList<ItemKey>> ItemsNotFound;
        /// <summary>
        /// Exception!
        /// </summary>
        event EventHandler<Exception> Error;

        ItemKey KeyAtIndex(int index);

        IAsyncOperation<IItemDataTyped> GetItemAsync(int index);
        IAsyncOperation<IItemDataTyped> GetItemByKeyAsync(ItemKey key);
        IAsyncOperation<IList<IItemDataTyped>> GetItemsAsync(int startAt, int count);

        IAsyncOperation<IItemDataTyped> GetLocalItemAsync(int index);
        IAsyncOperation<IItemDataTyped> GetLocalItemByKeyAsync(ItemKey key);
        IAsyncOperation<IList<ItemKey>> GetKeysForItemsNeedingDownload(int startAt, int count);
         
        IAsyncOperation<IItemDataTyped> EnsureItemAvailableAndGetAsync(int index);
        IAsyncOperation<IItemDataTyped> EnsureItemAvailableAndGetByKeyAsync(ItemKey key);
        IAsyncOperation<IList<IItemDataTyped>> EnsureItemsAvailableAndGetAsync(int startAt, int count);
                
        IAsyncOperation<IList<IItemDataTyped>> SelectAsync(PredicateDelegate predicate);
        IAsyncOperation<IList<ItemKey>> SelectKeysAsync(PredicateDelegate predicate);
        
        bool IsStale(int maxAgeInSeconds);
        IAsyncOperation<bool> SynchronizeAsync();
        
        ItemQuery GetSynchronizationQuery();
        IList<string> GetTypeVersions();
        void UpdateKeys(ViewKeyCollection keys);
    }
}
