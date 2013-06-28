// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class Culture : IHealthVaultType
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
