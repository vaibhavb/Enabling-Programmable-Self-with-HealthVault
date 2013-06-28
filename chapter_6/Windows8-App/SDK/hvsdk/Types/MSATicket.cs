// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("msa-ticket")]
    public class MSATicket : IValidatable
    {
        public MSATicket()
        { 
        }

        public MSATicket(string value)
        {
            this.Value = value;
        }

        [XmlText]
        public string Value
        {
            get; set;
        }

        public void Validate()
        {
            this.Value.ValidateRequired("MSATicketValue");
        }
    }
}
