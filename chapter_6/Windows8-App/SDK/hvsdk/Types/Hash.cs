// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public sealed class Hash : IValidatable
    {
        public Hash()
        {
        }

        public Hash(string algorithm, string value)
        {
            this.Algorithm = algorithm;
            this.Value = value;
            this.Validate();
        }

        /// <summary>
        /// Required - must be !NullOrEmpty
        /// </summary>
        [XmlAttribute("algName")]
        public string Algorithm
        {
            get; set;
        }

        /// <summary>
        /// Required - must be !NullOrEmpty
        /// </summary>
        [XmlText]
        public string Value
        {
            get; set;
        }

        public void Validate()
        {
            this.Algorithm.ValidateRequired("Algorithm");
            this.Value.ValidateRequired("Value");
        }
    }
}
