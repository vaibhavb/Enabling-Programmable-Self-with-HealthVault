using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("update-date")]
    public class GetUpdatedRecordsForApplicationParams : IValidatable
    {
        public GetUpdatedRecordsForApplicationParams()
        {
        }

        [XmlIgnore]
        public DateTimeOffset? UpdateDate
        {
            get; set;
        }

        [XmlText]
        public string UpdateDateString
        {
            get
            {
                
                return (this.UpdateDate != null) ? XmlConvert.ToString(this.UpdateDate.Value) : string.Empty;
            }
            set 
            {
                this.UpdateDate = XmlConvert.ToDateTimeOffset(value);
            }
        }

        public void Validate()
        {
            this.UpdateDate.ValidateRequired("UpdateDate");
        }
    }
}
