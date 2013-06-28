// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("content")]
    public class HmacContent : IValidatable
    {
        public HmacContent()
        {
        }

        public HmacContent(Guid appID, string macAlgorithm)
        {
            this.AppID = appID.ToString("D");
            this.HmacAlgorithm = macAlgorithm;
            this.SigningTime = DateTime.UtcNow;

            this.Validate();
        }

        [XmlElement("app-id", Order = 1)]
        public string AppID
        {
            get; set;
        }

        [XmlElement("hmac", Order = 2)]
        public string HmacAlgorithm
        {
            get; set;
        }

        [XmlElement("signing-time", Order = 3)]
        public DateTime SigningTime
        {
            get; set;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.AppID))
            {
                throw new ArgumentException("AppID");
            }
            if (string.IsNullOrEmpty(this.HmacAlgorithm))
            {
                throw new ArgumentException("HmacAlgorithm");
            }
        }
    }
}
