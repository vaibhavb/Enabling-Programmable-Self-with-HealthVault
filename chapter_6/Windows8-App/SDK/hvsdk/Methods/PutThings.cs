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
    public class PutThings : HealthVaultMethod
    {
        RecordReference m_record;
        RequestBody m_body;
        Type m_responseType;

        public PutThings(HealthVaultClient client, string personID, string recordID, RequestBody body, Type responseType)
            : this(client, new RecordReference(personID, recordID), body, responseType)
        {
        }

        public PutThings(HealthVaultClient client, RecordReference record, RequestBody body, Type responseType)
            : base(client)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (responseType == null)
            {
                throw new ArgumentNullException("responseType");
            }
            m_record = record;
            m_body = body;
            m_responseType = responseType;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("PutThings", 2, m_body);
            request.Record = m_record;
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}
