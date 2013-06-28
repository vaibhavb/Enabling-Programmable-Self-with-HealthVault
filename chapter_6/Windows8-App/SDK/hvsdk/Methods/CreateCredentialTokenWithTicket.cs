using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation.Methods
{
    public class CreateCredentialTokenWithTicket : HealthVaultMethod
    {
        MSATicket m_msaTicket;

        public CreateCredentialTokenWithTicket(HealthVaultClient client, string msaTicket)
            : this(client, new MSATicket(msaTicket))
        {
        }

        public CreateCredentialTokenWithTicket(HealthVaultClient client, MSATicket msaTicket)
            : base(client)
        {
            if (msaTicket == null)
            {
                throw new ArgumentNullException("msaTicket");
            }

            m_msaTicket = msaTicket;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("CreateCredentialTokenWithTicket", 1, null);
            request.IsAnonymous = false;
            request.ShouldEnsureOnlineToken = false;
            request.Body.Data = m_msaTicket;
            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, typeof(CreateCredentialTokenWithTicketResponse), cancelToken);
        }
    }
}
