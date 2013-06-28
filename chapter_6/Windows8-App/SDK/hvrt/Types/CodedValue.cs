// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class CodedValue : IHealthVaultTypeSerializable
    {
        public CodedValue()
        {
            Code = String.Empty;
            VocabFamily = String.Empty;
            VocabName = String.Empty;
            VocabVersion = String.Empty;
        }

        public CodedValue(string code, string vocabulary) : this()
        {
            if (string.IsNullOrEmpty(vocabulary))
            {
                throw new ArgumentException("vocabulary");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("code");
            }
            VocabName = vocabulary;
            Code = code;
        }

        public CodedValue(CodedValue source) : this()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Code = source.Code;
            VocabFamily = source.VocabFamily;
            VocabName = source.VocabName;
            VocabVersion = source.VocabVersion;
        }

        [XmlElement("value", Order = 1)]
        public string Code { get; set; }

        [XmlElement("family", Order = 2)]
        public string VocabFamily { get; set; }

        [XmlElement("type", Order = 3)]
        public string VocabName { get; set; }

        [XmlElement("version", Order = 4)]
        public string VocabVersion { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Code.ValidateRequired("Value");
            VocabName.ValidateRequired("Type");
        }

        #endregion

        public override bool Equals(object obj)
        {
            var coded = obj as CodedValue;
            if (coded == null)
            {
                return base.Equals(obj);
            }

            if (coded != null)
            {
                return (
                    Code.SafeEquals(coded.Code) &&
                        VocabName.SafeEquals(coded.VocabName) &&
                        VocabFamily.SafeEquals(coded.VocabFamily) &&
                        VocabVersion.SafeEquals(coded.VocabVersion)
                    );
            }

            return base.Equals(obj);
        }

        public bool Equals(string code, string vocabName)
        {
            return (
                Code.SafeEquals(code) &&
                    VocabName.SafeEquals(vocabName)
                );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool ShouldSerializeCode()
        {
            return Code != null; // We allow empty strings for Code due to schema bug.
        }

        public bool ShouldSerializeVocabFamily()
        {
            return !String.IsNullOrEmpty(VocabFamily);
        }

        public bool ShouldSerializeVocabName()
        {
            return !String.IsNullOrEmpty(VocabName);
        }

        public bool ShouldSerializeVocabVersion()
        {
            return !String.IsNullOrEmpty(VocabVersion);
        }
    }
}