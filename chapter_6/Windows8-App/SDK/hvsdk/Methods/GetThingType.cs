using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class GetThingType : HealthVaultMethod
    {
        private readonly RequestBody m_body;
        private readonly Type m_responseType;

        public GetThingType(HealthVaultClient client, RequestBody body, Type responseType)
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
            var request = new Request("GetThingType", 1, m_body);
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = CreateRequest();
            return Client.ExecuteRequestAsync(request, m_responseType, cancelToken);
        }
    }
}