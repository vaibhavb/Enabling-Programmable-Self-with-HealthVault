using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class GetPersonInfoResponse
    {
        [XmlElement("person-info", Order = 1)]
        public PersonInfo PersonInfo
        {
            get;
            set;
        }
    }
}