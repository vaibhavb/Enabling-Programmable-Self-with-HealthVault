// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("auth-info")]
    public class CASTRequestParams : IValidatable
    {
        public CASTRequestParams()
        {
        }

        public CASTRequestParams(AppProvisioningInfo provInfo, ICryptographer cryptographer)
        {            
            provInfo.ValidateRequired("provInfo");
            cryptographer.ValidateRequired("cryptographer");
            //
            // To create a session, we prove our possesion of the app shared secret
            // We do this by creating an HMAC of some HmacContent
            //
            HmacContent content = new HmacContent(provInfo.AppIdInstance, cryptographer.HmacAlgorithm);
            string xmlContent = content.ToXml();
            Hmac hmac = cryptographer.Hmac(provInfo.SharedSecret, xmlContent);

            this.AppIDInstance = provInfo.AppIdInstance;
            this.Credential = new CASTCredential(hmac, content);
        }

        [XmlElement("app-id", Order = 1)]
        public Guid AppIDInstance
        {
            get; set;
        }

        [XmlElement("credential", Order = 2)]
        public CASTCredential Credential
        {
            get; set;
        }

        public void Validate()
        {
            this.AppIDInstance.ValidateRequired("AppIDInstance");
            this.Credential.ValidateRequired("Credential");
        }
    }
}
