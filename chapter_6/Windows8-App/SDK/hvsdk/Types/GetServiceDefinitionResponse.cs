// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Foundation.Types
{
    public sealed class GetServiceDefinitionResponse
    {
        [XmlElement("platform", Order = 0)]
        public PlatformConfiguration Platform { get; set; }

        [XmlElement("shell", Order = 1)]
        public ShellConfiguration Shell { get; set; }

        [XmlElement("xml-method", Order = 2)]
        public XmlMethod[] XmlMethods { get; set; }

        [XmlElement("common-schema", Order = 3)]
        public string[] CommonSchemas { get; set; }

        [XmlElement("instances", Order = 4)]
        public InstanceList Instances { get; set; }

        [XmlElement("updated-date", Order = 5)]
        public DateTime UpdatedDate { get; set; }

        public string Serialize()
        {
            return this.ToXml();
        }
    }

    public class PlatformConfiguration
    {
        [XmlElement("url", Order = 0)]
        public string Url { get; set; }

        [XmlElement("version", Order = 1)]
        public string Version { get; set; }

        [XmlElement("configuration", Order = 2)]
        public ConfigurationEntry[] ConfigurationEntries { get; set; }
    }

    public class ShellConfiguration
    {
        [XmlElement("url", Order = 0)]
        public string Url { get; set; }

        [XmlElement("redirect-url", Order = 1)]
        public string RedirectUrl { get; set; }

        [XmlElement("redirect-token", Order = 2)]
        public RedirectToken[] RedirectTokenEntries { get; set; }
    }

    public class XmlMethod
    {
        [XmlElement("name", Order = 0)]
        public string Name { get; set; }

        [XmlElement("version", Order = 1)]
        public XmlMethodVersion[] XmlMethods { get; set; }
    }

    public class XmlMethodVersion
    {
        [XmlAttribute("number")]
        public int VersionNumber { get; set; }

        [XmlElement("request-schema-url", Order = 0)]
        public string RequestSchemaUrl { get; set; }

        [XmlElement("response-schema-url", Order = 1)]
        public string ResponseSchemaUrl { get; set; }
    }

    public class ConfigurationEntry
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class RedirectToken
    {
        [XmlElement("token", Order = 0)]
        public string Token { get; set; }

        [XmlElement("description", Order = 1)]
        public string Description { get; set; }

        [XmlElement("querystring-parameters", Order = 2)]
        public string QueryStringParameters { get; set; }
    }

    public class InstanceList
    {
        [XmlAttribute("current-instance-id")]
        public string CurrentInstanceId { get; set; }

        [XmlElement("instance", Order = 0)]
        public Instance[] AllInstances { get; set; }
    }
}