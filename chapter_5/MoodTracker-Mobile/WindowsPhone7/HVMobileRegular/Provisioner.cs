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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Health.Mobile
{
    /// <summary>
    /// The Provisioner class implements the application authorization process.
    /// </summary>
    internal static class Provisioner
    {
        private const int ErrorInvalidApp = 6;

        /// <summary>
        /// Authorize other records.
        /// </summary>
        /// <param name="service">The HealthVaultService instance.</param>
        /// <param name="authenticationCompleted">Handler to call when finished.</param>
        /// <param name="shellAuthRequired">Handler to call for authorization.</param>
        public static void BeginAuthorizeRecords(
            HealthVaultService service,
            EventHandler<HealthVaultResponseEventArgs> authenticationCompleted,
            EventHandler<HealthVaultResponseEventArgs> shellAuthRequired)
        {
            AuthenticationCheckState state = new AuthenticationCheckState(service, authenticationCompleted, shellAuthRequired);

            state.ShellAuthRequiredHandler(null, null);
        }

        /// <summary>
        /// Check that the application is authenticated.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="authenticationCompleted">Completion handler.</param>
        /// <param name="shellAuthRequired">Handler if shell authorization is required.</param>
        public static void BeginAuthenticationCheck(
            HealthVaultService service,
            EventHandler<HealthVaultResponseEventArgs> authenticationCompleted,
            EventHandler<HealthVaultResponseEventArgs> shellAuthRequired)
        {
            AuthenticationCheckState state = new AuthenticationCheckState(service, authenticationCompleted, shellAuthRequired);
            BeginAuthenticationCheck(state);
        }

        /// <summary>
        /// Start the authentication check.
        /// </summary>
        /// <param name="state">The state information.</param>
        private static void BeginAuthenticationCheck(AuthenticationCheckState state)
        {
                // always call GetAuthorizedPeople...
            if (!String.IsNullOrEmpty(state.Service.AuthorizationSessionToken))
            {
                // we have a session token for the app, but we don't know who authorized the app. 
                // We'll call GetAuthorizedPeople
                StartGetAuthorizedPeople(state);
            }
            else if (!String.IsNullOrEmpty(state.Service.SharedSecret))
            {
                // We have a shared secret, but not a session token. We will try a CAST call, which will work if the app
                // is auth'd; if it fails, we'll need to ask the application to do the auth
                StartCastCall(state);
            }
            else
            {
                // We're just starting. Call NewApplicationCreationInfo
                StartNewApplicationCreationInfo(state);
            }
        }

        /// <summary>
        /// Start getting the new application info from the HealthVault platform.
        /// </summary>
        /// <param name="state">The current state.</param>
        private static void StartNewApplicationCreationInfo(AuthenticationCheckState state)
        {
            HealthVaultRequest request = HealthVaultRequest.Create("NewApplicationCreationInfo", "1", null, NewApplicationCreationInfoCompleted);
            request.UserState = state;

            state.Service.BeginSendRequest(request);
        }

        /// <summary>
        /// Save the new application info and continue the process.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void NewApplicationCreationInfoCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            AuthenticationCheckState state = (AuthenticationCheckState)e.Request.UserState;

            if (e.ErrorText != null)
            {
                state.AuthenticationCompletedHandler(state.Service, e);
                return;
            }

            XElement response = XElement.Parse(e.ResponseXml);

            XNamespace responseNamespace = "urn:com.microsoft.wc.methods.response.NewApplicationCreationInfo";
            XElement info = response.Element(responseNamespace + "info");
            state.Service.AppIdInstance = new Guid(info.Element("app-id").Value);
            state.Service.SharedSecret = info.Element("shared-secret").Value;
            state.Service.ApplicationCreationToken = info.Element("app-token").Value;

            state.ShellAuthRequiredHandler(state.Service, e);
        }

        /// <summary>
        /// Start making the CreateAuthenticatedSessionToken call.
        /// </summary>
        /// <param name="state">The current state.</param>
        private static void StartCastCall(AuthenticationCheckState state)
        {
            XElement info = state.Service.CreateCastCallInfoSection();
            HealthVaultRequest request = HealthVaultRequest.Create("CreateAuthenticatedSessionToken", "2", info, CastCallCompleted);
            request.UserState = state;

            state.Service.BeginSendRequest(request);
        }

        /// <summary>
        /// Handle the response from the CAST call and continue the flow.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void CastCallCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            AuthenticationCheckState state = (AuthenticationCheckState)e.Request.UserState;

            if (e.ErrorCode == ErrorInvalidApp)
            {
                state.Service.SharedSecret = null;      // force creation of app from scratch...
                state.Service.AppIdInstance = Guid.Empty;
            }
            else if (e.ErrorText != null)
            {
                state.ShellAuthRequiredHandler(state.Service, e);
                return;
            }
            else
            {
                state.Service.SaveCastCallResults(e.ResponseXml);
            }

            BeginAuthenticationCheck(state);
        }

        /// <summary>
        /// Start getting the list of authorized people.
        /// </summary>
        /// <param name="state">The current state.</param>
        private static void StartGetAuthorizedPeople(AuthenticationCheckState state)
        {
            XElement info = new XElement("info",
                                new XElement("parameters")
                            );

            HealthVaultRequest request = HealthVaultRequest.Create("GetAuthorizedPeople", "1", info, GetAuthorizedPeopleCompleted);
            request.UserState = state;

            state.Service.BeginSendRequest(request);
        }

        /// <summary>
        /// Process the list of authorized people and continue the flow.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void GetAuthorizedPeopleCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            AuthenticationCheckState state = (AuthenticationCheckState)e.Request.UserState;

            if (e.ErrorText != null)
            {
                state.AuthenticationCompletedHandler(state.Service, e);
                return;
            }

            XElement response = XElement.Parse(e.ResponseXml);

            XElement responseResults = response.Descendants("response-results").Single();

            state.Service.Records.Clear();

            foreach (XElement personInfo in responseResults.Elements("person-info"))
            {
                Guid personId = new Guid(personInfo.Element("person-id").Value);
                string personName = personInfo.Element("name").Value;

                // If we loaded our settings, the current record is incomplete. We will try
                // to match it to one that we got back...
                HealthVaultRecord currentRecord = state.Service.CurrentRecord;
                state.Service.CurrentRecord = null;

                foreach (XElement recordNode in personInfo.Elements("record"))
                {
                    HealthVaultRecord record = HealthVaultRecord.Create(personId, personName, HealthVaultService.GetOuterXml(recordNode));
                    if (record != null)
                    {
                        state.Service.Records.Add(record);

                        if ((currentRecord != null) &&
                            (currentRecord.PersonId == record.PersonId) &&
                            (currentRecord.RecordId == record.RecordId))
                        {
                            state.Service.CurrentRecord = record;
                        }
                    }
                }
            }

            if (state.Service.Records.Count != 0)
            {
                // all done
                state.AuthenticationCompletedHandler(state.Service, null);
            }
            else
            {
                // unsuccessful, restart from scratch...
                state.Service.ClearProvisioningInformation();
                BeginAuthenticationCheck(state);
            }
        }

        /// <summary>
        /// The data required to perform the authentication flow.
        /// </summary>
        internal class AuthenticationCheckState
        {
            /// <summary>
            /// Initializes a new instance of the AuthenticationCheckState class.
            /// </summary>
            /// <param name="service">The service.</param>
            /// <param name="authenticationCompleted">Completion handler.</param>
            /// <param name="shellAuthRequired">Authorization required handler.</param>
            public AuthenticationCheckState(
                HealthVaultService service,
                EventHandler<HealthVaultResponseEventArgs> authenticationCompleted,
                EventHandler<HealthVaultResponseEventArgs> shellAuthRequired)
            {
                Service = service;
                AuthenticationCompletedHandler = authenticationCompleted;
                ShellAuthRequiredHandler = shellAuthRequired;
            }

            /// <summary>
            /// Gets or sets the service.
            /// </summary>
            public HealthVaultService Service { get; set; }

            /// <summary>
            /// Gets or sets the authentication completed handler.
            /// </summary>
            public EventHandler<HealthVaultResponseEventArgs> AuthenticationCompletedHandler { get; set; }

            /// <summary>
            /// Gets or sets the Shell Authorization required handler.
            /// </summary>
            public EventHandler<HealthVaultResponseEventArgs> ShellAuthRequiredHandler { get; set; }
        }
    }
}
