// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using HealthVault.Foundation;

namespace HealthVault.Store
{
    public sealed class FolderObjectStore : IObjectStore
    {
        StorageFolder m_folder;

        public FolderObjectStore(StorageFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }
            m_folder = folder;
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            StorageFile file = await this.GetFileAsync(key); 
            return (file != null);
        }

        public async Task DeleteAllAsync()
        {
            await DeleteFolderRecursivelyAsync(m_folder);
        }

        private async Task DeleteFolderRecursivelyAsync(StorageFolder folder)
        {
            IReadOnlyList<StorageFolder> folders = await folder.GetFoldersAsync();
            if (folders.Count > 0)
            {
                foreach (StorageFolder subFolder in folders)
                {
                    await DeleteFolderRecursivelyAsync(subFolder);
                }
            }

            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            foreach (StorageFile file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public async Task DeleteAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            StorageFile file = await this.GetFileAsync(key); 
            if (file == null)
            {
                return;
            }

            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public async Task<DateTimeOffset> GetUpdateDateAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            StorageFile file = await this.GetFileAsync(key);
            if (file == null)
            {
                return DateTimeOffset.MinValue;
            }

            BasicProperties fileProperties = await file.GetBasicPropertiesAsync();
            return fileProperties.DateModified;
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            try
            {
                Stream stream = await this.OpenReadStreamAsync(key);
                if (stream == null)
                {
                    return null;
                }
                
                using(stream)
                {
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        if (type == typeof(string))
                        {
                            return reader.ReadToEnd();
                        }

                        return HealthVaultClient.Serializer.Deserialize(reader, type, null);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public Task<object> RefreshAndGetAsync(string key, Type type)
        {
            return this.GetAsync(key, type);
        }

        public async Task PutAsync(string key, object value)
        {
            if (value == null)
            {
                await this.DeleteAsync(key);
                return;
            }

            using(Stream stream = await this.OpenWriteStreamAsync(key))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    string stringValue = value as string;
                    if (stringValue != null)
                    {
                        writer.Write(stringValue);
                    }
                    else
                    {
                        HealthVaultClient.Serializer.Serialize(writer, value, null);
                    }
                }
            }
        }

        public async Task<Stream> OpenReadStreamAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            StorageFile file = await this.GetFileAsync(key);
            if (file == null)
            {
                return null;
            }

            return await file.OpenStreamForReadAsync();
        }

        public async Task<Stream> OpenWriteStreamAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            StorageFile file = await m_folder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);
            if (file == null)
            {
                return null;
            }

            return await file.OpenStreamForWriteAsync();
        }

        public async Task<IRandomAccessStreamWithContentType> OpenContentStreamAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            StorageFile file = await this.GetFileAsync(key);
            if (file == null)
            {
                return null;
            }

            return await file.OpenReadAsync();
        }

        public async Task<IList<string>> GetAllKeysAsync()
        {
            IReadOnlyList<IStorageFile> files = await m_folder.GetFilesAsync();
            return (from file in files
                    select file.Name).ToArray();
        }

        public async Task<bool> ChildStoreExistsAsync(string childName)
        {
            StorageFolder child = null;

            try
            {
                child = await m_folder.GetFolderAsync(childName);
            }
            catch
            {
            }
                
            return (child != null);
        }

        public async Task<IObjectStore> CreateChildStoreAsync(string childName)
        {
            StorageFolder folder = await m_folder.CreateFolderAsync(childName, CreationCollisionOption.OpenIfExists);
            return new FolderObjectStore(folder);
        }

        public async Task DeleteChildStoreAsync(string childName)
        {
            StorageFolder folder = await this.GetFolderAsync(childName);
            if (folder != null)
            {
                await folder.DeleteAsync();
            }
        }

        public async Task<IStorageFile> GetStorageFileAsync(string key)
        {
            return await this.GetFileAsync(key);
        }

        public static FolderObjectStore CreateRoot()
        {
            return CreateRoot(ApplicationData.Current.LocalFolder);
        }

        public static FolderObjectStore CreateRoot(StorageFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            return new FolderObjectStore(folder);
        }

        async Task<StorageFile> GetFileAsync(string key)
        {
            StorageFile file = null;
            try
            {
                file = await m_folder.GetFileAsync(key);
            }
            catch
            {
            }

            return file;
        }

        async Task<StorageFolder> GetFolderAsync(string childName)
        {
            try
            {
                return await m_folder.GetFolderAsync(childName);
            }
            catch
            {
            }

            return null;
        }
    }
}
