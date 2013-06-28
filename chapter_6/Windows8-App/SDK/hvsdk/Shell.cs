// (c) Microsoft. All rights reserved
using System;
using System.Threading.Tasks;
using HealthVault.Foundation.Types;
using Windows.Security.Authentication.Web;

namespace HealthVault.Foundation
{
    public class Shell
    {
        private const string InstanceQueryParamKey = "instanceid=";

        private readonly HealthVaultClient m_client;
        private string m_authCompletePage;
        private string m_targetPage;

        public Shell(HealthVaultClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            TargetPage = "redirect.aspx";
            AuthCompletePage = "application/complete";

            m_client = client;
        }

        public string Url
        {
            get { return m_client.ServiceInfo.ShellUrl; }
        }

        public string TargetPage
        {
            get { return m_targetPage; }
            set
            {
                value.ValidateRequired("TargetPage");
                m_targetPage = value;
            }
        }

        public string AuthCompletePage
        {
            get { return m_authCompletePage; }
            set
            {
                value.ValidateRequired("AuthCompletePage");
                m_authCompletePage = value;
            }
        }

        public string UrlForTarget(string target, string queryString)
        {
            target.ValidateRequired("target");

            string fullQs = "target=" + target;
            if (!string.IsNullOrEmpty(queryString))
            {
                fullQs = fullQs + "&targetqs=" + Uri.EscapeDataString(queryString);
            }

            var builder = new UriBuilder(Url);
            builder.Path = TargetPage;
            builder.Query = fullQs;

            return builder.Uri.AbsoluteUri;
        }

        public string UrlForAppProvision()
        {
            m_client.VerifyHasProvisioningInfo();

            AppProvisioningInfo provInfo = m_client.State.ProvisioningInfo;
            AppInfo appInfo = m_client.AppInfo;

            string qs = string.Format(
                "appid={0}&appCreationToken={1}&instanceName={2}&ismra=true",
                appInfo.MasterAppId,
                Uri.EscapeDataString(provInfo.AppCreationToken),
                Uri.EscapeDataString(appInfo.InstanceName));

            if (m_client.AppInfo.IsMultiInstanceAware)
            {
                qs += "&aib=true";
            }

            return UrlForTarget(Targets.CreateApplication, qs);
        }

        public string UrlForAppAuthSuccess()
        {
            m_client.VerifyHasProvisioningInfo();
            
            string qs = string.Format(
                "appid={0}&target={1}",
                m_client.State.ProvisioningInfo.AppIdInstance,
                Targets.AppAuthSuccess);

            var builder = new UriBuilder(Url);
            builder.Path = AuthCompletePage;
            builder.Query = qs;

            return builder.Uri.AbsoluteUri;
        }

        public string CallbackUriForWebAppAuth()
        {
            m_client.VerifyHasProvisioningInfo();

            // call back uri for completion of webauthbroker as per shell 
            // is considered for all the redirect urls
            // with target starting with 'AppAuth', currently -
            // AppAuthReject, AppAuthSuccess, AppAuthFailure and
            // AppAuthInvalidRecord
            var builder = new UriBuilder(Url);
            builder.Path = AuthCompletePage;

            return builder.Uri.AbsoluteUri;
        }

        public string UrlForAppAuth()
        {
            m_client.VerifyProvisioned();

            string qs = string.Format("appid={0}&ismra=true", m_client.State.ProvisioningInfo.AppIdInstance);

            if (m_client.AppInfo.IsMultiInstanceAware)
            {
                qs += "&aib=true";
            }

            return UrlForTarget(Targets.AppAuth, qs);
        }

        public async Task<AuthResult> ProvisionApplicationAsync()
        {
            string authUrl = UrlForAppProvision();
            string callbackUriForWebAppAuth = CallbackUriForWebAppAuth();

            return await m_client.WebAuthorizer.AuthAsync(authUrl, callbackUriForWebAppAuth);
        }

        public async Task<AuthResult> AppAuthAsync()
        {
            string authUrl = UrlForAppAuth();
            string callbackUriForWebAppAuth = CallbackUriForWebAppAuth();

            return await m_client.WebAuthorizer.AuthAsync(authUrl, callbackUriForWebAppAuth);
        }

        public string ParseInstanceIdFromUri(string uri)
        {
            int instanceStartIndex = uri.IndexOf(InstanceQueryParamKey, StringComparison.OrdinalIgnoreCase);

            if (instanceStartIndex >= 0)
            {
                string instanceSubstring = uri.Substring(instanceStartIndex + InstanceQueryParamKey.Length);
                int instanceEndIndex = instanceSubstring.IndexOf("&");
                if (instanceEndIndex > 0)
                {
                    return instanceSubstring.Substring(0, instanceEndIndex);
                }
                else
                {
                    return instanceSubstring;
                }
            }

            return null;
        }

        #region Nested type: Targets

        public static class Targets
        {
            public const string CreateApplication = "CREATEAPPLICATION";
            public const string AppAuth = "APPAUTH";
            public const string AppAuthSuccess = "AppAuthSuccess";
        }

        #endregion
    }
}