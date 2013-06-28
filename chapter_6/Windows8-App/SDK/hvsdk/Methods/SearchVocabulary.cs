// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation;

namespace HealthVault.Foundation.Methods
{
    public class SearchVocabulary : HealthVaultMethod
    {        
        RequestBody m_body;
        Type m_responseType;

        public SearchVocabulary(HealthVaultClient client, RequestBody body, Type responseType)
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
            Request request = new Request("SearchVocabulary", 1, m_body);
            request.Header.AppId = this.Client.State.ProvisioningInfo.AppIdInstance;
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}
