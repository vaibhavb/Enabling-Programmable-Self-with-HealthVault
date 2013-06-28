// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HealthVault.Foundation;
using HealthVault.Foundation.Types;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HealthVault.Store
{
    /// <summary>
    /// Encrypted store. Seamlessly encrypts items stored in the inner Object store
    /// NOTE: only the Get/Put methods encrypt. 
    /// The streaming methods do not. 
    /// </summary>
    public class EncryptedObjectStore : IObjectStore
    {
        private readonly ICryptographer m_cryptographer;
        private readonly string m_encryptionKey;
        private readonly IObjectStore m_inner;

        public EncryptedObjectStore(IObjectStore inner, ICryptographer cryptographer, string encryptionKey)
        {
            m_inner = inner;
            m_cryptographer = cryptographer;
            m_encryptionKey = encryptionKey;
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await m_inner.KeyExistsAsync(key);
        }

        public async Task<DateTimeOffset> GetUpdateDateAsync(string key)
        {
            return await m_inner.GetUpdateDateAsync(key);
        }

        public async Task<IList<string>> GetAllKeysAsync()
        {
            return await m_inner.GetAllKeysAsync();
        }

        public async Task DeleteAllAsync()
        {
            await m_inner.DeleteAllAsync();
        }

        public async Task DeleteAsync(string key)
        {
            await m_inner.DeleteAsync(key);
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            var encrypted = await m_inner.GetAsync(key, typeof(Encrypted)) as Encrypted;

            if (encrypted == null)
            {
                return null;
            }

            try
            {
                string decryptedValue = m_cryptographer.Decrypt(m_encryptionKey, encrypted);
                using (var reader = new StringReader(decryptedValue))
                {
                    if (type == typeof(string))
                    {
                        return reader.ReadToEnd();
                    }

                    return HealthVaultClient.Serializer.Deserialize(reader, type, null);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<object> RefreshAndGetAsync(string key, Type type)
        {
            return await m_inner.RefreshAndGetAsync(key, type);
        }

        public async Task PutAsync(string key, object value)
        {
            if (value == null)
            {
                await DeleteAsync(key);
                return;
            }

            var stringBuilder = new StringBuilder();

            using (var writer = new StringWriter(stringBuilder))
            {
                var stringValue = value as string;
                if (stringValue != null)
                {
                    writer.Write(stringValue);
                }
                else
                {
                    HealthVaultClient.Serializer.Serialize(writer, value, null);
                }
            }

            string stringContent = stringBuilder.ToString();
            Encrypted encrypted = m_cryptographer.Encrypt(m_encryptionKey, stringContent);

            await m_inner.PutAsync(key, encrypted);
        }

        public async Task<Stream> OpenReadStreamAsync(string key)
        {
            return await m_inner.OpenReadStreamAsync(key);
        }

        public async Task<Stream> OpenWriteStreamAsync(string key)
        {
            return await m_inner.OpenWriteStreamAsync(key);
        }

        public async Task<IRandomAccessStreamWithContentType> OpenContentStreamAsync(string key)
        {
            return await m_inner.OpenContentStreamAsync(key);
        }

        public async Task<bool> ChildStoreExistsAsync(string childName)
        {
            return await m_inner.ChildStoreExistsAsync(childName);
        }

        public async Task<IObjectStore> CreateChildStoreAsync(string childName)
        {
            var childStore = await m_inner.CreateChildStoreAsync(childName);
            return new EncryptedObjectStore(childStore, m_cryptographer, m_encryptionKey);
        }

        public async Task DeleteChildStoreAsync(string childName)
        {
            await m_inner.DeleteChildStoreAsync(childName);
        }

        public async Task<IStorageFile> GetStorageFileAsync(string key)
        {
            return await m_inner.GetStorageFileAsync(key);
        }
    }
}