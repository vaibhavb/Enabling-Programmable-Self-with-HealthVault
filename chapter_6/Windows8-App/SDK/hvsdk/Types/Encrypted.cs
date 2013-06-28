// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using Windows.Security.Cryptography;

namespace HealthVault.Foundation.Types
{
    public sealed class Encrypted : IValidatable
    {
        public Encrypted()
        {
        }

        public Encrypted(string algorithm, BinaryStringEncoding encoding, string value)
        {
            Algorithm = algorithm;
            Value = value;
            Encoding = encoding;
            Validate();
        }

        /// <summary>
        ///     Required - must be !NullOrEmpty
        /// </summary>
        [XmlAttribute("algName")]
        public string Algorithm { get; set; }

        /// <summary>
        ///     Required - must be !NullOrEmpty
        /// </summary>
        [XmlText]
        public string Value { get; set; }

        /// <summary>
        ///     Required - must be !NullOrEmpty
        /// </summary>
        [XmlAttribute("encodingType")]
        public BinaryStringEncoding Encoding { get; set; }

        public void Validate()
        {
            Algorithm.ValidateRequired("Algorithm");
            Value.ValidateRequired("Value");
            Encoding.ValidateRequired("Encoding");
        }
    }
}