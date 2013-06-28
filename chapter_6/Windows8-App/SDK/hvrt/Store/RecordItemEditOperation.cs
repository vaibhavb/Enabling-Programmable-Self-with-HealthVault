using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using HealthVault.Foundation;
using HealthVault.Types;
using HealthVault.ItemTypes;
using Windows.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// This class manages the operations necessary to edit a RecordItem:
    ///  - Automatically Retain and Release the RecordItemLock for a RecordItem
    ///  - Commit OR Cancel an edit
    ///  - Clone the underlying object being edited, so as to not edit/overwrite existing or cached items until Commit is called
    /// </summary>
    public sealed class RecordItemEditOperation : IDisposable
    {
        RecordItemLock m_rLock;
        IItemDataTyped m_data;
        SynchronizedType m_sType;

        internal RecordItemEditOperation(SynchronizedType sType, IItemDataTyped data, RecordItemLock rLock)
        {
            m_sType = sType;
            m_rLock = rLock;
            m_data = data.Item.DeepClone().TypedData;
        }

        ~RecordItemEditOperation()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// You can alter the properties of this object safely
        /// </summary>
        public IItemDataTyped Data
        {
            get { return m_data;}
        }

        /// <summary>
        ///  - Commits your edits to the store, and register the change for committing to HealthVault
        ///  - Requests an immediate commit to HV
        /// </summary>
        public IAsyncAction CommitAsync()
        {
            if (m_rLock == null)
            {
                throw new StoreException(StoreErrorNumber.ItemNotLocked);
            }

            return AsyncInfo.Run(async cancelToken => {
                
                await m_sType.PutAsync(m_data, m_rLock); // This will write to the local store AND update the change log
                this.ReleaseLock();

                m_sType.StartCommitChanges();   // Request a commit.
            });
        }
        
        /// <summary>
        /// Cancel the edit. This will release the lock on the RecordItem
        /// </summary>
        public void Cancel()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        void Dispose(bool fromDispose)
        {
            if (fromDispose)
            {
                this.ReleaseLock();
                GC.SuppressFinalize(this);
            }

            m_rLock = null;
        }

        void ReleaseLock()
        {
            if (m_rLock != null)
            {
                m_rLock.Release();
                m_rLock = null;
            }
        }
    }
}
