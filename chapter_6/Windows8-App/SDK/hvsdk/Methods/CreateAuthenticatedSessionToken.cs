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
    public class CreateAuthenticatedSessionToken : HealthVaultMethod
    {
        public CreateAuthenticatedSessionToken(HealthVaultClient client)
            : base(client)
        {
        }

        public override Request CreateRequest()
        {
            this.Client.VerifyHasProvisioningInfo();

            AppProvisioningInfo provInfo = this.Client.State.ProvisioningInfo;

            Request request = new Request("CreateAuthenticatedSessionToken", 2, null);
            request.IsAnonymous = true;
            request.Header.AppId = provInfo.AppIdInstance;
            request.Body.Data = new CASTRequestParams(provInfo, this.Client.Cryptographer);

            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return await this.Client.ExecuteRequestAsync(request, typeof(SessionCredential), cancelToken);
        }
    }
}
