// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HealthVault.Foundation.Types
{
    public class BlobPutInfo
    {
        [XmlElement("blob-ref-url", Order = 1)]
        public string Url
        {
            get; set;
        }

        [XmlElement("blob-chunk-size", Order = 2)]
        public int ChunkSize
        {
            get; set;
        }

        [XmlElement("max-blob-size", Order = 3)]
        public int MaxSize
        {
            get; set;
        }

        [XmlElement("blob-hash-algorithm", Order = 4)]
        public string HashAlgorithm
        {
            get; set;
        }

        [XmlElement("blob-hash-parameters", Order = 5)]
        public BlobHashAlgorithmParameters HashParams
        {
            get; set;
        }
    }

    public class BlobHashAlgorithmParameters : IValidatable
    {
        [XmlElement("block-size")]
        public int BlockSize
        {
            get; set;
        }

        public void Validate()
        {
            
        }
    }

}
