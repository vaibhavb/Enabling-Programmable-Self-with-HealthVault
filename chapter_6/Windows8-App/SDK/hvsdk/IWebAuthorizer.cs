// (c) Microsoft. All rights reserved
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace HealthVault.Foundation
{
    public interface IWebAuthorizer
    {
        Task<AuthResult> AuthAsync(string startUrl, string endUrlPrefix);
    }

    public sealed class AuthResult
    {
        public WebAuthenticationStatus Status { get; set; }

        public string ResponseUri { get; set; }

        public AuthResult(WebAuthenticationResult result)
        {
            Status = result.ResponseStatus;
            ResponseUri = result.ResponseData;
        }

        public AuthResult(WebAuthenticationStatus status)
        {
            Status = status;
        }
    }
}