// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("instance-name")]
    public class AppInstanceName : IValidatable
    {
        public AppInstanceName()
        {
        }

        public AppInstanceName(string value)
        {
            this.Value = value;
        }

        [XmlText]
        public string Value
        {
            get;
            set;
        }

        public void Validate()
        {
            this.Value.ValidateRequired("InstanceName");
        }
    }
}
