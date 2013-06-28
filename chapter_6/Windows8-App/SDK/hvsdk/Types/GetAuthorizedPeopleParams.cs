// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("parameters")]
    public class GetAuthorizedPeopleParams : IValidatable
    {
        public GetAuthorizedPeopleParams()
        {
            this.ResultCount = 1;
        }

        [XmlElement("num-results")]
        public int ResultCount
        {
            get; set;
        }

        public void Validate()
        {
            if (this.ResultCount <= 0)
            {
                throw new ArgumentException("ResultCount");
            }
        }
    }
}
