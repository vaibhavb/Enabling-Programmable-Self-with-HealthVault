// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HealthVault.Foundation.Methods
{
    public abstract class HealthVaultMethod
    {
        HealthVaultClient m_client;

        public HealthVaultMethod(HealthVaultClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            m_client = client;
        }

        public HealthVaultClient Client
        {
            get { return m_client;}
        }

        public abstract Request CreateRequest();
        public async Task<Response> ExecuteAsync()
        {
            return await this.ExecuteAsync(CancellationToken.None);
        }

        public abstract Task<Response> ExecuteAsync(CancellationToken cancelToken);
    }
}
