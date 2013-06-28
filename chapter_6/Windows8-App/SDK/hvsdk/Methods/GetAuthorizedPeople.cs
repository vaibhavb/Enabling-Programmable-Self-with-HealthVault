// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation.Methods
{
    public class GetAuthorizedPeople : HealthVaultMethod
    {
        public GetAuthorizedPeople(HealthVaultClient client)
            : base(client)
        {
        }

        public override Request CreateRequest()
        {
            this.Client.VerifyProvisioned();

            Request request = new Request("GetAuthorizedPeople", 1, new GetAuthorizedPeopleParams());
            request.Header.AppId = this.Client.State.ProvisioningInfo.AppIdInstance;
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, typeof(GetAuthorizedPeopleResponse), cancelToken);
        }
    }
}
