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
using System.Xml.Linq;

namespace Microsoft.Health.Mobile
{
    /// <summary>
    /// This class encapsulates the data that is contained in a request.
    /// </summary>
    public class HealthVaultRequest
    {
        private static List<WebRequest> _mockRequests;
        private DateTime _messageTime = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the HealthVaultRequest class.
        /// </summary>
        /// <remarks>
        /// The method name and version must be one of the methods documented in the
        /// method reference at:
        /// http://developer.healthvault.com/pages/methods/methods.aspx
        /// </remarks>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="methodVersion">The version of the method.</param>
        /// <param name="infoSection">The request-specific xml to pass.</param>
        /// <param name="responseCallback">The method to call when the request has completed.</param>
        public HealthVaultRequest(
            string methodName,
            string methodVersion,
            XElement infoSection,
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            InfoSection = infoSection;
            ResponseCallback = responseCallback;
            MethodName = methodName;
            MethodVersion = methodVersion;
        }

        /// <summary>
        /// Gets the number of mock requests.
        /// </summary>
        public static int MockRequestCount
        {
            get { return _mockRequests.Count; }
        }

        /// <summary>
        /// Gets or sets the low-level <seealso cref="WebRequest"/> instance that 
        /// will be used to make the request.
        /// </summary>
        public WebRequest WebRequest { get; set; }

        /// <summary>
        /// Gets or sets the request-specific information.
        /// </summary>
        public XElement InfoSection { get; set; }

        /// <summary>
        /// Gets or sets the callback that will be called when the 
        /// request has completed.
        /// </summary>
        public EventHandler<HealthVaultResponseEventArgs> ResponseCallback { get; set; }

        /// <summary>
        /// Gets or sets the name of the method to be called.
        /// </summary>
        /// <remarks>
        /// The method name and version must be one of the methods documented in the
        /// method reference at:
        /// http://developer.healthvault.com/pages/methods/methods.aspx
        /// </remarks>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the version of the method to be called.
        /// </summary>
        /// <remarks>
        /// The method name and version must be one of the methods documented in the
        /// method reference at:
        /// http://developer.healthvault.com/pages/methods/methods.aspx
        /// </remarks>
        public string MethodVersion { get; set; }

        /// <summary>
        /// Gets or sets the response from the method.
        /// </summary>
        public HealthVaultResponse Response { get; set; }

        /// <summary>
        /// Gets or sets the time of the messsage.
        /// </summary>
        public DateTime MessageTime
        {
            get { return _messageTime; }
            set { _messageTime = value; }
        }

        /// <summary>
        /// Gets or sets the user state
        /// </summary>
        /// <remarks>
        /// User state can be used by the caller
        /// to pass state to the handler.
        /// </remarks>
        public object UserState { get; set; }

        /// <summary>
        /// Enables mock requests for this request.
        /// </summary>
        /// <param name="mockRequests">The mock requests.</param>
        public static void EnableMocks(params WebRequest[] mockRequests)
        {
            _mockRequests = new List<WebRequest>(mockRequests);
        }

        /// <summary>
        /// Return an instance of the class.
        /// </summary>
        /// <remarks>
        /// Called when HealthVaultService needs to create an instance; handles mocking through
        /// <see cref="EnableMocks"/>.
        /// </remarks>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="methodVersion">The version of the method.</param>
        /// <param name="infoSection">The request-specific xml to pass.</param>
        /// <param name="responseCallback">The method to call when the request has completed.</param>
        /// <returns>An instance</returns>
        internal static HealthVaultRequest Create(
            string methodName,
            string methodVersion,
            XElement infoSection,
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            HealthVaultRequest request = new HealthVaultRequest(methodName, methodVersion, infoSection, responseCallback);

            if (_mockRequests != null)
            {
                request.WebRequest = _mockRequests[0];
                _mockRequests.RemoveAt(0);
            }

            return request;
        }
    }
}
