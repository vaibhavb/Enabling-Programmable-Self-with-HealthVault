// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class CASTCredential : IValidatable
    {
        public CASTCredential()
        {
        }

        public CASTCredential(Hmac hmac, HmacContent content)
        {
            this.Hmac = new HmacCredential(hmac, content);
        }

        [XmlElement("appserver2")]
        public HmacCredential Hmac
        {
            get; set;
        }

        public void Validate()
        {
            this.Hmac.ValidateRequired("Hmac");
        }
    }
}
