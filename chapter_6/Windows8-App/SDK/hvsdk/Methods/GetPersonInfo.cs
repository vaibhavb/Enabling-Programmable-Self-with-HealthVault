// (c) Microsoft. All rights reserved

using HealthVault.Foundation.Types;
using System.Threading;
using System.Threading.Tasks;

namespace HealthVault.Foundation.Methods
{
    public class GetPersonInfo : HealthVaultMethod
    {
        public GetPersonInfo(HealthVaultClient client)
            : base(client)
        {
        }

        public override Request CreateRequest()
        {
            this.Client.VerifyProvisioned();

            Request request = new Request("GetPersonInfo", 1, null);
            request.Header.AppId = this.Client.State.ProvisioningInfo.AppIdInstance;
            return request;
        }

        public override Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();
            return this.Client.ExecuteRequestAsync(request, typeof(GetPersonInfoResponse), cancelToken);
        }
    }
}
