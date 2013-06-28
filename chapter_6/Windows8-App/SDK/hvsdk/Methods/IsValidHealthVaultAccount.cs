// (c) Microsoft. All rights reserved

using HealthVault.Foundation.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class IsValidHealthVaultAccount : HealthVaultMethod
    {
        MSATicket m_msaTicket;
        private Type m_responseType;

        public IsValidHealthVaultAccount(
            HealthVaultClient client, 
            string msaTicket,
            Type responseType)
            : this(client, new MSATicket(msaTicket), responseType)
        {
        }

        public IsValidHealthVaultAccount(
            HealthVaultClient client,
            MSATicket msaTicket,
            Type responseType)
            : base(client)
        {
            if (msaTicket == null)
            {
                throw new ArgumentNullException("msaTicket");
            }

            if (responseType == null)
            {
                throw new ArgumentNullException("responseType");
            }

            m_msaTicket = msaTicket;
            m_responseType = responseType;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("IsValidHealthVaultAccount", 1, null);
            request.IsAnonymous = true;
            request.Header.AppId = this.Client.AppInfo.MasterAppId;
            request.Body.Data = m_msaTicket;
            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}
