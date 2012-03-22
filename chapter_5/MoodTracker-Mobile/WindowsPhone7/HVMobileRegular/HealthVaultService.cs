// Copyright (c) Microsoft Corp.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Microsoft.Health.Mobile
{
    /// <summary>
    /// A class used to communicate with the HealthVault web service.
    /// </summary>
    public class HealthVaultService
    {
        private const int AuthenticatedSessionTokenExpired = 65;
        private const string SettingsVersion = "1";

        /// <summary>
        /// Initializes a new instance of the HealthVaultService class.
        /// </summary>
        /// <remarks>
        /// This class provides an simple way for applications to make
        /// requests to the HealthVault web service.
        /// The master application id must come from the HealthVault Application Configuration
        /// Center, and that application must set to be a SODA application. 
        /// Before requests can be made, the <seealso cref="BeginEnsureAuthenticationCheck"/> method must be called.
        /// </remarks>
        /// <param name="healthServiceUrl">The Url to use to talk to the HealthVault web service.</param>
        /// <param name="shellUrl">The Url to use for the HealthVault Shell.</param>
        /// <param name="masterAppId">The master application id for this application.</param>
        public HealthVaultService(
            string healthServiceUrl,
            string shellUrl,
            Guid masterAppId)
        {
            HealthServiceUrl = healthServiceUrl;
            ShellUrl = shellUrl;
            MasterAppId = masterAppId;

            Language = "en";
            Country = "US";

            Records = new List<HealthVaultRecord>();
        }

        /// <summary>
        /// Gets or sets the Url that is used to talk to the HealthVault Web Service.
        /// </summary>
        public string HealthServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the Url that is used to talk to the HealthVault Shell.
        /// </summary>
        public string ShellUrl { get; set; }

        /// <summary>
        /// Gets or sets the Authorization token that is required to talk to the HealthVault
        /// web service.
        /// </summary>
        /// <remarks>
        /// This property is set automatically.
        /// </remarks>
        public string AuthorizationSessionToken { get; set; }

        /// <summary>
        /// Gets or sets the application shared secret.
        /// </summary>
        /// <remarks>
        /// The shared secret is used to prove identity to the HealthVault service.
        /// The shared secret is obtained automatially.
        /// </remarks>
        public string SharedSecret { get; set; }

        /// <summary>
        /// Gets or sets the session shared secret.
        /// </summary>
        public string SessionSharedSecret { get; set; }

        /// <summary>
        /// Gets or sets the master app id.
        /// </summary>
        /// <remarks>
        /// The master application is predefined by the developer using the Application
        /// Configuration Center tool. 
        /// </remarks>
        public Guid MasterAppId { get; set; }

        /// <summary>
        /// Gets or sets the language that is used for responses.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the country that is used for responses.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Url that is used to talk to the HealthVault Web Service.
        /// </summary>
        /// <remarks>
        /// This application id will be automatically defined when the application is configured.
        /// </remarks>
        public Guid AppIdInstance { get; set; }

        /// <summary>
        /// Gets or sets the application creation token.
        /// </summary>
        /// <remarks>
        /// The application token is returned from the CreateApplicationRequest method, and is
        /// only useful for passing to the HealthVault Shell to start authorization and application
        /// creation process. 
        /// </remarks>
        public string ApplicationCreationToken { get; set; }

        /// <summary>
        /// Gets or sets the person and record that will be used.
        /// </summary>
        public HealthVaultRecord CurrentRecord { get; set; }

        /// <summary>
        /// Gets the list of records that this application is authorized to use.
        /// </summary>
        public List<HealthVaultRecord> Records { get; internal set; }

        /// <summary>
        /// Send a request to the HealthVault web service.
        /// </summary>
        /// <remarks>
        /// This method returns immediately; the results and any error information will be passed to the
        /// completion method stored in the request.
        /// </remarks>
        /// <param name="request">The request to send.</param>
        public virtual void BeginSendRequest(
            HealthVaultRequest request)
        {
            string requestXml = GenerateRequestXml(request);

            WebTransport transport = new WebTransport();
            transport.BeginSendPostRequest(HealthServiceUrl, requestXml, SendRequestCallback, request);
        }

        /// <summary>
        /// Returns the url string to use to provision the application.
        /// </summary>
        /// <returns>The URL string.</returns>
        public string GetApplicationCreationUrl()
        {
            string queryString = String.Format("?appid={0}&appCreationToken={1}&instanceName={2}&ismra=true",
                                            MasterAppId,
                                            Uri.EscapeDataString(ApplicationCreationToken),
                                            Uri.EscapeDataString(MobilePlatform.DeviceName));

            return String.Format("{0}/redirect.aspx?target=CREATEAPPLICATION&targetqs={1}",
                            ShellUrl,
                            Uri.EscapeDataString(queryString));
        }

        /// <summary>
        /// Returns the url string to use to authorize additional records.
        /// </summary>
        /// <returns>The URL string.</returns>
        public string GetUserAuthorizationUrl()
        {
            string queryString = String.Format("?appid={0}&ismra=true",
                                        AppIdInstance);

            return String.Format("{0}/redirect.aspx?target=APPAUTH&targetqs={1}",
                                ShellUrl,
                                Uri.EscapeDataString(queryString));
        }

        /// <summary>
        /// Start the authentication check.
        /// </summary>
        /// <remarks>
        /// The authentication check is required to be called every time the 
        /// application is initialized. It will perform the proper amount of initialization 
        /// based on the information that was saved.
        /// Applications should call <see cref="LoadsSettings"/> method before calling this 
        /// method, and should call <see cref="SaveSettings"/> in both of the handlers.
        /// </remarks>
        /// <param name="authenticationCompleted">Handler that is called when the authentication process is complete.</param>
        /// <param name="shellAuthRequired">Handler that is called when the application needs to perform authorization.</param>
        public void BeginAuthenticationCheck(
            EventHandler<HealthVaultResponseEventArgs> authenticationCompleted,
            EventHandler<HealthVaultResponseEventArgs> shellAuthRequired)
        {
            Provisioner.BeginAuthenticationCheck(this, authenticationCompleted, shellAuthRequired);
        }

        /// <summary>
        /// Authorize more records.
        /// </summary>
        /// <remarks>
        /// Applications should call <see cref="SaveSettings"/> in both of the handlers.
        /// </remarks>
        /// <param name="authenticationCompleted">The completion handler.</param>
        /// <param name="shellAuthRequired">The handler when authorization is required.</param>
        public void BeginAuthorizeRecords(
            EventHandler<HealthVaultResponseEventArgs> authenticationCompleted,
            EventHandler<HealthVaultResponseEventArgs> shellAuthRequired)
        {
            Provisioner.BeginAuthorizeRecords(this, authenticationCompleted, shellAuthRequired);
        }

        /// <summary>
        /// Saves the current configuration to isolated storage.
        /// </summary>
        /// <param name="name">The filename to use.</param>
        public void SaveSettings(string name)
        {
            XElement settings = new XElement("HealthVaultSettings",
                                        new XElement("Version", SettingsVersion),
                                        new XElement("ApplicationId", AppIdInstance.ToString()),
                                        new XElement("ApplicationCreationToken", ApplicationCreationToken),
                                        new XElement("AuthorizationSessionToken", AuthorizationSessionToken),
                                        new XElement("SharedSecret", SharedSecret),
                                        new XElement("Country", Country),
                                        new XElement("Language", Language),
                                        new XElement("SessionSharedSecret", SessionSharedSecret)
                                        );

            if (CurrentRecord != null)
            {
                settings.Add(new XElement("PersonId", CurrentRecord.PersonId.ToString()),
                             new XElement("RecordId", CurrentRecord.RecordId.ToString()),
                             new XElement("RecordName", CurrentRecord.RecordName));
            }

            MobilePlatform.SaveTextToFile(name + ".xml", settings.ToString());
        }

        /// <summary>
        /// Loads the last-saved configuration from isolated storage.
        /// </summary>
        /// <param name="name">The filename to use.</param>
        public void LoadSettings(string name)
        {
            string settingsXml = MobilePlatform.ReadTextFromFile(name + ".xml");

            if (settingsXml != null)
            {
                XElement settings = XElement.Parse(settingsXml);

                string version = settings.Element("Version").Value;

                string applicationIdString = settings.Element("ApplicationId").Value;
                AppIdInstance = new Guid(applicationIdString);
                AuthorizationSessionToken = settings.Element("AuthorizationSessionToken").Value;
                SharedSecret = settings.Element("SharedSecret").Value;
                Country = settings.Element("Country").Value;
                Language = settings.Element("Language").Value;
                SessionSharedSecret = settings.Element("SessionSharedSecret").Value;

                // Create a temporary current record until we fetch the real one.
                XElement personIdNode = settings.Element("PersonId");
                if (settings.Element("PersonId") != null)
                {
                    Guid personId = new Guid(settings.Element("PersonId").Value);
                    Guid recordId = new Guid(settings.Element("RecordId").Value);

                    CurrentRecord = new HealthVaultRecord(personId, recordId);
                    CurrentRecord.RecordName = settings.Element("RecordName").Value;
                }
            }
        }

        /// <summary>
        /// Returns the full inner xml of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The inner XML.</returns>
        internal static string GetInnerXml(XElement element)
        {
            XmlReader reader = element.CreateReader();
            reader.MoveToContent();
            string infoString = reader.ReadInnerXml();
            return infoString;
        }

        /// <summary>
        /// Returns the full outer xml of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The outer Xml.</returns>
        internal static string GetOuterXml(XElement element)
        {
            XElement outer = new XElement("outer", element);
            return GetInnerXml(outer);
        }

        /// <summary>
        /// Generate the XML for a request.
        /// </summary>
        /// <param name="clientRequest">The request.</param>
        /// <returns>The XML representation.</returns>
        internal string GenerateRequestXml(HealthVaultRequest clientRequest)
        {
            XElement request = XElement.Parse(@"<wc-request:request xmlns:wc-request=""urn:com.microsoft.wc.request"" />");

            XElement header = new XElement("header");
            {
                header.Add(new XElement("method", clientRequest.MethodName));
                header.Add(new XElement("method-version", clientRequest.MethodVersion));

                if (CurrentRecord != null)
                {
                    header.Add(new XElement("record-id", CurrentRecord.RecordId.ToString()));
                }

                if (!String.IsNullOrEmpty(AuthorizationSessionToken))
                {
                    XElement authSession = new XElement("auth-session");
                    authSession.Add(new XElement("auth-token", AuthorizationSessionToken));

                    if (CurrentRecord != null)
                    {
                        authSession.Add(new XElement("offline-person-info",
                                            new XElement("offline-person-id", CurrentRecord.PersonId.ToString())));
                    }

                    header.Add(authSession);
                }
                else
                {
                    if (AppIdInstance == Guid.Empty)
                    {
                        header.Add(new XElement("app-id", MasterAppId.ToString()));
                    }
                    else
                    {
                        header.Add(new XElement("app-id", AppIdInstance.ToString()));
                    }
                }

                header.Add(new XElement("language", Language));
                header.Add(new XElement("country", Country));
                header.Add(new XElement("msg-time", clientRequest.MessageTime.ToUniversalTime().ToString("O")));
                header.Add(new XElement("msg-ttl", "1800"));
                header.Add(new XElement("version", MobilePlatform.PlatformAbbreviationAndVersion));
            }

            XElement info = new XElement("info");
            if (clientRequest.InfoSection != null)
            {
                info = clientRequest.InfoSection;
            }

            if (clientRequest.MethodName != "CreateAuthenticatedSessionToken")
            {
                // if we have an info section, we need to compute the hash of that and put it in the header.
                if (clientRequest.InfoSection != null)
                {
                    string infoString = GetOuterXml(info);
                    header.Add(new XElement("info-hash",
                                    MobilePlatform.ComputeSha256HashAndWrap(infoString)));
                }

                if (!String.IsNullOrEmpty(SessionSharedSecret))
                {
                    byte[] sharedSecretKey = Convert.FromBase64String(SessionSharedSecret);
                    string headerXml = GetOuterXml(header);

                    request.Add(new XElement("auth",
                                    MobilePlatform.ComputeSha256HmacAndWrap(sharedSecretKey, headerXml)));
                }
            }

            request.Add(header);
            request.Add(info);

            string requestString = GetOuterXml(request);

            return requestString;
        }

        /// <summary>
        /// Create the info section for the cast call. 
        /// </summary>
        /// <returns>The infor section.</returns>
        internal XElement CreateCastCallInfoSection()
        {
            XElement content = new XElement("content",
                                            new XElement("app-id", AppIdInstance.ToString()),
                                            new XElement("hmac", "HMACSHA256"),
                                            new XElement("signing-time", DateTime.UtcNow.ToString("O"))
                                        );

            XElement outer = new XElement("outer", content);

            XmlReader reader = outer.CreateReader();
            reader.MoveToContent();
            string s = reader.ReadInnerXml();
            s = HealthVaultService.GetOuterXml(content);

            string hmac = MobilePlatform.ComputeSha256Hmac(Convert.FromBase64String(SharedSecret), s);

            XElement info = new XElement("info",
                                new XElement("auth-info",
                                    new XElement("app-id", AppIdInstance.ToString()),
                                    new XElement("credential",
                                        new XElement("appserver2",
                                            new XElement("hmacSig", hmac,
                                                new XAttribute("algName", "HMACSHA256")
                                            ),
                                            content
                                        )
                                    )
                                )
                            );

            return info;
        }

        /// <summary>
        /// Saves the results of a cast call into the session.
        /// </summary>
        /// <param name="responseXml">The response Xml.</param>
        internal void SaveCastCallResults(string responseXml)
        {
            XElement response = XElement.Parse(responseXml);

            XElement token = response.Descendants("token").Single();
            XElement sessionSharedSecret = response.Descendants("shared-secret").Single();

            AuthorizationSessionToken = token.Value;
            SessionSharedSecret = sessionSharedSecret.Value;
        }

        /// <summary>
        /// Handles the callback after sending a request.
        /// </summary>
        /// <param name="sender">The sender of the notification.</param>
        /// <param name="args">The event args.</param>
        private void SendRequestCallback(object sender, SendPostEventArgs args)
        {
            if (args.ErrorText != null)
            {
                string errorText = "Error in talking to HealthVault: " + args.ErrorText;
                InvokeApplicationResponseCallback(args.HealthVaultRequest, null, null, errorText);
                return;
            }

            HealthVaultResponse response = null;
            try
            {
                response = new HealthVaultResponse(args.ResponseData);
            }
            catch (InvalidOperationException)
            {
                string errorText = "Response was not a valid HealthVault response: " + args.ResponseData;
                InvokeApplicationResponseCallback(args.HealthVaultRequest, null, null, errorText);
                return;
            }

            // The token that is returned from GetAuthenticatedSessionToken has a limited lifetime. When it expires, 
            // we will get an error here. We detect that situation, get a new token, and then re-issue the call. 
            if (response.StatusCode == AuthenticatedSessionTokenExpired)
            {
                RefreshSessionToken(args);
                return;
            }

            InvokeApplicationResponseCallback(args.HealthVaultRequest, args.ResponseData, response, response.ErrorMessage);
        }

        /// <summary>
        /// Invoke the calling application's callback.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="resultText">The raw response.</param>
        /// <param name="response">The response object.</param>
        /// <param name="errorText">The error text, or null if successful. </param>
        private void InvokeApplicationResponseCallback(
            HealthVaultRequest request,
            string resultText,
            HealthVaultResponse response,
            string errorText)
        {
            HealthVaultResponseEventArgs eventArgs = new HealthVaultResponseEventArgs(request, resultText, response);
            eventArgs.ErrorText = errorText;

            if (request.ResponseCallback != null)
            {
                request.ResponseCallback(this, eventArgs);
            }
        }

        /// <summary>
        /// Refresh the session token.
        /// </summary>
        /// <remarks>Makes a CAST call to get a new session token, 
        /// and then re-issues the original request.</remarks>
        /// <param name="args">The request information.</param>
        private void RefreshSessionToken(SendPostEventArgs args)
        {
            AuthorizationSessionToken = null;

            XElement info = CreateCastCallInfoSection();
            HealthVaultRequest request = HealthVaultRequest.Create("CreateAuthenticatedSessionToken", "2", info, RefreshSessionTokenCompleted);
            request.UserState = args;

            BeginSendRequest(request);
        }

        /// <summary>
        /// Processes the CAST information and re-issues the original request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void RefreshSessionTokenCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SendPostEventArgs args = (SendPostEventArgs)e.Request.UserState;

            // any error just gets returned to the application.
            if (e.ErrorText != null)
            {
                InvokeApplicationResponseCallback(args.HealthVaultRequest, args.ResponseData, e.Response, e.Response.ErrorMessage);
                return;
            }

            // if the CAST was successful the results were saved and
            // the original request is restarted. 
            SaveCastCallResults(e.ResponseXml);

            BeginSendRequest(args.HealthVaultRequest);
        }

        /// <summary>
        /// Clean out all the registered information so that we will restart. 
        /// </summary>
        public void ClearProvisioningInformation()
        {
            SessionSharedSecret = null;
            AuthorizationSessionToken = null;
            SharedSecret = null;
            AppIdInstance = Guid.Empty;
            CurrentRecord = null;
        }
    }
}
