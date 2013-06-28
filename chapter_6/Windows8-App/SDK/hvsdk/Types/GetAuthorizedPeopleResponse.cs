// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class GetAuthorizedPeopleResponse
    {        
        public class ResponseResults
        {
            [XmlElement("person-info", Order = 1)]
            public PersonInfo[] Persons
            {
                get;
                set;
            }

            [XmlElement("more-results", Order = 2)]
            public bool HasMoreResults
            {
                get;
                set;
            }

            [XmlIgnore]
            public bool HasPersons
            {
                get { return !this.Persons.IsNullOrEmpty();}
            }
        }

        [XmlElement("response-results")]
        public ResponseResults Results
        {
            get; set;
        }

        [XmlIgnore]
        public bool HasResults
        {
            get { return (this.Results != null);}
        }
    }
}
