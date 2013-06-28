// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class RequestXsl
    {
        [XmlAttribute("content-type")]
        public string ContentType { get; set; }

        [XmlText]
        public string Xsl { get; set; }
    }
}