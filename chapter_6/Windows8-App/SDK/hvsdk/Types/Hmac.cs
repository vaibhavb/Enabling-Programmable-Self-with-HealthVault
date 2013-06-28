// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    [XmlType("hmac")]
    public sealed class Hmac : IValidatable
    {
        public Hmac()
        {
        }
 
        public Hmac(string algorithm, string value)
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
