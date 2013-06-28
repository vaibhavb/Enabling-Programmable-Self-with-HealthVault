// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HealthVault.Store
{
    public interface IObjectStore
    {   
        Task<bool> KeyExistsAsync(string key);
        Task<DateTimeOffset> GetUpdateDateAsync(string key);
        Task<IList<string>> GetAllKeysAsync();
        Task DeleteAllAsync();

        Task DeleteAsync(string key);
        Task<object> GetAsync(string key, Type type);
        Task<object> RefreshAndGetAsync(string key, Type type);

        Task PutAsync(string key, object value);

        Task<Stream> OpenReadStreamAsync(string key);
        Task<Stream> OpenWriteStreamAsync(string key);

        Task<IRandomAccessStreamWithContentType> OpenContentStreamAsync(string key);
        
        Task<bool> ChildStoreExistsAsync(string childName);
        Task<IObjectStore> CreateChildStoreAsync(string childName);
        Task DeleteChildStoreAsync(string childName);

        Task<IStorageFile> GetStorageFileAsync(string key);
    }
}
