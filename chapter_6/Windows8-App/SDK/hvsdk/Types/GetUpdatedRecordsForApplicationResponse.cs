using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class GetUpdatedRecordsForApplicationResponse
    {
        public class UpdatedRecords
        {
            public UpdatedRecords()
            {
            }

            [XmlElement("updated-record")]
            public UpdatedRecord[] Records
            {
                get;
                set;
            }

            [XmlIgnore]
            public bool HasRecords
            {
                get { return !this.Records.IsNullOrEmpty(); }
            }
        }

        [XmlElement("updated-records")]
        public UpdatedRecords Updates
        {
            get; set;
        }
    }
}
