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
    /// Response data from a request to the HealthVault service.
    /// </summary>
    public class HealthVaultResponse
    {
        /// <summary>
        /// Initializes a new instance of the HealthVaultResponse class..
        /// </summary>
        /// <param name="responseXml">The XML input.</param>
        public HealthVaultResponse(string responseXml)
        {
            XElement responseNode = XElement.Parse(responseXml);

            XElement statusNode = responseNode.Element("status");

            StatusCode = Int32.Parse(statusNode.Element("code").Value);

            XElement errorNode = statusNode.Element("error");
            if (errorNode != null)
            {
                ErrorMessage = errorNode.Element("message").Value;

                XElement contextNode = errorNode.Element("context");
                if (contextNode != null)
                {
                    ErrorContextXml = contextNode.ToString();
                }

                XElement errorInfoNode = errorNode.Element("error-info");
                if (errorInfoNode != null)
                {
                    ErrorInfo = errorInfoNode.Value;
                }
            }

            // remove the namespace qualifier from the info element so that the 
            // application doesn't have to specify it...
            foreach (XElement childNode in responseNode.Elements())
            {
                if (childNode.Name.LocalName == "info")
                {
                    childNode.Name = "info";

                    InfoNode = childNode;
                    Info = childNode.ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets numeric status code of the operation.
        /// </summary>
        /// <remarks>
        /// The value 0 indicates success.
        /// </remarks>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error message from the operation.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a contextual xml description of the the error.
        /// </summary>
        public string ErrorContextXml { get; set; }

        /// <summary>
        /// Gets or sets additional information about the error.
        /// </summary>
        public string ErrorInfo { get; set; }

        /// <summary>
        /// Gets or sets the informational part of the response.
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets the informational part of the response. 
        /// </summary>
        public XElement InfoNode { get; set; }
    }
}
