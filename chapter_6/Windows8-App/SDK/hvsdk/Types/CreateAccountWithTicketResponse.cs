using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class CreateAccountWithTicketResponse : IValidatable
    {
        public CreateAccountWithTicketResponse()
        {
        }

        [XmlElement("person-id", Order = 1)]
        public string PersonId
        {
            get;
            set;
        }

        public void Validate()
        {
            this.PersonId.ValidateRequired("PersonId");
        }
    }
}