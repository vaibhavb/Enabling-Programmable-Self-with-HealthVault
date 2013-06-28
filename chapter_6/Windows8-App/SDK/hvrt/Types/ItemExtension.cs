// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemExtension : IHealthVaultTypeSerializable
    {
        public ItemExtension()
        {
            Source = String.Empty;
            Ver = String.Empty;
            Logo = String.Empty;
            Xsl = String.Empty;
        }

        [XmlAttribute("source")]
        public string Source { get; set; }

        /// <remarks/>
        [XmlAttribute("ver")]
        public string Ver { get; set; }

        /// <remarks/>
        [XmlAttribute("logo")]
        public string Logo { get; set; }

        /// <remarks/>
        [XmlAttribute("xsl")]
        public string Xsl { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
        }

        #endregion

        public bool ShouldSerializeSource()
        {
            return !String.IsNullOrEmpty(Source);
        }

        public bool ShouldSerializeVer()
        {
            return !String.IsNullOrEmpty(Ver);
        }

        public bool ShouldSerializeLogo()
        {
            return !String.IsNullOrEmpty(Logo);
        }

        public bool ShouldSerializeXsl()
        {
            return !String.IsNullOrEmpty(Xsl);
        }
    }
}