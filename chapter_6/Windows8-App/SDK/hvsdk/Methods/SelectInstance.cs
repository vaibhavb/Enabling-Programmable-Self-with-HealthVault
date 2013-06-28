// (c) Microsoft. All rights reserved

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class SelectInstance : HealthVaultMethod
    {
        private RequestBody m_requestBody;
        private Type m_responseType;

        public SelectInstance(HealthVaultClient client, RequestBody requestBody, Type responseType) 
            : base(client)
        {
            if (requestBody == null)
            {
                throw new ArgumentNullException("requestBody");
            }

            if (responseType == null)
            {
                throw new ArgumentNullException("responseType");
            }

            m_requestBody = requestBody;
            m_responseType = responseType;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("SelectInstance", 1, null);
            request.Body = m_requestBody;
            request.IsAnonymous = true;
            request.Header.AppId = this.Client.AppInfo.MasterAppId;
    
            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}