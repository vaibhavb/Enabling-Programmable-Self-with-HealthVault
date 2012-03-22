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
using System.Xml.Linq;

namespace Microsoft.Health.Mobile
{
    /// <summary>
    /// Information that results from a call to a HealthVault method.
    /// </summary>
    public class HealthVaultResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the HealthVaultResponseEventArgs class.
        /// </summary>
        /// <param name="request">The request that is being processed.</param>
        /// <param name="responseXml">The raw response xml from the request.</param>
        /// <param name="response">A deserialized version of the request.</param>
        public HealthVaultResponseEventArgs(
            HealthVaultRequest request,
            string responseXml,
            HealthVaultResponse response)
        {
            Request = request;
            ResponseXml = responseXml;
            Response = response;

            if (responseXml != null)
            {
                XElement element = XElement.Parse(responseXml);

                XElement status = element.Element("status");

                ErrorCode = Int32.Parse(status.Element("code").Value);
            }
        }

        /// <summary>
        /// Gets or sets the raw xml that was returned from the request.
        /// </summary>
        public string ResponseXml { get; set; }

        /// <summary>
        /// Gets or sets the response in a decoded format.
        /// </summary>
        public HealthVaultResponse Response { get; set; }

        /// <summary>
        /// Gets or sets the request that was sent.
        /// </summary>
        public HealthVaultRequest Request { get; set; }

        /// <summary>
        /// Gets a value indicating whether there was an error in processing the request.
        /// </summary>
        public bool HasError
        {
            get { return ErrorText != null; }
        }

        /// <summary>
        /// Gets or sets the error code returned from the operation.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the text of the error that occurred.
        /// </summary>
        /// <remarks>
        /// If the error was returned from the HealthVault service,
        /// additional error information may be found in the <seealso cref="ResponseXml"/> or <seealso cref="Response"/> properties.
        /// </remarks>
        public string ErrorText { get; set; }
    }
}
