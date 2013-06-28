// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class HashData : IValidatable
    {
        public HashData()
        {
        }

        public HashData(Hash hash)
        {
            this.Hash = hash;
            this.Validate();
        }

        [XmlElement("hash-data")]
        public Hash Hash
        {
            get; set;
        }

        public void Validate()
        {
            this.Hash.ValidateRequired("Hash");
        }
    }
}
