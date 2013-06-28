using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation.Methods
{
    public class GetUpdatedRecordsForApplication : HealthVaultMethod
    {
        GetUpdatedRecordsForApplicationParams m_params;
        
        public GetUpdatedRecordsForApplication(HealthVaultClient client, DateTimeOffset lastUpdateDate)
            : base(client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            m_params = new GetUpdatedRecordsForApplicationParams() 
            {
                UpdateDate = lastUpdateDate
            };
        }

        public override Request CreateRequest()
        {
            Request request = new Request("GetUpdatedRecordsForApplication", 2);
            request.Body.Data = m_params;
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, typeof(GetUpdatedRecordsForApplicationResponse), cancelToken);
        }
    }
}
