// (c) Microsoft. All rights reserved
using Windows.Security.Credentials;

namespace HealthVault.Foundation
{
    public interface ISecretStore
    {
        string GetSecret(string name);
        void PutSecret(string name, string value);
        void RemoveSecret(string name);
    }

    public class SecretStore : ISecretStore
    {
        private readonly PasswordVault m_store;
        private readonly string m_storeName;

        public SecretStore(string name)
        {
            m_store = new PasswordVault();
            m_storeName = name;
        }

        #region ISecretStore Members

        public string GetSecret(string name)
        {
            PasswordCredential credential = GetCredential(name);
            return (credential != null) ? credential.Password : null;
        }

        public void PutSecret(string name, string value)
        {
            PasswordCredential existingCredential = GetCredential(name);
            if (existingCredential != null)
            {
                m_store.Remove(existingCredential);
            }

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var credential = new PasswordCredential(MakeResourceName(name), m_storeName, value);
            m_store.Add(credential);
        }

        public void RemoveSecret(string name)
        {
            PasswordCredential credential = GetCredential(name);
            if (credential != null)
            {
                m_store.Remove(credential);
            }
        }

        #endregion

        private PasswordCredential GetCredential(string name)
        {
            try
            {
                return m_store.Retrieve(MakeResourceName(name), m_storeName);
            }
            catch
            {
            }

            return null;
        }

        private string MakeResourceName(string name)
        {
            return string.Format("{0}/{1}", m_storeName, name);
        }
    }
}