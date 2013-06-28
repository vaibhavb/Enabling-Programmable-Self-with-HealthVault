using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Store
{  
    internal struct LockData
    {
        public long LockID;  
#if DEBUG
        public DateTimeOffset Timestamp;
#endif
        public bool IsLock(long lockID)
        {
            return (this.LockID == lockID);
        }

        public override string ToString()
        {
#if DEBUG
            return string.Format("LockID={0}, {1}", this.LockID, this.Timestamp.ToString("yyyy-mm-dd MM:hh:ss fff"));
#else
           return string.Format("{0}", this.LockID);
#endif
        }

        public static LockData Create(long lockID)
        {
            LockData rlock = new LockData { LockID = lockID};
#if DEBUG
            rlock.Timestamp = DateTimeOffset.Now;
#endif            
            return rlock;
        }
    }

    /// <summary>
    /// Lightweight lock table for acquiring long running locks on RecordItems
    /// The size of this table is == to the # of extant locks 
    /// </summary>
    public sealed class RecordItemLockTable
    {
        internal const long LockNotAcquired = 0;

        Dictionary<string, LockData> m_locks;
        long m_nextID;

        public RecordItemLockTable()
        {
            m_locks = new Dictionary<string,LockData>();
            m_nextID = LockNotAcquired;
        }

        public int Count
        {
            get { return m_locks.Count;}
        }

        /// <summary>
        /// Get a list of IDs for the items that are locked
        /// </summary>
        public IList<string> GetLockedItems()
        {
            lock(m_locks)
            {
                return m_locks.Keys.ToArray();
            }
        }

        public bool IsItemLocked(string itemID)
        {
            lock(m_locks)
            {
                return m_locks.ContainsKey(itemID);
            }
        }

        public static bool IsValidLockID(long lockID)
        {
            return (lockID > LockNotAcquired);
        }

        public void ValidateLock(string itemID, long lockID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }
            if (lockID == LockNotAcquired)
            {
                throw new ArgumentException("lockID");
            }
            lock(m_locks)
            {
                this.ValidateLock(this.GetLock(itemID), lockID);
            }
        }

        //
        // Returns 0 if lock was not acquired because somebody else already holds it
        // Else returns > 0
        // Does NOT BLOCK
        //
        public long AcquireLock(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }

            lock(m_locks)
            {
                if (this.GetLock(itemID) != null)
                {
                    return LockNotAcquired;
                }

                long lockID = this.NextLockID();
                Debug.Assert(RecordItemLockTable.IsValidLockID(lockID));

                m_locks[itemID] = LockData.Create(lockID);
                return lockID;
            }
        }

        internal RecordItemLock AcquireItemLock(string itemID)
        {
            long lockID = 0;
            try
            {
                lockID = this.AcquireLock(itemID);
                if (!IsValidLockID(lockID))
                {
                    return null;
                }

                RecordItemLock itemLock = new RecordItemLock(this, itemID, lockID);
                lockID = 0;
                return itemLock;
            }
            finally
            {
                if (IsValidLockID(lockID))
                {
                    this.ReleaseLock(itemID, lockID);
                }
            }
        }

        public void ReleaseLock(string itemID, long lockID)
        {
            lock(m_locks)
            {
                LockData? existingLock = this.GetLock(itemID);

                this.ValidateLock(existingLock, lockID);

                m_locks.Remove(itemID);
            }
        }

        internal void SafeReleaseLock(string itemID, long lockID)
        {
            try
            {
                this.ReleaseLock(itemID, lockID);        
            }
            catch
            {
                
            }
        }

        public string GetLockInfo(string itemID)
        {
            lock (m_locks)
            {
                LockData? existingLock = this.GetLock(itemID);
                if (existingLock != null)
                {
                    return existingLock.Value.ToString();
                }
                
                return null;
            }            
        }

        LockData? GetLock(string itemID)
        {
            LockData rLock;

            if (m_locks.TryGetValue(itemID, out rLock))
            {
                return rLock;
            }

            return null;    
        }
        
        void ValidateLock(LockData? rLock, long lockID)
        {
            if (rLock == null)
            {
                throw new StoreException(StoreErrorNumber.ItemNotLocked);
            }
            if (!rLock.Value.IsLock(lockID))
            {
                throw new StoreException(StoreErrorNumber.ItemLockMismatch);
            }
        }

        long NextLockID()
        {
            if (m_nextID == (long.MaxValue - 1))
            {
                m_nextID = LockNotAcquired;
            }

            ++m_nextID;
            return m_nextID;
        }
    }
}
