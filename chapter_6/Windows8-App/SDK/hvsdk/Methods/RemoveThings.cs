// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Foundation;


namespace HealthVault.Foundation.Methods
{
    public class RemoveThings : HealthVaultMethod
    {
        RecordReference m_record;
        RequestBody m_body;

        public RemoveThings(HealthVaultClient client, string personID, string recordID, RequestBody body)
            : this(client, new RecordReference(personID, recordID), body)
        {
        }

        public RemoveThings(HealthVaultClient client, RecordReference record, RequestBody body)
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

            m_record = record;
            m_body = body;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("RemoveThings", 1, m_body);
            request.Record = m_record;
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, null, cancelToken);
        }
    }
}
