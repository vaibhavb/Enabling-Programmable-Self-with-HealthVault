// (c) Microsoft. All rights reserved

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class CreateRecord : HealthVaultMethod
    {
        RequestBody m_requestBody;
        Type m_responseType;

        public CreateRecord(HealthVaultClient client, RequestBody requestBody, Type responseType)
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
            Request request = new Request("CreateRecord", 1, m_requestBody);
            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}
