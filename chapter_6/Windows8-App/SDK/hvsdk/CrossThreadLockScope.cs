// (c) Microsoft. All rights reserved

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace HealthVault.Foundation
{
    /// <summary>
    /// Manages a single use of CrossThreadLock, acquiring the lock and releasing it
    /// when no longer needed.
    /// </summary>
    public struct CrossThreadLockScope : IDisposable
    {
        private CrossThreadLock m_lock;

        public static IAsyncOperation<CrossThreadLockScope> Enter(CrossThreadLock ctLock)
        {
            return AsyncInfo.Run(
                async cancelToken =>
                {
                    await ctLock.WaitAsync();
                    return new CrossThreadLockScope(ctLock);
                });
        }

        private CrossThreadLockScope(CrossThreadLock ctLock)
        {
            m_lock = ctLock;
        }

        public void Dispose()
        {
            m_lock.Release();
        }

    }
}
