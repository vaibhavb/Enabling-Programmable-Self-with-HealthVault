using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class UpdatedRecord : IValidatable
    {
        public UpdatedRecord()
        {
        }

        [XmlElement("record-id", Order = 1)]
        public string RecordID
        {
            get; set;
        }

        [XmlElement("update-date", Order = 2)]
        public DateTimeOffset UpdateDate
        {
            get; set;
        }

        [XmlElement("person-id", Order = 3)]
        public string PersonID
        {
            get; set;
        }

        public void Validate()
        {
            this.RecordID.ValidateRequired("RecordID");
            this.PersonID.ValidateRequired("PersonID");
        }
    }
}
