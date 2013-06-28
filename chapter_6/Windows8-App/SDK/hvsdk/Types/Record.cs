// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Foundation.Types
{
    public class Record : IValidatable
    {
        public Record()
        {
        }
        
        [XmlAttribute("id")]
        public string ID
        {
            get; set;
        }

        [XmlAttribute("record-custodian")]
        public bool IsCustodian
        {
            get; set;
        }

        [XmlAttribute("state")]
        public RecordState State
        {
            get; set;
        }

        [XmlAttribute("display-name")]
        public string DisplayName
        {
            get; set;
        }

        [XmlAttribute("rel-type")]
        public int RelationshipType
        {
            get; set;
        }

        [XmlAttribute("rel-name")]
        public string Relationship
        {
            get; set;
        }

        [XmlText]
        public string Name
        {
            get; set;
        }

        public void Validate()
        {
            this.Name.ValidateRequired("Name");
        }

        public override string ToString()
        {
            return string.Format("[{0}], {1} ({2}), {3}", this.ID, this.DisplayName, this.Name, this.Relationship);
        }
    }
}
