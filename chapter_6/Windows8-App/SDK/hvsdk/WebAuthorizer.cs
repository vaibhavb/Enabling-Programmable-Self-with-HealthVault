// (c) Microsoft. All rights reserved
using System;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.System;

namespace HealthVault.Foundation
{
    public class WebAuthorizer : IWebAuthorizer
    {
        #region IWebAuthorizer Members

        public async Task<WebAuthenticationStatus> AuthAsync(string startUrl, string endUrlPrefix)
        {
            // For web auth broker, we can show a mobile UI
            var start = new Uri(startUrl + "&mobile=true");
            var end = new Uri(endUrlPrefix);
            WebAuthenticationResult result =
                await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, start, end);
            return result.ResponseStatus;
        }

        #endregion
    }
    
    public class BrowserWebAuthorizer : IWebAuthorizer
    {
        public async Task<WebAuthenticationStatus> AuthAsync(string startUrl, string endUrlPrefix)
        {
            var start = new Uri(startUrl);
            var result = await Launcher.LaunchUriAsync(start);
            if (!result)
            {
                return WebAuthenticationStatus.ErrorHttp;
            }

            return WebAuthenticationStatus.UserCancel;
        }
    }
}