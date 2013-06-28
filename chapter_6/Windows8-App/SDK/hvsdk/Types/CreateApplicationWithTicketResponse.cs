using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public sealed class CreateApplicationWithTicketResponse : IValidatable
    {
        public CreateApplicationWithTicketResponse()
        {
        }

        [XmlElement("app-id", Order=1)]
        public Guid ApplicationId
        {
            get;
            set;
        }

        [XmlElement("shared-secret", Order=2)]
        public string SharedSecret
        {
            get;
            set;
        }

        public void Validate()
        {
        }
    }
}
