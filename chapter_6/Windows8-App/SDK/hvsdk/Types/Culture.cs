// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class Culture : IValidatable
    {
        [XmlElement("language")]
        public string Language { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        public void Validate()
        {            
        }
    }
}
