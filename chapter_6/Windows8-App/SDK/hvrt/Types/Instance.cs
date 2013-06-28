// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class Instance : IHealthVaultType
    {
        public Instance()
        {
            Id = String.Empty;
            Name = String.Empty;
            Description = String.Empty;
            PlatformUrl = String.Empty;
            ShellUrl = String.Empty;
        }

        [XmlElement("id", Order = 1)]
        public string Id { get; set; }

        [XmlElement("name", Order = 2)]
        public string Name { get; set; }

        [XmlElement("description", Order = 3)]
        public string Description { get; set; }

        [XmlElement("platform-url", Order = 4)]
        public string PlatformUrl { get; set; }

        [XmlElement("shell-url", Order = 5)]
        public string ShellUrl { get; set; }

        public void Validate()
        {
            Id.ValidateRequired("Id");
            Name.ValidateRequired("Name");
            Description.ValidateRequired("Description");
            PlatformUrl.ValidateRequired("PlatformUrl");
            ShellUrl.ValidateRequired("ShellUrl");
        }
    }
}