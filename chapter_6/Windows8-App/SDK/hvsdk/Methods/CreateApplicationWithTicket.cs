// (c) Microsoft. All rights reserved
using HealthVault.Foundation.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    class CreateApplicationWithTicket : HealthVaultMethod
    {
        MSATicket m_msaTicket;
        AppInstanceName m_appInstanceName;

        public CreateApplicationWithTicket(HealthVaultClient client, string msaTicket, string appInstanceName)
            : this(client, new MSATicket(msaTicket), new AppInstanceName(appInstanceName))
        {
        }

        public CreateApplicationWithTicket(HealthVaultClient client, MSATicket msaTicket, AppInstanceName appInstanceName)
            : base(client)
        {
            if (msaTicket == null)
            {
                throw new ArgumentNullException("msaTicket");
            }

            if (appInstanceName == null)
            {
                throw new ArgumentNullException("appInstanceName");
            }
    
            m_msaTicket = msaTicket;
            m_appInstanceName = appInstanceName;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("CreateApplicationWithTicket", 1, null);
            request.IsAnonymous = true;
            request.Header.AppId = this.Client.AppInfo.MasterAppId;
            request.Body.Data = new object[] 
            {
                m_msaTicket,
                m_appInstanceName
            };

            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, typeof(AppProvisioningInfo), cancelToken);
        }
    }
}
