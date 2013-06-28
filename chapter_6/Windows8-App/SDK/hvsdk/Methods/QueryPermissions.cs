// (c) Microsoft. All rights reserved

using HealthVault.Foundation.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class QueryPermissions : HealthVaultMethod
    {
        private RecordReference m_record;
        private RequestBody m_requestBody;
        private Type m_responseType;

        public QueryPermissions(
            HealthVaultClient client,
            string personID, 
            string recordID,
            RequestBody body,
            Type responseType)
            : this(client, new RecordReference(personID, recordID), body, responseType)
        {
        }

        public QueryPermissions(
            HealthVaultClient client,
            RecordReference record,
            RequestBody body,
            Type responseType)
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
            m_requestBody = body;
            m_responseType = responseType;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("QueryPermissions", 1, m_requestBody);
            request.Record = m_record;
            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}