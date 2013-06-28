// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class VocabItem : IHealthVaultTypeSerializable
    {
        public VocabItem()
        {
            Code = String.Empty;
            DisplayText = String.Empty;
            Abbrv = String.Empty;
        }

        [XmlElement("code-value", Order = 1)]
        public string Code { get; set; }

        [XmlElement("display-text", Order = 2)]
        public string DisplayText { get; set; }

        [XmlElement("abbreviation-text", Order = 3)]
        public string Abbrv { get; set; }

        [XmlElement("info-xml", Order = 4)]
        public VocabData Data { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Code.ValidateRequired("Code");
        }

        #endregion

        public bool ShouldSerializeCode()
        {
            return !String.IsNullOrEmpty(Code);
        }

        public bool ShouldSerializeDisplayText()
        {
            return !String.IsNullOrEmpty(DisplayText);
        }

        public bool ShouldSerializeAbbrv()
        {
            return !String.IsNullOrEmpty(Abbrv);
        }
    }
}