// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation.Methods
{
    public sealed class CreateCredentialTokenWithTicketResponse : IValidatable
    {
        public CreateCredentialTokenWithTicketResponse()
        {
        }

        [XmlElement("token", Order = 1)]
        public string Token
        {
            get;
            set;
        }

        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.Token);
            }
        }

        public void Validate()
        {
            this.Token.ValidateRequired("Token");
        }

        public void Clear()
        {
            this.Token = null;
        }
    }
}
