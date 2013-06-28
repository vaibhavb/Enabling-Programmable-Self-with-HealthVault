// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public partial class SessionCredential : IValidatable
    {
        public SessionCredential()
        {
        }

        [XmlElement("token", Order = 1)]
        public string Token
        {
            get; set;
        }

        [XmlElement("shared-secret", Order = 2)]
        public string SharedSecret
        {
            get; set;
        }

        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return (!string.IsNullOrEmpty(this.Token) && !string.IsNullOrEmpty(this.SharedSecret));
            }
        }

        public void Validate()
        {
           this.Token.ValidateRequired("Token");
           this.SharedSecret.ValidateRequired("SharedSecret");
        }

        const string TokenProperty = "SessionToken";
        const string SecretProperty = "SessionSecret";

        public void Clear()
        {
            this.Token = null;
            this.SharedSecret = null;
        }
    }
}
