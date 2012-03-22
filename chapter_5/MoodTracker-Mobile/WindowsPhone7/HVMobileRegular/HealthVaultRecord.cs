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
    /// Stores the summary information associated with a record.
    /// </summary>
    public class HealthVaultRecord
    {
        /// <summary>
        /// Initializes a new instance of the HealthVaultRecord class.
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="recordId">The record id.</param>
        internal HealthVaultRecord(Guid personId, Guid recordId)
        {
            PersonId = personId;
            RecordId = recordId;
        }

        /// <summary>
        /// Prevents a default instance of the HealthVaultRecord class from being created.
        /// </summary>
        private HealthVaultRecord()
        {
        }

        /// <summary>
        /// Gets or sets the full XML description of this record...
        /// </summary>
        public string Xml { get; set; }

        /// <summary>
        /// Gets or sets the person id of the person who has access to this record.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who has access to this record.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the record id of this record.
        /// </summary>
        public Guid RecordId { get; set; }

        /// <summary>
        /// Gets or sets the name of this record.
        /// </summary>
        public string RecordName { get; set; }

        /// <summary>
        /// Creates a new instance of the HealthVaultRecord class.
        /// </summary>
        /// <param name="personId">The id of the person.</param>
        /// <param name="personName">The name of the person.</param>
        /// <param name="recordXml">The full XML describing this record.</param>
        /// <returns>An instance of the HealthVaultRecord class.</returns>
        public static HealthVaultRecord Create(Guid personId, string personName, string recordXml)
        {
            HealthVaultRecord record = new HealthVaultRecord();

            record.PersonId = personId;
            record.PersonName = personName;
            record.Xml = recordXml;

            XElement recordNode = XElement.Parse(recordXml);

            record.RecordId = new Guid(recordNode.Attribute("id").Value);
            record.RecordName = recordNode.Value;

            // if there are any auth issues, we don't keep this record...
            string appRecordAuthAction = recordNode.Attribute("app-record-auth-action").Value;
            if (appRecordAuthAction != "NoActionRequired")
            {
                record = null;
            }

            return record;
        }
    }
}
