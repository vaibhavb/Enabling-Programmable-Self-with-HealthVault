// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    [XmlType("status")]
    public class ResponseStatus : IValidatable
    {
        [XmlElement("code", Order = 1)]
        public int StatusCode { get; set; }

        [XmlElement("error", Order = 2)]
        public ServerError Error { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get { return (StatusCode != 0 || Error != null); }
        }

        public bool IsStatusCredentialsExpired
        {
            get { return (StatusCode == (int) ServerStatusCode.AuthenticatedSessionTokenExpired); }
        }

        public bool IsStatusInvalidOnlineToken
        {
            get { return (StatusCode == (int)ServerStatusCode.CredentialTokenExpired); }
        }

        public bool IsStatusServerFailure
        {
            get
            {
                return (
                    StatusCode == (int) ServerStatusCode.Failed ||
                        StatusCode == (int) ServerStatusCode.RequestTimedOut
                    );
            }
        }

        public bool IsStatusAccessDenied
        {
            get
            {
                return (
                    StatusCode == (int) ServerStatusCode.AccessDenied ||
                        StatusCode == (int) ServerStatusCode.InvalidApp
                    );
            }
        }

        #region IValidatable Members

        public void Validate()
        {
        }

        #endregion

        public override string ToString()
        {
            if (Error == null)
            {
                return string.Format("StatusCode={0}", StatusCode);
            }

            return string.Format("[StatusCode={0}], {1}", StatusCode, Error);
        }
    }
}