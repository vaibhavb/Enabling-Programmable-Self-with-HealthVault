// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class AppProvisioningInfo : IValidatable
    {
        public AppProvisioningInfo()
        {
        }

        [XmlElement("app-id", Order = 1)]
        public Optional<Guid> AppIdInstance
        {
            get; set;
        }

        [XmlElement("shared-secret", Order = 2)]
        public string SharedSecret
        {
            get; set;
        }

        [XmlElement("app-token", Order = 3)]
        public string AppCreationToken
        {
            get; set;
        }

        [XmlIgnore]
        public bool IsValid
        {
            get 
            { 
                return (this.AppIdInstance != null && 
                        !string.IsNullOrEmpty(this.SharedSecret));
            }
        }

        public void Clear()
        {
            this.AppIdInstance = null;
            this.AppCreationToken = null;
            this.SharedSecret = null;
        }

        public void Validate()
        {
        }
    }
}
