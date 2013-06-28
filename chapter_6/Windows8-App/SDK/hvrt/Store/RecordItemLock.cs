using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthVault.Store
{
    /// <summary>
    /// Manages a lock on a RecordItem. 
    /// </summary>
    public sealed class RecordItemLock : IDisposable
    {
        RecordItemLockTable m_lockTable;
        string m_itemID;
        long m_lockID;

        internal RecordItemLock(RecordItemLockTable lockTable, string itemID, long lockID)
        {
            m_lockTable = lockTable;
            m_lockID = lockID;
            m_itemID = itemID;
        }

        ~RecordItemLock()
        {
            this.Dispose(false);
        }

        internal long LockID
        {
            get { return m_lockID;}
        }

        public void Release()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        void Dispose(bool fromDispose)
        {
            if (m_lockID > 0)
            {
                m_lockTable.ReleaseLock(m_itemID, m_lockID);
                m_lockID = 0;
            }

            if (fromDispose)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
