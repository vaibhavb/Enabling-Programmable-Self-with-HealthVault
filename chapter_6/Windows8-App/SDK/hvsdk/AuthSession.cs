using System;
// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class AuthSession : IValidatable
    {
        public AuthSession()
        {
        }

        public AuthSession(string token, string personId)
        {
            Token = token;
            Person = new OfflinePersonInfo(personId);
        }

        [XmlElement("auth-token", Order = 1)]
        public string Token { get; set; }

        [XmlElement("user-auth-token", Order = 2)]
        public string OnlineToken { get; set; }

        [XmlElement("offline-person-info", Order = 3)]
        public OfflinePersonInfo Person { get; set; }

        #region IValidatable Members

        public void Validate()
        {
            Token.ValidateRequired("Token");

            if (Person != null && !String.IsNullOrEmpty(OnlineToken))
            {
                throw new ArgumentException("Person");
            }
        }

        #endregion
    }
}