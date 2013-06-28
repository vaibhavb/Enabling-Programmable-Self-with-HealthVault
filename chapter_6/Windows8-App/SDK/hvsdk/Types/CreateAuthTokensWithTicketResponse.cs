// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class CreateAuthTokensWithTicketResponse : IValidatable
    {
        [XmlElement("auth-token", Order = 1)]
        public SessionCredential SessionCredential
        {
            get;
            set;
        }

        [XmlElement("user-auth-token", Order = 2)]
        public string UserAuthToken
        {
            get;
            set;
        }

        [XmlElement("person-info", Order = 3)]
        public PersonInfo PersonInfo
        {
            get;
            set;
        }

        public void Validate()
        {
            this.SessionCredential.ValidateRequired("SessionCredential");
            this.UserAuthToken.ValidateRequired("UserAuthToken");
            this.PersonInfo.ValidateRequired("PersonInfo");
        }
    }
}