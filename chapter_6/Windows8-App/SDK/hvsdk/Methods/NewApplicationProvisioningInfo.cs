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
    public class NewApplicationProvisioningInfo : HealthVaultMethod
    {
        public NewApplicationProvisioningInfo(HealthVaultClient client)
            : base(client)
        {
        }

        public override Request CreateRequest()
        {
            Request request = new Request("NewApplicationCreationInfo", 1);
            request.IsAnonymous = true;
            request.Header.AppId = this.Client.AppInfo.MasterAppId;

            return request;
        }

        public override async Task<Response> ExecuteAsync(CancellationToken cancelToken)
        {
            Request request = this.CreateRequest();

            return await this.Client.ExecuteRequestAsync(request, typeof(AppProvisioningInfo), cancelToken);
        }
    }
}
