// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class ServerErrorContext
    {
        [XmlElement("server-name", Order = 1)]
        public string ServerName { get; set; }

        [XmlElement("server-ip", Order = 2)]
        public string ServerIp { get; set; }

        [XmlElement("exception", Order = 3)]
        public string Exception { get; set; }

        public override string ToString()
        {
            return Exception;
        }
    }
}