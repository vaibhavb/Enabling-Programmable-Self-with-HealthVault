// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class Location : IValidatable
    {
        [XmlElement("country", Order = 1)]
        public string Country { get; set; }

        [XmlElement("state-province", Order = 2)]
        public string State { get; set; }

        public void Validate()
        {
            Country.ValidateRequired("Country");
            if (!String.IsNullOrEmpty(State))
            {
                State.ValidateRequired("State");
            }
        }
    }
}