// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class HmacCredential : IValidatable
    {
        public HmacCredential()
        {
        }

        public HmacCredential(Hmac hmac, HmacContent content)
        {
            this.Hmac = hmac;
            this.HmacContent  = content;
            this.Validate();
        }

        [XmlElement("hmacSig")]
        public Hmac Hmac
        {
            get; set;
        }

        [XmlElement("content")]
        public HmacContent HmacContent
        {
            get; set;
        }

        public void Validate()
        {
            this.Hmac.ValidateRequired("Hmac");
            this.HmacContent.ValidateRequired("HmacContent");
        }
    }
}
