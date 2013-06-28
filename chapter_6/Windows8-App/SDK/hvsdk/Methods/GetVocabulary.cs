// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HealthVault.Foundation;

namespace HealthVault.Foundation.Methods
{
    public class GetVocabulary : HealthVaultMethod
    {
        RequestBody m_body;
        Type m_responseType;

        public GetVocabulary(HealthVaultClient client, RequestBody body, Type responseType)
            : base(client)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (responseType == null)
            {
                throw new ArgumentNullException("responseType");
            }

            m_body = body;
            m_responseType = responseType;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("GetVocabulary", 2, m_body);

            if (this.Client.State.Credentials == null || !this.Client.State.Credentials.IsValid)
            {
                request.IsAnonymous = true;
                request.Header.AppId = this.Client.AppInfo.MasterAppId;
            }
            else
            {
                request.Header.AppId = this.Client.State.ProvisioningInfo.AppIdInstance;
            }
            
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}
