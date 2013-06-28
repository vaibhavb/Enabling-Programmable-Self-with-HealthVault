using HealthVault.Foundation.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class CreateAccountWithTicket : HealthVaultMethod
    {
        private MSATicket m_msaTicket;
        private object m_createAccountPersonInfo;

        public CreateAccountWithTicket(HealthVaultClient client, string msaTicket, object createAccountPersonInfo)
            : base(client)
        {
            if (String.IsNullOrEmpty(msaTicket))
            {
                throw new ArgumentNullException("msaTicket");
            }

            if (createAccountPersonInfo == null)
            {
                throw new ArgumentNullException("createAccountPersonInfo");
            }

            m_msaTicket = new MSATicket(msaTicket);
            m_createAccountPersonInfo = createAccountPersonInfo;
        }

        public override Request CreateRequest()
        {
            Request request = new Request("CreateAccountWithTicket", 1);
            request.IsAnonymous = true;
            request.Header.AppId = this.Client.AppInfo.MasterAppId;

            request.Body.Data = new object[] 
            {
                m_msaTicket,
                m_createAccountPersonInfo
            };

            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, typeof(CreateAccountWithTicketResponse), cancelToken);
        }
    }
}
