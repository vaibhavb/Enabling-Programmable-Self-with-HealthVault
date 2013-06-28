// (c) Microsoft. All rights reserved

using System;
using System.Threading;
using Windows.Foundation;

namespace HealthVault.Foundation
{
    /// <summary>
    /// A mutex-style lock that works in the Windows 8 world, where the thread that
    /// resumes after an async operation may not be the thread that yielded.
    /// </summary>
    public sealed class CrossThreadLock
    {
        private SemaphoreSlim m_lock;

        public CrossThreadLock(bool initiallyOwned)
        {
            if (initiallyOwned)
            {
                m_lock = new SemaphoreSlim(0, 1);
            }
            else
            {
                m_lock = new SemaphoreSlim(1, 1);
            }
        }

        public IAsyncAction WaitAsync()
        {
            return m_lock.WaitAsync().AsAsyncAction();
        }

        public void Release()
        {
            m_lock.Release();
        }
    }
}
