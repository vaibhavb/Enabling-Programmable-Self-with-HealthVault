// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Store
{
    public delegate void PendingGetCompletionDelegate(object sender, PendingGetResult result);

    public sealed class PendingGetResult
    {
        private IList<ItemKey> m_keysFound;
        private IList<ItemKey> m_keysNotFound;

        internal PendingGetResult()
        {
        }

        internal PendingGetResult(IList<ItemKey> keysRequested)
        {
            KeysRequested = keysRequested;
            m_keysFound = keysRequested;
            m_keysNotFound = null;
        }

        public IList<ItemKey> KeysRequested { get; internal set; }

        /// <summary>
        /// Note: PendingKeys.Count CAN be > Items.Count
        /// </summary>
        public IList<ItemKey> KeysFound
        {
            get
            {
                EnsureSuccess();
                return m_keysFound;
            }
            internal set { m_keysFound = value; }
        }

        public IList<ItemKey> KeysNotFound
        {
            get
            {
                if (m_keysNotFound == null)
                {
                    m_keysNotFound = DetermineKeysNotFound();
                }

                return m_keysNotFound;
            }
        }

        public bool HasError
        {
            get { return (Exception != null); }
        }

        public bool HasKeysFound
        {
            get { return !KeysFound.IsNullOrEmpty(); }
        }

        internal Exception Exception { get; set; }

        public void EnsureSuccess()
        {
            if (Exception != null)
            {
                throw Exception;
            }
        }

        private IList<ItemKey> DetermineKeysNotFound()
        {
            IList<ItemKey> keysFound = KeysFound;
            if (keysFound.IsNullOrEmpty())
            {
                return KeysRequested;
            }

            if (keysFound.Count == KeysRequested.Count)
            {
                return null;
            }

            ILookup<string, ItemKey> foundKeysLookupTable = keysFound.ToLookup(key => key.ID);
            return (
                from key in KeysRequested
                where !(foundKeysLookupTable.Contains(key.ID))
                select key
                ).ToArray();
        }
    }
}