// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HealthVault.Foundation;
using Windows.Security.Authentication.OnlineId;
using Windows.Security.Authentication.Web;
using Windows.System;

namespace HealthVault
{
    public interface IWebAuthorizerProxy
    {
    }

    public sealed class WebAuthorizerFactory
    {
        public static IWebAuthorizerProxy CreateMSABrowserWebAuthorizer()
        {
            return new MSABrowserWebAuthorizer();
        }

        public static IWebAuthorizerProxy CreateWebAuthorizer()
        {
            return new WebAuthorizer();
        }

        public static IWebAuthorizerProxy CreateBrowserWebAuthorizer()
        {
            return new BrowserWebAuthorizer();
        }
    }

    internal class BrowserWebAuthorizer : IWebAuthorizer, IWebAuthorizerProxy
    {
        public async Task<AuthResult> AuthAsync(string startUrl, string endUrlPrefix)
        {
            var start = new Uri(startUrl);
            bool result = await Launcher.LaunchUriAsync(start);
            if (!result)
            {
                return new AuthResult(WebAuthenticationStatus.ErrorHttp);
            }

            return new AuthResult(WebAuthenticationStatus.UserCancel);
        }
    }

    internal class MSABrowserWebAuthorizer : IWebAuthorizer, IWebAuthorizerProxy
    {
        public MSABrowserWebAuthorizer()
            : this(ServiceDefinition.DefaultLiveIdAuthPolicy)
        {
        }

        public MSABrowserWebAuthorizer(string liveIdAuthPolicy)
        {
            if (String.IsNullOrEmpty(liveIdAuthPolicy))
            {
                throw new ArgumentNullException("liveIdAuthPolicy");
            }

            LiveIdAuthPolicy = liveIdAuthPolicy;
        }

        public string LiveIdAuthPolicy { get; set; }

        public async Task<AuthResult> AuthAsync(string startUrl, string endUrlPrefix)
        {
            AuthResult result = new AuthResult(WebAuthenticationStatus.UserCancel);
            Uri start = null;

            OnlineIdServiceTicketRequest[] tickets = new OnlineIdServiceTicketRequest[]
            {
                new OnlineIdServiceTicketRequest(
                    new Uri(startUrl).Host,
                    String.IsNullOrEmpty(LiveIdAuthPolicy)
                        ? ServiceDefinition.DefaultLiveIdAuthPolicy
                        : LiveIdAuthPolicy)
            };

            try
            {
                var onlineIdAuthenticator = new OnlineIdAuthenticator();
                UserIdentity useridentity =
                    await onlineIdAuthenticator.AuthenticateUserAsync(tickets, CredentialPromptType.PromptIfNeeded);

                if (useridentity != null && useridentity.Tickets != null && useridentity.Tickets.Count > 0)
                {
                    OnlineIdServiceTicket ticket = useridentity.Tickets.First();
                    start = new Uri(startUrl + WebUtility.UrlEncode("&" + ticket.Value) + "&mobile=true");
                }
            }
            catch (TaskCanceledException)
            {
                result.Status = WebAuthenticationStatus.UserCancel;
            }
            catch
            {
                start = new Uri(startUrl + "&mobile=true");
            }
            
            if (start != null)
            {
                WebAuthenticationResult webAuthResult = (await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, start, new Uri(endUrlPrefix)));
                result = new AuthResult(webAuthResult);
            }

            return result;
        }
    }

    internal class WebAuthorizer : IWebAuthorizer, IWebAuthorizerProxy
    {
        #region IWebAuthorizer Members

        public async Task<AuthResult> AuthAsync(string startUrl, string endUrlPrefix)
        {
            // For web auth broker, we can show a mobile UI
            var start = new Uri(startUrl + "&mobile=true");
            var end = new Uri(endUrlPrefix);
            WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, start, end);
            return new AuthResult(result);
        }

        #endregion
    }
}