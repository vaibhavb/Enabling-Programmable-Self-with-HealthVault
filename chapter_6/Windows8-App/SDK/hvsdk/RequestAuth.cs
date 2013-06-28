// (c) Microsoft. All rights reserved
using System.Xml.Serialization;
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation
{
    public sealed class RequestAuth : IValidatable
    {
        public RequestAuth()
        {
        }

        public RequestAuth(Hmac hmac)
        {
            Hmac = hmac;
            Validate();
        }

        [XmlElement("hmac-data", Order = 1)]
        public Hmac Hmac { get; set; }

        #region IValidatable Members

        public void Validate()
        {
            Hmac.ValidateRequired("Hmac");
        }

        #endregion
    }
}